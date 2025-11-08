using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soulvan.Systems
{
    /// <summary>
    /// Contributor progression system with 4-tier ladder.
    /// Features XP tracking, unlockable perks, DAO voting power, and role upgrades.
    /// Cinematic upgrades with Soulvan's approval stamp.
    /// </summary>
    public class ArchitectKit : MonoBehaviour
    {
        [Header("Contributor Data")]
        public string contributorId;
        public ContributorRole currentRole = ContributorRole.Initiate;
        public int currentXP = 0;
        public int contributorLevel = 1;

        [Header("Progression Ladder")]
        public int initiateXPRequired = 100;
        public int builderXPRequired = 500;
        public int architectXPRequired = 2000;
        public int oracleXPRequired = 10000;

        [Header("UI Elements")]
        public TextMeshProUGUI roleTitle;
        public TextMeshProUGUI xpText;
        public Image xpBar;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI daoPowerText;
        public GameObject upgradeNotification;

        [Header("Progression Visual")]
        public Image[] roleLadderIcons; // 4 icons: Initiate, Builder, Architect, Oracle
        public Color activeRoleColor = Color.cyan;
        public Color inactiveRoleColor = Color.gray;

        [Header("Perks")]
        public List<ContributorPerk> unlockedPerks = new List<ContributorPerk>();
        public TextMeshProUGUI perksListText;

        [Header("Lore Integration")]
        public GameObject loreScrollPrefab;
        public Transform loreScrollContainer;
        public List<LoreEntry> contributorLore = new List<LoreEntry>();

        [Header("FX")]
        public ParticleSystem upgradeXPFX;
        public ParticleSystem roleLadderFX;
        public AudioClip upgradeSound;
        public AudioClip xpGainSound;

        [Header("Wallet Integration")]
        public string contributorWallet;
        public WalletBridge walletBridge;

        private int currentDAOPower = 1;

        private void Start()
        {
            InitializeContributor();
            UpdateUI();
        }

        /// <summary>
        /// Initialize contributor with default values.
        /// </summary>
        private void InitializeContributor()
        {
            if (string.IsNullOrEmpty(contributorId))
            {
                contributorId = $"CONTRIBUTOR_{Random.Range(10000, 99999)}";
            }

            // Load from blockchain in production
            currentRole = ContributorRole.Initiate;
            currentXP = 0;
            contributorLevel = 1;
            currentDAOPower = 1;

            Debug.Log($"[ArchitectKit] Initialized contributor: {contributorId}");

            // Record to lore
            SoulvanLore.Record($"Contributor {contributorId} joined as Initiate");
        }

        /// <summary>
        /// Grant XP to contributor.
        /// </summary>
        public void GrantXP(int amount, string reason)
        {
            currentXP += amount;

            Debug.Log($"[ArchitectKit] +{amount} XP: {reason} (Total: {currentXP})");

            // Play XP gain FX
            if (xpGainSound != null)
            {
                AudioSource.PlayClipAtPoint(xpGainSound, transform.position);
            }

            if (upgradeXPFX != null)
            {
                upgradeXPFX.Play();
            }

            // Check for role upgrade
            CheckRoleUpgrade();

            // Update UI
            UpdateUI();

            // Record to lore
            AddLoreEntry($"Earned {amount} XP: {reason}", "XP_GAIN");
        }

        /// <summary>
        /// Check if contributor qualifies for role upgrade.
        /// </summary>
        private void CheckRoleUpgrade()
        {
            ContributorRole newRole = currentRole;

            if (currentXP >= oracleXPRequired && currentRole != ContributorRole.Oracle)
            {
                newRole = ContributorRole.Oracle;
            }
            else if (currentXP >= architectXPRequired && currentRole != ContributorRole.Architect)
            {
                newRole = ContributorRole.Architect;
            }
            else if (currentXP >= builderXPRequired && currentRole != ContributorRole.Builder)
            {
                newRole = ContributorRole.Builder;
            }
            else if (currentXP >= initiateXPRequired && currentRole == ContributorRole.Initiate)
            {
                newRole = ContributorRole.Builder;
            }

            if (newRole != currentRole)
            {
                UpgradeRole(newRole);
            }
        }

        /// <summary>
        /// Upgrade contributor role with cinematic sequence.
        /// </summary>
        private void UpgradeRole(ContributorRole newRole)
        {
            ContributorRole oldRole = currentRole;
            currentRole = newRole;

            Debug.Log($"[ArchitectKit] 🎖️ Role upgraded: {oldRole} → {newRole}");

            // Update DAO power
            currentDAOPower = GetDAOPower(newRole);

            // Unlock perks
            UnlockRolePerks(newRole);

            // Play upgrade FX
            if (upgradeSound != null)
            {
                AudioSource.PlayClipAtPoint(upgradeSound, transform.position);
            }

            if (roleLadderFX != null)
            {
                roleLadderFX.Play();
            }

            // Show notification
            if (upgradeNotification != null)
            {
                upgradeNotification.SetActive(true);
                StartCoroutine(HideNotificationAfterDelay(3f));
            }

            // Record to lore
            SoulvanLore.Record($"Contributor {contributorId} upgraded: {oldRole} → {newRole}");
            AddLoreEntry($"Role upgraded to {newRole}", "ROLE_UPGRADE");

            // Trigger cinematic cutscene
            ArchitectCutsceneTrigger cutscene = GetComponent<ArchitectCutsceneTrigger>();
            if (cutscene != null)
            {
                cutscene.TriggerCutscene(newRole);
            }

            // Update UI
            UpdateUI();
        }

        /// <summary>
        /// Unlock perks for role.
        /// </summary>
        private void UnlockRolePerks(ContributorRole role)
        {
            switch (role)
            {
                case ContributorRole.Builder:
                    unlockedPerks.Add(new ContributorPerk
                    {
                        name = "Badge Minting",
                        description = "Mint contributor badges as NFTs",
                        daoBonus = 0
                    });
                    break;

                case ContributorRole.Architect:
                    unlockedPerks.Add(new ContributorPerk
                    {
                        name = "Lore Export",
                        description = "Export mission lore as saga scrolls",
                        daoBonus = 5
                    });
                    unlockedPerks.Add(new ContributorPerk
                    {
                        name = "DAO Proposal",
                        description = "Submit proposals to Soulvan DAO",
                        daoBonus = 10
                    });
                    break;

                case ContributorRole.Oracle:
                    unlockedPerks.Add(new ContributorPerk
                    {
                        name = "Artifact Minting",
                        description = "Mint legendary saga artifacts",
                        daoBonus = 20
                    });
                    unlockedPerks.Add(new ContributorPerk
                    {
                        name = "Vote Multiplier",
                        description = "2x voting power in DAO",
                        daoBonus = 50
                    });
                    break;
            }

            Debug.Log($"[ArchitectKit] Unlocked {unlockedPerks.Count} perks");
        }

        /// <summary>
        /// Get DAO voting power for role.
        /// </summary>
        private int GetDAOPower(ContributorRole role)
        {
            switch (role)
            {
                case ContributorRole.Initiate: return 1;
                case ContributorRole.Builder: return 5;
                case ContributorRole.Architect: return 25;
                case ContributorRole.Oracle: return 100;
                default: return 1;
            }
        }

        /// <summary>
        /// Add lore entry to contributor timeline.
        /// </summary>
        public void AddLoreEntry(string content, string category)
        {
            LoreEntry entry = new LoreEntry
            {
                timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                content = content,
                category = category,
                contributorId = contributorId
            };

            contributorLore.Add(entry);

            Debug.Log($"[ArchitectKit] Lore entry added: {content}");

            // Spawn visual scroll if prefab exists
            if (loreScrollPrefab != null && loreScrollContainer != null)
            {
                GameObject scroll = Instantiate(loreScrollPrefab, loreScrollContainer);
                LoreScroll scrollComponent = scroll.GetComponent<LoreScroll>();
                if (scrollComponent != null)
                {
                    scrollComponent.SetContent(entry);
                }
            }
        }

        /// <summary>
        /// Export contributor lore as saga scroll NFT.
        /// </summary>
        public void ExportSagaScroll()
        {
            if (currentRole < ContributorRole.Architect)
            {
                Debug.Log("[ArchitectKit] ❌ Architect role required for lore export");
                return;
            }

            Debug.Log("[ArchitectKit] 📜 Exporting saga scroll...");

            // Export lore to blockchain
            if (walletBridge != null)
            {
                walletBridge.MintSagaScrollNFT(contributorId, contributorLore);
            }

            // Record to lore
            SoulvanLore.Record($"Contributor {contributorId} exported saga scroll with {contributorLore.Count} entries");

            // Trigger export cutscene
            ExportCutsceneTrigger exportCutscene = FindObjectOfType<ExportCutsceneTrigger>();
            if (exportCutscene != null)
            {
                exportCutscene.TriggerSagaScrollExport(contributorLore);
            }
        }

        /// <summary>
        /// Export contributor badges.
        /// </summary>
        public void ExportBadges()
        {
            if (currentRole < ContributorRole.Builder)
            {
                Debug.Log("[ArchitectKit] ❌ Builder role required for badge export");
                return;
            }

            Debug.Log("[ArchitectKit] 🎖️ Exporting badges...");

            // Mint badge NFT
            SoulvanLore.MintBadge(contributorId, currentRole.ToString());

            // Trigger export cutscene
            ExportCutsceneTrigger exportCutscene = FindObjectOfType<ExportCutsceneTrigger>();
            if (exportCutscene != null)
            {
                exportCutscene.TriggerBadgeExport(contributorId, currentRole);
            }
        }

        /// <summary>
        /// Update UI elements.
        /// </summary>
        private void UpdateUI()
        {
            // Role title
            if (roleTitle != null)
            {
                roleTitle.text = currentRole.ToString().ToUpper();
            }

            // XP bar
            int nextRoleXP = GetNextRoleXP();
            if (xpText != null)
            {
                xpText.text = $"{currentXP} / {nextRoleXP} XP";
            }
            if (xpBar != null)
            {
                xpBar.fillAmount = (float)currentXP / nextRoleXP;
            }

            // Level
            if (levelText != null)
            {
                levelText.text = $"Level {contributorLevel}";
            }

            // DAO power
            if (daoPowerText != null)
            {
                daoPowerText.text = $"DAO Power: {currentDAOPower}";
            }

            // Role ladder visual
            UpdateRoleLadderVisual();

            // Perks list
            UpdatePerksDisplay();
        }

        /// <summary>
        /// Get XP required for next role.
        /// </summary>
        private int GetNextRoleXP()
        {
            switch (currentRole)
            {
                case ContributorRole.Initiate: return builderXPRequired;
                case ContributorRole.Builder: return architectXPRequired;
                case ContributorRole.Architect: return oracleXPRequired;
                case ContributorRole.Oracle: return oracleXPRequired;
                default: return 100;
            }
        }

        /// <summary>
        /// Update role ladder visual.
        /// </summary>
        private void UpdateRoleLadderVisual()
        {
            if (roleLadderIcons.Length != 4) return;

            for (int i = 0; i < roleLadderIcons.Length; i++)
            {
                if (i <= (int)currentRole)
                {
                    roleLadderIcons[i].color = activeRoleColor;
                }
                else
                {
                    roleLadderIcons[i].color = inactiveRoleColor;
                }
            }
        }

        /// <summary>
        /// Update perks display.
        /// </summary>
        private void UpdatePerksDisplay()
        {
            if (perksListText == null) return;

            string perksText = "<b>Unlocked Perks:</b>\n";
            foreach (var perk in unlockedPerks)
            {
                perksText += $"• {perk.name}: {perk.description}\n";
            }

            perksListText.text = perksText;
        }

        /// <summary>
        /// Hide notification after delay.
        /// </summary>
        private System.Collections.IEnumerator HideNotificationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (upgradeNotification != null)
            {
                upgradeNotification.SetActive(false);
            }
        }

        /// <summary>
        /// Get contributor stats for dashboard.
        /// </summary>
        public ContributorStats GetStats()
        {
            return new ContributorStats
            {
                contributorId = contributorId,
                role = currentRole,
                xp = currentXP,
                level = contributorLevel,
                daoPower = currentDAOPower,
                loreCount = contributorLore.Count,
                perkCount = unlockedPerks.Count
            };
        }
    }

    /// <summary>
    /// Contributor role enum.
    /// </summary>
    public enum ContributorRole
    {
        Initiate = 0,
        Builder = 1,
        Architect = 2,
        Oracle = 3
    }

    /// <summary>
    /// Contributor perk data.
    /// </summary>
    [System.Serializable]
    public class ContributorPerk
    {
        public string name;
        public string description;
        public int daoBonus;
    }

    /// <summary>
    /// Lore entry data.
    /// </summary>
    [System.Serializable]
    public class LoreEntry
    {
        public string timestamp;
        public string content;
        public string category;
        public string contributorId;
    }

    /// <summary>
    /// Contributor stats data.
    /// </summary>
    [System.Serializable]
    public class ContributorStats
    {
        public string contributorId;
        public ContributorRole role;
        public int xp;
        public int level;
        public int daoPower;
        public int loreCount;
        public int perkCount;
    }
}
