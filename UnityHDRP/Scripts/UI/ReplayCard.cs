using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soulvan.Systems
{
    /// <summary>
    /// Replay card component for mission replay vault.
    /// </summary>
    public class ReplayCard : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI missionTitleText;
        public TextMeshProUGUI completionDateText;
        public TextMeshProUGUI rewardText;
        public TextMeshProUGUI highlightsText;
        public Button playReplayButton;
        public Button exportButton;

        [Header("FX")]
        public ParticleSystem replayFX;

        private MissionReplay replayData;

        /// <summary>
        /// Set replay data.
        /// </summary>
        public void SetData(MissionReplay replay)
        {
            replayData = replay;

            if (missionTitleText != null)
            {
                missionTitleText.text = replay.missionTitle;
            }

            if (completionDateText != null)
            {
                completionDateText.text = $"Completed: {replay.completionDate}";
            }

            if (rewardText != null)
            {
                rewardText.text = $"Reward: {replay.rewardEarned} SVN";
                rewardText.color = Color.green;
            }

            if (highlightsText != null)
            {
                highlightsText.text = $"Highlights: {replay.highlights}";
            }

            // Setup buttons
            if (playReplayButton != null)
            {
                playReplayButton.onClick.AddListener(PlayReplay);
            }

            if (exportButton != null)
            {
                exportButton.onClick.AddListener(ExportReplay);
            }
        }

        /// <summary>
        /// Play mission replay.
        /// </summary>
        private void PlayReplay()
        {
            Debug.Log($"[ReplayCard] Playing replay: {replayData.missionTitle}");

            // Play replay FX
            if (replayFX != null)
            {
                replayFX.Play();
            }

            // Load mission replay scene
            // UnityEngine.SceneManagement.SceneManager.LoadScene(replayData.missionId);
        }

        /// <summary>
        /// Export replay as NFT.
        /// </summary>
        private void ExportReplay()
        {
            Debug.Log($"[ReplayCard] Exporting replay: {replayData.missionTitle}");
            
            SoulvanLore.ExportMissionLore(replayData.missionId, replayData.rewardEarned, 0);
        }
    }
}
