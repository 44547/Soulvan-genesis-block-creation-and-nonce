using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// SpawnManager: Handles enemy spawns for heist missions with NavMesh placement.
/// Works with AIDirector_Heist for tension-based spawn scaling.
/// Manages spawn points, enemy pooling, and aggression settings.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Configuration")]
    [Tooltip("Enemy prefab to spawn")]
    public GameObject enemyPrefab;

    [Tooltip("Spawn points in the scene")]
    public Transform[] spawnPoints;

    [Tooltip("Minimum distance from player to spawn")]
    public float minSpawnDistanceFromPlayer = 30f;

    [Tooltip("Maximum active enemies at once")]
    public int maxActiveEnemies = 20;

    [Header("Enemy Pool")]
    [Tooltip("Enable object pooling for enemies")]
    public bool usePooling = true;

    [Tooltip("Initial pool size")]
    public int initialPoolSize = 10;

    [Header("Debug")]
    public bool showSpawnGizmos = true;
    public Color spawnGizmoColor = Color.red;

    // Internal state
    private List<GameObject> _activeEnemies = new List<GameObject>();
    private Queue<GameObject> _enemyPool = new Queue<GameObject>();
    private Transform _playerTransform;
    private int _totalSpawned = 0;

    void Awake()
    {
        // Find player transform
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }

        // Initialize pool
        if (usePooling)
        {
            InitializePool();
        }

        // Validate spawn points
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("SpawnManager: No spawn points assigned, attempting to find spawn points in scene");
            FindSpawnPoints();
        }
    }

    /// <summary>
    /// Initialize enemy pool
    /// </summary>
    void InitializePool()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("SpawnManager: Enemy prefab not assigned!");
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, transform);
            enemy.SetActive(false);
            _enemyPool.Enqueue(enemy);
        }

        Debug.Log($"SpawnManager: Initialized pool with {initialPoolSize} enemies");
    }

    /// <summary>
    /// Find spawn points in scene (fallback)
    /// </summary>
    void FindSpawnPoints()
    {
        var spawnPointObjects = GameObject.FindGameObjectsWithTag("EnemySpawnPoint");
        if (spawnPointObjects.Length > 0)
        {
            spawnPoints = new Transform[spawnPointObjects.Length];
            for (int i = 0; i < spawnPointObjects.Length; i++)
            {
                spawnPoints[i] = spawnPointObjects[i].transform;
            }
            Debug.Log($"SpawnManager: Found {spawnPoints.Length} spawn points in scene");
        }
        else
        {
            Debug.LogError("SpawnManager: No spawn points found in scene!");
        }
    }

    /// <summary>
    /// Request enemy spawns (called by AIDirector_Heist)
    /// </summary>
    /// <param name="count">Number of enemies to spawn</param>
    /// <param name="aggression">Aggression multiplier (0-1)</param>
    public void RequestSpawns(int count, float aggression)
    {
        // Check if we can spawn more enemies
        if (_activeEnemies.Count >= maxActiveEnemies)
        {
            Debug.LogWarning("SpawnManager: Max active enemies reached, skipping spawn request");
            return;
        }

        // Limit spawn count to available capacity
        int actualCount = Mathf.Min(count, maxActiveEnemies - _activeEnemies.Count);

        for (int i = 0; i < actualCount; i++)
        {
            SpawnEnemy(aggression);
        }
    }

    /// <summary>
    /// Spawn single enemy at valid spawn point
    /// </summary>
    void SpawnEnemy(float aggression)
    {
        // Find valid spawn point
        Transform spawnPoint = GetValidSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogWarning("SpawnManager: No valid spawn point found");
            return;
        }

        // Get or create enemy
        GameObject enemy = GetEnemyFromPool();
        if (enemy == null)
        {
            Debug.LogWarning("SpawnManager: Failed to get enemy from pool");
            return;
        }

        // Position and activate enemy
        enemy.transform.position = spawnPoint.position;
        enemy.transform.rotation = spawnPoint.rotation;
        enemy.SetActive(true);

        // Configure enemy aggression
        var enemyAI = enemy.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.SetAggression(aggression);
        }

        // Track active enemy
        _activeEnemies.Add(enemy);
        _totalSpawned++;

        Debug.Log($"SpawnManager: Spawned enemy #{_totalSpawned} at {spawnPoint.position} (aggression: {aggression:F2})");
    }

    /// <summary>
    /// Get enemy from pool or create new one
    /// </summary>
    GameObject GetEnemyFromPool()
    {
        if (usePooling && _enemyPool.Count > 0)
        {
            return _enemyPool.Dequeue();
        }
        else if (enemyPrefab != null)
        {
            return Instantiate(enemyPrefab, transform);
        }
        return null;
    }

    /// <summary>
    /// Return enemy to pool
    /// </summary>
    public void ReturnEnemyToPool(GameObject enemy)
    {
        if (enemy == null) return;

        _activeEnemies.Remove(enemy);
        enemy.SetActive(false);

        if (usePooling)
        {
            _enemyPool.Enqueue(enemy);
        }
        else
        {
            Destroy(enemy);
        }
    }

    /// <summary>
    /// Find valid spawn point (far from player, with NavMesh)
    /// </summary>
    Transform GetValidSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return null;
        }

        // Shuffle spawn points to avoid predictable spawns
        List<Transform> shuffledPoints = new List<Transform>(spawnPoints);
        for (int i = 0; i < shuffledPoints.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledPoints.Count);
            Transform temp = shuffledPoints[i];
            shuffledPoints[i] = shuffledPoints[randomIndex];
            shuffledPoints[randomIndex] = temp;
        }

        // Find first valid spawn point
        foreach (Transform point in shuffledPoints)
        {
            if (point == null) continue;

            // Check distance from player
            if (_playerTransform != null)
            {
                float distance = Vector3.Distance(point.position, _playerTransform.position);
                if (distance < minSpawnDistanceFromPlayer)
                {
                    continue;
                }
            }

            // Check if point is on NavMesh
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(point.position, out hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
            {
                return point;
            }
        }

        // No valid spawn point found, return first point as fallback
        return shuffledPoints[0];
    }

    /// <summary>
    /// Get count of active enemies
    /// </summary>
    public int GetActiveEnemyCount()
    {
        return _activeEnemies.Count;
    }

    /// <summary>
    /// Get total spawned count
    /// </summary>
    public int GetTotalSpawnedCount()
    {
        return _totalSpawned;
    }

    /// <summary>
    /// Clear all active enemies
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in _activeEnemies)
        {
            if (enemy != null)
            {
                if (usePooling)
                {
                    enemy.SetActive(false);
                    _enemyPool.Enqueue(enemy);
                }
                else
                {
                    Destroy(enemy);
                }
            }
        }
        _activeEnemies.Clear();
        Debug.Log("SpawnManager: Cleared all active enemies");
    }

    void OnDrawGizmos()
    {
        if (!showSpawnGizmos || spawnPoints == null) return;

        Gizmos.color = spawnGizmoColor;
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 1f);
                Gizmos.DrawLine(point.position, point.position + point.forward * 2f);
            }
        }
    }
}

/// <summary>
/// Placeholder EnemyAI component (referenced by SpawnManager)
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Range(0f, 2f)]
    public float aggression = 1f;

    public void SetAggression(float value)
    {
        aggression = Mathf.Clamp(value, 0f, 2f);
        // TODO: Apply aggression to AI behavior (attack speed, accuracy, etc.)
    }
}
