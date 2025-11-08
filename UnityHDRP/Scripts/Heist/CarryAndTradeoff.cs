using UnityEngine;
using System.Collections;

/// <summary>
/// CarryAndTradeoff: Player carrying mechanic for datacores with movement penalties.
/// Features: Pick up datacores, movement/combat restrictions while carrying,
/// tradeoff mechanics between players, drop on damage.
/// </summary>
public class CarryAndTradeoff : MonoBehaviour
{
    [Header("Carry Configuration")]
    [Tooltip("Movement speed multiplier while carrying")]
    [Range(0.1f, 1f)]
    public float carrySpeedMultiplier = 0.7f;

    [Tooltip("Can player sprint while carrying?")]
    public bool canSprintWhileCarrying = false;

    [Tooltip("Can player use weapons while carrying?")]
    public bool canUseWeaponsWhileCarrying = false;

    [Tooltip("Distance for tradeoff interaction")]
    public float tradeoffDistance = 3f;

    [Tooltip("Time to complete tradeoff")]
    public float tradeoffTime = 2f;

    [Tooltip("Drop datacore on taking damage?")]
    public bool dropOnDamage = true;

    [Tooltip("Datacore layer mask")]
    public LayerMask datacoreLayer;

    [Header("Visual Feedback")]
    public GameObject carryVisualPrefab;
    public Transform carryAnchor;
    public Color carryingGlowColor = Color.cyan;

    // Internal state
    private GameObject _carriedDatacore;
    private GameObject _carryVisual;
    private bool _isCarrying = false;
    private bool _isTrading = false;
    private float _originalSpeed;
    private PlayerController _playerController;
    private MissionController _mc;
    private string _playerId;

    // Events
    public delegate void CarryEventHandler(GameObject datacore);
    public event CarryEventHandler OnPickup;
    public event CarryEventHandler OnDrop;
    public event CarryEventHandler OnTradeoffComplete;

    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _mc = FindObjectOfType<MissionController>();
        _playerId = gameObject.name; // Use GameObject name as player ID for now

        if (_playerController != null)
        {
            _originalSpeed = _playerController.moveSpeed;
        }
    }

    void Update()
    {
        if (_isCarrying && !_isTrading)
        {
            UpdateCarryVisual();
            CheckForTradeoffTarget();
        }
    }

    /// <summary>
    /// Attempt to pick up datacore
    /// </summary>
    public bool TryPickup(GameObject datacore)
    {
        if (_isCarrying)
        {
            Debug.LogWarning("CarryAndTradeoff: Already carrying a datacore");
            return false;
        }

        if (datacore == null)
        {
            Debug.LogWarning("CarryAndTradeoff: Datacore is null");
            return false;
        }

        var datacoreComponent = datacore.GetComponent<Datacore>();
        if (datacoreComponent == null)
        {
            Debug.LogWarning("CarryAndTradeoff: Object is not a datacore");
            return false;
        }

        // Pick up datacore
        _carriedDatacore = datacore;
        _isCarrying = true;

        // Disable datacore physics/collider
        var rb = datacore.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        var collider = datacore.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Parent to carry anchor
        if (carryAnchor != null)
        {
            datacore.transform.SetParent(carryAnchor);
            datacore.transform.localPosition = Vector3.zero;
            datacore.transform.localRotation = Quaternion.identity;
        }

        // Apply carry penalties
        ApplyCarryPenalties();

        // Spawn carry visual
        if (carryVisualPrefab != null && carryAnchor != null)
        {
            _carryVisual = Instantiate(carryVisualPrefab, carryAnchor);
        }

        // Register module activation in mission
        if (_mc != null)
        {
            _mc.RegisterModuleActivation("datacore:pickup");
        }

        OnPickup?.Invoke(datacore);

        Debug.Log($"CarryAndTradeoff: {_playerId} picked up datacore {datacore.name}");
        return true;
    }

    /// <summary>
    /// Drop currently carried datacore
    /// </summary>
    public void Drop(bool forced = false)
    {
        if (!_isCarrying || _carriedDatacore == null)
        {
            return;
        }

        // Unparent datacore
        _carriedDatacore.transform.SetParent(null);
        _carriedDatacore.transform.position = transform.position + transform.forward * 1f;

        // Re-enable physics/collider
        var rb = _carriedDatacore.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        var collider = _carriedDatacore.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        // Remove carry penalties
        RemoveCarryPenalties();

        // Destroy carry visual
        if (_carryVisual != null)
        {
            Destroy(_carryVisual);
        }

        OnDrop?.Invoke(_carriedDatacore);

        // Register drop in mission if forced (e.g., by damage)
        if (_mc != null && forced)
        {
            _mc.RegisterModuleActivation("datacore:dropped");
        }

        Debug.Log($"CarryAndTradeoff: {_playerId} dropped datacore {_carriedDatacore.name} (forced: {forced})");

        _carriedDatacore = null;
        _isCarrying = false;
    }

    /// <summary>
    /// Apply movement and combat penalties while carrying
    /// </summary>
    void ApplyCarryPenalties()
    {
        if (_playerController != null)
        {
            _playerController.moveSpeed = _originalSpeed * carrySpeedMultiplier;
            _playerController.canSprint = canSprintWhileCarrying;
            _playerController.canUseWeapons = canUseWeaponsWhileCarrying;
        }
    }

    /// <summary>
    /// Remove carry penalties
    /// </summary>
    void RemoveCarryPenalties()
    {
        if (_playerController != null)
        {
            _playerController.moveSpeed = _originalSpeed;
            _playerController.canSprint = true;
            _playerController.canUseWeapons = true;
        }
    }

    /// <summary>
    /// Update carry visual position and effects
    /// </summary>
    void UpdateCarryVisual()
    {
        if (_carryVisual != null)
        {
            // Add glow pulsing effect
            float pulse = Mathf.Sin(Time.time * 2f) * 0.5f + 0.5f;
            var renderer = _carryVisual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.SetColor("_EmissionColor", carryingGlowColor * pulse);
            }
        }
    }

    /// <summary>
    /// Check for nearby players to tradeoff datacore
    /// </summary>
    void CheckForTradeoffTarget()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Find nearby players
            var nearbyPlayers = Physics.OverlapSphere(transform.position, tradeoffDistance);
            foreach (var collider in nearbyPlayers)
            {
                var otherCarry = collider.GetComponent<CarryAndTradeoff>();
                if (otherCarry != null && otherCarry != this && !otherCarry._isCarrying)
                {
                    StartTradeoff(otherCarry);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Start tradeoff with another player
    /// </summary>
    void StartTradeoff(CarryAndTradeoff targetPlayer)
    {
        if (_isTrading || !_isCarrying)
        {
            return;
        }

        Debug.Log($"CarryAndTradeoff: Starting tradeoff from {_playerId} to {targetPlayer._playerId}");
        StartCoroutine(TradeoffCoroutine(targetPlayer));
    }

    /// <summary>
    /// Tradeoff coroutine with timer
    /// </summary>
    IEnumerator TradeoffCoroutine(CarryAndTradeoff targetPlayer)
    {
        _isTrading = true;
        float elapsed = 0f;

        // TODO: Show tradeoff UI progress bar

        while (elapsed < tradeoffTime)
        {
            // Check if players moved too far apart
            float distance = Vector3.Distance(transform.position, targetPlayer.transform.position);
            if (distance > tradeoffDistance)
            {
                Debug.Log("CarryAndTradeoff: Tradeoff cancelled - players too far apart");
                _isTrading = false;
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Complete tradeoff
        GameObject datacore = _carriedDatacore;
        Drop(false);
        targetPlayer.TryPickup(datacore);

        _isTrading = false;

        OnTradeoffComplete?.Invoke(datacore);

        // Register tradeoff in mission
        if (_mc != null)
        {
            _mc.RegisterModuleActivation("datacore:tradeoff");
        }

        Debug.Log($"CarryAndTradeoff: Tradeoff complete from {_playerId} to {targetPlayer._playerId}");
    }

    /// <summary>
    /// Handle player taking damage
    /// </summary>
    public void OnPlayerDamaged(float damage)
    {
        if (dropOnDamage && _isCarrying)
        {
            Debug.Log($"CarryAndTradeoff: {_playerId} took damage, dropping datacore");
            Drop(true);
        }
    }

    /// <summary>
    /// Get current carrying status
    /// </summary>
    public bool IsCarrying()
    {
        return _isCarrying;
    }

    /// <summary>
    /// Get carried datacore
    /// </summary>
    public GameObject GetCarriedDatacore()
    {
        return _carriedDatacore;
    }

    void OnDrawGizmosSelected()
    {
        // Draw tradeoff range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, tradeoffDistance);
    }
}

/// <summary>
/// Datacore component for pickup objects
/// </summary>
public class Datacore : MonoBehaviour
{
    [Header("Datacore Configuration")]
    public string datacoreId;
    public SO_DatacoreTier tier;
    public int creditReward = 1000;

    [Header("Visual")]
    public Color glowColor = Color.cyan;
    public float glowIntensity = 2f;

    private Renderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        ApplyVisuals();
    }

    void ApplyVisuals()
    {
        if (_renderer != null && tier != null)
        {
            _renderer.material.SetColor("_EmissionColor", tier.uiColor * glowIntensity);
        }
    }

    void Update()
    {
        // Gentle rotation and bob
        transform.Rotate(Vector3.up, 30f * Time.deltaTime);
        float bob = Mathf.Sin(Time.time * 2f) * 0.1f;
        transform.position = new Vector3(transform.position.x, transform.position.y + bob * Time.deltaTime, transform.position.z);
    }
}

/// <summary>
/// Placeholder PlayerController component (referenced by CarryAndTradeoff)
/// </summary>
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool canSprint = true;
    public bool canUseWeapons = true;
}
