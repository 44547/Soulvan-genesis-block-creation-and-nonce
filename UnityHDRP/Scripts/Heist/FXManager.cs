using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// FXManager: Centralized manager for heist visual effects including rune pulses,
/// ripples, tier bursts, and environmental FX. Integrates with DAO overlay and mission events.
/// </summary>
public class FXManager : MonoBehaviour
{
    public static FXManager Instance { get; private set; }

    [Header("FX Prefabs")]
    public GameObject runePulsePrefab;
    public GameObject ripplePrefab;
    public GameObject tierBurstPrefab;
    public GameObject explosionPrefab;
    public GameObject muzzleFlashPrefab;
    public GameObject impactSparksPrefab;

    [Header("FX Pools")]
    [Tooltip("Enable object pooling for frequently used FX")]
    public bool usePooling = true;

    [Tooltip("Initial pool size per FX type")]
    public int initialPoolSize = 10;

    [Header("Environmental FX")]
    public ParticleSystem ambientDust;
    public ParticleSystem neonGlowParticles;
    public Light[] dynamicLights;

    // Internal pools
    private Dictionary<string, Queue<GameObject>> _fxPools = new Dictionary<string, Queue<GameObject>>();
    private List<GameObject> _activeFX = new List<GameObject>();

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
            return;
        }

        if (usePooling)
        {
            InitializePools();
        }
    }

    /// <summary>
    /// Initialize FX pools
    /// </summary>
    void InitializePools()
    {
        InitializePool("RunePulse", runePulsePrefab);
        InitializePool("Ripple", ripplePrefab);
        InitializePool("TierBurst", tierBurstPrefab);
        InitializePool("Explosion", explosionPrefab);
        InitializePool("MuzzleFlash", muzzleFlashPrefab);
        InitializePool("ImpactSparks", impactSparksPrefab);

        Debug.Log("FXManager: Initialized FX pools");
    }

    /// <summary>
    /// Initialize pool for specific FX type
    /// </summary>
    void InitializePool(string poolName, GameObject prefab)
    {
        if (prefab == null) return;

        Queue<GameObject> pool = new Queue<GameObject>();

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject fx = Instantiate(prefab, transform);
            fx.SetActive(false);
            pool.Enqueue(fx);
        }

        _fxPools[poolName] = pool;
        Debug.Log($"FXManager: Initialized pool '{poolName}' with {initialPoolSize} instances");
    }

    /// <summary>
    /// Spawn FX at position
    /// </summary>
    public GameObject SpawnFX(string fxType, Vector3 position, Quaternion rotation, float duration = 0f)
    {
        GameObject fx = GetFXFromPool(fxType);
        if (fx == null)
        {
            Debug.LogWarning($"FXManager: FX type '{fxType}' not found");
            return null;
        }

        fx.transform.position = position;
        fx.transform.rotation = rotation;
        fx.SetActive(true);

        _activeFX.Add(fx);

        // Auto-destroy or return to pool after duration
        if (duration > 0f)
        {
            StartCoroutine(ReturnToPoolAfterDelay(fx, fxType, duration));
        }

        return fx;
    }

    /// <summary>
    /// Spawn FX with parent
    /// </summary>
    public GameObject SpawnFX(string fxType, Transform parent, Vector3 localPosition, float duration = 0f)
    {
        GameObject fx = GetFXFromPool(fxType);
        if (fx == null)
        {
            Debug.LogWarning($"FXManager: FX type '{fxType}' not found");
            return null;
        }

        fx.transform.SetParent(parent);
        fx.transform.localPosition = localPosition;
        fx.transform.localRotation = Quaternion.identity;
        fx.SetActive(true);

        _activeFX.Add(fx);

        if (duration > 0f)
        {
            StartCoroutine(ReturnToPoolAfterDelay(fx, fxType, duration));
        }

        return fx;
    }

    /// <summary>
    /// Get FX from pool or create new instance
    /// </summary>
    GameObject GetFXFromPool(string fxType)
    {
        // Try to get from pool
        if (usePooling && _fxPools.ContainsKey(fxType) && _fxPools[fxType].Count > 0)
        {
            return _fxPools[fxType].Dequeue();
        }

        // Create new instance
        GameObject prefab = GetPrefabByType(fxType);
        if (prefab != null)
        {
            return Instantiate(prefab, transform);
        }

        return null;
    }

    /// <summary>
    /// Return FX to pool after delay
    /// </summary>
    System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject fx, string fxType, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnFXToPool(fx, fxType);
    }

    /// <summary>
    /// Return FX to pool
    /// </summary>
    public void ReturnFXToPool(GameObject fx, string fxType)
    {
        if (fx == null) return;

        _activeFX.Remove(fx);
        fx.SetActive(false);
        fx.transform.SetParent(transform);

        if (usePooling && _fxPools.ContainsKey(fxType))
        {
            _fxPools[fxType].Enqueue(fx);
        }
        else
        {
            Destroy(fx);
        }
    }

    /// <summary>
    /// Get prefab by FX type
    /// </summary>
    GameObject GetPrefabByType(string fxType)
    {
        switch (fxType)
        {
            case "RunePulse": return runePulsePrefab;
            case "Ripple": return ripplePrefab;
            case "TierBurst": return tierBurstPrefab;
            case "Explosion": return explosionPrefab;
            case "MuzzleFlash": return muzzleFlashPrefab;
            case "ImpactSparks": return impactSparksPrefab;
            default: return null;
        }
    }

    // Convenience methods for common FX

    /// <summary>
    /// Spawn rune pulse FX
    /// </summary>
    public GameObject SpawnRunePulse(Vector3 position, Color color, float scale = 1f)
    {
        GameObject fx = SpawnFX("RunePulse", position, Quaternion.identity, 2f);
        if (fx != null)
        {
            var particleSystem = fx.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                var main = particleSystem.main;
                main.startColor = color;
                main.startSize = scale;
            }
        }
        return fx;
    }

    /// <summary>
    /// Spawn ripple FX
    /// </summary>
    public GameObject SpawnRipple(Vector3 position, Color color, float radius = 5f)
    {
        GameObject fx = SpawnFX("Ripple", position, Quaternion.identity, 3f);
        if (fx != null)
        {
            var particleSystem = fx.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                var main = particleSystem.main;
                main.startColor = color;
                var shape = particleSystem.shape;
                shape.radius = radius;
            }
        }
        return fx;
    }

    /// <summary>
    /// Spawn tier burst FX
    /// </summary>
    public GameObject SpawnTierBurst(Vector3 position, Color color, float intensity = 1f)
    {
        GameObject fx = SpawnFX("TierBurst", position, Quaternion.identity, 5f);
        if (fx != null)
        {
            var particleSystem = fx.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                var main = particleSystem.main;
                main.startColor = color;
                var emission = particleSystem.emission;
                emission.rateOverTime = 50f * intensity;
            }
        }
        return fx;
    }

    /// <summary>
    /// Spawn explosion FX
    /// </summary>
    public GameObject SpawnExplosion(Vector3 position, float scale = 1f)
    {
        GameObject fx = SpawnFX("Explosion", position, Quaternion.identity, 3f);
        if (fx != null)
        {
            fx.transform.localScale = Vector3.one * scale;
        }
        return fx;
    }

    /// <summary>
    /// Spawn muzzle flash
    /// </summary>
    public GameObject SpawnMuzzleFlash(Transform firePoint)
    {
        return SpawnFX("MuzzleFlash", firePoint, Vector3.zero, 0.1f);
    }

    /// <summary>
    /// Spawn impact sparks
    /// </summary>
    public GameObject SpawnImpactSparks(Vector3 position, Vector3 normal)
    {
        Quaternion rotation = Quaternion.LookRotation(normal);
        return SpawnFX("ImpactSparks", position, rotation, 1f);
    }

    /// <summary>
    /// Clear all active FX
    /// </summary>
    public void ClearAllFX()
    {
        foreach (GameObject fx in _activeFX)
        {
            if (fx != null)
            {
                fx.SetActive(false);
            }
        }
        _activeFX.Clear();

        Debug.Log("FXManager: Cleared all active FX");
    }

    /// <summary>
    /// Get active FX count
    /// </summary>
    public int GetActiveFXCount()
    {
        return _activeFX.Count;
    }
}
