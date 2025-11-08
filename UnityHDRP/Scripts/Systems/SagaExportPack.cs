using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soulvan.Systems
{
    /// <summary>
    /// Saga export pack system for contributor lore, badges, and artifacts.
    /// Features scrollable timeline, badge gallery, replay vault, and artifact minting.
    /// Cinematic exports with Soulvan's approval stamp.
    /// </summary>
    public class SagaExportPack : MonoBehaviour
    {
        [Header("Contributor Data")]
        public string contributorId;
        public ArchitectKit architectKit;

        [Header("Saga Timeline UI")]
        public GameObject timelinePanel;
        public ScrollRect timelineScrollView;
        public Transform timelineContent;
        public GameObject loreEntryPrefab;

        [Header("Badge Gallery UI")]
        public GameObject badgeGalleryPanel;
        public Transform badgeContainer;
        public GameObject badgeCardPrefab;
        public TextMeshProUGUI badgeCountText;

        [Header("Replay Vault UI")]
        public GameObject replayVaultPanel;
        public Transform replayContainer;
        public GameObject replayCardPrefab;
        public TextMeshProUGUI replayCountText;

        [Header("Artifact Panel")]
        public GameObject artifactPanel;
        public GameObject artifactModel;
        public ParticleSystem artifactRuneGlow;
        public TextMeshProUGUI artifactStatsText;

        [Header("Export Buttons")]
        public Button exportSagaScrollButton;
        public Button exportBadgesButton;
        public Button exportReplaysButton;
        public Button exportArtifactButton;

        [Header("Wallet Integration")]
        public WalletBridge walletBridge;
        public TextMeshProUGUI walletAddressText;

        [Header("FX")]
        public ParticleSystem exportFX;
        public AudioClip exportSound;

        private List<LoreEntry> contributorLore = new List<LoreEntry>();
        private List<BadgeData> earnedBadges = new List<BadgeData>();
        private List<MissionReplay> missionReplays = new List<MissionReplay>();

        private void Start()
        {
            InitializeExportPack();
            SetupExportButtons();
            LoadContributorData();
            UpdateAllPanels();
        }

        /// <summary>
        /// Initialize export pack.
        /// </summary>
        private void InitializeExportPack()
        {
            if (architectKit == null)
            {
                architectKit = FindObjectOfType<ArchitectKit>();
            }

            if (architectKit != null)
            {
                contributorId = architectKit.contributorId;
            }

            Debug.Log($"[SagaExportPack] Initialized for contributor: {contributorId}");
        }

        /// <summary>
        /// Setup export button callbacks.
        /// </summary>
        private void SetupExportButtons()
        {
            if (exportSagaScrollButton != null)
            {
                exportSagaScrollButton.onClick.AddListener(ExportSagaScroll);
            }

            if (exportBadgesButton != null)
            {
                exportBadgesButton.onClick.AddListener(ExportBadges);
            }

            if (exportReplaysButton != null)
            {
                exportReplaysButton.onClick.AddListener(ExportReplays);
            }

            if (exportArtifactButton != null)
            {
                exportArtifactButton.onClick.AddListener(ExportArtifact);
            }
        }

        /// <summary>
        /// Load contributor data from ArchitectKit.
        /// </summary>
        private void LoadContributorData()
        {
            if (architectKit != null)
            {
                contributorLore = architectKit.contributorLore;
            }

            // Load badges (example data)
            LoadBadges();

            // Load mission replays (example data)
            LoadMissionReplays();

            Debug.Log($"[SagaExportPack] Loaded {contributorLore.Count} lore entries, {earnedBadges.Count} badges, {missionReplays.Count} replays");
        }

        /// <summary>
        /// Load contributor badges.
        /// </summary>
        private void LoadBadges()
        {
            // In production, load from blockchain
            earnedBadges.Add(new BadgeData
            {
                badgeId = "GENESIS_CONTRIBUTOR",
                badgeName = "Genesis Contributor",
                description = "Joined during genesis block",
                earnedDate = "2023-12-25",
                daoImpact = "+5 voting power"
            });

            earnedBadges.Add(new BadgeData
            {
                badgeId = "SKYFIRE_VICTOR",
                badgeName = "Skyfire Victor",
                description = "Completed Skyfire Pursuit mission",
                earnedDate = "2024-01-15",
                daoImpact = "+10 voting power"
            });
        }

        /// <summary>
        /// Load mission replays.
        /// </summary>
        private void LoadMissionReplays()
        {
            // In production, load from LoreChronicle contract
            missionReplays.Add(new MissionReplay
            {
                missionId = "SKYFIRE_TOKYO",
                missionTitle = "Skyfire Pursuit: Tokyo Rooftops",
                completionDate = "2024-01-15",
                rewardEarned = 850f,
                highlights = "47 enemies neutralized, 6 battle phases, grapple mastery"
            });

            missionReplays.Add(new MissionReplay
            {
                missionId = "VAULT_BREACH_001",
                missionTitle = "Vault Breach: Command Chamber",
                completionDate = "2024-02-20",
                rewardEarned = 1200f,
                highlights = "Rune puzzle solved, lore terminal accessed, artifact recovered"
            });
        }

        /// <summary>
        /// Update all UI panels.
        /// </summary>
        private void UpdateAllPanels()
        {
            UpdateSagaTimeline();
            UpdateBadgeGallery();
            UpdateReplayVault();
            UpdateArtifactPanel();
            UpdateWalletDisplay();
        }

        /// <summary>
        /// Update saga timeline panel.
        /// </summary>
        private void UpdateSagaTimeline()
        {
            if (timelineContent == null || loreEntryPrefab == null) return;

            // Clear existing entries
            foreach (Transform child in timelineContent)
            {
                Destroy(child.gameObject);
            }

            // Spawn lore entry cards
            foreach (var entry in contributorLore)
            {
                GameObject entryObj = Instantiate(loreEntryPrefab, timelineContent);
                LoreEntryCard card = entryObj.GetComponent<LoreEntryCard>();
                if (card != null)
                {
                    card.SetData(entry);
                }
            }

            Debug.Log($"[SagaExportPack] Timeline updated with {contributorLore.Count} entries");
        }

        /// <summary>
        /// Update badge gallery panel.
        /// </summary>
        private void UpdateBadgeGallery()
        {
            if (badgeContainer == null || badgeCardPrefab == null) return;

            // Clear existing badges
            foreach (Transform child in badgeContainer)
            {
                Destroy(child.gameObject);
            }

            // Spawn badge cards
            foreach (var badge in earnedBadges)
            {
                GameObject badgeObj = Instantiate(badgeCardPrefab, badgeContainer);
                BadgeCard card = badgeObj.GetComponent<BadgeCard>();
                if (card != null)
                {
                    card.SetData(badge);
                }
            }

            // Update count
            if (badgeCountText != null)
            {
                badgeCountText.text = $"Badges Earned: {earnedBadges.Count}";
            }

            Debug.Log($"[SagaExportPack] Badge gallery updated with {earnedBadges.Count} badges");
        }

        /// <summary>
        /// Update replay vault panel.
        /// </summary>
        private void UpdateReplayVault()
        {
            if (replayContainer == null || replayCardPrefab == null) return;

            // Clear existing replays
            foreach (Transform child in replayContainer)
            {
                Destroy(child.gameObject);
            }

            // Spawn replay cards
            foreach (var replay in missionReplays)
            {
                GameObject replayObj = Instantiate(replayCardPrefab, replayContainer);
                ReplayCard card = replayObj.GetComponent<ReplayCard>();
                if (card != null)
                {
                    card.SetData(replay);
                }
            }

            // Update count
            if (replayCountText != null)
            {
                replayCountText.text = $"Mission Replays: {missionReplays.Count}";
            }

            Debug.Log($"[SagaExportPack] Replay vault updated with {missionReplays.Count} replays");
        }

        /// <summary>
        /// Update artifact panel.
        /// </summary>
        private void UpdateArtifactPanel()
        {
            if (artifactStatsText == null) return;

            // Calculate artifact power
            int totalLore = contributorLore.Count;
            int totalBadges = earnedBadges.Count;
            int totalReplays = missionReplays.Count;
            int artifactPower = (totalLore * 10) + (totalBadges * 50) + (totalReplays * 100);

            // Update stats
            string statsText = "<b>Artifact Power</b>\n";
            statsText += $"Lore Entries: {totalLore}\n";
            statsText += $"Badges: {totalBadges}\n";
            statsText += $"Replays: {totalReplays}\n";
            statsText += $"<color=cyan>Total Power: {artifactPower}</color>";

            artifactStatsText.text = statsText;

            // Animate artifact glow
            if (artifactRuneGlow != null)
            {
                artifactRuneGlow.Play();
            }
        }

        /// <summary>
        /// Update wallet display.
        /// </summary>
        private void UpdateWalletDisplay()
        {
            if (walletAddressText != null && architectKit != null)
            {
                string wallet = architectKit.contributorWallet;
                if (!string.IsNullOrEmpty(wallet))
                {
                    walletAddressText.text = $"Wallet: {wallet.Substring(0, 6)}...{wallet.Substring(wallet.Length - 4)}";
                }
                else
                {
                    walletAddressText.text = "Wallet: Not Connected";
                }
            }
        }

        /// <summary>
        /// Export saga scroll as NFT.
        /// </summary>
        public void ExportSagaScroll()
        {
            Debug.Log("[SagaExportPack] 📜 Exporting saga scroll...");

            // Play export FX
            if (exportFX != null)
            {
                exportFX.Play();
            }

            if (exportSound != null)
            {
                AudioSource.PlayClipAtPoint(exportSound, transform.position);
            }

            // Mint NFT
            if (walletBridge != null)
            {
                walletBridge.MintSagaScrollNFT(contributorId, contributorLore);
            }

            // Record to lore
            SoulvanLore.Record($"Contributor {contributorId} exported saga scroll with {contributorLore.Count} entries");

            // Trigger cutscene
            ExportCutsceneTrigger cutscene = GetComponent<ExportCutsceneTrigger>();
            if (cutscene != null)
            {
                cutscene.TriggerSagaScrollExport(contributorLore);
            }
        }

        /// <summary>
        /// Export badges as NFTs.
        /// </summary>
        public void ExportBadges()
        {
            Debug.Log("[SagaExportPack] 🎖️ Exporting badges...");

            // Play export FX
            if (exportFX != null)
            {
                exportFX.Play();
            }

            if (exportSound != null)
            {
                AudioSource.PlayClipAtPoint(exportSound, transform.position);
            }

            // Mint badge NFTs
            foreach (var badge in earnedBadges)
            {
                SoulvanLore.MintBadge(contributorId, badge.badgeName);
            }

            // Record to lore
            SoulvanLore.Record($"Contributor {contributorId} exported {earnedBadges.Count} badges");

            // Trigger cutscene
            ExportCutsceneTrigger cutscene = GetComponent<ExportCutsceneTrigger>();
            if (cutscene != null)
            {
                cutscene.TriggerBadgeExport(contributorId, architectKit.currentRole);
            }
        }

        /// <summary>
        /// Export mission replays as NFTs.
        /// </summary>
        public void ExportReplays()
        {
            Debug.Log("[SagaExportPack] 🎬 Exporting replays...");

            // Play export FX
            if (exportFX != null)
            {
                exportFX.Play();
            }

            if (exportSound != null)
            {
                AudioSource.PlayClipAtPoint(exportSound, transform.position);
            }

            // Export replay data
            foreach (var replay in missionReplays)
            {
                SoulvanLore.ExportMissionLore(replay.missionId, replay.rewardEarned, 0);
            }

            // Record to lore
            SoulvanLore.Record($"Contributor {contributorId} exported {missionReplays.Count} mission replays");

            // Trigger cutscene
            ExportCutsceneTrigger cutscene = GetComponent<ExportCutsceneTrigger>();
            if (cutscene != null)
            {
                cutscene.TriggerReplayExport(missionReplays);
            }
        }

        /// <summary>
        /// Export legendary artifact (bundles lore, badges, replays).
        /// </summary>
        public void ExportArtifact()
        {
            if (architectKit == null || architectKit.currentRole < ContributorRole.Oracle)
            {
                Debug.Log("[SagaExportPack] ❌ Oracle role required for artifact export");
                return;
            }

            Debug.Log("[SagaExportPack] 🔱 Exporting legendary artifact...");

            // Play export FX
            if (exportFX != null)
            {
                exportFX.Play();
            }

            if (exportSound != null)
            {
                AudioSource.PlayClipAtPoint(exportSound, transform.position);
            }

            // Calculate artifact power
            int artifactPower = (contributorLore.Count * 10) + (earnedBadges.Count * 50) + (missionReplays.Count * 100);

            // Mint artifact NFT
            if (walletBridge != null)
            {
                walletBridge.MintArtifactNFT(contributorId, artifactPower, contributorLore, earnedBadges, missionReplays);
            }

            // Record to lore
            SoulvanLore.Record($"Contributor {contributorId} exported legendary artifact with power {artifactPower}");

            // Trigger cutscene
            ExportCutsceneTrigger cutscene = GetComponent<ExportCutsceneTrigger>();
            if (cutscene != null)
            {
                cutscene.TriggerArtifactExport(artifactPower);
            }
        }
    }

    /// <summary>
    /// Badge data structure.
    /// </summary>
    [System.Serializable]
    public class BadgeData
    {
        public string badgeId;
        public string badgeName;
        public string description;
        public string earnedDate;
        public string daoImpact;
    }

    /// <summary>
    /// Mission replay data structure.
    /// </summary>
    [System.Serializable]
    public class MissionReplay
    {
        public string missionId;
        public string missionTitle;
        public string completionDate;
        public float rewardEarned;
        public string highlights;
    }
}
