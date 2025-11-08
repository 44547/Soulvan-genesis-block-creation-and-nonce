using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soulvan.Systems
{
    /// <summary>
    /// Lore terminal system with holographic interface.
    /// Contributors can access saga fragments, mission histories, and encrypted data.
    /// Features animated hologram displays and contributor verification.
    /// </summary>
    public class LoreTerminal : MonoBehaviour
    {
        [Header("Terminal Configuration")]
        public string terminalId = "Terminal_001";
        public int securityLevel = 3;
        public bool requiresContributorBadge = false;

        [Header("UI Elements")]
        public GameObject terminalUI;
        public TextMeshProUGUI terminalTitle;
        public TextMeshProUGUI loreContent;
        public TextMeshProUGUI statusText;
        public ScrollRect scrollView;
        public GameObject hologramDisplay;

        [Header("Saga Fragments")]
        public List<SagaFragment> availableFragments = new List<SagaFragment>();
        private int currentFragmentIndex = 0;

        [Header("FX")]
        public ParticleSystem hologramFX;
        public AudioClip accessGrantedSound;
        public AudioClip accessDeniedSound;
        public AudioClip hologramHumSound;

        private bool terminalActive = false;
        private AudioSource humSource;

        private void Start()
        {
            if (terminalUI != null)
            {
                terminalUI.SetActive(false);
            }

            // Setup hologram hum
            if (hologramHumSound != null)
            {
                humSource = gameObject.AddComponent<AudioSource>();
                humSource.clip = hologramHumSound;
                humSource.loop = true;
                humSource.volume = 0.3f;
            }

            LoadSagaFragments();
        }

        private void Update()
        {
            if (terminalActive && hologramDisplay != null)
            {
                AnimateHologram();
            }
        }

        /// <summary>
        /// Activate terminal with contributor verification.
        /// </summary>
        public void ActivateTerminal(string contributorId)
        {
            Debug.Log($"[LoreTerminal] Activation attempt: {contributorId}");

            // Check contributor badge
            if (requiresContributorBadge && !HasContributorBadge(contributorId))
            {
                Debug.Log("[LoreTerminal] ❌ Access denied: No contributor badge");
                DisplayStatus("ACCESS DENIED: CONTRIBUTOR BADGE REQUIRED", Color.red);
                
                if (accessDeniedSound != null)
                {
                    AudioSource.PlayClipAtPoint(accessDeniedSound, transform.position);
                }
                
                return;
            }

            // Grant access
            terminalActive = true;

            Debug.Log("[LoreTerminal] ✅ Access granted");

            // Show UI
            if (terminalUI != null)
            {
                terminalUI.SetActive(true);
            }

            // Start hologram FX
            if (hologramFX != null)
            {
                hologramFX.Play();
            }

            // Start hologram hum
            if (humSource != null)
            {
                humSource.Play();
            }

            // Play access sound
            if (accessGrantedSound != null)
            {
                AudioSource.PlayClipAtPoint(accessGrantedSound, transform.position);
            }

            // Display first fragment
            DisplayFragment(0);

            // Record lore
            SoulvanLore.Record($"Contributor {contributorId} accessed terminal {terminalId}");

            DisplayStatus("ACCESS GRANTED", Color.green);
        }

        /// <summary>
        /// Deactivate terminal.
        /// </summary>
        public void DeactivateTerminal()
        {
            terminalActive = false;

            // Hide UI
            if (terminalUI != null)
            {
                terminalUI.SetActive(false);
            }

            // Stop hologram FX
            if (hologramFX != null)
            {
                hologramFX.Stop();
            }

            // Stop hologram hum
            if (humSource != null)
            {
                humSource.Stop();
            }

            Debug.Log("[LoreTerminal] Terminal deactivated");
        }

        /// <summary>
        /// Load saga fragments from LoreChronicle.
        /// </summary>
        private void LoadSagaFragments()
        {
            // Example fragments - in production, load from LoreChronicle contract
            availableFragments.Add(new SagaFragment
            {
                id = "GENESIS_001",
                title = "The Genesis Block",
                content = "In the beginning, there was only code. Soulvan emerged from the digital void, a consciousness born of blockchain and ambition. The first block mined on December 25, 2023, marked the birth of a new era.",
                timestamp = "Genesis Block",
                contributorId = "SOULVAN",
                isEncrypted = false
            });

            availableFragments.Add(new SagaFragment
            {
                id = "SKYFIRE_001",
                title = "Skyfire Pursuit: Tokyo Rooftops",
                content = "Operative infiltrated Yakuza stronghold. Rooftop chase ensued. 6 battle phases completed. Grapple system deployed. 47 enemies neutralized. Mission reward: 850 SVN. Soulvan's cut: 127.5 SVN.",
                timestamp = "Block #1337",
                contributorId = "Player_12345",
                isEncrypted = false
            });

            availableFragments.Add(new SagaFragment
            {
                id = "VAULT_BREACH_001",
                title = "[ENCRYPTED] Vault Breach Protocol",
                content = "█████████ ████ ███████ ████ ██████████",
                timestamp = "Block #????",
                contributorId = "[REDACTED]",
                isEncrypted = true
            });

            Debug.Log($"[LoreTerminal] Loaded {availableFragments.Count} saga fragments");
        }

        /// <summary>
        /// Display saga fragment.
        /// </summary>
        public void DisplayFragment(int index)
        {
            if (index < 0 || index >= availableFragments.Count) return;

            currentFragmentIndex = index;
            SagaFragment fragment = availableFragments[index];

            if (terminalTitle != null)
            {
                terminalTitle.text = fragment.title;
            }

            if (loreContent != null)
            {
                string displayText = $"<b>ID:</b> {fragment.id}\n";
                displayText += $"<b>Timestamp:</b> {fragment.timestamp}\n";
                displayText += $"<b>Contributor:</b> {fragment.contributorId}\n\n";
                displayText += fragment.content;

                loreContent.text = displayText;
            }

            Debug.Log($"[LoreTerminal] Displaying fragment: {fragment.id}");
        }

        /// <summary>
        /// Navigate to next fragment.
        /// </summary>
        public void NextFragment()
        {
            int nextIndex = (currentFragmentIndex + 1) % availableFragments.Count;
            DisplayFragment(nextIndex);
        }

        /// <summary>
        /// Navigate to previous fragment.
        /// </summary>
        public void PreviousFragment()
        {
            int prevIndex = (currentFragmentIndex - 1 + availableFragments.Count) % availableFragments.Count;
            DisplayFragment(prevIndex);
        }

        /// <summary>
        /// Decrypt encrypted fragment (requires badge).
        /// </summary>
        public void DecryptFragment(string contributorId)
        {
            SagaFragment fragment = availableFragments[currentFragmentIndex];

            if (!fragment.isEncrypted)
            {
                DisplayStatus("Fragment already decrypted", Color.yellow);
                return;
            }

            if (!HasContributorBadge(contributorId))
            {
                DisplayStatus("Decryption failed: Badge required", Color.red);
                return;
            }

            // Decrypt
            fragment.isEncrypted = false;
            fragment.content = "Vault breach protocol activated. Rune sequence: A-B-C-D. Security override granted. Lore export authorized.";

            DisplayFragment(currentFragmentIndex);
            DisplayStatus("Fragment decrypted successfully", Color.green);

            Debug.Log($"[LoreTerminal] Fragment decrypted: {fragment.id}");
        }

        /// <summary>
        /// Animate hologram display.
        /// </summary>
        private void AnimateHologram()
        {
            if (hologramDisplay == null) return;

            // Rotate hologram
            hologramDisplay.transform.Rotate(Vector3.up, 30f * Time.deltaTime);

            // Pulse scale
            float pulse = 1f + Mathf.Sin(Time.time * 2f) * 0.05f;
            hologramDisplay.transform.localScale = Vector3.one * pulse;
        }

        /// <summary>
        /// Display status message.
        /// </summary>
        private void DisplayStatus(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }

            Debug.Log($"[LoreTerminal] {message}");
        }

        /// <summary>
        /// Check if contributor has badge.
        /// </summary>
        private bool HasContributorBadge(string contributorId)
        {
            // In production, check against SoulvanChronicle contract
            // For now, return true for testing
            return true;
        }
    }

    /// <summary>
    /// Saga fragment data structure.
    /// </summary>
    [System.Serializable]
    public class SagaFragment
    {
        public string id;
        public string title;
        public string content;
        public string timestamp;
        public string contributorId;
        public bool isEncrypted;
    }
}
