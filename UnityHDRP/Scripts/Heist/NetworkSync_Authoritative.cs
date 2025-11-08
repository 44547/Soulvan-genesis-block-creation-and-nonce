using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// NetworkSync_Authoritative: Server-authoritative networking for heist missions.
/// Features: Player sync, mission state replication, anti-cheat validation.
/// Integrates with MissionController for authoritative game state.
/// </summary>
public class NetworkSync_Authoritative : MonoBehaviour
{
    [Header("Server Configuration")]
    [Tooltip("Server URL for mission sync")]
    public string serverUrl = "https://api.soulvan";

    [Tooltip("Sync interval in seconds")]
    public float syncInterval = 0.5f;

    [Tooltip("Enable client-side prediction")]
    public bool enablePrediction = true;

    [Tooltip("Maximum allowed client deviation (meters)")]
    public float maxClientDeviation = 5f;

    [Header("Anti-Cheat")]
    [Tooltip("Enable server validation for critical events")]
    public bool enableServerValidation = true;

    [Tooltip("Maximum heat change per second (anti-cheat)")]
    public float maxHeatChangePerSecond = 50f;

    // Internal state
    private MissionController _mc;
    private float _syncTimer = 0f;
    private Dictionary<string, PlayerSyncData> _playerStates = new Dictionary<string, PlayerSyncData>();
    private MissionSyncData _lastServerState;
    private bool _isConnected = false;

    // Events
    public delegate void SyncEventHandler(MissionSyncData serverState);
    public event SyncEventHandler OnServerStateReceived;

    void Awake()
    {
        _mc = FindObjectOfType<MissionController>();
    }

    void Start()
    {
        StartCoroutine(ConnectToServer());
    }

    void Update()
    {
        if (!_isConnected) return;

        _syncTimer += Time.deltaTime;
        if (_syncTimer >= syncInterval)
        {
            _syncTimer = 0f;
            SyncWithServer();
        }
    }

    /// <summary>
    /// Connect to server
    /// </summary>
    IEnumerator ConnectToServer()
    {
        Debug.Log("NetworkSync: Connecting to server...");

        // Get auth token from ServerRpcClient
        string authToken = ServerRpcClient.Instance != null ? ServerRpcClient.Instance.authToken : "";

        if (string.IsNullOrEmpty(authToken))
        {
            Debug.LogWarning("NetworkSync: No auth token, sync disabled");
            yield break;
        }

        _isConnected = true;
        Debug.Log("NetworkSync: Connected to server");
    }

    /// <summary>
    /// Sync with server (send client state, receive server state)
    /// </summary>
    void SyncWithServer()
    {
        if (_mc == null || _mc.state == null) return;

        // Build sync payload
        MissionSyncData syncData = new MissionSyncData
        {
            missionId = _mc.state.missionId,
            elapsed = _mc.state.elapsed,
            heat = _mc.state.heat,
            missionState = _mc.state.missionState.ToString(),
            playerStates = GetPlayerSyncData()
        };

        // Send to server
        StartCoroutine(SendSyncData(syncData));
    }

    /// <summary>
    /// Get player sync data
    /// </summary>
    List<PlayerSyncData> GetPlayerSyncData()
    {
        List<PlayerSyncData> playerData = new List<PlayerSyncData>();

        // Find all players in scene
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            PlayerSyncData data = new PlayerSyncData
            {
                playerId = player.name,
                position = player.transform.position,
                rotation = player.transform.rotation.eulerAngles,
                velocity = Vector3.zero // TODO: Get from Rigidbody
            };

            playerData.Add(data);

            // Cache for prediction
            _playerStates[player.name] = data;
        }

        return playerData;
    }

    /// <summary>
    /// Send sync data to server
    /// </summary>
    IEnumerator SendSyncData(MissionSyncData syncData)
    {
        string json = JsonUtility.ToJson(syncData);
        string endpoint = "mission/sync";

        if (ServerRpcClient.Instance != null)
        {
            bool success = false;
            string response = null;

            yield return ServerRpcClient.Instance.PostJson(endpoint, json, (s, r) =>
            {
                success = s;
                response = r;
            });

            if (success)
            {
                ProcessServerResponse(response);
            }
        }
    }

    /// <summary>
    /// Process server response
    /// </summary>
    void ProcessServerResponse(string json)
    {
        try
        {
            MissionSyncResponse response = JsonUtility.FromJson<MissionSyncResponse>(json);

            // Validate server state
            if (enableServerValidation)
            {
                ValidateServerState(response.serverState);
            }

            // Apply server corrections
            if (response.corrections != null && response.corrections.Count > 0)
            {
                ApplyServerCorrections(response.corrections);
            }

            _lastServerState = response.serverState;
            OnServerStateReceived?.Invoke(response.serverState);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"NetworkSync: Failed to parse server response: {e.Message}");
        }
    }

    /// <summary>
    /// Validate server state against client state
    /// </summary>
    void ValidateServerState(MissionSyncData serverState)
    {
        if (_mc == null || _mc.state == null) return;

        // Check heat deviation (anti-cheat)
        float heatDiff = Mathf.Abs(serverState.heat - _mc.state.heat);
        float timeSinceLastSync = syncInterval;
        float maxAllowedHeatChange = maxHeatChangePerSecond * timeSinceLastSync;

        if (heatDiff > maxAllowedHeatChange)
        {
            Debug.LogWarning($"NetworkSync: Heat deviation too large ({heatDiff}), applying server correction");
            _mc.state.heat = serverState.heat;
        }

        // Check position deviation for players
        foreach (var serverPlayerState in serverState.playerStates)
        {
            if (_playerStates.ContainsKey(serverPlayerState.playerId))
            {
                PlayerSyncData clientState = _playerStates[serverPlayerState.playerId];
                float positionDiff = Vector3.Distance(serverPlayerState.position, clientState.position);

                if (positionDiff > maxClientDeviation)
                {
                    Debug.LogWarning($"NetworkSync: Player {serverPlayerState.playerId} position deviation ({positionDiff}m), correcting");
                    CorrectPlayerPosition(serverPlayerState.playerId, serverPlayerState.position);
                }
            }
        }
    }

    /// <summary>
    /// Apply server corrections
    /// </summary>
    void ApplyServerCorrections(List<ServerCorrection> corrections)
    {
        foreach (var correction in corrections)
        {
            Debug.Log($"NetworkSync: Applying correction - {correction.type}: {correction.reason}");

            switch (correction.type)
            {
                case "heat":
                    if (_mc != null && _mc.state != null)
                    {
                        _mc.state.heat = correction.value;
                    }
                    break;

                case "position":
                    CorrectPlayerPosition(correction.playerId, correction.position);
                    break;

                case "mission_state":
                    // Server forcing mission state change (e.g., mission failed due to cheat detection)
                    if (_mc != null)
                    {
                        Debug.LogWarning($"NetworkSync: Server forced mission state to {correction.reason}");
                        _mc.EndMissionFail();
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Correct player position
    /// </summary>
    void CorrectPlayerPosition(string playerId, Vector3 serverPosition)
    {
        var player = GameObject.Find(playerId);
        if (player != null)
        {
            player.transform.position = serverPosition;

            // Smooth interpolation for client
            // TODO: Implement smooth correction over multiple frames
        }
    }

    /// <summary>
    /// Request server validation for critical event
    /// </summary>
    public IEnumerator ValidateEvent(string eventType, Dictionary<string, object> eventData, System.Action<bool> callback)
    {
        if (!enableServerValidation)
        {
            callback?.Invoke(true);
            yield break;
        }

        EventValidationRequest request = new EventValidationRequest
        {
            missionId = _mc != null && _mc.state != null ? _mc.state.missionId : "",
            eventType = eventType,
            eventData = eventData,
            timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        string json = JsonUtility.ToJson(request);
        string endpoint = "mission/validate-event";

        bool isValid = false;

        if (ServerRpcClient.Instance != null)
        {
            bool success = false;
            string response = null;

            yield return ServerRpcClient.Instance.PostJson(endpoint, json, (s, r) =>
            {
                success = s;
                response = r;
            });

            if (success)
            {
                try
                {
                    EventValidationResponse validationResponse = JsonUtility.FromJson<EventValidationResponse>(response);
                    isValid = validationResponse.isValid;

                    if (!isValid)
                    {
                        Debug.LogWarning($"NetworkSync: Event '{eventType}' rejected by server: {validationResponse.reason}");
                    }
                }
                catch
                {
                    isValid = false;
                }
            }
        }

        callback?.Invoke(isValid);
    }
}

/// <summary>
/// Mission sync data
/// </summary>
[System.Serializable]
public class MissionSyncData
{
    public string missionId;
    public float elapsed;
    public float heat;
    public string missionState;
    public List<PlayerSyncData> playerStates;
}

/// <summary>
/// Player sync data
/// </summary>
[System.Serializable]
public class PlayerSyncData
{
    public string playerId;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 velocity;
}

/// <summary>
/// Server sync response
/// </summary>
[System.Serializable]
public class MissionSyncResponse
{
    public MissionSyncData serverState;
    public List<ServerCorrection> corrections;
}

/// <summary>
/// Server correction
/// </summary>
[System.Serializable]
public class ServerCorrection
{
    public string type; // "heat", "position", "mission_state"
    public string playerId;
    public float value;
    public Vector3 position;
    public string reason;
}

/// <summary>
/// Event validation request
/// </summary>
[System.Serializable]
public class EventValidationRequest
{
    public string missionId;
    public string eventType;
    public Dictionary<string, object> eventData;
    public long timestamp;
}

/// <summary>
/// Event validation response
/// </summary>
[System.Serializable]
public class EventValidationResponse
{
    public bool isValid;
    public string reason;
}
