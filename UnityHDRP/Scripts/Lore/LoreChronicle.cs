using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Soulvan.Wallet;

namespace Soulvan.Lore
{
    /// <summary>
    /// Lore Chronicle system for logging saga entries on-chain.
    /// Records mission completions, badge mints, DAO votes, and contributor actions.
    /// Integrates with SoulvanChronicle smart contract for permanent lore storage.
    /// Extended with Soulvan Boss tribute tracking.
    /// </summary>
    public class LoreChronicle : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private WalletController walletController;
        [SerializeField] private bool logToBlockchain = true;
        [SerializeField] private bool cacheOffChain = true;

        [Header("Lore Stats")]
        public int missionsLogged = 0;
        public int tierUpgradesLogged = 0;
        public int votesLogged = 0;

        private List<LoreEntry> offChainCache = new List<LoreEntry>();

        private void Awake()
        {
            if (walletController == null)
            {
                walletController = FindObjectOfType<WalletController>();
            }
        }

        /// <summary>
        /// Log mission completion to Chronicle.
        /// </summary>
        public async void LogMission(string missionId, string walletAddress)
        {
            Debug.Log($"[LoreChronicle] Mission {missionId} completed by {walletAddress}");

            LoreEntry entry = new LoreEntry
            {
                timestamp = DateTime.UtcNow,
                player = walletAddress,
                eventType = "mission_complete",
                data = missionId
            };

            if (cacheOffChain)
            {
                offChainCache.Add(entry);
            }

            if (logToBlockchain && walletController != null && walletController.IsConnected)
            {
                await LogToBlockchain(entry);
            }

            missionsLogged++;
        }

        /// <summary>
        /// Log tier upgrade to Chronicle.
        /// </summary>
        public async void LogTierUpgrade(int tier, string walletAddress)
        {
            Debug.Log($"[LoreChronicle] Tier {tier} unlocked by {walletAddress}");

            LoreEntry entry = new LoreEntry
            {
                timestamp = DateTime.UtcNow,
                player = walletAddress,
                eventType = "tier_upgrade",
                data = tier.ToString()
            };

            if (cacheOffChain)
            {
                offChainCache.Add(entry);
            }

            if (logToBlockchain && walletController != null && walletController.IsConnected)
            {
                await LogToBlockchain(entry);
            }

            tierUpgradesLogged++;
        }

        /// <summary>
        /// Log DAO vote to Chronicle.
        /// </summary>
        public async void LogVote(string proposalId, int choice, string walletAddress)
        {
            Debug.Log($"[LoreChronicle] Vote {choice} on {proposalId} by {walletAddress}");

            LoreEntry entry = new LoreEntry
            {
                timestamp = DateTime.UtcNow,
                player = walletAddress,
                eventType = "dao_vote",
                data = $"{proposalId}:{choice}"
            };

            if (cacheOffChain)
            {
                offChainCache.Add(entry);
            }

            if (logToBlockchain && walletController != null && walletController.IsConnected)
            {
                await LogToBlockchain(entry);
            }

            votesLogged++;
        }

        /// <summary>
        /// Log boss defeat to Chronicle.
        /// </summary>
        public async void LogBossDefeat(string bossId, string walletAddress)
        {
            Debug.Log($"[LoreChronicle] Boss {bossId} defeated by {walletAddress}");

            LoreEntry entry = new LoreEntry
            {
                timestamp = DateTime.UtcNow,
                player = walletAddress,
                eventType = "boss_defeat",
                data = bossId
            };

            if (cacheOffChain)
            {
                offChainCache.Add(entry);
            }

            if (logToBlockchain && walletController != null && walletController.IsConnected)
            {
                await LogToBlockchain(entry);
            }
        }

        /// <summary>
        /// Write lore entry to SoulvanChronicle contract.
        /// </summary>
        private async Task LogToBlockchain(LoreEntry entry)
        {
            try
            {
                await SoulvanChronicleAPI.LogEntryAsync(
                    entry.player,
                    entry.eventType,
                    entry.data
                );

                Debug.Log($"[LoreChronicle] Logged to blockchain: {entry.eventType} - {entry.data}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoreChronicle] Failed to log to blockchain: {e.Message}");
            }
        }

        /// <summary>
        /// Get all cached lore entries for player.
        /// </summary>
        public List<LoreEntry> GetPlayerLore(string walletAddress)
        {
            return offChainCache.FindAll(e => e.player == walletAddress);
        }

        /// <summary>
        /// Export saga chapter for DAO.
        /// Packages recent lore entries into narrative format.
        /// </summary>
        public string ExportSagaChapter(string walletAddress, int chapterNumber)
        {
            var playerLore = GetPlayerLore(walletAddress);
            
            string saga = $"=== Saga Chapter {chapterNumber}: {walletAddress} ===\n\n";
            
            foreach (var entry in playerLore)
            {
                saga += $"[{entry.timestamp:yyyy-MM-dd HH:mm:ss}] {entry.eventType}: {entry.data}\n";
            }

            Debug.Log($"[LoreChronicle] Exported saga chapter {chapterNumber}");
            
            return saga;
        }

        /// <summary>
        /// Clear off-chain cache (for testing or memory management).
        /// </summary>
        public void ClearCache()
        {
            offChainCache.Clear();
            Debug.Log("[LoreChronicle] Off-chain cache cleared");
        }

        /// <summary>
        /// Record Soulvan's boss cut from mission.
        /// </summary>
        public async Task RecordBossCut(string bossName, string missionName, float cut, string operative)
        {
            string loreText = $"🏆 {bossName} receives {cut:F2} SVN tribute from mission '{missionName}' completed by {operative}";
            
            await LogLore(loreText, "BossCut");
            
            Debug.Log($"[LoreChronicle] Boss cut recorded: {bossName} -> {cut:F2} SVN from {missionName}");
        }
    }

    /// <summary>
    /// Lore entry data structure.
    /// </summary>
    [Serializable]
    public class LoreEntry
    {
        public DateTime timestamp;
        public string player;
        public string eventType;
        public string data;
    }

    /// <summary>
    /// Static API wrapper for SoulvanChronicle contract.
    /// </summary>
    public static class SoulvanChronicleAPI
    {
        public static async Task LogEntryAsync(string player, string eventType, string data)
        {
            // Stub: Call SoulvanChronicle.logEntry() via Web3 provider
            await Task.Delay(300); // Simulate network latency
            
            Debug.Log($"[SoulvanChronicleAPI] Entry logged: {player} - {eventType} - {data}");
        }

        public static async Task<LoreEntry[]> GetPlayerEntriesAsync(string player)
        {
            // Stub: Query Chronicle entries for player
            await Task.Delay(300);
            
            return new LoreEntry[0];
        }
    }
}
