using UnityEngine;
using System.Collections.Generic;

namespace Soulvan.Systems
{
    /// <summary>
    /// Reward types for blockchain integration.
    /// </summary>
    public enum RewardType
    {
        SVNCoin,
        CarSkinNFT,
        SeasonalBadge,
        MissionRelic,
        GovernanceToken,
        CinematicReplay
    }

    [System.Serializable]
    public class RewardData
    {
        public RewardType type;
        public string rewardId;
        public float amount;
        public string metadata; // IPFS/Arweave hash
    }

    /// <summary>
    /// Integrates with blockchain contracts for minting rewards.
    /// Connects to deployed SoulvanCoin, CarSkin NFT, and MissionRegistry contracts.
    /// </summary>
    public class RewardService : MonoBehaviour
    {
        [Header("Blockchain Integration")]
        [SerializeField] private string rpcUrl = "http://localhost:8545";
        [SerializeField] private string coinContractAddress;
        [SerializeField] private string nftContractAddress;
        [SerializeField] private string missionRegistryAddress;

        [Header("Reward Multipliers")]
        [SerializeField] private float racingMultiplier = 1.0f;
        [SerializeField] private float missionMultiplier = 1.5f;
        [SerializeField] private float bossMultiplier = 2.5f;

        private Queue<RewardData> pendingRewards = new Queue<RewardData>();

        /// <summary>
        /// Grant seasonal badge NFT to player wallet.
        /// </summary>
        public void GrantSeasonalBadge(string badgeId, string walletAddress)
        {
            Debug.Log($"[RewardService] Granting badge '{badgeId}' to {walletAddress}");
            
            var reward = new RewardData
            {
                type = RewardType.SeasonalBadge,
                rewardId = badgeId,
                amount = 1f,
                metadata = $"ipfs://badge-{badgeId}"
            };

            pendingRewards.Enqueue(reward);
            ProcessRewards(walletAddress);
        }

        /// <summary>
        /// Mint SVN tokens for mission completion.
        /// </summary>
        public void MintSVNReward(string walletAddress, float baseAmount, RewardType context)
        {
            float multiplier = context switch
            {
                RewardType.SVNCoin => racingMultiplier,
                RewardType.MissionRelic => missionMultiplier,
                RewardType.GovernanceToken => bossMultiplier,
                _ => 1f
            };

            float finalAmount = baseAmount * multiplier;
            Debug.Log($"[RewardService] Minting {finalAmount} SVN to {walletAddress}");

            // TODO: Integrate with ethers.js / web3.js to call coin.mint()
            // Example pseudo-code:
            // await coinContract.mint(walletAddress, ethers.parseEther(finalAmount.ToString()));
        }

        /// <summary>
        /// Mint car skin NFT.
        /// </summary>
        public void MintCarSkin(string walletAddress, string metadataURI)
        {
            Debug.Log($"[RewardService] Minting car skin NFT to {walletAddress}: {metadataURI}");
            
            // TODO: Call nftContract.mint(walletAddress, metadataURI)
        }

        /// <summary>
        /// Record mission completion on-chain via MissionRegistry.
        /// </summary>
        public void CompleteMissionOnChain(int missionId, string playerAddress, string resultHash)
        {
            Debug.Log($"[RewardService] Recording mission {missionId} completion for {playerAddress}");
            
            // TODO: Call missionRegistry.completeMission(missionId, playerAddress, resultHash)
        }

        /// <summary>
        /// Process pending reward queue (batched for gas efficiency).
        /// </summary>
        private void ProcessRewards(string walletAddress)
        {
            if (pendingRewards.Count == 0) return;

            Debug.Log($"[RewardService] Processing {pendingRewards.Count} pending rewards");
            
            while (pendingRewards.Count > 0)
            {
                var reward = pendingRewards.Dequeue();
                // Send to blockchain via smart contract calls
                // For now, just logging
                Debug.Log($" - {reward.type}: {reward.rewardId} x{reward.amount}");
            }
        }

        /// <summary>
        /// Get player's SVN balance from blockchain.
        /// </summary>
        public async System.Threading.Tasks.Task<float> GetSVNBalance(string walletAddress)
        {
            // TODO: Query coinContract.balanceOf(walletAddress)
            await System.Threading.Tasks.Task.Delay(100); // Simulate async call
            return 1000f; // Stub
        }

        /// <summary>
        /// Check if player owns specific NFT.
        /// </summary>
        public async System.Threading.Tasks.Task<bool> OwnsCarSkin(string walletAddress, int tokenId)
        {
            // TODO: Query nftContract.ownerOf(tokenId) == walletAddress
            await System.Threading.Tasks.Task.Delay(100);
            return false; // Stub
        }
    }
}
