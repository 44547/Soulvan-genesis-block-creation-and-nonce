using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AIDirector_Heist: Adaptive AI system that scales enemy spawns, aggression, and audio intensity
/// based on mission tension computed from heat, elapsed time, and triggered alerts.
/// Integrates with MissionController for state tracking and SpawnManager for enemy placement.
/// </summary>
public class AIDirector_Heist : MonoBehaviour
{
    [Header("Tuning")]
    [Tooltip("Base spawn interval in seconds when tension is zero")]
    public float baseSpawnInterval = 5f;
    
    [Tooltip("Base enemy count per spawn wave")]
    public int baseEnemyCount = 3;
    
    [Range(0f, 1f)]
    [Tooltip("Current tension level (0 = calm, 1 = maximum intensity)")]
    public float tension = 0f;
    
    [Tooltip("Maps tension (0-1) to spawn rate multiplier")]
    public AnimationCurve tensionToSpawnRate = AnimationCurve.Linear(0, 1, 1, 0.2f);
    
    [Tooltip("Maps tension (0-1) to aggression multiplier")]
    public AnimationCurve tensionToAggression = AnimationCurve.Linear(0, 0.2f, 1, 1f);
    
    [Header("Tension Weights")]
    [Range(0f, 1f)]
    public float heatWeight = 0.6f;
    
    [Range(0f, 1f)]
    public float timeWeight = 0.2f;
    
    [Range(0f, 1f)]
    public float alertWeight = 0.2f;
    
    [Header("Tension Thresholds")]
    [Tooltip("Heat value at which heat contribution reaches 1.0")]
    public float maxHeatForTension = 100f;
    
    [Tooltip("Time in seconds at which time contribution reaches 1.0")]
    public float maxTimeForTension = 300f; // 5 minutes
    
    [Header("Spawn Scaling")]
    [Tooltip("Maximum enemies per spawn wave")]
    public int maxEnemiesPerWave = 12;
    
    [Tooltip("Multiplier for enemy count based on tension")]
    public float tensionEnemyMultiplier = 2f;
    
    [Header("Audio Integration")]
    [Tooltip("AudioMixer parameter name for chase intensity")]
    public string chaseIntensityParameter = "ChaseIntensity";
    
    [Tooltip("AudioMixer parameter name for tension pad volume")]
    public string tensionPadParameter = "TensionPadVolume";

    // Internal state
    private float _lastSpawnTime;
    private MissionController _mc;
    private SpawnManager _spawnManager;
    private bool _isInitialized = false;
    
    // Cache for alert scale metadata key
    private const string ALERT_SCALE_KEY = "alertScale";

    void Awake()
    {
        _mc = FindObjectOfType<MissionController>();
        _spawnManager = FindObjectOfType<SpawnManager>();
        
        if (_mc == null)
        {
            Debug.LogError("AIDirector_Heist: MissionController not found in scene!");
        }
        
        if (_spawnManager == null)
        {
            Debug.LogWarning("AIDirector_Heist: SpawnManager not found, enemy spawning disabled");
        }
    }

    /// <summary>
    /// Initialize AI Director for new mission
    /// </summary>
    public void Initialize()
    {
        tension = 0f;
        _lastSpawnTime = Time.time;
        _isInitialized = true;
        
        Debug.Log($"AIDirector_Heist: Initialized at time {Time.time}");
    }

    void Update()
    {
        if (!_isInitialized || _mc == null || _mc.state == null)
        {
            return;
        }
        
        // Only process if mission is in progress
        if (_mc.state.missionState != MissionStateEnum.InProgress)
        {
            return;
        }
        
        UpdateTension();
        TickSpawns();
        AdjustGlobalAudio();
    }

    /// <summary>
    /// Calculate tension from heat, elapsed time, and triggered alerts
    /// </summary>
    void UpdateTension()
    {
        // Heat component: normalized heat level
        float heatNorm = Mathf.Clamp01(_mc.state.heat / maxHeatForTension);
        
        // Time component: escalates over mission duration
        float timeFactor = Mathf.Clamp01(_mc.state.elapsed / maxTimeForTension);
        
        // Alert component: immediate spikes from triggered alarms
        float alertFactor = 0f;
        if (_mc.state.metadata.ContainsKey(ALERT_SCALE_KEY))
        {
            alertFactor = Mathf.Clamp01((float)_mc.state.metadata[ALERT_SCALE_KEY]);
        }
        
        // Weighted blend (configurable via inspector)
        tension = Mathf.Clamp01(
            heatNorm * heatWeight + 
            timeFactor * timeWeight + 
            alertFactor * alertWeight
        );
        
        // Store tension in metadata for external access
        _mc.state.metadata["tension"] = tension;
    }

    /// <summary>
    /// Spawn enemies based on tension-adjusted intervals
    /// </summary>
    void TickSpawns()
    {
        if (_spawnManager == null)
        {
            return;
        }
        
        // Calculate spawn interval from tension curve
        float spawnInterval = baseSpawnInterval * tensionToSpawnRate.Evaluate(tension);
        
        if (Time.time - _lastSpawnTime >= spawnInterval)
        {
            _lastSpawnTime = Time.time;
            
            // Scale enemy count with tension
            int spawnCount = Mathf.Clamp(
                Mathf.RoundToInt(baseEnemyCount * (1f + tension * tensionEnemyMultiplier)),
                1,
                maxEnemiesPerWave
            );
            
            // Calculate aggression from tension curve
            float aggression = tensionToAggression.Evaluate(tension);
            
            SpawnEnemies(spawnCount, aggression);
        }
    }

    /// <summary>
    /// Request enemy spawns from SpawnManager
    /// </summary>
    void SpawnEnemies(int count, float aggression)
    {
        if (_spawnManager == null)
        {
            Debug.LogWarning("AIDirector_Heist: Cannot spawn enemies, SpawnManager not available");
            return;
        }
        
        _spawnManager.RequestSpawns(count, aggression);
        
        Debug.Log($"AIDirector_Heist: Spawned {count} enemies at aggression {aggression:F2} (tension: {tension:F2})");
    }

    /// <summary>
    /// Adjust audio mixer parameters based on tension
    /// </summary>
    void AdjustGlobalAudio()
    {
        // Map tension to chase intensity (0-1)
        float chaseIntensity = Mathf.Lerp(0f, 1f, tension);
        
        // Map tension to tension pad volume (in dB, -80 to 0)
        float tensionPadVolume = Mathf.Lerp(-80f, 0f, tension);
        
        // Set audio mixer parameters
        AudioMixerHelper.SetParameter(chaseIntensityParameter, chaseIntensity);
        AudioMixerHelper.SetParameter(tensionPadParameter, tensionPadVolume);
    }

    /// <summary>
    /// External interface to spike tension from immediate alerts (e.g., alarm triggered)
    /// </summary>
    public void AddImmediateAlert(float alertAmount)
    {
        if (_mc == null || _mc.state == null)
        {
            return;
        }
        
        // Get current alert scale or default to 0
        float currentAlert = 0f;
        if (_mc.state.metadata.ContainsKey(ALERT_SCALE_KEY))
        {
            currentAlert = (float)_mc.state.metadata[ALERT_SCALE_KEY];
        }
        
        // Add alert and clamp to 0-1
        float newAlert = Mathf.Clamp01(currentAlert + alertAmount);
        _mc.state.metadata[ALERT_SCALE_KEY] = newAlert;
        
        Debug.Log($"AIDirector_Heist: Alert spiked by {alertAmount:F2}, new alert scale: {newAlert:F2}");
    }

    /// <summary>
    /// Decay alert scale over time (call from external systems if desired)
    /// </summary>
    public void DecayAlert(float decayRate)
    {
        if (_mc == null || _mc.state == null || !_mc.state.metadata.ContainsKey(ALERT_SCALE_KEY))
        {
            return;
        }
        
        float currentAlert = (float)_mc.state.metadata[ALERT_SCALE_KEY];
        float newAlert = Mathf.Max(0f, currentAlert - decayRate * Time.deltaTime);
        _mc.state.metadata[ALERT_SCALE_KEY] = newAlert;
    }

    /// <summary>
    /// Get current tension value for external queries
    /// </summary>
    public float GetTension()
    {
        return tension;
    }

    /// <summary>
    /// Get current aggression multiplier for external AI
    /// </summary>
    public float GetAggressionMultiplier()
    {
        return tensionToAggression.Evaluate(tension);
    }

    /// <summary>
    /// Force set tension (for debugging/testing)
    /// </summary>
    public void SetTensionOverride(float overrideTension)
    {
        tension = Mathf.Clamp01(overrideTension);
        Debug.Log($"AIDirector_Heist: Tension override set to {tension:F2}");
    }
}

/// <summary>
/// Static helper for AudioMixer parameter access
/// </summary>
public static class AudioMixerHelper
{
    private static UnityEngine.Audio.AudioMixer _mixer;

    public static void SetMixer(UnityEngine.Audio.AudioMixer mixer)
    {
        _mixer = mixer;
    }

    public static void SetParameter(string parameterName, float value)
    {
        if (_mixer == null)
        {
            // Try to find mixer in resources
            _mixer = Resources.Load<UnityEngine.Audio.AudioMixer>("AudioMixer/MainMixer");
        }

        if (_mixer != null)
        {
            _mixer.SetFloat(parameterName, value);
        }
        else
        {
            Debug.LogWarning($"AudioMixerHelper: AudioMixer not set, cannot update parameter '{parameterName}'");
        }
    }

    public static float GetParameter(string parameterName)
    {
        if (_mixer == null)
        {
            _mixer = Resources.Load<UnityEngine.Audio.AudioMixer>("AudioMixer/MainMixer");
        }

        if (_mixer != null)
        {
            float value;
            if (_mixer.GetFloat(parameterName, out value))
            {
                return value;
            }
        }

        return 0f;
    }
}
