using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Soulvan.Lore
{
    /// <summary>
    /// Static lore API for easy access across all Soulvan systems.
    /// Handles lore recording, badge minting, boss cut tracking, and saga exports.
    /// </summary>
    public static class SoulvanLore
    {
        private static LoreChronicle instance;

        private static LoreChronicle GetInstance()
        {
            if (instance == null)
            {
                instance = UnityEngine.Object.FindObjectOfType<LoreChronicle>();
            }
            return instance;
        }

        /// <summary>
        /// Record general lore entry.
        /// </summary>
        public static void Record(string entry)
        {
            Debug.Log($"[SoulvanLore] 📜 Entry: {entry}");
            
            var chronicle = GetInstance();
            if (chronicle != null)
            {
                chronicle.LogLore(entry, "General");
            }
        }

        /// <summary>
        /// Record Soulvan boss tribute from mission.
        /// </summary>
        public static void RecordBossCut(string bossName, string missionName, float cut, string operative)
        {
            Debug.Log($"[SoulvanLore] 💰 Boss Cut: {bossName} receives {cut:F2} SVN from {missionName}");
            
            var chronicle = GetInstance();
            if (chronicle != null)
            {
                chronicle.RecordBossCut(bossName, missionName, cut, operative);
            }
        }

        /// <summary>
        /// Mint contributor badge with lore recording.
        /// </summary>
        public static void MintBadge(string contributorId, string badgeType)
        {
            Debug.Log($"[SoulvanLore] 🏅 Badge minted: {badgeType} for {contributorId}");
            
            // Stub: Mint badge NFT via BadgeMintService
            Record($"Badge minted: {badgeType} for contributor {contributorId}");
        }

        /// <summary>
        /// Play cutscene by ID.
        /// </summary>
        public static void PlayCutscene(string cutsceneId)
        {
            Debug.Log($"[SoulvanLore] 🎬 Playing cutscene: {cutsceneId}");
            
            // Stub: Trigger cutscene via CutsceneTrigger
            Record($"Cutscene played: {cutsceneId}");
        }

        /// <summary>
        /// Export mission lore for replay NFT.
        /// </summary>
        public static void ExportMissionLore(string missionId, float time, int enemiesDefeated)
        {
            Debug.Log($"[SoulvanLore] 📤 Exporting mission lore: {missionId}");
            
            string loreEntry = $"Mission {missionId} completed in {time:F1}s, {enemiesDefeated} enemies defeated";
            Record(loreEntry);
        }
    }

    /// <summary>
    /// Legacy LoreLog class for backward compatibility.
    /// </summary>
    public static class LoreLog
    {
        public static void Record(string entry)
        {
            SoulvanLore.Record(entry);
        }

        public static void RecordBossCut(string bossName, string missionName, float cut)
        {
            SoulvanLore.RecordBossCut(bossName, missionName, cut, "Unknown");
        }
    }
}
