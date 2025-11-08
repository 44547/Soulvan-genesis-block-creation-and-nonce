using System;
using System.Collections.Generic;
using UnityEngine;
using Soulvan.Wallet;

namespace Soulvan.Systems
{
    /// <summary>
    /// 5-tier progression system for Soulvan.
    /// Tracks player journey from Street Racer to Mythic Legend.
    /// Integrates with Soulvan Wallet for identity evolution and DAO unlocks.
    /// </summary>
    public class ProgressionSystem : MonoBehaviour
    {
        [Header("Current Status")]
        [SerializeField] private int currentTier = 1;
        [SerializeField] private int missionsCompleted = 0;
        [SerializeField] private int bossesDefeated = 0;
        [SerializeField] private int daoVotesParticipated = 0;
        [SerializeField] private float svnBalance = 0f;

        [Header("References")]
        [SerializeField] private WalletController walletController;

        private Dictionary<int, TierData> tierDatabase;

        private void Awake()
        {
            InitializeTierDatabase();
            LoadProgress();
        }

        private void InitializeTierDatabase()
        {
            tierDatabase = new Dictionary<int, TierData>
            {
                {
                    1, new TierData
                    {
                        tier = 1,
                        name = "Street Racer",
                        description = "New driver on Soulvan's neon streets",
                        requiredMissions = 0,
                        requiredBosses = 0,
                        requiredDaoVotes = 0,
                        requiredSVN = 0f,
                        unlockedFeatures = new List<string> { "Basic hypercars", "Driving missions", "Garage access" },
                        walletIdentityLevel = 1,
                        avatarEvolution = "Basic street racer avatar with default skin",
                        motifAccess = new List<Motif> { Motif.Storm }
                    }
                },
                {
                    2, new TierData
                    {
                        tier = 2,
                        name = "Mission Runner",
                        description = "Trusted courier for underground operations",
                        requiredMissions = 10,
                        requiredBosses = 0,
                        requiredDaoVotes = 0,
                        requiredSVN = 100f,
                        unlockedFeatures = new List<string> { "Stealth missions", "Biometric infiltration", "On-foot gameplay" },
                        walletIdentityLevel = 2,
                        avatarEvolution = "Mission runner outfit with glowing accents (Storm motif)",
                        motifAccess = new List<Motif> { Motif.Storm, Motif.Calm }
                    }
                },
                {
                    3, new TierData
                    {
                        tier = 3,
                        name = "Arc Champion",
                        description = "Victor of Soulvan's narrative arcs",
                        requiredMissions = 25,
                        requiredBosses = 3,
                        requiredDaoVotes = 0,
                        requiredSVN = 500f,
                        unlockedFeatures = new List<string> { "Boss battles", "Elite hypercars", "Cosmic motif access" },
                        walletIdentityLevel = 3,
                        avatarEvolution = "Arc Champion armor with cosmic aura effects",
                        motifAccess = new List<Motif> { Motif.Storm, Motif.Calm, Motif.Cosmic }
                    }
                },
                {
                    4, new TierData
                    {
                        tier = 4,
                        name = "DAO Hero",
                        description = "Governance leader shaping Soulvan's future",
                        requiredMissions = 50,
                        requiredBosses = 5,
                        requiredDaoVotes = 10,
                        requiredSVN = 2000f,
                        unlockedFeatures = new List<string> { "DAO rituals", "Governance voting", "Proposal creation", "Oracle motif" },
                        walletIdentityLevel = 4,
                        avatarEvolution = "DAO Hero regalia with oracle runes and aura",
                        motifAccess = new List<Motif> { Motif.Storm, Motif.Calm, Motif.Cosmic, Motif.Oracle }
                    }
                },
                {
                    5, new TierData
                    {
                        tier = 5,
                        name = "Mythic Legend",
                        description = "Transcended hero of Soulvan lore",
                        requiredMissions = 100,
                        requiredBosses = 10,
                        requiredDaoVotes = 50,
                        requiredSVN = 10000f,
                        unlockedFeatures = new List<string> { "All content unlocked", "Mythic hypercars", "Exclusive DAO proposals", "All motifs", "Legendary NFTs" },
                        walletIdentityLevel = 5,
                        avatarEvolution = "Mythic Legend form with transcendent VFX (multi-motif aura)",
                        motifAccess = new List<Motif> { Motif.Storm, Motif.Calm, Motif.Cosmic, Motif.Oracle }
                    }
                }
            };
        }

        public void AddMissionCompletion(string missionId)
        {
            missionsCompleted++;
            SaveProgress();
            CheckTierUnlock();

            Debug.Log($"[ProgressionSystem] Mission completed: {missionId} (Total: {missionsCompleted})");
        }

        public void AddBossDefeat(string bossName)
        {
            bossesDefeated++;
            SaveProgress();
            CheckTierUnlock();

            Debug.Log($"[ProgressionSystem] Boss defeated: {bossName} (Total: {bossesDefeated})");
        }

        public void AddDaoVoteParticipation()
        {
            daoVotesParticipated++;
            SaveProgress();
            CheckTierUnlock();

            Debug.Log($"[ProgressionSystem] DAO vote participated (Total: {daoVotesParticipated})");
        }

        public void UpdateSVNBalance(float newBalance)
        {
            svnBalance = newBalance;
            SaveProgress();
            CheckTierUnlock();
        }

        private void CheckTierUnlock()
        {
            // Check if requirements for next tier are met
            int nextTier = currentTier + 1;
            
            if (nextTier > 5) return; // Max tier reached

            TierData nextTierData = tierDatabase[nextTier];

            bool meetsRequirements = 
                missionsCompleted >= nextTierData.requiredMissions &&
                bossesDefeated >= nextTierData.requiredBosses &&
                daoVotesParticipated >= nextTierData.requiredDaoVotes &&
                svnBalance >= nextTierData.requiredSVN;

            if (meetsRequirements)
            {
                UnlockTier(nextTier);
            }
        }

        private void UnlockTier(int tier)
        {
            currentTier = tier;
            TierData tierData = tierDatabase[tier];

            Debug.Log($"[ProgressionSystem] 🎉 TIER UNLOCKED: {tierData.name} (Tier {tier})");
            Debug.Log($"[ProgressionSystem] Description: {tierData.description}");
            Debug.Log($"[ProgressionSystem] Avatar Evolution: {tierData.avatarEvolution}");

            // Update wallet identity level
            if (walletController != null)
            {
                walletController.UpdateIdentityLevel(tier);
            }

            // Emit tier unlock event
            EventBus.EmitTierUnlocked(tier, tierData.name);

            // Trigger cinematic sequence
            StartCoroutine(PlayTierUnlockCinematic(tierData));

            SaveProgress();
        }

        private System.Collections.IEnumerator PlayTierUnlockCinematic(TierData tierData)
        {
            Debug.Log($"[ProgressionSystem] Playing tier unlock cinematic for {tierData.name}");

            // Apply motif overlays based on tier
            MotifAPI motifAPI = FindObjectOfType<MotifAPI>();
            if (motifAPI != null)
            {
                switch (tierData.tier)
                {
                    case 2: motifAPI.SetMotif(Motif.Calm, 1f); break;
                    case 3: motifAPI.SetMotif(Motif.Cosmic, 1f); break;
                    case 4: motifAPI.SetMotif(Motif.Oracle, 1f); break;
                    case 5: motifAPI.SetMotif(Motif.Cosmic, 1f); break; // Multi-motif for Mythic
                }
            }

            // Wait for cinematic duration
            yield return new UnityEngine.WaitForSeconds(5f);

            // Display unlocked features
            Debug.Log($"[ProgressionSystem] Unlocked Features:");
            foreach (string feature in tierData.unlockedFeatures)
            {
                Debug.Log($"  - {feature}");
            }
        }

        public TierData GetCurrentTierData()
        {
            return tierDatabase[currentTier];
        }

        public TierData GetTierData(int tier)
        {
            if (tierDatabase.ContainsKey(tier))
            {
                return tierDatabase[tier];
            }
            return null;
        }

        public bool IsFeatureUnlocked(string feature)
        {
            TierData currentTierData = GetCurrentTierData();
            return currentTierData.unlockedFeatures.Contains(feature);
        }

        public bool IsMotifUnlocked(Motif motif)
        {
            TierData currentTierData = GetCurrentTierData();
            return currentTierData.motifAccess.Contains(motif);
        }

        public float GetProgressToNextTier()
        {
            int nextTier = currentTier + 1;
            
            if (nextTier > 5) return 1f; // Max tier reached

            TierData nextTierData = tierDatabase[nextTier];

            // Calculate weighted progress across all requirements
            float missionProgress = Mathf.Clamp01((float)missionsCompleted / nextTierData.requiredMissions);
            float bossProgress = Mathf.Clamp01((float)bossesDefeated / nextTierData.requiredBosses);
            float daoProgress = Mathf.Clamp01((float)daoVotesParticipated / nextTierData.requiredDaoVotes);
            float svnProgress = Mathf.Clamp01(svnBalance / nextTierData.requiredSVN);

            // Average all progress metrics
            float totalProgress = (missionProgress + bossProgress + daoProgress + svnProgress) / 4f;
            return totalProgress;
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetInt("PlayerTier", currentTier);
            PlayerPrefs.SetInt("MissionsCompleted", missionsCompleted);
            PlayerPrefs.SetInt("BossesDefeated", bossesDefeated);
            PlayerPrefs.SetInt("DaoVotesParticipated", daoVotesParticipated);
            PlayerPrefs.SetFloat("SVNBalance", svnBalance);
            PlayerPrefs.Save();

            // Also record on blockchain Chronicle
            if (walletController != null)
            {
                walletController.RecordProgressionMilestone(currentTier, missionsCompleted, bossesDefeated);
            }
        }

        private void LoadProgress()
        {
            currentTier = PlayerPrefs.GetInt("PlayerTier", 1);
            missionsCompleted = PlayerPrefs.GetInt("MissionsCompleted", 0);
            bossesDefeated = PlayerPrefs.GetInt("BossesDefeated", 0);
            daoVotesParticipated = PlayerPrefs.GetInt("DaoVotesParticipated", 0);
            svnBalance = PlayerPrefs.GetFloat("SVNBalance", 0f);

            Debug.Log($"[ProgressionSystem] Loaded progress: Tier {currentTier}, {missionsCompleted} missions, {bossesDefeated} bosses, {daoVotesParticipated} votes");
        }
    }

    /// <summary>
    /// Data structure for tier requirements and unlocks.
    /// </summary>
    [System.Serializable]
    public class TierData
    {
        public int tier;
        public string name;
        public string description;
        
        [Header("Requirements")]
        public int requiredMissions;
        public int requiredBosses;
        public int requiredDaoVotes;
        public float requiredSVN;
        
        [Header("Unlocks")]
        public List<string> unlockedFeatures;
        public int walletIdentityLevel;
        public string avatarEvolution;
        public List<Motif> motifAccess;
    }

    /// <summary>
    /// Extensions to EventBus for progression events.
    /// </summary>
    public static partial class EventBus
    {
        public static event Action<int, string> OnTierUnlocked;
        public static event Action OnPlayerAttack;
        public static event Action OnPlayerDodge;

        public static void EmitTierUnlocked(int tier, string tierName)
        {
            OnTierUnlocked?.Invoke(tier, tierName);
        }

        public static void EmitPlayerAttack()
        {
            OnPlayerAttack?.Invoke();
        }

        public static void EmitPlayerDodge()
        {
            OnPlayerDodge?.Invoke();
        }
    }
}
