using System;
using System.Threading.Tasks;
using UnityEngine;
using Soulvan.Systems;

namespace Soulvan.Wallet
{
    /// <summary>
    /// Main wallet controller integrating blockchain operations with game systems.
    /// Handles rewards, governance, avatar rendering, and motif overlays.
    /// </summary>
    public class WalletController : MonoBehaviour
    {
        [Header("Wallet Configuration")]
        [SerializeField] private string defaultRpcUrl = "http://localhost:8545";
        [SerializeField] private int chainId = 31337; // Hardhat local
        [SerializeField] private bool useHardwareWallet = false;

        [Header("Game Integration")]
        [SerializeField] private MotifAPI motifs;
        [SerializeField] private AvatarRenderer avatar;
        [SerializeField] private RewardService rewardService;

        [Header("Contract Addresses")]
        [SerializeField] private string soulvanCoinAddress;
        [SerializeField] private string carSkinAddress;
        [SerializeField] private string chronicleAddress;
        [SerializeField] private string governanceAddress;
        [SerializeField] private string seasonManagerAddress;
        [SerializeField] private string missionRegistryAddress;

        [Header("UI References")]
        [SerializeField] private WalletHUD walletHUD;
        [SerializeField] private GovernancePanel governancePanel;
        [SerializeField] private AssetPanel assetPanel;

        public ISoulvanWallet Wallet { get; private set; }
        public bool IsConnected => Wallet != null && Wallet.IsUnlocked;

        private OffChainCache offChainCache;
        private ReconcilerService reconciler;

        private void Start()
        {
            InitializeWallet();
            InitializeServices();
            
            EventBus.OnMissionCompleted += OnMissionCompleted;
            EventBus.OnBossDefeated += OnBossDefeated;
            EventBus.OnSeasonChanged += OnSeasonChanged;
        }

        private void OnDestroy()
        {
            EventBus.OnMissionCompleted -= OnMissionCompleted;
            EventBus.OnBossDefeated -= OnBossDefeated;
            EventBus.OnSeasonChanged -= OnSeasonChanged;
        }

        #region Initialization

        private async void InitializeWallet()
        {
            // Create wallet instance
            if (useHardwareWallet)
            {
                Wallet = new HardwareWallet();
            }
            else
            {
                Wallet = new SoftwareWallet(defaultRpcUrl, chainId);
            }

            // Try to auto-unlock with session passphrase
            string sessionPass = PlayerPrefs.GetString("SESSION_PASS", "");
            if (!string.IsNullOrEmpty(sessionPass))
            {
                bool unlocked = await Wallet.UnlockAsync(sessionPass);
                if (unlocked)
                {
                    await OnWalletUnlocked();
                }
            }

            Debug.Log($"[WalletController] Initialized - Address: {Wallet.Address ?? "Locked"}");
        }

        private void InitializeServices()
        {
            offChainCache = new OffChainCache();
            reconciler = new ReconcilerService(Wallet, offChainCache);
            
            // Start reconciler loop
            InvokeRepeating(nameof(ReconcileOffChainState), 30f, 30f);
        }

        private async Task OnWalletUnlocked()
        {
            // Load balances and NFTs
            var balances = await Wallet.GetBalancesAsync();
            UpdateHUD(balances);

            // Update avatar with wallet identity
            UpdateAvatar();

            // Load proposals
            if (governancePanel != null)
            {
                var proposals = await Wallet.GetProposalsAsync();
                governancePanel.DisplayProposals(proposals);
            }

            // Load assets
            if (assetPanel != null)
            {
                var nfts = await Wallet.GetNftsAsync();
                assetPanel.DisplayAssets(nfts);
            }

            Debug.Log($"[WalletController] Wallet unlocked: {Wallet.Address}");
        }

        #endregion

        #region Public API

        public async Task<bool> UnlockWallet(string passphrase)
        {
            bool success = await Wallet.UnlockAsync(passphrase);
            
            if (success)
            {
                PlayerPrefs.SetString("SESSION_PASS", passphrase);
                await OnWalletUnlocked();
            }
            
            return success;
        }

        public async Task LockWallet()
        {
            await Wallet.LockAsync();
            PlayerPrefs.DeleteKey("SESSION_PASS");
            
            if (walletHUD != null)
                walletHUD.ShowLocked();
            
            Debug.Log("[WalletController] Wallet locked");
        }

        public async Task ClaimReward(string rewardId, string metadataUri)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[WalletController] Wallet not connected");
                return;
            }

            // Cache reward off-chain for immediate feedback
            offChainCache.AddPendingReward(rewardId, metadataUri);
            
            // Show claim animation
            ShowClaimAnimation(rewardId);
            
            try
            {
                // Mint NFT on-chain
                string txHash = await Wallet.MintNftAsync(metadataUri);
                
                // Log to chronicle
                await LogChronicleEntry("reward_claim", txHash);
                
                // Apply Calm motif overlay
                if (motifs != null)
                    motifs.SetMotif(Motif.Calm, 0.8f);
                
                // Update UI
                var balances = await Wallet.GetBalancesAsync();
                UpdateHUD(balances);
                
                Debug.Log($"[WalletController] Reward claimed: {rewardId}, tx: {txHash}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WalletController] Claim failed: {e.Message}");
                offChainCache.RemovePendingReward(rewardId);
            }
        }

        public async Task VoteOnProposal(string proposalId, int choice)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[WalletController] Wallet not connected");
                return;
            }

            try
            {
                // Show cinematic vote ritual
                ShowVoteRitual();
                
                // Cast vote on-chain
                string txHash = await Wallet.CastVoteAsync(proposalId, choice);
                
                // Log to chronicle
                await LogChronicleEntry("dao_vote", txHash);
                
                // Apply Oracle motif
                if (motifs != null)
                    motifs.SetMotif(Motif.Oracle, 1f);
                
                Debug.Log($"[WalletController] Vote cast: {proposalId}, choice: {choice}, tx: {txHash}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WalletController] Vote failed: {e.Message}");
            }
        }

        public void UpdateIdentityLevel(int tier)
        {
            StartCoroutine(UpdateIdentityLevelAsync(tier));
        }

        private async System.Collections.IEnumerator UpdateIdentityLevelAsync(int tier)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[WalletController] Wallet not connected");
                yield break;
            }

            await Wallet.UpdateIdentityLevelAsync(tier);
            Debug.Log($"[WalletController] Identity level updated to tier {tier}");
        }

        public void RecordProgressionMilestone(int tier, int missions, int bosses)
        {
            StartCoroutine(RecordProgressionMilestoneAsync(tier, missions, bosses));
        }

        private async System.Collections.IEnumerator RecordProgressionMilestoneAsync(int tier, int missions, int bosses)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[WalletController] Wallet not connected");
                yield break;
            }

            await Wallet.RecordMilestoneAsync(tier, missions, bosses);
            Debug.Log($"[WalletController] Progression milestone recorded on blockchain");
        }
                ShowVoteRitual(proposalId, choice);
                
                // Cast vote on-chain
                string txHash = await Wallet.VoteAsync(proposalId, choice);
                
                // Log to chronicle
                await LogChronicleEntry("governance_vote", txHash);
                
                // Apply Oracle motif overlay
                if (motifs != null)
                    motifs.SetMotif(Motif.Oracle, 1.0f);
                
                // Update avatar rune
                UpdateAvatar();
                
                Debug.Log($"[WalletController] Vote cast: {proposalId}, choice: {choice}, tx: {txHash}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WalletController] Vote failed: {e.Message}");
            }
        }

        #endregion

        #region Game Event Handlers

        private async void OnMissionCompleted(string missionId)
        {
            Debug.Log($"[WalletController] Mission completed: {missionId}");
            
            if (!IsConnected) return;

            // Queue reward mint
            string metadataUri = $"https://soulvan.io/nft/mission/{missionId}";
            await ClaimReward(missionId, metadataUri);
        }

        private async void OnBossDefeated(string bossName)
        {
            Debug.Log($"[WalletController] Boss defeated: {bossName}");
            
            if (!IsConnected) return;

            // Mint boss trophy NFT
            string metadataUri = $"https://soulvan.io/nft/boss/{bossName}";
            await ClaimReward($"boss_{bossName}", metadataUri);
            
            // Apply Cosmic motif
            if (motifs != null)
                motifs.SetMotif(Motif.Cosmic, 1.0f);
        }

        private void OnSeasonChanged(int newSeason)
        {
            Debug.Log($"[WalletController] Season changed: {newSeason}");
            
            // Update avatar for new season
            UpdateAvatar();
        }

        #endregion

        #region UI Updates

        private void UpdateHUD(BalanceState balances)
        {
            if (walletHUD != null)
            {
                walletHUD.UpdateBalances(balances);
            }

            // Apply motif intensity based on progression
            if (motifs != null)
            {
                float intensity = Mathf.Clamp01(balances.votingPower / 1000f);
                motifs.UpdateIntensity(intensity);
            }
        }

        private void UpdateAvatar()
        {
            if (avatar == null || !IsConnected) return;

            // Derive intensity from wallet milestones
            float intensity = 0.5f; // Stub: calculate from balances/NFTs/votes
            avatar.RenderForAddress(Wallet.Address, intensity);
        }

        private void ShowClaimAnimation(string rewardId)
        {
            // Cinematic reward claim animation
            Debug.Log($"[WalletController] Showing claim animation for {rewardId}");
            // Stub: trigger particle effects, audio, haptics
        }

        private void ShowVoteRitual(string proposalId, int choice)
        {
            // Cinematic vote ritual with oracle runes + haptics
            Debug.Log($"[WalletController] Showing vote ritual for {proposalId}, choice {choice}");
            // Stub: trigger Niagara effects, sacred geometry, audio chants
        }

        #endregion

        #region Off-Chain Reconciliation

        private async void ReconcileOffChainState()
        {
            if (!IsConnected) return;

            try
            {
                await reconciler.ReconcileAsync();
                Debug.Log("[WalletController] Off-chain state reconciled");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WalletController] Reconciliation failed: {e.Message}");
            }
        }

        #endregion

        #region Chronicle Integration

        private async Task LogChronicleEntry(string entryType, string txHash)
        {
            // Stub: Log to Chronicle contract
            Debug.Log($"[WalletController] Chronicle entry: {entryType}, tx: {txHash}");
            await Task.Delay(100);
        }

        #endregion

        #region Stubs (Implementations)

        // These classes would be fully implemented in production
        private class SoftwareWallet : ISoulvanWallet
        {
            private string rpcUrl;
            private int chainId;
            private string address;
            private bool unlocked;

            public string Address => address;
            public bool IsUnlocked => unlocked;

            public SoftwareWallet(string rpcUrl, int chainId)
            {
                this.rpcUrl = rpcUrl;
                this.chainId = chainId;
            }

            public async Task<bool> UnlockAsync(string passphrase)
            {
                // Stub: Decrypt keystore with passphrase
                await Task.Delay(100);
                address = "0x1234...5678"; // Stub address
                unlocked = true;
                return true;
            }

            public async Task LockAsync()
            {
                unlocked = false;
                await Task.CompletedTask;
            }

            public async Task<string> SignMessageAsync(string message)
            {
                await Task.Delay(50);
                return "0xabcd...signature";
            }

            public async Task<string> SignTransactionAsync(TransactionRequest tx)
            {
                await Task.Delay(50);
                return "0xabcd...signature";
            }

            public async Task<string> SendAsync(string to, decimal amount, decimal maxFee)
            {
                await Task.Delay(100);
                return "0xtxhash...";
            }

            public async Task<BalanceState> GetBalancesAsync()
            {
                await Task.Delay(100);
                return new BalanceState
                {
                    soulvanCoin = 1000m,
                    eth = 0.5m,
                    nftCount = 5,
                    badgeCount = 2,
                    votingPower = 100
                };
            }

            public async Task UpdateIdentityLevelAsync(int tier)
            {
                await Task.Delay(50);
                Debug.Log($"[WalletProvider] Updated identity level to tier {tier}");
            }

            public async Task RecordMilestoneAsync(int tier, int missions, int bosses)
            {
                await Task.Delay(50);
                Debug.Log($"[WalletProvider] Recorded milestone: Tier {tier}, {missions} missions, {bosses} bosses");
            }

            public async Task<string> MintNftAsync(string metadataUri)
            {
                await Task.Delay(200);
                return "0xminttx...";
            }

            public async Task<string> TransferNftAsync(string tokenId, string to)
            {
                await Task.Delay(200);
                return "0xtransfertx...";
            }

            public async Task<Nft[]> GetNftsAsync()
            {
                await Task.Delay(100);
                return new Nft[0];
            }

            public async Task<string> VoteAsync(string proposalId, int choice)
            {
                await Task.Delay(200);
                return "0xvotetx...";
            }

            public async Task<string> ProposeAsync(string description, byte[] calldata)
            {
                await Task.Delay(200);
                return "0xproposetx...";
            }

            public async Task<Proposal[]> GetProposalsAsync()
            {
                await Task.Delay(100);
                return new Proposal[0];
            }

            public async Task<ChronicleEntry[]> GetChronicleEntriesAsync()
            {
                await Task.Delay(100);
                return new ChronicleEntry[0];
            }

            public async Task<bool> ExportSeedAsync(string outputPath)
            {
                await Task.Delay(100);
                return true;
            }

            public async Task<bool> ChangePassphraseAsync(string oldPass, string newPass)
            {
                await Task.Delay(100);
                return true;
            }
        }

        private class HardwareWallet : ISoulvanWallet
        {
            // Stub: Ledger/Trezor integration
            public string Address => "0xhardware...";
            public bool IsUnlocked => true; // Hardware wallets handle their own unlocking

            public Task<bool> UnlockAsync(string passphrase) => Task.FromResult(true);
            public Task LockAsync() => Task.CompletedTask;
            public Task<string> SignMessageAsync(string message) => Task.FromResult("0xhwsig...");
            public Task<string> SignTransactionAsync(TransactionRequest tx) => Task.FromResult("0xhwsig...");
            public Task<string> SendAsync(string to, decimal amount, decimal maxFee) => Task.FromResult("0xhwtx...");
            public Task<BalanceState> GetBalancesAsync() => Task.FromResult(new BalanceState());
            public Task<string> MintNftAsync(string metadataUri) => Task.FromResult("0xhwminttx...");
            public Task<string> TransferNftAsync(string tokenId, string to) => Task.FromResult("0xhwtransfertx...");
            public Task<Nft[]> GetNftsAsync() => Task.FromResult(new Nft[0]);
            public Task<string> VoteAsync(string proposalId, int choice) => Task.FromResult("0xhwvotetx...");
            public Task<string> ProposeAsync(string description, byte[] calldata) => Task.FromResult("0xhwproposetx...");
            public Task<Proposal[]> GetProposalsAsync() => Task.FromResult(new Proposal[0]);
            public Task<ChronicleEntry[]> GetChronicleEntriesAsync() => Task.FromResult(new ChronicleEntry[0]);
            public Task<bool> ExportSeedAsync(string outputPath) => Task.FromResult(false); // Hardware wallets don't export seeds
            public Task<bool> ChangePassphraseAsync(string oldPass, string newPass) => Task.FromResult(false);
        }

        #endregion
    }
}
