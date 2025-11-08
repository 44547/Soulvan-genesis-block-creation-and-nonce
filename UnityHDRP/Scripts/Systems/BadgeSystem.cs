using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soulvan.Systems
{
    /// <summary>
    /// Comprehensive contributor badge system.
    /// Features badge ladder, unlock animations, vote impact tracking, and badge history.
    /// </summary>
    public class BadgeSystem : MonoBehaviour
    {
        [Header("Badge Configuration")]
        public List<BadgeTier> badgeTiers = new List<BadgeTier>();
        public Dictionary<string, List<BadgeData>> contributorBadges = new Dictionary<string, List<BadgeData>>();

        [Header("Badge Ladder UI")]
        public Transform badgeLadderContainer;
        public GameObject badgeTierCardPrefab;
        public TextMeshProUGUI ladderTitleText;

        [Header("Badge Grid UI")]
        public Transform badgeGridContainer;
        public GameObject badgeIconPrefab;
        public TextMeshProUGUI badgeCountText;

        [Header("Badge History UI")]
        public Transform badgeHistoryContainer;
        public GameObject badgeHistoryEntryPrefab;
        public ScrollRect historyScrollView;

        [Header("Vote Impact Tracker")]
        public TextMeshProUGUI voteImpactText;
        public Image voteImpactBar;
        public int votesForNextBadge = 10;

        [Header("Unlock Animation")]
        public ParticleSystem unlockRuneFX;
        public AudioClip unlockSound;
        public AudioClip soulvanVoiceLine;
        public GameObject glowFX;

        [Header("Cinematic")]
        public BadgeUnlockCutsceneTrigger cutsceneTrigger;

        private void Start()
        {
            InitializeBadgeSystem();
            LoadBadgeTiers();
            UpdateUI();
        }

        /// <summary>
        /// Initialize badge system.
        /// </summary>
        private void InitializeBadgeSystem()
        {
            Debug.Log("[BadgeSystem] Initializing contributor badge system");

            SoulvanLore.Record("Badge system initialized");
        }

        /// <summary>
        /// Load badge tiers.
        /// </summary>
        private void LoadBadgeTiers()
        {
            badgeTiers.Add(new BadgeTier
            {
                tierId = "INITIATE",
                tierName = "Initiate",
                description = "First step in the saga",
                requiredXP = 0,
                daoPower = 1,
                tierColor = new Color(0.5f, 0.5f, 0.5f)
            });

            badgeTiers.Add(new BadgeTier
            {
                tierId = "BUILDER",
                tierName = "Builder",
                description = "Contributor to the Soulvan world",
                requiredXP = 500,
                daoPower = 5,
                tierColor = new Color(0f, 0.8f, 1f)
            });

            badgeTiers.Add(new BadgeTier
            {
                tierId = "ARCHITECT",
                tierName = "Architect",
                description = "Designer of saga branches",
                requiredXP = 2000,
                daoPower = 25,
                tierColor = new Color(0.8f, 0f, 1f)
            });

            badgeTiers.Add(new BadgeTier
            {
                tierId = "ORACLE",
                tierName = "Oracle",
                description = "Keeper of lore and prophecy",
                requiredXP = 10000,
                daoPower = 100,
                tierColor = new Color(1f, 0.8f, 0f)
            });

            badgeTiers.Add(new BadgeTier
            {
                tierId = "OPERATIVE",
                tierName = "Operative",
                description = "Master of missions and combat",
                requiredXP = 5000,
                daoPower = 50,
                tierColor = new Color(1f, 0f, 0f)
            });

            badgeTiers.Add(new BadgeTier
            {
                tierId = "LEGEND",
                tierName = "Legend",
                description = "Mythic contributor of the saga",
                requiredXP = 50000,
                daoPower = 500,
                tierColor = new Color(1f, 1f, 0f)
            });

            Debug.Log($"[BadgeSystem] Loaded {badgeTiers.Count} badge tiers");
        }

        /// <summary>
        /// Mint badge for contributor.
        /// </summary>
        public void MintBadge(string contributorId, string badgeType, string reason)
        {
            Debug.Log($"[BadgeSystem] 🎖️ Minting badge: {badgeType} for {contributorId}");

            BadgeData badge = new BadgeData
            {
                badgeId = $"BADGE_{System.Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                badgeName = badgeType,
                description = reason,
                earnedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                contributorId = contributorId,
                daoImpact = $"+{GetDAOPowerForBadge(badgeType)} voting power"
            };

            // Add to contributor badges
            if (!contributorBadges.ContainsKey(contributorId))
            {
                contributorBadges[contributorId] = new List<BadgeData>();
            }
            contributorBadges[contributorId].Add(badge);

            // Play unlock animation
            PlayUnlockAnimation(badge);

            // Record to lore
            SoulvanLore.Record($"Badge minted: {badgeType} for contributor {contributorId}");
            SoulvanLore.MintBadge(contributorId, badgeType);

            // Update UI
            UpdateUI();

            // Trigger cutscene
            TriggerBadgeUnlockCutscene();
        }

        /// <summary>
        /// Upgrade badge tier.
        /// </summary>
        public void UpgradeBadge(string contributorId, string fromTier, string toTier, string reason)
        {
            Debug.Log($"[BadgeSystem] ⬆️ Upgrading badge: {fromTier} → {toTier}");

            MintBadge(contributorId, toTier, reason);

            SoulvanLore.Record($"Badge upgraded: {fromTier} → {toTier} for contributor {contributorId}: {reason}");
        }

        /// <summary>
        /// Play unlock animation.
        /// </summary>
        private void PlayUnlockAnimation(BadgeData badge)
        {
            // Play rune FX
            if (unlockRuneFX != null)
            {
                unlockRuneFX.Play();
            }

            // Play unlock sound
            if (unlockSound != null)
            {
                AudioSource.PlayClipAtPoint(unlockSound, transform.position);
            }

            // Play Soulvan voice line
            if (soulvanVoiceLine != null)
            {
                AudioSource.PlayClipAtPoint(soulvanVoiceLine, transform.position);
                Debug.Log("[BadgeSystem] 🎙️ Soulvan: 'Your legend grows. The saga remembers.'");
            }

            // Spawn glow FX
            if (glowFX != null)
            {
                Instantiate(glowFX, transform.position, Quaternion.identity);
            }
        }

        /// <summary>
        /// Track vote impact for badge unlock.
        /// </summary>
        public void TrackVoteImpact(string contributorId, int votingPower)
        {
            // Check if vote impact triggers badge upgrade
            if (votingPower >= votesForNextBadge)
            {
                UpgradeBadge(contributorId, "Builder", "Architect", "DAO vote impact milestone reached");
            }

            // Update vote impact UI
            if (voteImpactText != null)
            {
                voteImpactText.text = $"Vote Impact: {votingPower}/{votesForNextBadge}";
            }

            if (voteImpactBar != null)
            {
                voteImpactBar.fillAmount = (float)votingPower / votesForNextBadge;
            }
        }

        /// <summary>
        /// Export badge as scroll/NFT.
        /// </summary>
        public void ExportBadge(string contributorId, string badgeId)
        {
            Debug.Log($"[BadgeSystem] 📜 Exporting badge: {badgeId}");

            SoulvanLore.Record($"Badge exported: {badgeId} by contributor {contributorId}");
        }

        /// <summary>
        /// Get DAO power for badge type.
        /// </summary>
        private int GetDAOPowerForBadge(string badgeType)
        {
            BadgeTier tier = badgeTiers.Find(t => t.tierName == badgeType);
            return tier != null ? tier.daoPower : 1;
        }

        /// <summary>
        /// Get badge history for contributor.
        /// </summary>
        public List<BadgeData> GetBadgeHistory(string contributorId)
        {
            if (contributorBadges.ContainsKey(contributorId))
            {
                return contributorBadges[contributorId];
            }

            return new List<BadgeData>();
        }

        /// <summary>
        /// Trigger badge unlock cutscene.
        /// </summary>
        private void TriggerBadgeUnlockCutscene()
        {
            if (cutsceneTrigger != null)
            {
                cutsceneTrigger.TriggerCutscene();
            }
        }

        /// <summary>
        /// Update UI.
        /// </summary>
        private void UpdateUI()
        {
            // Update badge ladder
            UpdateBadgeLadder();

            // Update badge grid
            UpdateBadgeGrid();

            // Update badge history
            UpdateBadgeHistory();
        }

        /// <summary>
        /// Update badge ladder display.
        /// </summary>
        private void UpdateBadgeLadder()
        {
            if (badgeLadderContainer == null || badgeTierCardPrefab == null) return;

            // Clear existing cards
            foreach (Transform child in badgeLadderContainer)
            {
                Destroy(child.gameObject);
            }

            // Create tier cards
            foreach (var tier in badgeTiers)
            {
                GameObject card = Instantiate(badgeTierCardPrefab, badgeLadderContainer);
                BadgeTierCard cardComponent = card.GetComponent<BadgeTierCard>();
                if (cardComponent != null)
                {
                    cardComponent.SetData(tier);
                }
            }
        }

        /// <summary>
        /// Update badge grid display.
        /// </summary>
        private void UpdateBadgeGrid()
        {
            if (badgeGridContainer == null || badgeIconPrefab == null) return;

            // Clear existing icons
            foreach (Transform child in badgeGridContainer)
            {
                Destroy(child.gameObject);
            }

            // Show badges for all contributors (or filter by active contributor)
            int totalBadges = 0;
            foreach (var kvp in contributorBadges)
            {
                foreach (var badge in kvp.Value)
                {
                    GameObject icon = Instantiate(badgeIconPrefab, badgeGridContainer);
                    BadgeIcon iconComponent = icon.GetComponent<BadgeIcon>();
                    if (iconComponent != null)
                    {
                        iconComponent.SetData(badge);
                    }

                    totalBadges++;
                }
            }

            // Update count
            if (badgeCountText != null)
            {
                badgeCountText.text = $"Total Badges: {totalBadges}";
            }
        }

        /// <summary>
        /// Update badge history display.
        /// </summary>
        private void UpdateBadgeHistory()
        {
            if (badgeHistoryContainer == null || badgeHistoryEntryPrefab == null) return;

            // Clear existing entries
            foreach (Transform child in badgeHistoryContainer)
            {
                Destroy(child.gameObject);
            }

            // Show history for all contributors
            foreach (var kvp in contributorBadges)
            {
                foreach (var badge in kvp.Value)
                {
                    GameObject entry = Instantiate(badgeHistoryEntryPrefab, badgeHistoryContainer);
                    BadgeHistoryEntry entryComponent = entry.GetComponent<BadgeHistoryEntry>();
                    if (entryComponent != null)
                    {
                        entryComponent.SetData(badge);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Badge tier data.
    /// </summary>
    [System.Serializable]
    public class BadgeTier
    {
        public string tierId;
        public string tierName;
        public string description;
        public int requiredXP;
        public int daoPower;
        public Color tierColor;
    }
}
