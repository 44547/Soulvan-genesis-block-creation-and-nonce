using System;
using UnityEngine;

namespace Soulvan.Infra
{
    /// <summary>
    /// Global event bus for decoupled communication between systems.
    /// Enables orchestration of motifs, missions, rewards, and DAO integration.
    /// </summary>
    public static class EventBus
    {
        // Motif events
        public static event Action<string> OnMotifChanged;
        
        // Mission events
        public static event Action<string> OnMissionStarted;
        public static event Action<string> OnMissionCompleted;
        public static event Action<string, float> OnMissionProgress;
        
        // Combat events
        public static event Action<string> OnBossDefeated;
        public static event Action<string, string> OnPlayerDamaged;
        
        // Reward events
        public static event Action<string, float> OnSVNMinted;
        public static event Action<string, string> OnNFTMinted;
        
        // DAO events
        public static event Action<int> OnProposalCreated;
        public static event Action<int, bool> OnVoteCast;
        
        // Season events
        public static event Action<int> OnSeasonChanged;

        /// <summary>
        /// Emit motif change event.
        /// </summary>
        public static void EmitMotif(string motifId)
        {
            Debug.Log($"[EventBus] Motif changed: {motifId}");
            OnMotifChanged?.Invoke(motifId);
        }

        /// <summary>
        /// Emit mission started event.
        /// </summary>
        public static void EmitMissionStarted(string missionId)
        {
            Debug.Log($"[EventBus] Mission started: {missionId}");
            OnMissionStarted?.Invoke(missionId);
        }

        /// <summary>
        /// Emit mission completed event.
        /// </summary>
        public static void EmitMissionCompleted(string missionId)
        {
            Debug.Log($"[EventBus] Mission completed: {missionId}");
            OnMissionCompleted?.Invoke(missionId);
        }

        /// <summary>
        /// Emit mission progress update.
        /// </summary>
        public static void EmitMissionProgress(string missionId, float progress01)
        {
            OnMissionProgress?.Invoke(missionId, progress01);
        }

        /// <summary>
        /// Emit boss defeated event.
        /// </summary>
        public static void EmitBossDefeated(string bossId)
        {
            Debug.Log($"[EventBus] Boss defeated: {bossId}");
            OnBossDefeated?.Invoke(bossId);
        }

        /// <summary>
        /// Emit player damaged event.
        /// </summary>
        public static void EmitPlayerDamaged(string playerId, string sourceId)
        {
            OnPlayerDamaged?.Invoke(playerId, sourceId);
        }

        /// <summary>
        /// Emit SVN minted event.
        /// </summary>
        public static void EmitSVNMinted(string wallet, float amount)
        {
            Debug.Log($"[EventBus] SVN minted: {amount} to {wallet}");
            OnSVNMinted?.Invoke(wallet, amount);
        }

        /// <summary>
        /// Emit NFT minted event.
        /// </summary>
        public static void EmitNFTMinted(string wallet, string tokenId)
        {
            Debug.Log($"[EventBus] NFT minted: {tokenId} to {wallet}");
            OnNFTMinted?.Invoke(wallet, tokenId);
        }

        /// <summary>
        /// Emit proposal created event.
        /// </summary>
        public static void EmitProposalCreated(int proposalId)
        {
            Debug.Log($"[EventBus] Proposal created: #{proposalId}");
            OnProposalCreated?.Invoke(proposalId);
        }

        /// <summary>
        /// Emit vote cast event.
        /// </summary>
        public static void EmitVoteCast(int proposalId, bool support)
        {
            Debug.Log($"[EventBus] Vote cast on #{proposalId}: {(support ? "FOR" : "AGAINST")}");
            OnVoteCast?.Invoke(proposalId, support);
        }

        /// <summary>
        /// Emit season changed event.
        /// </summary>
        public static void EmitSeasonChanged(int seasonId)
        {
            Debug.Log($"[EventBus] Season changed: {seasonId}");
            OnSeasonChanged?.Invoke(seasonId);
        }

        /// <summary>
        /// Clear all event subscriptions (use sparingly, typically on scene unload).
        /// </summary>
        public static void ClearAllEvents()
        {
            OnMotifChanged = null;
            OnMissionStarted = null;
            OnMissionCompleted = null;
            OnMissionProgress = null;
            OnBossDefeated = null;
            OnPlayerDamaged = null;
            OnSVNMinted = null;
            OnNFTMinted = null;
            OnProposalCreated = null;
            OnVoteCast = null;
            OnSeasonChanged = null;
        }
    }
}
