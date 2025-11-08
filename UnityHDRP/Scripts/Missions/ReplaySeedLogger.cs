using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

/**
 * Soulvan Replay Seed Logger
 * 
 * Logs mission replay seeds to RemixForge API for saga remixing
 * Features:
 * - Asynchronous queue processing
 * - Retry logic with exponential backoff
 * - Local persistence for failed sends
 * - Deterministic seed validation
 */

public class ReplaySeedLogger : MonoBehaviour
{
    public static ReplaySeedLogger Instance { get; private set; }

    [Header("Networking")]
    public string replayLogEndpoint = "http://localhost:3002/replay/log";
    public string authToken;
    public int maxRetryAttempts = 3;
    public float retryBackoffMultiplier = 2f;
    
    [Header("Local Persistence")]
    public bool enableLocalPersistence = true;
    public string persistencePath = "ReplayLogs";

    private Queue<ReplayLogDto> _pending = new Queue<ReplayLogDto>();
    private bool _isProcessing = false;
    private List<ReplayLogDto> _failedLogs = new List<ReplayLogDto>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Load auth token from PlayerPrefs or environment
        if (string.IsNullOrEmpty(authToken))
        {
            authToken = PlayerPrefs.GetString("SoulvanAuthToken", "");
        }
        
        // Load any persisted failed logs
        if (enableLocalPersistence)
        {
            LoadPersistedLogs();
        }
    }

    // ============================
    // Public API
    // ============================

    public void QueueAndSend(ReplayLogDto dto)
    {
        lock (_pending)
        {
            _pending.Enqueue(dto);
            Debug.Log($"[ReplaySeedLogger] Queued replay log: {dto.missionId}");
        }
        
        if (!_isProcessing)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    public void RetryFailedLogs()
    {
        if (_failedLogs.Count > 0)
        {
            Debug.Log($"[ReplaySeedLogger] Retrying {_failedLogs.Count} failed logs");
            
            foreach (var log in _failedLogs)
            {
                lock (_pending)
                {
                    _pending.Enqueue(log);
                }
            }
            
            _failedLogs.Clear();
            
            if (!_isProcessing)
            {
                StartCoroutine(ProcessQueue());
            }
        }
    }

    // ============================
    // Queue Processing
    // ============================

    IEnumerator ProcessQueue()
    {
        _isProcessing = true;
        
        while (true)
        {
            ReplayLogDto dto = null;
            
            lock (_pending)
            {
                if (_pending.Count > 0)
                {
                    dto = _pending.Dequeue();
                }
            }
            
            if (dto == null)
            {
                break;
            }

            // Local write for provenance
            if (enableLocalPersistence)
            {
                LocalWrite(dto);
            }

            // Async send to API with retry
            yield return StartCoroutine(SendWithRetry(dto));
        }
        
        _isProcessing = false;
    }

    void LocalWrite(ReplayLogDto dto)
    {
        try
        {
            string filename = $"{dto.missionId}_{dto.seed}_{System.DateTime.Now:yyyyMMddHHmmss}.json";
            string path = System.IO.Path.Combine(Application.persistentDataPath, persistencePath);
            
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            
            string fullPath = System.IO.Path.Combine(path, filename);
            string json = JsonUtility.ToJson(dto, true);
            
            System.IO.File.WriteAllText(fullPath, json);
            Debug.Log($"[ReplaySeedLogger] Persisted locally: {filename}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ReplaySeedLogger] Local write failed: {e.Message}");
        }
    }

    IEnumerator SendWithRetry(ReplayLogDto dto)
    {
        int attempts = 0;
        
        while (attempts < maxRetryAttempts)
        {
            attempts++;
            
            var json = JsonUtility.ToJson(dto);
            var www = new UnityWebRequest(replayLogEndpoint, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            
            if (!string.IsNullOrEmpty(authToken))
            {
                www.SetRequestHeader("Authorization", $"Bearer {authToken}");
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[ReplaySeedLogger] Successfully sent replay log: {dto.missionId}");
                
                // Parse response to get replayId
                try
                {
                    var response = JsonUtility.FromJson<ReplayLogResponse>(www.downloadHandler.text);
                    Debug.Log($"[ReplaySeedLogger] Received replay ID: {response.replayId}");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ReplaySeedLogger] Could not parse response: {e.Message}");
                }
                
                yield break;
            }
            else
            {
                Debug.LogWarning($"[ReplaySeedLogger] Send attempt {attempts} failed: {www.error}");
                
                // Exponential backoff
                float waitTime = Mathf.Pow(retryBackoffMultiplier, attempts - 1);
                yield return new WaitForSeconds(waitTime);
            }
        }

        // If failed after all retries, persist for manual sync
        Debug.LogError($"[ReplaySeedLogger] Failed to send replay log after {maxRetryAttempts} attempts: {dto.missionId}");
        PersistForManualSync(dto);
    }

    void PersistForManualSync(ReplayLogDto dto)
    {
        _failedLogs.Add(dto);
        
        if (enableLocalPersistence)
        {
            try
            {
                string filename = $"FAILED_{dto.missionId}_{dto.seed}.json";
                string path = System.IO.Path.Combine(Application.persistentDataPath, persistencePath, "Failed");
                
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                
                string fullPath = System.IO.Path.Combine(path, filename);
                string json = JsonUtility.ToJson(dto, true);
                
                System.IO.File.WriteAllText(fullPath, json);
                Debug.Log($"[ReplaySeedLogger] Persisted failed log for manual sync: {filename}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ReplaySeedLogger] Failed to persist for manual sync: {e.Message}");
            }
        }
    }

    void LoadPersistedLogs()
    {
        try
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, persistencePath, "Failed");
            
            if (System.IO.Directory.Exists(path))
            {
                string[] files = System.IO.Directory.GetFiles(path, "FAILED_*.json");
                
                foreach (string file in files)
                {
                    string json = System.IO.File.ReadAllText(file);
                    var dto = JsonUtility.FromJson<ReplayLogDto>(json);
                    _failedLogs.Add(dto);
                }
                
                if (_failedLogs.Count > 0)
                {
                    Debug.Log($"[ReplaySeedLogger] Loaded {_failedLogs.Count} failed logs from disk");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ReplaySeedLogger] Failed to load persisted logs: {e.Message}");
        }
    }

    // ============================
    // Public Utilities
    // ============================

    public int GetQueuedCount()
    {
        lock (_pending)
        {
            return _pending.Count;
        }
    }

    public int GetFailedCount()
    {
        return _failedLogs.Count;
    }

    public bool IsProcessing()
    {
        return _isProcessing;
    }
}

// ============================
// Data Structures
// ============================

[System.Serializable]
public class ReplayLogResponse
{
    public string replayId;
    public string remixSeed;
    public string message;
}
