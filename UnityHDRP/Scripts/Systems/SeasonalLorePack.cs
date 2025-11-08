using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soulvan.Systems
{
    /// <summary>
    /// Seasonal Lore Pack system with holographic calendar and seasonal arcs.
    /// Features mission quests, contributor progression, and seasonal exports.
    /// </summary>
    public class SeasonalLorePack : MonoBehaviour
    {
        [Header("Season Configuration")]
        public string currentSeason = "Storm of the Vault";
        public int seasonNumber = 1;
        public string seasonStart = "2025-11-01";
        public string seasonEnd = "2026-02-01";

        [Header("Holographic Calendar")]
        public GameObject calendarHologram;
        public Transform[] seasonalArcNodes;
        public ParticleSystem calendarRuneFX;

        [Header("Seasonal Arcs")]
        public List<SeasonalArc> availableArcs = new List<SeasonalArc>();
        public TextMeshProUGUI currentArcText;

        [Header("Mission Entry Pads")]
        public Transform[] missionPads;
        public GameObject padHUDOverlay;
        public ParticleSystem padRuneFX;

        [Header("Progression Tracker")]
        public Image seasonProgressBar;
        public TextMeshProUGUI progressText;
        public TextMeshProUGUI badgeMilestonesText;

        [Header("Export System")]
        public Button exportSeasonalLoreButton;
        public Button exportReplayNFTButton;
        public Button exportArtifactButton;

        [Header("FX")]
        public ParticleSystem seasonalThemeFX;
        public AudioClip seasonalMusic;
        public AudioClip questCompleteSound;

        [Header("Cinematic")]
        public SeasonalCutsceneTrigger cutsceneTrigger;

        private int seasonXP = 0;
        private int seasonXPRequired = 10000;
        private List<string> completedQuests = new List<string>();

        private void Start()
        {
            InitializeSeason();
            LoadSeasonalArcs();
            SetupMissionPads();
            UpdateUI();
        }

        /// <summary>
        /// Initialize seasonal system.
        /// </summary>
        private void InitializeSeason()
        {
            Debug.Log($"[SeasonalLorePack] Initializing Season {seasonNumber}: {currentSeason}");

            // Play seasonal theme FX
            if (seasonalThemeFX != null)
            {
                seasonalThemeFX.Play();
            }

            // Play seasonal music
            if (seasonalMusic != null)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.clip = seasonalMusic;
                source.loop = true;
                source.volume = 0.3f;
                source.Play();
            }

            // Record to lore
            SoulvanLore.Record($"Season {seasonNumber} started: {currentSeason}");
        }

        /// <summary>
        /// Load seasonal arcs.
        /// </summary>
        private void LoadSeasonalArcs()
        {
            availableArcs.Add(new SeasonalArc
            {
                arcId = "STORM_VAULT",
                arcName = "Storm of the Vault",
                description = "Breach the ancient vault during the storm",
                questCount = 12,
                xpReward = 5000,
                badgeUnlock = "Vault Breacher",
                isActive = true
            });

            availableArcs.Add(new SeasonalArc
            {
                arcId = "RISE_ARCHITECTS",
                arcName = "Rise of the Architects",
                description = "Contribute to DAO expansion and lore growth",
                questCount = 8,
                xpReward = 3000,
                badgeUnlock = "Architect Rising",
                isActive = true
            });

            availableArcs.Add(new SeasonalArc
            {
                arcId = "CHRONO_DRIFT",
                arcName = "Chrono Drift",
                description = "Navigate timeline rifts and saga divergence",
                questCount = 15,
                xpReward = 7500,
                badgeUnlock = "Chrono Navigator",
                isActive = false
            });

            Debug.Log($"[SeasonalLorePack] Loaded {availableArcs.Count} seasonal arcs");
        }

        /// <summary>
        /// Setup mission entry pads.
        /// </summary>
        private void SetupMissionPads()
        {
            for (int i = 0; i < missionPads.Length; i++)
            {
                Transform pad = missionPads[i];

                // Add rune FX
                if (padRuneFX != null)
                {
                    ParticleSystem fx = Instantiate(padRuneFX, pad.position + Vector3.up * 0.2f, Quaternion.identity, pad);
                    fx.Play();
                }
            }

            Debug.Log($"[SeasonalLorePack] Setup {missionPads.Length} mission entry pads");
        }

        /// <summary>
        /// Complete seasonal quest.
        /// </summary>
        public void CompleteQuest(string questId, string contributorId, int xpGain)
        {
            if (completedQuests.Contains(questId))
            {
                Debug.Log($"[SeasonalLorePack] Quest already completed: {questId}");
                return;
            }

            completedQuests.Add(questId);
            seasonXP += xpGain;

            Debug.Log($"[SeasonalLorePack] ✅ Quest completed: {questId} (+{xpGain} XP)");

            // Play quest complete sound
            if (questCompleteSound != null)
            {
                AudioSource.PlayClipAtPoint(questCompleteSound, transform.position);
            }

            // Grant XP to contributor
            ArchitectKit kit = FindObjectOfType<ArchitectKit>();
            if (kit != null && kit.contributorId == contributorId)
            {
                kit.GrantXP(xpGain, $"Seasonal Quest: {questId}");
            }

            // Check for badge milestones
            CheckBadgeMilestones(contributorId);

            // Record to lore
            SoulvanLore.Record($"Contributor {contributorId} completed seasonal quest: {questId}");

            // Update UI
            UpdateUI();

            // Trigger cutscene for major quests
            if (xpGain >= 1000)
            {
                TriggerSeasonalCutscene();
            }
        }

        /// <summary>
        /// Check for badge milestones.
        /// </summary>
        private void CheckBadgeMilestones(string contributorId)
        {
            int questsCompleted = completedQuests.Count;

            if (questsCompleted == 5)
            {
                SoulvanLore.MintBadge(contributorId, "Seasonal Initiate");
                Debug.Log($"[SeasonalLorePack] 🎖️ Badge unlocked: Seasonal Initiate");
            }
            else if (questsCompleted == 10)
            {
                SoulvanLore.MintBadge(contributorId, "Seasonal Veteran");
                Debug.Log($"[SeasonalLorePack] 🎖️ Badge unlocked: Seasonal Veteran");
            }
            else if (questsCompleted == 20)
            {
                SoulvanLore.MintBadge(contributorId, "Seasonal Master");
                Debug.Log($"[SeasonalLorePack] 🎖️ Badge unlocked: Seasonal Master");
            }
        }

        /// <summary>
        /// Export seasonal lore logs.
        /// </summary>
        public void ExportSeasonalLore()
        {
            Debug.Log("[SeasonalLorePack] 📜 Exporting seasonal lore...");

            // Export lore for season
            SoulvanLore.Record($"Seasonal lore exported: {currentSeason} ({completedQuests.Count} quests)");

            // Trigger cutscene
            TriggerSeasonalCutscene();
        }

        /// <summary>
        /// Export replay NFT.
        /// </summary>
        public void ExportReplayNFT()
        {
            Debug.Log("[SeasonalLorePack] 🎬 Exporting replay NFT...");

            // Export seasonal replay bundle
            SoulvanLore.ExportMissionLore($"SEASON_{seasonNumber}", seasonXP, completedQuests.Count);
        }

        /// <summary>
        /// Export DAO-bound artifact.
        /// </summary>
        public void ExportArtifact()
        {
            if (seasonXP < seasonXPRequired)
            {
                Debug.Log($"[SeasonalLorePack] ❌ Insufficient XP for artifact: {seasonXP}/{seasonXPRequired}");
                return;
            }

            Debug.Log("[SeasonalLorePack] 🔱 Exporting DAO-bound artifact...");

            // Mint seasonal artifact
            SoulvanLore.Record($"Seasonal artifact minted: {currentSeason} (XP: {seasonXP})");
        }

        /// <summary>
        /// Trigger seasonal cutscene.
        /// </summary>
        private void TriggerSeasonalCutscene()
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
            // Current arc
            if (currentArcText != null)
            {
                currentArcText.text = $"Current Arc: {currentSeason}";
            }

            // Progress bar
            if (seasonProgressBar != null)
            {
                seasonProgressBar.fillAmount = (float)seasonXP / seasonXPRequired;
            }

            if (progressText != null)
            {
                progressText.text = $"{seasonXP} / {seasonXPRequired} XP";
            }

            // Badge milestones
            if (badgeMilestonesText != null)
            {
                badgeMilestonesText.text = $"Quests Completed: {completedQuests.Count}";
            }
        }
    }

    /// <summary>
    /// Seasonal arc data.
    /// </summary>
    [System.Serializable]
    public class SeasonalArc
    {
        public string arcId;
        public string arcName;
        public string description;
        public int questCount;
        public int xpReward;
        public string badgeUnlock;
        public bool isActive;
    }
}
