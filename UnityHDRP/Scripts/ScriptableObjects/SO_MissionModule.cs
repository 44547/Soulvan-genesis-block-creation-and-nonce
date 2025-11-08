using UnityEngine;

/// <summary>
/// SO_MissionModule: ScriptableObject defining modular mission components
/// for procedural mission generation in the Neon Vault Heist system.
/// </summary>
[CreateAssetMenu(fileName = "MissionModule", menuName = "Soulvan/Mission Module", order = 1)]
public class SO_MissionModule : ScriptableObject
{
    [Header("Module Identification")]
    [Tooltip("Unique module ID (e.g., 'approach:magrail')")]
    public string moduleId;

    [Tooltip("Display name for UI")]
    public string displayName;

    [Tooltip("Description for debug/editor")]
    [TextArea(3, 5)]
    public string description;

    [Header("Procedural Composition")]
    [Tooltip("Weight for random selection (higher = more likely)")]
    [Range(0f, 1f)]
    public float weight = 0.5f;

    [Tooltip("Entry tags required to activate this module")]
    public string[] entryTags;

    [Tooltip("Exit tags provided by this module")]
    public string[] exitTags;

    [Header("Heat Configuration")]
    [Tooltip("Heat modifier applied when module is activated")]
    public float heatModifier = 0f;

    [Tooltip("Heat multiplier (e.g., 1.2 = 20% more heat)")]
    [Range(0.5f, 2f)]
    public float heatMultiplier = 1f;

    [Header("Rewards")]
    [Tooltip("Credit reward bonus for completing this module")]
    public int creditBonus = 0;

    [Tooltip("Experience reward for completing this module")]
    public int experienceReward = 0;

    [Header("Notes")]
    [Tooltip("Design notes for developers")]
    [TextArea(3, 5)]
    public string notes;

    /// <summary>
    /// Check if this module can be activated given current tags
    /// </summary>
    public bool CanActivate(string[] currentTags)
    {
        if (entryTags == null || entryTags.Length == 0)
        {
            return true;
        }

        foreach (string requiredTag in entryTags)
        {
            bool found = false;
            foreach (string currentTag in currentTags)
            {
                if (currentTag == requiredTag)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Get combined heat delta
    /// </summary>
    public float GetHeatDelta(float baseHeat)
    {
        return (baseHeat + heatModifier) * heatMultiplier;
    }
}
