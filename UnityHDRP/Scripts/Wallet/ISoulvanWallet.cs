using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Soulvan.Wallet
{
    /// <summary>
    /// Soulvan Wallet interface for non-custodial blockchain operations.
    /// Supports SoulvanCoin, NFTs, governance voting, and chronicle integration.
    /// </summary>
    public interface ISoulvanWallet
    {
        string Address { get; }
        bool IsUnlocked { get; }
        
        Task<bool> UnlockAsync(string passphrase);
        Task LockAsync();
        Task<string> SignMessageAsync(string message);
        Task<string> SignTransactionAsync(TransactionRequest tx);
        
        // Token operations
        Task<string> SendAsync(string to, decimal amount, decimal maxFee);
        Task<BalanceState> GetBalancesAsync();
        
        // NFT operations
        Task<string> MintNftAsync(string metadataUri);
        Task<string> TransferNftAsync(string tokenId, string to);
        Task<Nft[]> GetNftsAsync();
        
        // Governance
        Task<string> VoteAsync(string proposalId, int choice);
        Task<string> ProposeAsync(string description, bytes calldata);
        Task<Proposal[]> GetProposalsAsync();
        
        // Chronicle
        Task<ChronicleEntry[]> GetChronicleEntriesAsync();
        
        // Security
        Task<bool> ExportSeedAsync(string outputPath);
        Task<bool> ChangePassphraseAsync(string oldPass, string newPass);
    }

    [Serializable]
    public class TransactionRequest
    {
        public string to;
        public decimal value;
        public string data;
        public decimal maxFee;
        public int chainId;
    }

    [Serializable]
    public class BalanceState
    {
        public decimal soulvanCoin;
        public decimal eth;
        public int nftCount;
        public int badgeCount;
        public int votingPower;
    }

    [Serializable]
    public class Nft
    {
        public string tokenId;
        public string name;
        public string description;
        public string imageUri;
        public string rarity;
        public int seasonChapter;
        public NftType type;
    }

    [Serializable]
    public class Proposal
    {
        public string id;
        public string description;
        public int forVotes;
        public int againstVotes;
        public int abstainVotes;
        public ProposalState state;
        public uint deadline;
    }

    [Serializable]
    public class ChronicleEntry
    {
        public uint index;
        public uint timestamp;
        public uint entryType; // 0=Race, 1=Mission, 2=Governance, etc.
        public string data;
        public string wallet;
    }

    public enum NftType
    {
        CarSkin,
        Relic,
        ReplayToken,
        SeasonalBadge,
        BossTrophy
    }

    public enum ProposalState
    {
        Pending,
        Active,
        Canceled,
        Defeated,
        Succeeded,
        Queued,
        Expired,
        Executed
    }
}
