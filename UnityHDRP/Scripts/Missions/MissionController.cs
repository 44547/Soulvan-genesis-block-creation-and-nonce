using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/**
 * Soulvan Mission System
 * 
 * Core mission orchestration system for GTA-style heist missions
 * Supports:
 * - Mission state management with deterministic seed generation
 * - Role-based multiplayer (Driver, Infiltrator, Systems, Support)
 * - Adaptive AI Director for dynamic difficulty
 * - Replay seed logging for RemixForge integration
 * - DAO impact tracking for governance integration
 */

// ============================
// Enums & Data Structures
// ============================

public enum MissionStateEnum { Waiting, InProgress, Paused, Completed, Failed }
public enum PlayerRole { Driver, Infiltrator, Systems, Support }
public enum MissionType { Deliver, Heist, Boss, RaceAssist, Infiltration, Extraction }
public enum EnvironmentType { City, Industrial, Cosmic, Mountain, Underwater, Sky }

[Serializable]
public class MissionState
{
    public string missionId;
    public MissionStateEnum state;
    public float startTime;
    public float elapsed => Time.time - startTime;
    public List<string> activatedModules = new List<string>();
    public string datacoreTier;
    public int heat;
    public Dictionary<string, object> metadata = new Dictionary<string, object>();
    public List<string> roleAssignments = new List<string>();
}

[Serializable]
public class ReplayLogDto
{
    public string contributorId;
    public string missionId;
    public string seed;
    public string[] modules;
    public string datacoreTier;
    public int durationSec;
    public string outcome;
    public object loot;
}

// ============================
// Mission Controller
// ============================

public class MissionController : MonoBehaviour
{
    public string missionId = "neon_vault_heist_v1";
    public SO_MissionModule[] availableModules;
    public SO_DatacoreTier[] datacoreTiers;
    
    [Header("Mission Configuration")]
    public MissionType missionType = MissionType.Heist;
    public EnvironmentType environmentType = EnvironmentType.City;
    public int rewardSVN = 1000;
    public float missionTimeLimit = 1200f; // 20 minutes
    
    [Header("Role Configuration")]
    public bool enableRoles = true;
    public int maxPlayers = 4;

    // Events
    public event Action<MissionState> OnMissionStarted;
    public event Action<MissionState> OnMissionUpdated;
    public event Action<MissionState> OnMissionCompleted;
    public event Action<MissionState> OnMissionFailed;

    private MissionState _state;
    private AIDirectorHeist _aiDirector;

    void Awake()
    {
        _state = new MissionState 
        { 
            missionId = missionId, 
            state = MissionStateEnum.Waiting 
        };
        
        _aiDirector = GetComponent<AIDirectorHeist>();
        if (_aiDirector == null)
        {
            _aiDirector = gameObject.AddComponent<AIDirectorHeist>();
        }
    }

    // ============================
    // Mission Flow Control
    // ============================

    public void StartMission()
    {
        _state.state = MissionStateEnum.InProgress;
        _state.startTime = Time.time;
        _state.heat = 0;
        
        OnMissionStarted?.Invoke(_state);
        StartCoroutine(RunMissionLoop());
        
        SoulvanLore.Record($"Mission started: {missionId}");
    }

    public void PauseMission()
    {
        _state.state = MissionStateEnum.Paused;
        SoulvanLore.Record($"Mission paused: {missionId}");
    }

    public void ResumeMission()
    {
        _state.state = MissionStateEnum.InProgress;
        SoulvanLore.Record($"Mission resumed: {missionId}");
    }

    public void EndMissionSuccess()
    {
        _state.state = MissionStateEnum.Completed;
        _state.datacoreTier = ChooseDatacoreTier();
        
        OnMissionCompleted?.Invoke(_state);
        FinalizeAndLog();
        
        SoulvanLore.Record($"Mission completed: {missionId} - Tier: {_state.datacoreTier}");
    }

    public void EndMissionFail()
    {
        _state.state = MissionStateEnum.Failed;
        
        OnMissionFailed?.Invoke(_state);
        FinalizeAndLog();
        
        SoulvanLore.Record($"Mission failed: {missionId}");
    }

    IEnumerator RunMissionLoop()
    {
        while (_state.state == MissionStateEnum.InProgress)
        {
            // Check time limit
            if (_state.elapsed > missionTimeLimit)
            {
                EndMissionFail();
                yield break;
            }
            
            // AI Director tick
            if (_aiDirector != null)
            {
                _aiDirector.EvaluateTension(_state);
            }
            
            OnMissionUpdated?.Invoke(_state);
            
            yield return new WaitForSeconds(0.5f);
        }
    }

    // ============================
    // Module Management
    // ============================

    public void RegisterModuleActivation(string moduleId)
    {
        if (!_state.activatedModules.Contains(moduleId))
        {
            _state.activatedModules.Add(moduleId);
            
            // Add heat based on module type
            _state.heat += CalculateModuleHeat(moduleId);
            
            SoulvanLore.Record($"Module activated: {moduleId} (Heat: {_state.heat})");
        }
    }

    int CalculateModuleHeat(string moduleId)
    {
        // Base heat calculation
        if (moduleId.Contains("alarm")) return 15;
        if (moduleId.Contains("breach")) return 10;
        if (moduleId.Contains("combat")) return 20;
        if (moduleId.Contains("stealth")) return 5;
        
        return 8;
    }

    string ChooseDatacoreTier()
    {
        if (datacoreTiers == null || datacoreTiers.Length == 0)
        {
            return "standard";
        }
        
        // Weighted selection based on performance
        float performanceScore = CalculatePerformanceScore();
        
        if (performanceScore > 0.9f && datacoreTiers.Length > 2)
        {
            return datacoreTiers[datacoreTiers.Length - 1].tierId; // Mythic
        }
        else if (performanceScore > 0.7f && datacoreTiers.Length > 1)
        {
            return datacoreTiers[datacoreTiers.Length - 2].tierId; // Rare
        }
        else
        {
            return datacoreTiers[0].tierId; // Standard
        }
    }

    float CalculatePerformanceScore()
    {
        // Score based on time efficiency, low heat, and completion
        float timeScore = 1f - Mathf.Clamp01(_state.elapsed / missionTimeLimit);
        float heatScore = 1f - Mathf.Clamp01(_state.heat / 100f);
        
        return (timeScore * 0.6f) + (heatScore * 0.4f);
    }

    // ============================
    // Replay Logging
    // ============================

    void FinalizeAndLog()
    {
        var dto = new ReplayLogDto
        {
            contributorId = GetLocalContributorId(),
            missionId = missionId,
            seed = GenerateDeterministicSeed(),
            modules = _state.activatedModules.ToArray(),
            datacoreTier = _state.datacoreTier,
            durationSec = Mathf.RoundToInt(_state.elapsed),
            outcome = _state.state == MissionStateEnum.Completed ? "success" : "fail",
            loot = new { datacore = _state.datacoreTier, credits = rewardSVN }
        };

        // Queue replay log
        ReplaySeedLogger logger = ReplaySeedLogger.Instance;
        if (logger != null)
        {
            logger.QueueAndSend(dto);
        }
        
        // Notify DAO system
        DAOMissionNotifier notifier = DAOMissionNotifier.Instance;
        if (notifier != null)
        {
            notifier.NotifyImpact(
                dto.contributorId, 
                dto.missionId, 
                dto.seed, 
                CalculateHeatDelta()
            );
        }
    }

    string GetLocalContributorId()
    {
        // Read from player profile or wallet binding
        // For now, return placeholder
        return PlayerPrefs.GetString("ContributorId", "C_LOCAL");
    }

    string GenerateDeterministicSeed()
    {
        // Generate deterministic seed from mission parameters
        string seedInput = $"{missionId}_{_state.startTime}_{string.Join(",", _state.activatedModules)}_{_state.heat}";
        return HashSeed(seedInput);
    }

    string HashSeed(string input)
    {
        // Simple hash for deterministic seed
        // In production, use HMAC with server salt
        int hash = input.GetHashCode();
        return $"0x{hash:X8}{UnityEngine.Random.Range(0, int.MaxValue):X8}";
    }

    int CalculateHeatDelta()
    {
        return _state.heat;
    }

    // ============================
    // Role Management
    // ============================

    public void AssignRole(string playerId, PlayerRole role)
    {
        string assignment = $"{playerId}:{role}";
        if (!_state.roleAssignments.Contains(assignment))
        {
            _state.roleAssignments.Add(assignment);
            SoulvanLore.Record($"Role assigned: {assignment}");
        }
    }

    public PlayerRole GetPlayerRole(string playerId)
    {
        foreach (string assignment in _state.roleAssignments)
        {
            if (assignment.StartsWith(playerId))
            {
                string roleStr = assignment.Split(':')[1];
                return (PlayerRole)Enum.Parse(typeof(PlayerRole), roleStr);
            }
        }
        
        return PlayerRole.Driver; // Default
    }

    // ============================
    // Public Accessors
    // ============================

    public MissionState GetState()
    {
        return _state;
    }

    public float GetElapsedTime()
    {
        return _state.elapsed;
    }

    public int GetHeat()
    {
        return _state.heat;
    }

    public void AddHeat(int amount)
    {
        _state.heat += amount;
        _state.heat = Mathf.Max(0, _state.heat);
        
        if (_state.heat > 100)
        {
            // Mission becomes very difficult
            if (_aiDirector != null)
            {
                _aiDirector.SetAggressionMultiplier(2f);
            }
        }
    }
}

// ============================
// AI Director for Adaptive Difficulty
// ============================

public class AIDirectorHeist : MonoBehaviour
{
    [Header("Tension Parameters")]
    public float minEnemyDensity = 0.5f;
    public float maxEnemyDensity = 2.0f;
    public float minChaseIntensity = 0.3f;
    public float maxChaseIntensity = 1.5f;
    
    [Header("Adaptive Tuning")]
    public float tensionSmoothTime = 2f;
    public AnimationCurve tensionCurve;
    
    private float currentTension = 0.5f;
    private float aggressionMultiplier = 1f;

    public void EvaluateTension(MissionState state)
    {
        // Calculate tension based on heat, time, and player performance
        float heatFactor = Mathf.Clamp01(state.heat / 50f);
        float timeFactor = Mathf.Clamp01(state.elapsed / 600f); // Ramps over 10 minutes
        
        float targetTension = (heatFactor * 0.7f) + (timeFactor * 0.3f);
        
        // Smooth tension changes
        currentTension = Mathf.Lerp(currentTension, targetTension, Time.deltaTime / tensionSmoothTime);
        
        // Apply tension curve
        if (tensionCurve != null && tensionCurve.length > 0)
        {
            currentTension = tensionCurve.Evaluate(currentTension);
        }
        
        // Adjust game parameters based on tension
        ApplyTensionEffects();
    }

    void ApplyTensionEffects()
    {
        float enemyDensity = Mathf.Lerp(minEnemyDensity, maxEnemyDensity, currentTension) * aggressionMultiplier;
        float chaseIntensity = Mathf.Lerp(minChaseIntensity, maxChaseIntensity, currentTension) * aggressionMultiplier;
        
        // Broadcast tension parameters to AI systems
        // This would be picked up by enemy spawners, pursuit AI, etc.
        BroadcastTensionParameters(enemyDensity, chaseIntensity);
    }

    void BroadcastTensionParameters(float enemyDensity, float chaseIntensity)
    {
        // Send messages to AI systems
        gameObject.SendMessage("OnTensionUpdate", new { enemyDensity, chaseIntensity }, SendMessageOptions.DontRequireReceiver);
    }

    public void SetAggressionMultiplier(float multiplier)
    {
        aggressionMultiplier = multiplier;
    }

    public float GetCurrentTension()
    {
        return currentTension;
    }
}

// ============================
// ScriptableObjects
// ============================

[CreateAssetMenu(menuName = "Soulvan/MissionModule")]
public class SO_MissionModule : ScriptableObject
{
    public string moduleId;
    public string displayName;
    [TextArea] public string description;
    public float weight = 1f;
    public string[] entryTags;
    public string[] exitTags;
    public int heatModifier = 0;
}

[CreateAssetMenu(menuName = "Soulvan/DatacoreTier")]
public class SO_DatacoreTier : ScriptableObject
{
    public string tierId;
    public string displayName;
    public int rarityScore;
    public Color uiColor = Color.white;
    public int creditReward;
}
