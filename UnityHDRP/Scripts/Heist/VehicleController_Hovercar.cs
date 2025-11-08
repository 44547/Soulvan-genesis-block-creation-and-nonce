using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// VehicleController_Hovercar: Arcade-style hovercar physics for getaway sequences.
/// Features: Hover stabilization, boost mechanics, pursuit evasion, damage handling.
/// Integrates with MissionController for heat tracking and AIDirector for pursuit spawns.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class VehicleController_Hovercar : MonoBehaviour
{
    [Header("Hover Configuration")]
    [Tooltip("Hover height above ground")]
    public float hoverHeight = 2f;

    [Tooltip("Hover force strength")]
    public float hoverForce = 100f;

    [Tooltip("Hover damping")]
    public float hoverDamping = 5f;

    [Tooltip("Raycast layers for ground detection")]
    public LayerMask groundLayer;

    [Header("Movement")]
    [Tooltip("Forward acceleration")]
    public float acceleration = 80f;

    [Tooltip("Maximum speed")]
    public float maxSpeed = 420f;

    [Tooltip("Turn speed in degrees per second")]
    public float turnSpeed = 90f;

    [Tooltip("Braking force")]
    public float brakeForce = 150f;

    [Tooltip("Drag coefficient")]
    public float drag = 0.5f;

    [Header("Boost")]
    [Tooltip("Boost force multiplier")]
    public float boostMultiplier = 2f;

    [Tooltip("Boost duration in seconds")]
    public float boostDuration = 3f;

    [Tooltip("Boost cooldown in seconds")]
    public float boostCooldown = 10f;

    [Tooltip("Driver role boost bonus")]
    public float driverBoostBonus = 1.3f;

    [Header("Damage & Destruction")]
    [Tooltip("Maximum health")]
    public float maxHealth = 100f;

    [Tooltip("Current health")]
    public float currentHealth = 100f;

    [Tooltip("Destroy vehicle on zero health?")]
    public bool destroyOnDeath = true;

    [Header("Audio")]
    public AudioSource engineAudioSource;
    public AudioClip boostSound;
    public AudioClip damageSound;
    public AudioClip explosionSound;

    [Header("Visual Effects")]
    public ParticleSystem boostParticles;
    public ParticleSystem damageParticles;
    public GameObject explosionPrefab;
    public Renderer[] bodyRenderers;

    // Internal state
    private Rigidbody _rb;
    private float _currentSpeed;
    private bool _isBoosting = false;
    private float _boostTimer = 0f;
    private float _boostCooldownTimer = 0f;
    private bool _isDead = false;
    private MissionController _mc;
    private string _driverId;
    private PlayerRole _driverRole;

    // Hover ray positions (corners of vehicle)
    private Vector3[] _hoverRayPositions = new Vector3[4];

    // Events
    public delegate void VehicleEventHandler();
    public event VehicleEventHandler OnBoostActivated;
    public event VehicleEventHandler OnVehicleDestroyed;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false; // Hover system handles gravity
        _rb.drag = drag;

        _mc = FindObjectOfType<MissionController>();
        currentHealth = maxHealth;

        // Setup hover ray positions (relative to vehicle center)
        _hoverRayPositions[0] = new Vector3(-1f, 0f, 1.5f); // Front-left
        _hoverRayPositions[1] = new Vector3(1f, 0f, 1.5f);  // Front-right
        _hoverRayPositions[2] = new Vector3(-1f, 0f, -1.5f); // Rear-left
        _hoverRayPositions[3] = new Vector3(1f, 0f, -1.5f);  // Rear-right
    }

    void Update()
    {
        if (_isDead) return;

        UpdateBoostCooldown();
        UpdateEngineAudio();
    }

    void FixedUpdate()
    {
        if (_isDead) return;

        ApplyHoverForce();
        HandleMovement();
        ClampSpeed();
    }

    /// <summary>
    /// Apply hover force to maintain altitude
    /// </summary>
    void ApplyHoverForce()
    {
        foreach (var localPos in _hoverRayPositions)
        {
            Vector3 worldPos = transform.TransformPoint(localPos);
            RaycastHit hit;

            if (Physics.Raycast(worldPos, -transform.up, out hit, hoverHeight * 2f, groundLayer))
            {
                float compressionRatio = (hoverHeight - hit.distance) / hoverHeight;
                float springForce = compressionRatio * hoverForce;
                float dampingForce = -_rb.GetPointVelocity(worldPos).y * hoverDamping;

                Vector3 totalForce = transform.up * (springForce + dampingForce);
                _rb.AddForceAtPosition(totalForce, worldPos);

                Debug.DrawRay(worldPos, -transform.up * hit.distance, Color.green);
            }
            else
            {
                Debug.DrawRay(worldPos, -transform.up * hoverHeight * 2f, Color.red);
            }
        }
    }

    /// <summary>
    /// Handle player input for movement
    /// </summary>
    void HandleMovement()
    {
        // Get input
        float throttle = Input.GetAxis("Vertical");
        float steering = Input.GetAxis("Horizontal");
        bool brake = Input.GetKey(KeyCode.Space);
        bool boost = Input.GetKey(KeyCode.LeftShift);

        // Apply acceleration
        if (throttle > 0.1f)
        {
            float accelForce = acceleration * throttle;

            // Apply boost multiplier if active
            if (_isBoosting)
            {
                accelForce *= boostMultiplier;
            }

            _rb.AddForce(transform.forward * accelForce);
        }

        // Apply braking
        if (brake)
        {
            _rb.AddForce(-_rb.velocity.normalized * brakeForce);
        }

        // Apply steering (only when moving)
        if (Mathf.Abs(_currentSpeed) > 1f)
        {
            float turnAmount = steering * turnSpeed * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
            _rb.MoveRotation(_rb.rotation * turnRotation);
        }

        // Handle boost activation
        if (boost && !_isBoosting && _boostCooldownTimer <= 0f)
        {
            ActivateBoost();
        }

        // Update current speed
        _currentSpeed = Vector3.Dot(_rb.velocity, transform.forward);
    }

    /// <summary>
    /// Clamp speed to max
    /// </summary>
    void ClampSpeed()
    {
        float speedLimit = maxSpeed;
        if (_isBoosting)
        {
            speedLimit *= boostMultiplier;
        }

        if (_rb.velocity.magnitude > speedLimit)
        {
            _rb.velocity = _rb.velocity.normalized * speedLimit;
        }
    }

    /// <summary>
    /// Activate boost ability
    /// </summary>
    void ActivateBoost()
    {
        _isBoosting = true;
        _boostTimer = boostDuration;
        _boostCooldownTimer = boostCooldown;

        // Apply driver role bonus
        if (_driverRole == PlayerRole.Driver)
        {
            _boostTimer *= driverBoostBonus;
        }

        // Visual effects
        if (boostParticles != null)
        {
            boostParticles.Play();
        }

        // Audio
        if (engineAudioSource != null && boostSound != null)
        {
            engineAudioSource.PlayOneShot(boostSound);
        }

        OnBoostActivated?.Invoke();

        Debug.Log($"VehicleController_Hovercar: Boost activated (duration: {_boostTimer}s)");
    }

    /// <summary>
    /// Update boost cooldown
    /// </summary>
    void UpdateBoostCooldown()
    {
        if (_isBoosting)
        {
            _boostTimer -= Time.deltaTime;
            if (_boostTimer <= 0f)
            {
                _isBoosting = false;

                if (boostParticles != null)
                {
                    boostParticles.Stop();
                }

                Debug.Log("VehicleController_Hovercar: Boost ended");
            }
        }

        if (_boostCooldownTimer > 0f)
        {
            _boostCooldownTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Update engine audio pitch based on speed
    /// </summary>
    void UpdateEngineAudio()
    {
        if (engineAudioSource != null)
        {
            float speedRatio = _currentSpeed / maxSpeed;
            engineAudioSource.pitch = Mathf.Lerp(0.8f, 2f, speedRatio);
            engineAudioSource.volume = Mathf.Lerp(0.5f, 1f, speedRatio);
        }
    }

    /// <summary>
    /// Apply damage to vehicle
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        // Visual feedback
        if (damageParticles != null)
        {
            damageParticles.Play();
        }

        // Audio feedback
        if (engineAudioSource != null && damageSound != null)
        {
            engineAudioSource.PlayOneShot(damageSound);
        }

        // Material flash effect
        StartCoroutine(DamageFlash());

        Debug.Log($"VehicleController_Hovercar: Took {damage} damage, health: {currentHealth}/{maxHealth}");

        // Check for death
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Damage flash effect
    /// </summary>
    IEnumerator DamageFlash()
    {
        foreach (var renderer in bodyRenderers)
        {
            if (renderer != null)
            {
                renderer.material.SetColor("_EmissionColor", Color.red * 2f);
            }
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var renderer in bodyRenderers)
        {
            if (renderer != null)
            {
                renderer.material.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    /// <summary>
    /// Destroy vehicle
    /// </summary>
    void Die()
    {
        _isDead = true;

        // Spawn explosion
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Audio
        if (engineAudioSource != null && explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        OnVehicleDestroyed?.Invoke();

        // Register mission failure if in mission
        if (_mc != null)
        {
            _mc.RegisterModuleActivation("vehicle:destroyed");
        }

        Debug.Log("VehicleController_Hovercar: Vehicle destroyed");

        if (destroyOnDeath)
        {
            Destroy(gameObject, 0.5f);
        }
    }

    /// <summary>
    /// Set driver and apply role bonuses
    /// </summary>
    public void SetDriver(string playerId, PlayerRole role)
    {
        _driverId = playerId;
        _driverRole = role;

        Debug.Log($"VehicleController_Hovercar: Driver set to {playerId} (role: {role})");
    }

    /// <summary>
    /// Get current speed
    /// </summary>
    public float GetSpeed()
    {
        return _currentSpeed;
    }

    /// <summary>
    /// Get boost availability
    /// </summary>
    public bool IsBoostReady()
    {
        return !_isBoosting && _boostCooldownTimer <= 0f;
    }

    /// <summary>
    /// Get boost cooldown ratio (0-1)
    /// </summary>
    public float GetBoostCooldownRatio()
    {
        return 1f - (_boostCooldownTimer / boostCooldown);
    }

    void OnDrawGizmos()
    {
        // Draw hover ray positions
        Gizmos.color = Color.yellow;
        foreach (var localPos in _hoverRayPositions)
        {
            Vector3 worldPos = transform.TransformPoint(localPos);
            Gizmos.DrawWireSphere(worldPos, 0.2f);
        }
    }
}
