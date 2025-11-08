using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soulvan.Systems
{
    /// <summary>
    /// Badge card component for badge gallery.
    /// </summary>
    public class BadgeCard : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI badgeNameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI earnedDateText;
        public TextMeshProUGUI daoImpactText;
        public Image badgeIcon;
        public GameObject tooltipPanel;

        [Header("FX")]
        public ParticleSystem badgeGlow;

        private BadgeData badgeData;

        /// <summary>
        /// Set badge data.
        /// </summary>
        public void SetData(BadgeData badge)
        {
            badgeData = badge;

            if (badgeNameText != null)
            {
                badgeNameText.text = badge.badgeName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = badge.description;
            }

            if (earnedDateText != null)
            {
                earnedDateText.text = $"Earned: {badge.earnedDate}";
            }

            if (daoImpactText != null)
            {
                daoImpactText.text = badge.daoImpact;
            }

            // Play glow FX
            if (badgeGlow != null)
            {
                badgeGlow.Play();
            }
        }

        /// <summary>
        /// Show tooltip on hover.
        /// </summary>
        public void ShowTooltip()
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hide tooltip.
        /// </summary>
        public void HideTooltip()
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }
    }
}
