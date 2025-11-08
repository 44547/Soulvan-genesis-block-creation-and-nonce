using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soulvan.Systems
{
    /// <summary>
    /// Lore scroll visual component for animated timeline entries.
    /// </summary>
    public class LoreScroll : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI contentText;
        public TextMeshProUGUI timestampText;
        public TextMeshProUGUI categoryText;
        public Image scrollBackground;

        [Header("Animation")]
        public float scrollSpeed = 50f;
        public ParticleSystem scrollFX;

        private LoreEntry entryData;

        private void Start()
        {
            AnimateScroll();
        }

        /// <summary>
        /// Set lore entry content.
        /// </summary>
        public void SetContent(LoreEntry entry)
        {
            entryData = entry;

            if (contentText != null)
            {
                contentText.text = entry.content;
            }

            if (timestampText != null)
            {
                timestampText.text = entry.timestamp;
            }

            if (categoryText != null)
            {
                categoryText.text = $"[{entry.category}]";
                categoryText.color = GetCategoryColor(entry.category);
            }

            Debug.Log($"[LoreScroll] Content set: {entry.content}");
        }

        /// <summary>
        /// Animate scroll appearance.
        /// </summary>
        private void AnimateScroll()
        {
            // Play FX
            if (scrollFX != null)
            {
                scrollFX.Play();
            }

            // Fade in
            if (scrollBackground != null)
            {
                StartCoroutine(FadeIn());
            }
        }

        /// <summary>
        /// Fade in animation.
        /// </summary>
        private System.Collections.IEnumerator FadeIn()
        {
            float elapsed = 0f;
            float duration = 0.5f;

            Color startColor = scrollBackground.color;
            startColor.a = 0f;
            scrollBackground.color = startColor;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);

                Color newColor = scrollBackground.color;
                newColor.a = alpha;
                scrollBackground.color = newColor;

                yield return null;
            }
        }

        /// <summary>
        /// Get color for category.
        /// </summary>
        private Color GetCategoryColor(string category)
        {
            switch (category)
            {
                case "XP_GAIN":
                    return Color.yellow;
                case "ROLE_UPGRADE":
                    return Color.cyan;
                case "MISSION_COMPLETE":
                    return Color.green;
                case "BADGE_EARNED":
                    return Color.magenta;
                default:
                    return Color.white;
            }
        }
    }
}
