using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soulvan.Systems
{
    /// <summary>
    /// UI components for Soulvan systems.
    /// </summary>

    // VOTE FEED ENTRY
    public class VoteFeedEntry : MonoBehaviour
    {
        public TextMeshProUGUI contributorText;
        public TextMeshProUGUI voteChoiceText;
        public TextMeshProUGUI reasonText;
        public TextMeshProUGUI timestampText;

        public void SetData(Vote vote)
        {
            if (contributorText != null)
                contributorText.text = vote.contributorId;
            
            if (voteChoiceText != null)
            {
                voteChoiceText.text = vote.choice.ToString().ToUpper();
                voteChoiceText.color = vote.choice == VoteChoice.Yes ? Color.green : Color.red;
            }
            
            if (reasonText != null)
                reasonText.text = vote.reason;
            
            if (timestampText != null)
                timestampText.text = vote.timestamp;
        }
    }

    // VOTE ARCHIVE ENTRY
    public class VoteArchiveEntry : MonoBehaviour
    {
        public TextMeshProUGUI proposalText;
        public TextMeshProUGUI contributorText;
        public TextMeshProUGUI voteText;
        public TextMeshProUGUI powerText;

        public void SetData(Vote vote)
        {
            if (proposalText != null)
                proposalText.text = $"Proposal: {vote.proposalId}";
            
            if (contributorText != null)
                contributorText.text = vote.contributorId;
            
            if (voteText != null)
            {
                voteText.text = vote.choice.ToString();
                voteText.color = vote.choice == VoteChoice.Yes ? Color.green : Color.red;
            }
            
            if (powerText != null)
                powerText.text = $"Power: {vote.votingPower}";
        }
    }

    // BADGE TIER CARD
    public class BadgeTierCard : MonoBehaviour
    {
        public TextMeshProUGUI tierNameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI xpRequiredText;
        public TextMeshProUGUI daoPowerText;
        public Image tierBackground;

        public void SetData(BadgeTier tier)
        {
            if (tierNameText != null)
                tierNameText.text = tier.tierName;
            
            if (descriptionText != null)
                descriptionText.text = tier.description;
            
            if (xpRequiredText != null)
                xpRequiredText.text = $"XP Required: {tier.requiredXP}";
            
            if (daoPowerText != null)
                daoPowerText.text = $"DAO Power: {tier.daoPower}";
            
            if (tierBackground != null)
                tierBackground.color = tier.tierColor;
        }
    }

    // BADGE ICON
    public class BadgeIcon : MonoBehaviour
    {
        public Image badgeImage;
        public TextMeshProUGUI badgeNameText;
        public GameObject tooltipPanel;
        public ParticleSystem glowFX;

        public void SetData(BadgeData badge)
        {
            if (badgeNameText != null)
                badgeNameText.text = badge.badgeName;
            
            if (glowFX != null)
                glowFX.Play();
        }

        public void ShowTooltip()
        {
            if (tooltipPanel != null)
                tooltipPanel.SetActive(true);
        }

        public void HideTooltip()
        {
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }
    }

    // BADGE HISTORY ENTRY
    public class BadgeHistoryEntry : MonoBehaviour
    {
        public TextMeshProUGUI badgeNameText;
        public TextMeshProUGUI contributorText;
        public TextMeshProUGUI earnedDateText;
        public TextMeshProUGUI descriptionText;
        public Button exportButton;

        public void SetData(BadgeData badge)
        {
            if (badgeNameText != null)
                badgeNameText.text = badge.badgeName;
            
            if (contributorText != null)
                contributorText.text = badge.contributorId;
            
            if (earnedDateText != null)
                earnedDateText.text = badge.earnedDate;
            
            if (descriptionText != null)
                descriptionText.text = badge.description;
            
            if (exportButton != null)
                exportButton.onClick.AddListener(() => ExportBadge(badge.badgeId));
        }

        private void ExportBadge(string badgeId)
        {
            Debug.Log($"[BadgeHistoryEntry] Exporting badge: {badgeId}");
            SoulvanLore.Record($"Badge exported: {badgeId}");
        }
    }
}
