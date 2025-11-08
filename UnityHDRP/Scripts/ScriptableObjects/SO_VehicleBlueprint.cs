using UnityEngine;

/// <summary>
/// SO_VehicleBlueprint: ScriptableObject defining vehicle specifications
/// for the Neon Vault Heist system.
/// </summary>
[CreateAssetMenu(fileName = "VehicleBlueprint", menuName = "Soulvan/Vehicle Blueprint", order = 4)]
public class SO_VehicleBlueprint : ScriptableObject
{
    [Header("Blueprint Identification")]
    [Tooltip("Unique blueprint ID (e.g., 'hovercar:prototypeA')")]
    public string blueprintId;

    [Tooltip("Display name for UI")]
    public string displayName;

    [Tooltip("Description")]
    [TextArea(3, 5)]
    public string description;

    [Header("Performance")]
    [Tooltip("Maximum speed (units per second)")]
    public float maxSpeed = 420f;

    [Tooltip("Acceleration rate")]
    public float acceleration = 80f;

    [Tooltip("Handling (0-1, higher = better)")]
    [Range(0f, 1f)]
    public float handling = 0.85f;

    [Tooltip("Braking force")]
    public float brakeForce = 150f;

    [Header("Boost")]
    [Tooltip("Boost multiplier")]
    [Range(1f, 3f)]
    public float boostMultiplier = 2f;

    [Tooltip("Boost duration (seconds)")]
    public float boostDuration = 3f;

    [Tooltip("Boost cooldown (seconds)")]
    public float boostCooldown = 10f;

    [Header("Detection & Signature")]
    [Tooltip("Base signature (detection radius for enemies)")]
    public float signatureBase = 10f;

    [Tooltip("Signature while boosting")]
    public float signatureBoost = 20f;

    [Tooltip("Signature while damaged")]
    public float signatureDamaged = 15f;

    [Header("Durability")]
    [Tooltip("Maximum health")]
    public float maxHealth = 100f;

    [Tooltip("Armor rating (damage reduction %)")]
    [Range(0f, 0.5f)]
    public float armorRating = 0f;

    [Header("Upgrade Slots")]
    [Tooltip("Available upgrade slots")]
    public string[] upgradeSlots = new string[] { "engine", "thruster", "stealth" };

    [Header("Visual & Audio")]
    [Tooltip("Vehicle prefab")]
    public GameObject vehiclePrefab;

    [Tooltip("Engine audio clip")]
    public AudioClip engineSound;

    [Tooltip("Boost audio clip")]
    public AudioClip boostSound;

    /// <summary>
    /// Calculate effective signature based on state
    /// </summary>
    public float GetEffectiveSignature(bool isBoosting, bool isDamaged)
    {
        float signature = signatureBase;

        if (isBoosting)
        {
            signature = Mathf.Max(signature, signatureBoost);
        }

        if (isDamaged)
        {
            signature = Mathf.Max(signature, signatureDamaged);
        }

        return signature;
    }

    /// <summary>
    /// Calculate damage reduction from armor
    /// </summary>
    public float ApplyArmorReduction(float incomingDamage)
    {
        return incomingDamage * (1f - armorRating);
    }
}
