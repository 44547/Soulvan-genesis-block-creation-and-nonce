using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soulvan.Systems
{
    /// <summary>
    /// Vault door system with breach countdown and cinematic opening.
    /// Manages secure vault access and breach animations.
    /// </summary>
    public class VaultDoor : MonoBehaviour
    {
        [Header("Door Configuration")]
        public GameObject doorModel;
        public Transform[] doorPanels;
        public float openSpeed = 2f;

        [Header("Breach System")]
        public bool isLocked = true;
        public float breachCountdown = 30f;
        private float currentCountdown;
        private bool breachActive = false;

        [Header("HUD")]
        public TextMeshProUGUI countdownText;
        public TextMeshProUGUI statusText;
        public Image countdownBar;
        public GameObject breachHUD;

        [Header("FX")]
        public ParticleSystem breachFX;
        public ParticleSystem unlockFX;
        public AudioClip breachAlarm;
        public AudioClip unlockSound;

        [Header("Security")]
        public int securityLevel = 5;
        public string requiredKeycard = "SoulvanAccess_L5";

        private void Start()
        {
            if (breachHUD != null)
            {
                breachHUD.SetActive(false);
            }

            UpdateStatusDisplay();
        }

        private void Update()
        {
            if (breachActive)
            {
                UpdateBreachCountdown();
            }
        }

        /// <summary>
        /// Unlock vault door with keycard.
        /// </summary>
        public void UnlockDoor(string keycard)
        {
            if (!isLocked)
            {
                Debug.Log("[VaultDoor] Already unlocked");
                return;
            }

            if (keycard != requiredKeycard)
            {
                Debug.Log($"[VaultDoor] ❌ Invalid keycard: {keycard}");
                DisplayMessage("ACCESS DENIED", Color.red);
                return;
            }

            Debug.Log("[VaultDoor] ✅ Keycard accepted. Unlocking...");

            isLocked = false;

            // Play unlock FX
            if (unlockFX != null)
            {
                unlockFX.Play();
            }

            // Play unlock sound
            if (unlockSound != null)
            {
                AudioSource.PlayClipAtPoint(unlockSound, transform.position);
            }

            // Open door
            StartCoroutine(OpenDoor());

            // Record lore
            SoulvanLore.Record("Vault door unlocked with keycard.");

            UpdateStatusDisplay();
        }

        /// <summary>
        /// Start breach countdown (forced entry).
        /// </summary>
        public void StartBreach()
        {
            if (!isLocked)
            {
                Debug.Log("[VaultDoor] Already unlocked");
                return;
            }

            if (breachActive)
            {
                Debug.Log("[VaultDoor] Breach already in progress");
                return;
            }

            Debug.Log("[VaultDoor] ⚠️ Breach initiated! Countdown started.");

            breachActive = true;
            currentCountdown = breachCountdown;

            // Show breach HUD
            if (breachHUD != null)
            {
                breachHUD.SetActive(true);
            }

            // Play breach FX
            if (breachFX != null)
            {
                breachFX.Play();
            }

            // Play breach alarm
            if (breachAlarm != null)
            {
                AudioSource alarmSource = gameObject.AddComponent<AudioSource>();
                alarmSource.clip = breachAlarm;
                alarmSource.loop = true;
                alarmSource.Play();
            }

            // Record lore
            SoulvanLore.Record("Vault breach initiated. Countdown started.");

            DisplayMessage("BREACH INITIATED", Color.red);
        }

        /// <summary>
        /// Update breach countdown.
        /// </summary>
        private void UpdateBreachCountdown()
        {
            currentCountdown -= Time.deltaTime;

            // Update HUD
            if (countdownText != null)
            {
                int minutes = Mathf.FloorToInt(currentCountdown / 60f);
                int seconds = Mathf.FloorToInt(currentCountdown % 60f);
                countdownText.text = $"{minutes:00}:{seconds:00}";
            }

            if (countdownBar != null)
            {
                countdownBar.fillAmount = currentCountdown / breachCountdown;
            }

            // Check if countdown complete
            if (currentCountdown <= 0f)
            {
                CompleteBreach();
            }
        }

        /// <summary>
        /// Complete breach and open door.
        /// </summary>
        private void CompleteBreach()
        {
            breachActive = false;
            isLocked = false;

            Debug.Log("[VaultDoor] ✅ Breach complete! Door opening.");

            // Stop alarm
            AudioSource alarmSource = GetComponent<AudioSource>();
            if (alarmSource != null)
            {
                Destroy(alarmSource);
            }

            // Stop breach FX
            if (breachFX != null)
            {
                breachFX.Stop();
            }

            // Play unlock FX
            if (unlockFX != null)
            {
                unlockFX.Play();
            }

            // Open door
            StartCoroutine(OpenDoor());

            // Hide breach HUD
            if (breachHUD != null)
            {
                breachHUD.SetActive(false);
            }

            // Record lore
            SoulvanLore.Record("Vault breach successful. Door opened.");

            // Trigger cutscene
            CutsceneTrigger cutscene = FindObjectOfType<CutsceneTrigger>();
            if (cutscene != null)
            {
                cutscene.TriggerVaultBreachCutscene();
            }

            DisplayMessage("VAULT BREACHED", Color.green);
        }

        /// <summary>
        /// Animate door opening.
        /// </summary>
        private System.Collections.IEnumerator OpenDoor()
        {
            Debug.Log("[VaultDoor] Opening door...");

            if (doorPanels.Length > 0)
            {
                // Slide panels apart
                float elapsed = 0f;
                float duration = 1f / openSpeed;

                Vector3[] startPositions = new Vector3[doorPanels.Length];
                Vector3[] endPositions = new Vector3[doorPanels.Length];

                for (int i = 0; i < doorPanels.Length; i++)
                {
                    startPositions[i] = doorPanels[i].position;
                    endPositions[i] = startPositions[i] + doorPanels[i].right * (i % 2 == 0 ? -3f : 3f);
                }

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    
                    for (int i = 0; i < doorPanels.Length; i++)
                    {
                        doorPanels[i].position = Vector3.Lerp(startPositions[i], endPositions[i], elapsed / duration);
                    }

                    yield return null;
                }
            }
            else
            {
                // Simple upward movement
                Vector3 startPos = doorModel.transform.position;
                Vector3 endPos = startPos + Vector3.up * 5f;

                float elapsed = 0f;
                float duration = 1f / openSpeed;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    doorModel.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                    yield return null;
                }
            }

            Debug.Log("[VaultDoor] Door opened");

            UpdateStatusDisplay();
        }

        /// <summary>
        /// Update status display.
        /// </summary>
        private void UpdateStatusDisplay()
        {
            if (statusText != null)
            {
                statusText.text = isLocked ? "🔒 LOCKED" : "🔓 UNLOCKED";
                statusText.color = isLocked ? Color.red : Color.green;
            }
        }

        /// <summary>
        /// Display message on HUD.
        /// </summary>
        private void DisplayMessage(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }
        }
    }
}
