using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Soulvan.Lore;

namespace Soulvan.Systems
{
    /// <summary>
    /// Soulvan - The mythic boss of the entire mission ecosystem.
    /// Architect of vaults, keeper of the saga, final judge of contributor legends.
    /// Takes 15% cut from all mission rewards, displayed in cinematic hologram chamber.
    /// </summary>
    public class SoulvanBoss : MonoBehaviour
    {
        [Header("Boss Configuration")]
        public string bossName = "Soulvan";
        public float coinCutPercentage = 0.15f; // 15% of mission reward

        [Header("Visual Elements")]
        public GameObject coinHologramPrefab;
        public Transform hologramSpawnPoint;
        public GameObject throneFX;
        public GameObject vaultShardFX;

        [Header("Audio")]
        public AudioClip[] soulvanVoiceLines;
        public AudioSource voiceSource;

        [Header("Stats")]
        [SerializeField] private float totalCoinsCollected = 0f;
        [SerializeField] private int missionsCompleted = 0;
        [SerializeField] private List<string> contributorLegends = new List<string>();

        private CutsceneTrigger cutsceneTrigger;

        private void Start()
        {
            InitializeSoulvan();
        }

        /// <summary>
        /// Initialize Soulvan's command chamber.
        /// </summary>
        private void InitializeSoulvan()
        {
            Debug.Log($"[SoulvanBoss] {bossName} awakens. The saga begins.");

            // Initialize cutscene system
            cutsceneTrigger = GetComponent<CutsceneTrigger>();
            if (cutsceneTrigger == null)
            {
                cutsceneTrigger = gameObject.AddComponent<CutsceneTrigger>();
            }

            // Activate throne FX
            if (throneFX != null)
            {
                throneFX.SetActive(true);
            }

            // Activate vault shard FX
            if (vaultShardFX != null)
            {
                vaultShardFX.SetActive(true);
            }

            SoulvanLore.Record($"{bossName} initialized. Command chamber active.");
        }

        /// <summary>
        /// Receive mission report from operative and take Soulvan's cut.
        /// </summary>
        public void ReceiveMissionReport(MissionReport report)
        {
            if (!report.success)
            {
                Debug.Log($"[SoulvanBoss] Mission failed: {report.missionName}. No tribute.");
                SpeakVoiceLine("Mission failed. Do better.");
                return;
            }

            // Calculate Soulvan's cut
            float soulvanCut = report.totalReward * coinCutPercentage;
            totalCoinsCollected += soulvanCut;
            missionsCompleted++;

            Debug.Log($"[SoulvanBoss] 💰 {bossName} receives {soulvanCut:F2} SoulvanCoin from mission: {report.missionName}");
            Debug.Log($"[SoulvanBoss] Total collected: {totalCoinsCollected:F2} SVN across {missionsCompleted} missions");

            // Spawn coin hologram
            SpawnCoinHologram(soulvanCut);

            // Record in lore
            SoulvanLore.RecordBossCut(bossName, report.missionName, soulvanCut, report.operativeName);

            // Add contributor to legends if worthy
            if (report.totalReward >= 1000f)
            {
                AddToLegends(report.operativeName);
            }

            // Trigger cutscene
            TriggerTributeCutscene(report, soulvanCut);

            // Speak voice line
            SpeakVoiceLine($"You did well, {report.operativeName}. My cut is secured. The saga continues.");
        }

        /// <summary>
        /// Spawn coin hologram in command chamber.
        /// </summary>
        private void SpawnCoinHologram(float amount)
        {
            if (coinHologramPrefab == null || hologramSpawnPoint == null)
            {
                Debug.LogWarning("[SoulvanBoss] Coin hologram prefab or spawn point not set");
                return;
            }

            GameObject hologram = Instantiate(
                coinHologramPrefab,
                hologramSpawnPoint.position,
                Quaternion.identity,
                hologramSpawnPoint
            );

            CoinHologram coinScript = hologram.GetComponent<CoinHologram>();
            if (coinScript != null)
            {
                coinScript.SetAmount(amount);
            }

            Debug.Log($"[SoulvanBoss] Coin hologram spawned: {amount:F2} SVN");
        }

        /// <summary>
        /// Add contributor to legends list.
        /// </summary>
        private void AddToLegends(string contributorName)
        {
            if (!contributorLegends.Contains(contributorName))
            {
                contributorLegends.Add(contributorName);
                Debug.Log($"[SoulvanBoss] 🏆 {contributorName} added to legends!");
                SoulvanLore.Record($"{contributorName} ascended to legendary status.");
            }
        }

        /// <summary>
        /// Trigger tribute cutscene when operative reports to Soulvan.
        /// </summary>
        private void TriggerTributeCutscene(MissionReport report, float cut)
        {
            if (cutsceneTrigger != null)
            {
                cutsceneTrigger.TriggerMissionTributeCutscene(report, cut);
            }
        }

        /// <summary>
        /// Speak Soulvan voice line.
        /// </summary>
        private void SpeakVoiceLine(string text)
        {
            Debug.Log($"[SoulvanBoss] 🗣️ Soulvan: \"{text}\"");

            if (voiceSource != null && soulvanVoiceLines != null && soulvanVoiceLines.Length > 0)
            {
                AudioClip clip = soulvanVoiceLines[UnityEngine.Random.Range(0, soulvanVoiceLines.Length)];
                voiceSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Grant contributor badge with Soulvan's approval.
        /// </summary>
        public void GrantBadge(string contributorId, string badgeType)
        {
            Debug.Log($"[SoulvanBoss] Granting badge to {contributorId}: {badgeType}");

            SoulvanLore.MintBadge(contributorId, badgeType);
            SpeakVoiceLine($"Your legend grows, contributor. Badge earned: {badgeType}");

            // Trigger badge mint cutscene
            if (cutsceneTrigger != null)
            {
                cutsceneTrigger.TriggerBadgeMintCutscene(contributorId, badgeType);
            }
        }

        /// <summary>
        /// Get Soulvan's stats.
        /// </summary>
        public SoulvanStats GetStats()
        {
            return new SoulvanStats
            {
                bossName = bossName,
                totalCoinsCollected = totalCoinsCollected,
                missionsCompleted = missionsCompleted,
                contributorLegends = new List<string>(contributorLegends),
                cutPercentage = coinCutPercentage * 100f
            };
        }
    }

    #region Data Structures

    /// <summary>
    /// Mission report submitted by operatives to Soulvan.
    /// </summary>
    [Serializable]
    public class MissionReport
    {
        public string missionName;
        public float totalReward;
        public string operativeName;
        public bool success;
        public float completionTime;
        public int enemiesDefeated;

        public MissionReport(string name, float reward, string operative, bool status)
        {
            missionName = name;
            totalReward = reward;
            operativeName = operative;
            success = status;
            completionTime = 0f;
            enemiesDefeated = 0;
        }
    }

    /// <summary>
    /// Soulvan's stats.
    /// </summary>
    [Serializable]
    public class SoulvanStats
    {
        public string bossName;
        public float totalCoinsCollected;
        public int missionsCompleted;
        public List<string> contributorLegends;
        public float cutPercentage;
    }

    #endregion
}
