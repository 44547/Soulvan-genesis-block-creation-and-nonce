using UnityEngine;
using Soulvan.Wallet;
using Soulvan.Lore;

namespace Soulvan.Systems
{
    /// <summary>
    /// Tracks tier unlocks and triggers wallet events.
    /// Integrates with existing ProgressionSystem for seamless tier advancement.
    /// Handles badge minting, avatar updates, and lore logging.
    /// </summary>
    public class ProgressionTracker : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField] private WalletController wallet;
        [SerializeField] private AvatarRenderer avatar;
        [SerializeField] private BadgeMintService badgeMint;
        [SerializeField] private LoreChronicle chronicle;
        [SerializeField] private ProgressionSystem progressionSystem;

        [Header("Current Status")]
        public int currentTier = 1;

        private void Awake()
        {
            // Subscribe to progression system events
            EventBus.OnTierUnlocked += HandleTierUnlock;
            EventBus.OnMissionComplete += OnMissionComplete;
            EventBus.OnProposalVoted += OnVoteCast;
        }

        private void OnDestroy()
        {
            EventBus.OnTierUnlocked -= HandleTierUnlock;
            EventBus.OnMissionComplete -= OnMissionComplete;
            EventBus.OnProposalVoted -= OnVoteCast;
        }

        private void HandleTierUnlock(int newTier, string tierName)
        {
            AdvanceTier(newTier);
        }

        public void AdvanceTier(int newTier)
        {
            currentTier = newTier;

            Debug.Log($"[ProgressionTracker] Advancing to Tier {newTier}");

            // Update avatar visual identity
            if (avatar != null)
            {
                avatar.UpdateForTier(newTier);
            }

            // Mint tier badge NFT
            if (badgeMint != null && wallet != null && wallet.IsConnected)
            {
                badgeMint.MintTierBadge(newTier, wallet.walletAddress);
            }

            // Log tier upgrade to on-chain Chronicle
            if (chronicle != null && wallet != null && wallet.IsConnected)
            {
                chronicle.LogTierUpgrade(newTier, wallet.walletAddress);
            }

            // Update wallet identity level (existing integration)
            if (wallet != null && wallet.IsConnected)
            {
                wallet.UpdateIdentityLevel(newTier);
            }

            // Emit tier upgrade event for bridge service
            EventBus.EmitTierUpgrade(newTier);
        }

        public void OnMissionComplete(string missionId, bool success)
        {
            if (!success) return;

            Debug.Log($"[ProgressionTracker] Mission completed: {missionId}");

            // Log mission to Chronicle
            if (chronicle != null && wallet != null && wallet.IsConnected)
            {
                chronicle.LogMission(missionId, wallet.walletAddress);
            }

            // Check for boss mission badges
            if (missionId.StartsWith("boss_"))
            {
                string bossId = missionId.Replace("boss_", "");
                
                if (badgeMint != null && wallet != null && wallet.IsConnected)
                {
                    badgeMint.MintBossBadge(bossId, wallet.walletAddress);
                }

                Debug.Log($"[ProgressionTracker] Boss badge minted for: {bossId}");
            }

            // Add mission completion to progression system
            if (progressionSystem != null)
            {
                progressionSystem.AddMissionCompletion(missionId);
            }
        }

        public void OnVoteCast(string proposalId)
        {
            Debug.Log($"[ProgressionTracker] DAO vote cast: {proposalId}");

            // Log vote to Chronicle
            if (chronicle != null && wallet != null && wallet.IsConnected)
            {
                // Note: choice parameter not available in OnProposalVoted event
                // Consider extending EventBus to include choice
                chronicle.LogVote(proposalId, 1, wallet.walletAddress);
            }

            // Trigger Oracle flare effect on avatar
            if (avatar != null)
            {
                avatar.TriggerOracleFlare();
            }

            // Add DAO participation to progression system
            if (progressionSystem != null)
            {
                progressionSystem.AddDaoVoteParticipation();
            }

            // Emit vote cast event for bridge service
            EventBus.EmitVoteCast(proposalId, 1);
        }

        /// <summary>
        /// Manual tier advancement for testing or admin purposes.
        /// </summary>
        public void ForceTierAdvancement(int tier)
        {
            if (tier < 1 || tier > 5)
            {
                Debug.LogWarning($"[ProgressionTracker] Invalid tier: {tier}. Must be 1-5.");
                return;
            }

            AdvanceTier(tier);
        }

        /// <summary>
        /// Get current tier data from progression system.
        /// </summary>
        public TierData GetCurrentTierData()
        {
            if (progressionSystem != null)
            {
                return progressionSystem.GetCurrentTierData();
            }

            return null;
        }

        /// <summary>
        /// Check if feature is unlocked for current tier.
        /// </summary>
        public bool IsFeatureUnlocked(string feature)
        {
            if (progressionSystem != null)
            {
                return progressionSystem.IsFeatureUnlocked(feature);
            }

            return false;
        }
    }

    /// <summary>
    /// Extended EventBus with progression tracking events.
    /// </summary>
    public static partial class EventBus
    {
        public static event System.Action<int> OnTierUpgrade;
        public static event System.Action<string, int> OnVoteCast;

        public static void EmitTierUpgrade(int tier)
        {
            OnTierUpgrade?.Invoke(tier);
            Debug.Log($"[EventBus] Tier upgrade emitted: {tier}");
        }

        public static void EmitVoteCast(string proposalId, int choice)
        {
            OnVoteCast?.Invoke(proposalId, choice);
            Debug.Log($"[EventBus] Vote cast emitted: {proposalId}, choice: {choice}");
        }
    }
}
