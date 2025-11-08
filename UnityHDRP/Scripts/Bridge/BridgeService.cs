using System.Threading.Tasks;
using UnityEngine;
using Soulvan.Wallet;
using Soulvan.Lore;

namespace Soulvan.Bridge
{
    /// <summary>
    /// Routes gameplay events to chain adapters (SoulvanCoin L1, EVM, Ordinals).
    /// Handles mission completions, tier upgrades, and DAO votes.
    /// Integrates with Reconciler for tx verification and retry logic.
    /// </summary>
    public class BridgeService : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WalletController wallet;
        [SerializeField] private Reconciler reconciler;
        [SerializeField] private BadgeMintService badgeMint;
        [SerializeField] private LoreChronicle chronicle;

        [Header("Configuration")]
        [SerializeField] private bool enableBridge = true;

        private void OnEnable()
        {
            if (!enableBridge) return;

            // Subscribe to gameplay events
            EventBus.OnMissionComplete += HandleMission;
            EventBus.OnVoteCast += HandleVote;
            EventBus.OnTierUpgrade += HandleTier;
            EventBus.OnBadgeMinted += HandleBadgeMint;

            Debug.Log("[BridgeService] Enabled and listening for events");
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            EventBus.OnMissionComplete -= HandleMission;
            EventBus.OnVoteCast -= HandleVote;
            EventBus.OnTierUpgrade -= HandleTier;
            EventBus.OnBadgeMinted -= HandleBadgeMint;

            Debug.Log("[BridgeService] Disabled");
        }

        /// <summary>
        /// Handle mission completion event.
        /// Mints badge and verifies transaction.
        /// </summary>
        private async void HandleMission(string missionId, bool success)
        {
            if (!success || wallet == null || !wallet.IsConnected) return;

            Debug.Log($"[BridgeService] Handling mission: {missionId}");

            try
            {
                // Mint mission badge
                string badgeId = $"mission_{missionId}_badge";
                string tx = await SoulvanMintingAPI.MintBadgeAsync(
                    badgeId,
                    wallet.walletAddress,
                    $"ipfs://soulvan/badges/{badgeId}.json"
                );

                // Verify transaction
                if (reconciler != null)
                {
                    reconciler.Verify(tx, "mission", missionId);
                }

                Debug.Log($"[BridgeService] Mission badge minted: {tx}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BridgeService] Mission handling failed: {e.Message}");
            }
        }

        /// <summary>
        /// Handle DAO vote event.
        /// Records vote on-chain and verifies transaction.
        /// </summary>
        private async void HandleVote(string proposalId, int choice)
        {
            if (wallet == null || !wallet.IsConnected) return;

            Debug.Log($"[BridgeService] Handling vote: {proposalId}, choice: {choice}");

            try
            {
                // Cast vote via wallet
                await wallet.VoteOnProposal(proposalId, choice);

                // Transaction hash would be returned by VoteOnProposal
                string tx = "0x..."; // Stub

                // Verify transaction
                if (reconciler != null)
                {
                    reconciler.Verify(tx, "vote", proposalId);
                }

                Debug.Log($"[BridgeService] Vote cast: {tx}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BridgeService] Vote handling failed: {e.Message}");
            }
        }

        /// <summary>
        /// Handle tier upgrade event.
        /// Mints tier badge and verifies transaction.
        /// </summary>
        private async void HandleTier(int tier)
        {
            if (wallet == null || !wallet.IsConnected) return;

            Debug.Log($"[BridgeService] Handling tier upgrade: {tier}");

            try
            {
                // Mint tier badge (already handled by BadgeMintService)
                // Just verify if needed
                string badgeId = $"tier_{tier}_badge";
                string tx = "0x..."; // Stub - would come from BadgeMintService

                // Verify transaction
                if (reconciler != null)
                {
                    reconciler.Verify(tx, "tier", tier.ToString());
                }

                Debug.Log($"[BridgeService] Tier badge verified: {tx}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BridgeService] Tier handling failed: {e.Message}");
            }
        }

        /// <summary>
        /// Handle badge minted event.
        /// Records badge in Chronicle.
        /// </summary>
        private void HandleBadgeMint(string badgeId, string walletAddress)
        {
            Debug.Log($"[BridgeService] Badge minted: {badgeId} → {walletAddress}");

            // Log badge mint to Chronicle
            if (chronicle != null)
            {
                // Create badge mint lore entry
                var entry = new LoreEntry
                {
                    timestamp = System.DateTime.UtcNow,
                    player = walletAddress,
                    eventType = "badge_mint",
                    data = badgeId
                };

                Debug.Log($"[BridgeService] Badge mint logged to Chronicle");
            }
        }
    }

    /// <summary>
    /// Verifies on-chain transactions, retries failed mints, logs lore entries.
    /// </summary>
    public class Reconciler : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int maxRetries = 3;
        [SerializeField] private float retryDelay = 5f;

        [Header("Stats")]
        public int verificationsSuccessful = 0;
        public int verificationsFailed = 0;
        public int retriesAttempted = 0;

        /// <summary>
        /// Verify transaction confirmation on-chain.
        /// Retries if verification fails.
        /// </summary>
        public async void Verify(string txHash, string type, string id)
        {
            Debug.Log($"[Reconciler] Verifying {type} {id}: {txHash}");

            bool confirmed = await SoulvanChainAPI.VerifyTxAsync(txHash);

            if (!confirmed)
            {
                Debug.LogWarning($"[Reconciler] Verification failed for {type} {id}");
                verificationsFailed++;

                await RetryMint(type, id);
            }
            else
            {
                Debug.Log($"[Reconciler] Verification successful for {type} {id}");
                verificationsSuccessful++;

                LogLoreEntry(type, id, txHash);
            }
        }

        /// <summary>
        /// Retry failed mint operation.
        /// </summary>
        private async Task RetryMint(string type, string id)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                Debug.Log($"[Reconciler] Retry {i + 1}/{maxRetries} for {type} {id}");
                retriesAttempted++;

                await Task.Delay((int)(retryDelay * 1000));

                string badgeId = $"{type}_{id}_badge";
                
                try
                {
                    string tx = await SoulvanMintingAPI.MintBadgeAsync(
                        badgeId,
                        SoulvanWallet.Current?.Address ?? "0x...",
                        $"ipfs://soulvan/badges/{badgeId}.json"
                    );

                    // Recursive verification
                    await VerifyAsync(tx, type, id);
                    return;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Reconciler] Retry {i + 1} failed: {e.Message}");
                }
            }

            Debug.LogError($"[Reconciler] All retries exhausted for {type} {id}");
        }

        private async Task VerifyAsync(string txHash, string type, string id)
        {
            bool confirmed = await SoulvanChainAPI.VerifyTxAsync(txHash);

            if (confirmed)
            {
                verificationsSuccessful++;
                LogLoreEntry(type, id, txHash);
            }
            else
            {
                verificationsFailed++;
            }
        }

        /// <summary>
        /// Log lore entry for verified transaction.
        /// </summary>
        private void LogLoreEntry(string type, string id, string txHash)
        {
            Debug.Log($"[Reconciler] Lore entry logged: {type} {id} → {txHash}");

            // Optional: Write to Chronicle contract
            var chronicle = FindObjectOfType<LoreChronicle>();
            if (chronicle != null)
            {
                // Would call chronicle.LogCustomEntry(type, id, txHash);
            }
        }
    }

    /// <summary>
    /// Static API wrapper for chain verification.
    /// </summary>
    public static class SoulvanChainAPI
    {
        public static async Task<bool> VerifyTxAsync(string txHash)
        {
            // Stub: Query blockchain for tx confirmation
            await Task.Delay(1000); // Simulate network latency

            // Simulate 90% success rate
            bool confirmed = UnityEngine.Random.value > 0.1f;

            Debug.Log($"[SoulvanChainAPI] Tx verification: {txHash} → {confirmed}");

            return confirmed;
        }
    }

    /// <summary>
    /// Stub for SoulvanWallet singleton access.
    /// </summary>
    public static class SoulvanWallet
    {
        public static WalletController Current => Object.FindObjectOfType<WalletController>();
    }
}
