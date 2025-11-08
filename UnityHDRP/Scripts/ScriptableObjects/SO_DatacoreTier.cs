using UnityEngine;

/// <summary>
/// SO_DatacoreTier: ScriptableObject defining datacore tiers and rewards
/// for the Neon Vault Heist system.
/// </summary>
[CreateAssetMenu(fileName = "DatacoreTier", menuName = "Soulvan/Datacore Tier", order = 2)]
public class SO_DatacoreTier : ScriptableObject
{
    [Header("Tier Identification")]
    [Tooltip("Unique tier ID (e.g., 'datacore:mythic')")]
    public string tierId;

    [Tooltip("Display name for UI")]
    public string displayName;

    [Header("Rarity")]
    [Tooltip("Rarity score (lower = more rare)")]
    [Range(1, 100)]
    public int rarityScore = 10;

    [Tooltip("Drop chance modifier")]
    [Range(0.01f, 1f)]
    public float dropChance = 0.1f;

    [Header("Visual")]
    [Tooltip("UI color for this tier")]
    public Color uiColor = Color.cyan;

    [Tooltip("Glow intensity multiplier")]
    [Range(1f, 5f)]
    public float glowIntensity = 2f;

    [Tooltip("Particle effect prefab override")]
    public GameObject particleEffectPrefab;

    [Header("Rewards")]
    [Tooltip("Credit reward for extracting this datacore")]
    public int creditReward = 1000;

    [Tooltip("Experience reward")]
    public int experienceReward = 100;

    [Tooltip("DAO influence bonus")]
    public float daoInfluenceBonus = 5f;

    [Header("Requirements")]
    [Tooltip("Minimum performance score required (0-1)")]
    [Range(0f, 1f)]
    public float minPerformanceScore = 0f;

    [Tooltip("Required role (empty = any role)")]
    public string requiredRole = "";

    /// <summary>
    /// Check if player qualifies for this tier
    /// </summary>
    public bool QualifiesFor(float performanceScore, string playerRole)
    {
        if (performanceScore < minPerformanceScore)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(requiredRole) && playerRole != requiredRole)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Get total reward value
    /// </summary>
    public int GetTotalRewardValue()
    {
        return creditReward + (experienceReward * 10);
    }
}
