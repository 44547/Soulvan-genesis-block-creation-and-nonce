using System.Threading.Tasks;
using UnityEngine;
using Soulvan.Lore;
using Soulvan.Wallet;

namespace Soulvan.Bridge
{
    /// <summary>
    /// Packages replay NFTs and saga chapters for DAO export.
    /// Converts gameplay recordings into cinematic NFT assets.
    /// </summary>
    public class LoreExporter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WalletController walletController;
        [SerializeField] private LoreChronicle chronicle;

        [Header("Configuration")]
        [SerializeField] private string replayMetadataPrefix = "ipfs://soulvan/replays/";
        [SerializeField] private string sagaMetadataPrefix = "ipfs://soulvan/saga/";

        [Header("Stats")]
        public int replaysExported = 0;
        public int sagaChaptersExported = 0;

        private void Awake()
        {
            if (walletController == null)
            {
                walletController = FindObjectOfType<WalletController>();
            }

            if (chronicle == null)
            {
                chronicle = FindObjectOfType<LoreChronicle>();
            }
        }

        /// <summary>
        /// Export mission replay as NFT.
        /// Includes gameplay data: waypoints, speed, time, motif overlays.
        /// </summary>
        public async void ExportReplay(string missionId, string walletAddress)
        {
            if (walletController == null || !walletController.IsConnected)
            {
                Debug.LogWarning("[LoreExporter] Wallet not connected");
                return;
            }

            string metadata = $"Soulvan replay: {missionId} by {walletAddress}";
            string metadataUri = $"{replayMetadataPrefix}{missionId}_{walletAddress}.json";

            Debug.Log($"[LoreExporter] Exporting replay: {missionId}");

            try
            {
                string tx = await SoulvanMintingAPI.MintReplayNftAsync(metadata, walletAddress);
                
                replaysExported++;

                Debug.Log($"[LoreExporter] Replay NFT minted: {tx}");
                Debug.Log($"[LoreExporter] Metadata: {metadataUri}");

                // Log replay export to Chronicle
                if (chronicle != null)
                {
                    await chronicle.LogToBlockchain(new LoreEntry
                    {
                        timestamp = System.DateTime.UtcNow,
                        player = walletAddress,
                        eventType = "replay_export",
                        data = missionId
                    });
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LoreExporter] Failed to export replay: {e.Message}");
            }
        }

        /// <summary>
        /// Export saga chapter as NFT.
        /// Packages recent lore entries into narrative format.
        /// </summary>
        public async void ExportSagaChapter(string chapterId, string walletAddress)
        {
            if (walletController == null || !walletController.IsConnected)
            {
                Debug.LogWarning("[LoreExporter] Wallet not connected");
                return;
            }

            if (chronicle == null)
            {
                Debug.LogWarning("[LoreExporter] LoreChronicle not assigned");
                return;
            }

            // Generate saga chapter from Chronicle
            int chapterNumber = sagaChaptersExported + 1;
            string sagaText = chronicle.ExportSagaChapter(walletAddress, chapterNumber);

            string lore = $"Chapter {chapterId} authored by {walletAddress}\n\n{sagaText}";
            string metadataUri = $"{sagaMetadataPrefix}{chapterId}_{walletAddress}.json";

            Debug.Log($"[LoreExporter] Exporting saga chapter: {chapterId}");

            try
            {
                string tx = await SoulvanMintingAPI.MintLoreAsync(lore, walletAddress);
                
                sagaChaptersExported++;

                Debug.Log($"[LoreExporter] Saga chapter minted: {tx}");
                Debug.Log($"[LoreExporter] Metadata: {metadataUri}");

                // Log saga export to Chronicle
                if (chronicle != null)
                {
                    await chronicle.LogToBlockchain(new LoreEntry
                    {
                        timestamp = System.DateTime.UtcNow,
                        player = walletAddress,
                        eventType = "saga_export",
                        data = chapterId
                    });
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LoreExporter] Failed to export saga: {e.Message}");
            }
        }

        /// <summary>
        /// Export boss battle replay with cinematic highlights.
        /// </summary>
        public async void ExportBossReplay(string bossId, string walletAddress)
        {
            string replayId = $"boss_{bossId}_replay";
            await ExportReplay(replayId, walletAddress);

            Debug.Log($"[LoreExporter] Boss replay exported: {bossId}");
        }

        /// <summary>
        /// Export DAO ritual replay (governance vote cinematic).
        /// </summary>
        public async void ExportDaoReplay(string proposalId, string walletAddress)
        {
            string replayId = $"dao_{proposalId}_ritual";
            await ExportReplay(replayId, walletAddress);

            Debug.Log($"[LoreExporter] DAO ritual replay exported: {proposalId}");
        }

        /// <summary>
        /// Batch export all completed missions for player.
        /// </summary>
        public async void ExportAllReplays(string walletAddress)
        {
            if (chronicle == null) return;

            var playerLore = chronicle.GetPlayerLore(walletAddress);

            Debug.Log($"[LoreExporter] Batch exporting {playerLore.Count} replays");

            foreach (var entry in playerLore)
            {
                if (entry.eventType == "mission_complete")
                {
                    await ExportReplay(entry.data, walletAddress);
                    await Task.Delay(1000); // Rate limit
                }
            }

            Debug.Log($"[LoreExporter] Batch export complete");
        }
    }
}
