using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// PursuitAI: Enemy AI for pursuit drones and police vehicles during heist chase sequences.
/// Features: NavMesh pathfinding, chase behavior, ramming attacks, tension-based aggression.
/// Integrates with AIDirector_Heist for difficulty scaling.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class PursuitAI : MonoBehaviour
{
    [Header("Chase Configuration")]
    [Tooltip("Chase target (usually player vehicle)")]
    public Transform chaseTarget;

    [Tooltip("Minimum chase distance")]
    public float minChaseDistance = 10f;

    [Tooltip("Maximum chase distance (break off if exceeded)")]
    public float maxChaseDistance = 100f;

    [Tooltip("Ramming distance")]
    public float rammingDistance = 5f;

    [Tooltip("Ramming force")]
    public float rammingForce = 500f;

    [Tooltip("Ramming cooldown")]
    public float rammingCooldown = 5f;

    [Header("AI Behavior")]
    [Tooltip("Base aggression level (0-2)")]
    [Range(0f, 2f)]
    public float baseAggression = 1f;

    [Tooltip("Tension-based aggression multiplier from AIDirector")]
    [Range(0f, 2f)]
    public float tensionAggression = 1f;

    [Tooltip("Update path interval")]
    public float pathUpdateInterval = 0.5f;

    [Tooltip("Prediction time for target intercept")]
    public float predictionTime = 1f;

    [Header("Combat")]
    [Tooltip("Weapon range")]
    public float weaponRange = 30f;

    [Tooltip("Fire rate (shots per second)")]
    public float fireRate = 2f;

    [Tooltip("Weapon damage per shot")]
    public float weaponDamage = 10f;

    [Tooltip("Projectile prefab")]
    public GameObject projectilePrefab;

    [Tooltip("Fire point transform")]
    public Transform firePoint;

    [Header("Health")]
    [Tooltip("Maximum health")]
    public float maxHealth = 50f;

    [Tooltip("Current health")]
    public float currentHealth = 50f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip chaseSound;
    public AudioClip rammingSound;
    public AudioClip weaponSound;

    // Internal state
    private NavMeshAgent _agent;
    private Rigidbody _rb;
    private AIDirector_Heist _aiDirector;
    private float _pathUpdateTimer = 0f;
    private float _rammingCooldownTimer = 0f;
    private float _fireRateTimer = 0f;
    private bool _isRamming = false;
    private bool _isDead = false;

    // Chase states
    private enum ChaseState
    {
        Pursuing,
        Ramming,
        Shooting,
        Retreating
    }

    private ChaseState _currentState = ChaseState.Pursuing;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
        _aiDirector = FindObjectOfType<AIDirector_Heist>();

        currentHealth = maxHealth;

        // Find chase target if not assigned
        if (chaseTarget == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                chaseTarget = player.transform;
            }
        }
    }

    void Update()
    {
        if (_isDead || chaseTarget == null) return;

        // Get tension-based aggression from AI Director
        if (_aiDirector != null)
        {
            tensionAggression = _aiDirector.GetAggressionMultiplier();
        }

        UpdateChaseState();
        UpdateBehavior();
        UpdateTimers();
    }

    /// <summary>
    /// Update chase state based on distance and conditions
    /// </summary>
    void UpdateChaseState()
    {
        float distance = Vector3.Distance(transform.position, chaseTarget.position);

        // Check if target is too far (break off chase)
        if (distance > maxChaseDistance)
        {
            _currentState = ChaseState.Retreating;
            return;
        }

        // Determine state based on distance and aggression
        if (distance <= rammingDistance && _rammingCooldownTimer <= 0f)
        {
            _currentState = ChaseState.Ramming;
        }
        else if (distance <= weaponRange && distance > rammingDistance)
        {
            _currentState = ChaseState.Shooting;
        }
        else
        {
            _currentState = ChaseState.Pursuing;
        }
    }

    /// <summary>
    /// Update AI behavior based on current state
    /// </summary>
    void UpdateBehavior()
    {
        switch (_currentState)
        {
            case ChaseState.Pursuing:
                Pursue();
                break;
            case ChaseState.Ramming:
                Ram();
                break;
            case ChaseState.Shooting:
                Shoot();
                break;
            case ChaseState.Retreating:
                Retreat();
                break;
        }
    }

    /// <summary>
    /// Pursue target with NavMesh pathfinding
    /// </summary>
    void Pursue()
    {
        _pathUpdateTimer += Time.deltaTime;
        if (_pathUpdateTimer >= pathUpdateInterval)
        {
            _pathUpdateTimer = 0f;

            // Predict target position
            Vector3 predictedPosition = PredictTargetPosition();
            _agent.SetDestination(predictedPosition);

            // Adjust speed based on aggression
            _agent.speed = 15f * (baseAggression + tensionAggression);
        }
    }

    /// <summary>
    /// Predict target position based on velocity
    /// </summary>
    Vector3 PredictTargetPosition()
    {
        var targetRb = chaseTarget.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            return chaseTarget.position + targetRb.velocity * predictionTime;
        }
        return chaseTarget.position;
    }

    /// <summary>
    /// Ram target vehicle
    /// </summary>
    void Ram()
    {
        if (_isRamming) return;

        _isRamming = true;
        _rammingCooldownTimer = rammingCooldown;

        // Disable NavMesh and apply ramming force
        _agent.enabled = false;

        if (_rb != null)
        {
            Vector3 rammingDirection = (chaseTarget.position - transform.position).normalized;
            _rb.AddForce(rammingDirection * rammingForce * (baseAggression + tensionAggression), ForceMode.Impulse);
        }

        // Audio
        if (audioSource != null && rammingSound != null)
        {
            audioSource.PlayOneShot(rammingSound);
        }

        Debug.Log($"PursuitAI: Ramming target with force {rammingForce * (baseAggression + tensionAggression)}");

        // Re-enable NavMesh after delay
        Invoke(nameof(ResetRamming), 1f);
    }

    /// <summary>
    /// Reset ramming state
    /// </summary>
    void ResetRamming()
    {
        _isRamming = false;
        if (!_isDead)
        {
            _agent.enabled = true;
        }
    }

    /// <summary>
    /// Shoot at target
    /// </summary>
    void Shoot()
    {
        // Continue moving while shooting
        Pursue();

        // Check fire rate
        _fireRateTimer += Time.deltaTime;
        if (_fireRateTimer >= 1f / fireRate)
        {
            _fireRateTimer = 0f;

            // Face target
            Vector3 lookDirection = chaseTarget.position - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);
            }

            // Fire projectile
            if (projectilePrefab != null && firePoint != null)
            {
                GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                var projectileRb = projectile.GetComponent<Rigidbody>();
                if (projectileRb != null)
                {
                    projectileRb.velocity = firePoint.forward * 50f;
                }

                // Set projectile damage
                var projectileComponent = projectile.GetComponent<Projectile>();
                if (projectileComponent != null)
                {
                    projectileComponent.damage = weaponDamage * (baseAggression + tensionAggression);
                }
            }

            // Audio
            if (audioSource != null && weaponSound != null)
            {
                audioSource.PlayOneShot(weaponSound);
            }
        }
    }

    /// <summary>
    /// Retreat (break off chase)
    /// </summary>
    void Retreat()
    {
        // Move to random NavMesh point away from target
        Vector3 retreatDirection = (transform.position - chaseTarget.position).normalized;
        Vector3 retreatPosition = transform.position + retreatDirection * 20f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(retreatPosition, out hit, 20f, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }

        // Despawn after reaching retreat position
        if (_agent.remainingDistance <= 1f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Update cooldown timers
    /// </summary>
    void UpdateTimers()
    {
        if (_rammingCooldownTimer > 0f)
        {
            _rammingCooldownTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Take damage
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        Debug.Log($"PursuitAI: Took {damage} damage, health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Die and despawn
    /// </summary>
    void Die()
    {
        _isDead = true;
        _agent.enabled = false;

        // TODO: Spawn explosion VFX

        Debug.Log("PursuitAI: Destroyed");

        Destroy(gameObject, 2f);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Apply ramming damage to target
        if (_isRamming && collision.gameObject == chaseTarget.gameObject)
        {
            var vehicleController = collision.gameObject.GetComponent<VehicleController_Hovercar>();
            if (vehicleController != null)
            {
                float rammingDamage = 20f * (baseAggression + tensionAggression);
                vehicleController.TakeDamage(rammingDamage);
                Debug.Log($"PursuitAI: Ramming hit for {rammingDamage} damage");
            }
        }
    }
}

/// <summary>
/// Simple projectile component
/// </summary>
public class Projectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Apply damage to vehicle
        var vehicleController = collision.gameObject.GetComponent<VehicleController_Hovercar>();
        if (vehicleController != null)
        {
            vehicleController.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
