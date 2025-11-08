using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

/**
 * Soulvan DAO Mission Notifier
 * 
 * Notifies DAO Replay Impact API when missions affect governance
 * Features:
 * - Automatic DAO impact calculation
 * - Tier upgrade evaluation
 * - Influence score tracking
 * - Integration with DAOOverlayFXManager for visual feedback
 */

public class DAOMissionNotifier : MonoBehaviour
{
    public static DAOMissionNotifier Instance { get; private set; }

    [Header("DAO API")]
    public string daoImpactEndpoint = "http://localhost:3004/dao/impact";
    public string authToken;
    
    [Header("Impact Calculation")]
    public float heatToInfluenceMultiplier = 0.1f;
    public int tierUpgradeThreshold = 50; // Heat required for tier upgrade check
    
    [Header("Visual Integration")]
    public bool triggerVisualFX = true;

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
        // Load auth token
        if (string.IsNullOrEmpty(authToken))
        {
            authToken = PlayerPrefs.GetString("SoulvanAuthToken", "");
        }
    }

    // ============================
    // Public API
    // ============================

    public void NotifyImpact(string contributorId, string missionId, string replaySeed, int heatDelta)
    {
        var payload = new DAOImpactPayload
        {
            proposalId = null, // Missions don't always link to specific proposals
            contributorId = contributorId,
            replayId = replaySeed,
            heatDelta = heatDelta,
            influenceScore = CalculateInfluenceScore(heatDelta),
            tierPulse = EvaluateTierPulse(contributorId, heatDelta)
        };

        StartCoroutine(PostImpact(payload, contributorId));
        
        SoulvanLore.Record($"DAO impact notified: {contributorId} - Heat: {heatDelta}, Influence: {payload.influenceScore:F2}");
    }

    public void NotifyProposalImpact(string proposalId, string contributorId, int votePower)
    {
        var payload = new DAOImpactPayload
        {
            proposalId = proposalId,
            contributorId = contributorId,
            replayId = null,
            heatDelta = 0,
            influenceScore = votePower * 0.1f,
            tierPulse = null
        };

        StartCoroutine(PostImpact(payload, contributorId));
        
        SoulvanLore.Record($"DAO proposal impact: {proposalId} by {contributorId}");
    }

    // ============================
    // Impact Calculation
    // ============================

    float CalculateInfluenceScore(int heatDelta)
    {
        // Map heat to influence score (0-10 scale)
        float baseInfluence = heatDelta * heatToInfluenceMultiplier;
        return Mathf.Clamp(baseInfluence, 0f, 10f);
    }

    TierPulseData EvaluateTierPulse(string contributorId, int heatDelta)
    {
        // Check if heat threshold met for potential tier upgrade
        if (heatDelta < tierUpgradeThreshold)
        {
            return null;
        }
        
        // Get current tier from PlayerPrefs or profile system
        string currentTier = PlayerPrefs.GetString($"{contributorId}_Tier", "Initiate");
        
        // Simple upgrade logic - in production, this would check actual XP/badge system
        string nextTier = GetNextTier(currentTier);
        
        if (nextTier != currentTier)
        {
            return new TierPulseData
            {
                from = currentTier,
                to = nextTier
            };
        }
        
        return null;
    }

    string GetNextTier(string currentTier)
    {
        switch (currentTier)
        {
            case "Initiate": return "Builder";
            case "Builder": return "Architect";
            case "Architect": return "Oracle";
            case "Oracle": return "Legend";
            case "Operative": return "Legend";
            case "Legend": return "Legend"; // Max tier
            default: return currentTier;
        }
    }

    // ============================
    // API Communication
    // ============================

    IEnumerator PostImpact(DAOImpactPayload payload, string contributorId)
    {
        var json = JsonUtility.ToJson(payload);
        var www = new UnityWebRequest(daoImpactEndpoint, "POST");
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
            Debug.Log($"[DAOMissionNotifier] Impact posted successfully for {contributorId}");
            
            // Parse response
            try
            {
                var response = JsonUtility.FromJson<DAOImpactResponse>(www.downloadHandler.text);
                ProcessImpactResponse(response, contributorId);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[DAOMissionNotifier] Could not parse response: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"[DAOMissionNotifier] Failed to post impact: {www.error}");
        }
    }

    void ProcessImpactResponse(DAOImpactResponse response, string contributorId)
    {
        Debug.Log($"[DAOMissionNotifier] Impact ID: {response.impactId}, Ripple Nodes: {response.rippleNodes.Length}");
        
        // Trigger visual FX if enabled
        if (triggerVisualFX)
        {
            DAOOverlayFXManager fxManager = DAOOverlayFXManager.Instance;
            if (fxManager != null)
            {
                fxManager.OnImpactResponse(JsonUtility.ToJson(response));
            }
        }
        
        // Check for tier upgrade
        if (response.badgeUpgrades > 0)
        {
            TriggerTierUpgradeEffects(contributorId);
        }
    }

    void TriggerTierUpgradeEffects(string contributorId)
    {
        // Get current tier
        string currentTier = PlayerPrefs.GetString($"{contributorId}_Tier", "Initiate");
        string nextTier = GetNextTier(currentTier);
        
        // Update tier in storage
        PlayerPrefs.SetString($"{contributorId}_Tier", nextTier);
        PlayerPrefs.Save();
        
        // Trigger visual effects
        if (triggerVisualFX)
        {
            DAOOverlayFXManager fxManager = DAOOverlayFXManager.Instance;
            if (fxManager != null)
            {
                // Get contributor position from RemixForgeGraph or default
                Vector3 position = GetContributorPosition(contributorId);
                fxManager.TriggerTierUpgradeBurst(contributorId, currentTier, nextTier, position);
            }
        }
        
        Debug.Log($"[DAOMissionNotifier] Tier upgrade: {contributorId} from {currentTier} to {nextTier}");
        SoulvanLore.Record($"Contributor {contributorId} upgraded from {currentTier} to {nextTier}");
    }

    Vector3 GetContributorPosition(string contributorId)
    {
        // Try to get position from RemixForgeGraph
        RemixForgeGraph graph = FindObjectOfType<RemixForgeGraph>();
        if (graph != null)
        {
            // In a real implementation, this would query the graph for the contributor's node position
            // For now, return a default position
            return Vector3.zero;
        }
        
        return Vector3.zero;
    }

    // ============================
    // Public Utilities
    // ============================

    public void SetAuthToken(string token)
    {
        authToken = token;
        PlayerPrefs.SetString("SoulvanAuthToken", token);
        PlayerPrefs.Save();
    }
}

// ============================
// Data Structures
// ============================

[System.Serializable]
public class DAOImpactPayload
{
    public string proposalId;
    public string contributorId;
    public string replayId;
    public int heatDelta;
    public float influenceScore;
    public TierPulseData tierPulse;
}

[System.Serializable]
public class TierPulseData
{
    public string from;
    public string to;
}

[System.Serializable]
public class DAOImpactResponse
{
    public string impactId;
    public string proposalId;
    public string contributorId;
    public string[] rippleNodes;
    public int badgeUpgrades;
    public int heatDelta;
    public float influenceScore;
    public string recordedAt;
    public string message;
}
