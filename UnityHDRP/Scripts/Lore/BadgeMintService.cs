using System.Threading.Tasks;
using UnityEngine;
using Soulvan.Wallet;

namespace Soulvan.Lore
{
    /// <summary>
    /// Service for minting tier badges and boss trophies.
    /// Integrates with WalletController for async NFT minting.
    /// </summary>
    public class BadgeMintService : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string baseMetadataUri = "ipfs://soulvan/badges/";
        [SerializeField] private WalletController walletController;

        [Header("Badge Stats")]
        public int tierBadgesMinted = 0;
        public int bossBadgesMinted = 0;

        private void Awake()
        {
            if (walletController == null)
            {
                walletController = FindObjectOfType<WalletController>();
            }
        }

        /// <summary>
        /// Mint tier badge NFT for tier advancement.
        /// Badges: tier_1_badge, tier_2_badge, etc.
        /// </summary>
        public async void MintTierBadge(int tier, string walletAddress)
        {
            if (walletController == null || !walletController.IsConnected)
            {
                Debug.LogWarning("[BadgeMintService] Wallet not connected");
                return;
            }

            string badgeId = $"tier_{tier}_badge";
            string metadataUri = $"{baseMetadataUri}{badgeId}.json";

            Debug.Log($"[BadgeMintService] Minting tier badge: {badgeId} for {walletAddress}");

            try
            {
                await SoulvanMintingAPI.MintBadgeAsync(badgeId, walletAddress, metadataUri);
                tierBadgesMinted++;
                
                Debug.Log($"[BadgeMintService] Tier badge minted successfully: {badgeId}");
                
                // Emit badge minted event
                EventBus.EmitBadgeMinted(badgeId, walletAddress);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BadgeMintService] Failed to mint tier badge: {e.Message}");
            }
        }

        /// <summary>
        /// Mint boss trophy NFT for boss battle victories.
        /// Trophies: boss_flame_samurai_trophy, boss_shadow_racer_trophy, etc.
        /// </summary>
        public async void MintBossBadge(string bossId, string walletAddress)
        {
            if (walletController == null || !walletController.IsConnected)
            {
                Debug.LogWarning("[BadgeMintService] Wallet not connected");
                return;
            }

            string badgeId = $"boss_{bossId}_trophy";
            string metadataUri = $"{baseMetadataUri}{badgeId}.json";

            Debug.Log($"[BadgeMintService] Minting boss badge: {badgeId} for {walletAddress}");

            try
            {
                await SoulvanMintingAPI.MintBadgeAsync(badgeId, walletAddress, metadataUri);
                bossBadgesMinted++;
                
                Debug.Log($"[BadgeMintService] Boss badge minted successfully: {badgeId}");
                
                // Emit badge minted event
                EventBus.EmitBadgeMinted(badgeId, walletAddress);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BadgeMintService] Failed to mint boss badge: {e.Message}");
            }
        }

        /// <summary>
        /// Mint special event badge (seasonal, DAO milestone, etc.)
        /// </summary>
        public async void MintEventBadge(string eventId, string walletAddress)
        {
            if (walletController == null || !walletController.IsConnected)
            {
                Debug.LogWarning("[BadgeMintService] Wallet not connected");
                return;
            }

            string badgeId = $"event_{eventId}_badge";
            string metadataUri = $"{baseMetadataUri}{badgeId}.json";

            Debug.Log($"[BadgeMintService] Minting event badge: {badgeId} for {walletAddress}");

            try
            {
                await SoulvanMintingAPI.MintBadgeAsync(badgeId, walletAddress, metadataUri);
                
                Debug.Log($"[BadgeMintService] Event badge minted successfully: {badgeId}");
                
                EventBus.EmitBadgeMinted(badgeId, walletAddress);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BadgeMintService] Failed to mint event badge: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Static API wrapper for Soulvan badge minting.
    /// Routes to SoulvanCarSkin contract (ERC-721).
    /// </summary>
    public static class SoulvanMintingAPI
    {
        public static async Task MintBadgeAsync(string badgeId, string walletAddress, string metadataUri)
        {
            // Stub: Call SoulvanCarSkin.safeMint() via Web3 provider
            await Task.Delay(500); // Simulate network latency
            
            Debug.Log($"[SoulvanMintingAPI] Badge minted: {badgeId} → {walletAddress}");
            Debug.Log($"[SoulvanMintingAPI] Metadata: {metadataUri}");
        }

        public static async Task MintReplayNftAsync(string metadata, string walletAddress)
        {
            // Stub: Mint replay NFT with encoded gameplay data
            await Task.Delay(500);
            
            Debug.Log($"[SoulvanMintingAPI] Replay NFT minted: {metadata} → {walletAddress}");
        }

        public static async Task MintLoreAsync(string lore, string walletAddress)
        {
            // Stub: Mint lore chapter NFT
            await Task.Delay(500);
            
            Debug.Log($"[SoulvanMintingAPI] Lore chapter minted: {lore} → {walletAddress}");
        }
    }

    /// <summary>
    /// Extended EventBus with badge minting events.
    /// </summary>
    public static partial class EventBus
    {
        public static event System.Action<string, string> OnBadgeMinted;

        public static void EmitBadgeMinted(string badgeId, string walletAddress)
        {
            OnBadgeMinted?.Invoke(badgeId, walletAddress);
            Debug.Log($"[EventBus] Badge minted emitted: {badgeId} → {walletAddress}");
        }
    }
}
