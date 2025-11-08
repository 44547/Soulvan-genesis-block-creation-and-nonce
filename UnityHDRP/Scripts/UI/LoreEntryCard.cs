using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soulvan.Systems
{
    /// <summary>
    /// Lore entry card component for saga timeline.
    /// </summary>
    public class LoreEntryCard : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI timestampText;
        public TextMeshProUGUI contentText;
        public TextMeshProUGUI categoryBadge;
        public Image cardBackground;

        [Header("Export")]
        public Button exportButton;

        private LoreEntry entryData;

        /// <summary>
        /// Set card data.
        /// </summary>
        public void SetData(LoreEntry entry)
        {
            entryData = entry;

            if (timestampText != null)
            {
                timestampText.text = entry.timestamp;
            }

            if (contentText != null)
            {
                contentText.text = entry.content;
            }

            if (categoryBadge != null)
            {
                categoryBadge.text = entry.category;
            }

            if (exportButton != null)
            {
                exportButton.onClick.AddListener(ExportEntry);
            }
        }

        /// <summary>
        /// Export single lore entry.
        /// </summary>
        private void ExportEntry()
        {
            Debug.Log($"[LoreEntryCard] Exporting entry: {entryData.content}");
            SoulvanLore.Record($"Lore entry exported: {entryData.content}");
        }
    }
}
