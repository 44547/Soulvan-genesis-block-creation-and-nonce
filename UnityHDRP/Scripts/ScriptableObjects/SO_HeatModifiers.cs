using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// SO_HeatModifiers: ScriptableObject defining heat modifiers for various actions
/// in the Neon Vault Heist system.
/// </summary>
[CreateAssetMenu(fileName = "HeatModifiers", menuName = "Soulvan/Heat Modifiers", order = 3)]
public class SO_HeatModifiers : ScriptableObject
{
    [System.Serializable]
    public class HeatModifier
    {
        [Tooltip("Action identifier")]
        public string action;

        [Tooltip("Heat delta (positive = increase, negative = decrease)")]
        public float heatDelta;

        [Tooltip("Description for UI/debug")]
        public string description;
    }

    [Header("Heat Modifiers")]
    public List<HeatModifier> modifiers = new List<HeatModifier>();

    /// <summary>
    /// Get heat delta for action
    /// </summary>
    public float GetHeatDelta(string action)
    {
        foreach (var modifier in modifiers)
        {
            if (modifier.action == action)
            {
                return modifier.heatDelta;
            }
        }

        Debug.LogWarning($"SO_HeatModifiers: No heat modifier found for action '{action}'");
        return 0f;
    }

    /// <summary>
    /// Check if action exists
    /// </summary>
    public bool HasAction(string action)
    {
        foreach (var modifier in modifiers)
        {
            if (modifier.action == action)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Get all actions with positive heat (dangerous actions)
    /// </summary>
    public List<string> GetDangerousActions()
    {
        List<string> actions = new List<string>();
        foreach (var modifier in modifiers)
        {
            if (modifier.heatDelta > 0)
            {
                actions.Add(modifier.action);
            }
        }
        return actions;
    }

    /// <summary>
    /// Get all actions with negative heat (cooling actions)
    /// </summary>
    public List<string> GetCoolingActions()
    {
        List<string> actions = new List<string>();
        foreach (var modifier in modifiers)
        {
            if (modifier.heatDelta < 0)
            {
                actions.Add(modifier.action);
            }
        }
        return actions;
    }
}
