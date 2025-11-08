using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// RoleManager: Manages player role assignments for co-op heist missions.
/// Supports 4 roles: Driver, Infiltrator, Systems, Support with role-specific abilities.
/// Integrates with MissionController for role-based objectives and rewards.
/// </summary>
public class RoleManager : MonoBehaviour
{
    [Header("Role Configuration")]
    [Tooltip("Maximum players per mission")]
    public int maxPlayers = 4;

    [Header("Role Bonuses")]
    [Tooltip("Credit reward multiplier per role")]
    public RoleBonusConfig[] roleBonuses = new RoleBonusConfig[]
    {
        new RoleBonusConfig { role = PlayerRole.Driver, creditMultiplier = 1.2f, specialAbility = "Vehicle Boost" },
        new RoleBonusConfig { role = PlayerRole.Infiltrator, creditMultiplier = 1.1f, specialAbility = "Stealth Cloak" },
        new RoleBonusConfig { role = PlayerRole.Systems, creditMultiplier = 1.15f, specialAbility = "Hack Speed" },
        new RoleBonusConfig { role = PlayerRole.Support, creditMultiplier = 1.0f, specialAbility = "Team Shield" }
    };

    // Internal state
    private Dictionary<string, PlayerRole> _roleAssignments = new Dictionary<string, PlayerRole>();
    private Dictionary<PlayerRole, string> _roleToPlayer = new Dictionary<PlayerRole, string>();
    private HashSet<PlayerRole> _availableRoles = new HashSet<PlayerRole>();
    private MissionController _mc;

    void Awake()
    {
        _mc = FindObjectOfType<MissionController>();
        InitializeAvailableRoles();
    }

    /// <summary>
    /// Initialize available roles (all roles available at start)
    /// </summary>
    void InitializeAvailableRoles()
    {
        _availableRoles.Clear();
        _availableRoles.Add(PlayerRole.Driver);
        _availableRoles.Add(PlayerRole.Infiltrator);
        _availableRoles.Add(PlayerRole.Systems);
        _availableRoles.Add(PlayerRole.Support);
    }

    /// <summary>
    /// Reset role assignments for new mission
    /// </summary>
    public void ResetRoles()
    {
        _roleAssignments.Clear();
        _roleToPlayer.Clear();
        InitializeAvailableRoles();
        Debug.Log("RoleManager: Roles reset for new mission");
    }

    /// <summary>
    /// Assign role to player
    /// </summary>
    public bool AssignRole(string playerId, PlayerRole role)
    {
        // Check if role is available
        if (!_availableRoles.Contains(role))
        {
            Debug.LogWarning($"RoleManager: Role {role} is not available");
            return false;
        }

        // Check if player already has a role
        if (_roleAssignments.ContainsKey(playerId))
        {
            Debug.LogWarning($"RoleManager: Player {playerId} already has role {_roleAssignments[playerId]}");
            return false;
        }

        // Check max players
        if (_roleAssignments.Count >= maxPlayers)
        {
            Debug.LogWarning($"RoleManager: Maximum players ({maxPlayers}) reached");
            return false;
        }

        // Assign role
        _roleAssignments[playerId] = role;
        _roleToPlayer[role] = playerId;
        _availableRoles.Remove(role);

        Debug.Log($"RoleManager: Assigned {role} to player {playerId}");

        // Notify mission controller
        if (_mc != null)
        {
            _mc.AssignRole(playerId, role);
        }

        return true;
    }

    /// <summary>
    /// Remove player and free their role
    /// </summary>
    public void RemovePlayer(string playerId)
    {
        if (_roleAssignments.ContainsKey(playerId))
        {
            PlayerRole role = _roleAssignments[playerId];
            _roleAssignments.Remove(playerId);
            _roleToPlayer.Remove(role);
            _availableRoles.Add(role);

            Debug.Log($"RoleManager: Removed player {playerId} (role {role} now available)");
        }
    }

    /// <summary>
    /// Get player's assigned role
    /// </summary>
    public PlayerRole GetPlayerRole(string playerId)
    {
        return _roleAssignments.ContainsKey(playerId) ? _roleAssignments[playerId] : PlayerRole.None;
    }

    /// <summary>
    /// Get player assigned to role
    /// </summary>
    public string GetRolePlayer(PlayerRole role)
    {
        return _roleToPlayer.ContainsKey(role) ? _roleToPlayer[role] : null;
    }

    /// <summary>
    /// Check if role is available
    /// </summary>
    public bool IsRoleAvailable(PlayerRole role)
    {
        return _availableRoles.Contains(role);
    }

    /// <summary>
    /// Get all available roles
    /// </summary>
    public List<PlayerRole> GetAvailableRoles()
    {
        return new List<PlayerRole>(_availableRoles);
    }

    /// <summary>
    /// Get all assigned roles
    /// </summary>
    public Dictionary<string, PlayerRole> GetAllAssignments()
    {
        return new Dictionary<string, PlayerRole>(_roleAssignments);
    }

    /// <summary>
    /// Get role bonus configuration
    /// </summary>
    public RoleBonusConfig GetRoleBonus(PlayerRole role)
    {
        foreach (var bonus in roleBonuses)
        {
            if (bonus.role == role)
            {
                return bonus;
            }
        }
        return new RoleBonusConfig { role = role, creditMultiplier = 1.0f };
    }

    /// <summary>
    /// Calculate credit reward with role bonus
    /// </summary>
    public int CalculateCreditReward(string playerId, int baseCredits)
    {
        PlayerRole role = GetPlayerRole(playerId);
        if (role == PlayerRole.None)
        {
            return baseCredits;
        }

        RoleBonusConfig bonus = GetRoleBonus(role);
        return Mathf.RoundToInt(baseCredits * bonus.creditMultiplier);
    }

    /// <summary>
    /// Check if all required roles are filled (for mission start validation)
    /// </summary>
    public bool AreMinimumRolesFilled(int minimumPlayers = 1)
    {
        return _roleAssignments.Count >= minimumPlayers;
    }

    /// <summary>
    /// Get role description for UI
    /// </summary>
    public string GetRoleDescription(PlayerRole role)
    {
        switch (role)
        {
            case PlayerRole.Driver:
                return "Vehicle specialist. Handles getaway and pursuit evasion. Bonus: Vehicle Boost.";
            case PlayerRole.Infiltrator:
                return "Stealth expert. Bypasses security and minimizes heat. Bonus: Stealth Cloak.";
            case PlayerRole.Systems:
                return "Hacking specialist. Disables security and opens vaults. Bonus: Hack Speed.";
            case PlayerRole.Support:
                return "Team support. Provides buffs and backup. Bonus: Team Shield.";
            default:
                return "No role assigned.";
        }
    }

    /// <summary>
    /// Trigger role-specific ability
    /// </summary>
    public void ActivateRoleAbility(string playerId)
    {
        PlayerRole role = GetPlayerRole(playerId);
        if (role == PlayerRole.None)
        {
            Debug.LogWarning($"RoleManager: Player {playerId} has no role assigned");
            return;
        }

        Debug.Log($"RoleManager: Activating {role} ability for player {playerId}");

        switch (role)
        {
            case PlayerRole.Driver:
                ActivateDriverBoost(playerId);
                break;
            case PlayerRole.Infiltrator:
                ActivateInfiltratorCloak(playerId);
                break;
            case PlayerRole.Systems:
                ActivateSystemsHack(playerId);
                break;
            case PlayerRole.Support:
                ActivateSupportShield(playerId);
                break;
        }
    }

    void ActivateDriverBoost(string playerId)
    {
        // Placeholder: implement vehicle boost mechanics
        Debug.Log($"RoleManager: Driver {playerId} activated Vehicle Boost");
        // TODO: Apply speed multiplier to player's vehicle
    }

    void ActivateInfiltratorCloak(string playerId)
    {
        // Placeholder: implement stealth cloak mechanics
        Debug.Log($"RoleManager: Infiltrator {playerId} activated Stealth Cloak");
        // TODO: Reduce player detection radius temporarily
    }

    void ActivateSystemsHack(string playerId)
    {
        // Placeholder: implement hack speed mechanics
        Debug.Log($"RoleManager: Systems {playerId} activated Hack Speed");
        // TODO: Increase hack progress rate temporarily
    }

    void ActivateSupportShield(string playerId)
    {
        // Placeholder: implement team shield mechanics
        Debug.Log($"RoleManager: Support {playerId} activated Team Shield");
        // TODO: Apply damage reduction buff to nearby teammates
    }
}

/// <summary>
/// Role bonus configuration
/// </summary>
[System.Serializable]
public class RoleBonusConfig
{
    public PlayerRole role;
    [Range(0.5f, 2.0f)]
    public float creditMultiplier = 1.0f;
    public string specialAbility;
}
