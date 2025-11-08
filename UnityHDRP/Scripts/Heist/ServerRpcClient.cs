using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

/// <summary>
/// ServerRpcClient: Robust RPC wrapper for server communication with retry logic,
/// exponential backoff, and auth token management. Centralizes all API calls
/// for replay logging, DAO impact tracking, and mission sync.
/// </summary>
public class ServerRpcClient : MonoBehaviour
{
    public static ServerRpcClient Instance { get; private set; }

    [Header("Configuration")]
    [Tooltip("Base API URL (e.g., https://api.soulvan)")]
    public string baseUrl = "https://api.soulvan";

    [Tooltip("Authentication token (JWT)")]
    public string authToken;

    [Header("Retry Settings")]
    [Tooltip("Maximum retry attempts for transient failures")]
    public int maxRetries = 3;

    [Tooltip("Base delay in seconds for exponential backoff")]
    public float baseRetryDelay = 0.5f;

    [Header("Timeout Settings")]
    [Tooltip("Request timeout in seconds")]
    public int requestTimeout = 30;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DebugLog("ServerRpcClient: Initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Generic POST JSON method with retry logic
    /// </summary>
    /// <param name="endpoint">API endpoint (e.g., "replay/log")</param>
    /// <param name="jsonBody">JSON request body</param>
    /// <param name="onComplete">Callback with (success, responseBody)</param>
    public IEnumerator PostJson(string endpoint, string jsonBody, Action<bool, string> onComplete)
    {
        int attempt = 0;
        string fullUrl = $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

        while (attempt < maxRetries)
        {
            attempt++;
            DebugLog($"ServerRpcClient: POST {fullUrl} (attempt {attempt}/{maxRetries})");

            using (var www = new UnityWebRequest(fullUrl, "POST"))
            {
                // Setup request
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.timeout = requestTimeout;

                // Set headers
                www.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(authToken))
                {
                    www.SetRequestHeader("Authorization", $"Bearer {authToken}");
                }

                yield return www.SendWebRequest();

                // Check result
                if (www.result == UnityWebRequest.Result.Success)
                {
                    DebugLog($"ServerRpcClient: Success - {www.downloadHandler.text}");
                    onComplete?.Invoke(true, www.downloadHandler.text);
                    yield break;
                }
                else
                {
                    DebugLog($"ServerRpcClient: Error - {www.error} (code: {www.responseCode})");

                    // Check if this is a permanent failure (4xx client errors)
                    if (IsPermanentFailure(www.responseCode))
                    {
                        Debug.LogError($"ServerRpcClient: Permanent failure for {endpoint}: {www.error}");
                        onComplete?.Invoke(false, www.downloadHandler.text);
                        yield break;
                    }

                    // Transient failure - retry with exponential backoff
                    if (attempt < maxRetries)
                    {
                        float delay = baseRetryDelay * Mathf.Pow(2, attempt - 1);
                        DebugLog($"ServerRpcClient: Retrying in {delay}s...");
                        yield return new WaitForSeconds(delay);
                    }
                }
            }
        }

        // All retries exhausted
        Debug.LogError($"ServerRpcClient: Max retries exhausted for {endpoint}");
        onComplete?.Invoke(false, null);
    }

    /// <summary>
    /// Generic GET method with retry logic
    /// </summary>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="onComplete">Callback with (success, responseBody)</param>
    public IEnumerator GetJson(string endpoint, Action<bool, string> onComplete)
    {
        int attempt = 0;
        string fullUrl = $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

        while (attempt < maxRetries)
        {
            attempt++;
            DebugLog($"ServerRpcClient: GET {fullUrl} (attempt {attempt}/{maxRetries})");

            using (var www = UnityWebRequest.Get(fullUrl))
            {
                www.timeout = requestTimeout;

                // Set headers
                if (!string.IsNullOrEmpty(authToken))
                {
                    www.SetRequestHeader("Authorization", $"Bearer {authToken}");
                }

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    DebugLog($"ServerRpcClient: Success - {www.downloadHandler.text}");
                    onComplete?.Invoke(true, www.downloadHandler.text);
                    yield break;
                }
                else
                {
                    DebugLog($"ServerRpcClient: Error - {www.error} (code: {www.responseCode})");

                    if (IsPermanentFailure(www.responseCode))
                    {
                        Debug.LogError($"ServerRpcClient: Permanent failure for {endpoint}: {www.error}");
                        onComplete?.Invoke(false, www.downloadHandler.text);
                        yield break;
                    }

                    if (attempt < maxRetries)
                    {
                        float delay = baseRetryDelay * Mathf.Pow(2, attempt - 1);
                        DebugLog($"ServerRpcClient: Retrying in {delay}s...");
                        yield return new WaitForSeconds(delay);
                    }
                }
            }
        }

        Debug.LogError($"ServerRpcClient: Max retries exhausted for {endpoint}");
        onComplete?.Invoke(false, null);
    }

    /// <summary>
    /// Check if HTTP status code indicates permanent failure (don't retry)
    /// </summary>
    bool IsPermanentFailure(long code)
    {
        // 400-range are client errors (don't retry)
        // 500-range are server errors (retry)
        // 0 means network error (retry)
        return code >= 400 && code < 500;
    }

    /// <summary>
    /// Set authentication token
    /// </summary>
    public void SetAuthToken(string token)
    {
        authToken = token;
        DebugLog($"ServerRpcClient: Auth token updated");
    }

    /// <summary>
    /// Set base URL
    /// </summary>
    public void SetBaseUrl(string url)
    {
        baseUrl = url;
        DebugLog($"ServerRpcClient: Base URL set to {baseUrl}");
    }

    // Convenience wrappers for common endpoints

    /// <summary>
    /// Post replay log to server
    /// </summary>
    public IEnumerator PostReplayLog(string json, Action<bool, string> callback)
    {
        yield return PostJson("replay/log", json, callback);
    }

    /// <summary>
    /// Post DAO impact event to server
    /// </summary>
    public IEnumerator PostDaoImpact(string json, Action<bool, string> callback)
    {
        yield return PostJson("dao/impact", json, callback);
    }

    /// <summary>
    /// Post DAO vote to server
    /// </summary>
    public IEnumerator PostDaoVote(string json, Action<bool, string> callback)
    {
        yield return PostJson("dao/vote", json, callback);
    }

    /// <summary>
    /// Get DAO proposals
    /// </summary>
    public IEnumerator GetDaoProposals(Action<bool, string> callback)
    {
        yield return GetJson("dao/proposals", callback);
    }

    /// <summary>
    /// Get contributor stats
    /// </summary>
    public IEnumerator GetContributorStats(string contributorId, Action<bool, string> callback)
    {
        yield return GetJson($"dao/contributor-stats/{contributorId}", callback);
    }

    /// <summary>
    /// Get vote ripples for proposal
    /// </summary>
    public IEnumerator GetVoteRipples(string proposalId, Action<bool, string> callback)
    {
        yield return GetJson($"dao/vote-ripples/{proposalId}", callback);
    }

    /// <summary>
    /// Verify server signature for replay seed
    /// </summary>
    public IEnumerator VerifyReplaySeed(string signedSeed, Action<bool, string> callback)
    {
        string json = $"{{\"signedSeed\":\"{signedSeed}\"}}";
        yield return PostJson("replay/verify", json, callback);
    }

    /// <summary>
    /// Initialize authentication with wallet
    /// </summary>
    public IEnumerator InitAuth(string walletAddress, Action<bool, string> callback)
    {
        string json = $"{{\"wallet\":\"{walletAddress}\"}}";
        yield return PostJson("auth/init", json, (success, response) =>
        {
            if (success)
            {
                // Parse JWT token from response
                try
                {
                    var authResponse = JsonUtility.FromJson<AuthInitResponse>(response);
                    SetAuthToken(authResponse.token);
                    DebugLog($"ServerRpcClient: Auth initialized for contributor {authResponse.contributorId}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"ServerRpcClient: Failed to parse auth response: {e.Message}");
                }
            }
            callback?.Invoke(success, response);
        });
    }

    void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
}

/// <summary>
/// Response DTO for auth initialization
/// </summary>
[Serializable]
public class AuthInitResponse
{
    public string token;
    public string contributorId;
    public long expiresAt;
}
