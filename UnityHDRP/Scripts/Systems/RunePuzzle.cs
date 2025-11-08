using System;
using System.Collections.Generic;
using UnityEngine;

namespace Soulvan.Systems
{
    /// <summary>
    /// Rune puzzle system for vault breaching.
    /// Contributors must solve encrypted rune sequences to unlock vaults.
    /// Features cinematic unlocks and lore exports.
    /// </summary>
    public class RunePuzzle : MonoBehaviour
    {
        [Header("Puzzle Configuration")]
        public GameObject[] runeNodes;
        public string correctSequence = "A-B-C-D";
        public GameObject vaultDoor;

        [Header("Progress")]
        private string playerSequence = "";
        private bool puzzleSolved = false;

        [Header("FX")]
        public GameObject unlockFX;
        public ParticleSystem runeGlowParticles;
        public AudioClip successSound;

        /// <summary>
        /// Activate rune node in sequence.
        /// </summary>
        public void ActivateNode(string nodeId)
        {
            if (puzzleSolved) return;

            playerSequence += nodeId + "-";

            Debug.Log($"[RunePuzzle] Node activated: {nodeId}, Sequence: {playerSequence.TrimEnd('-')}");

            // Highlight activated node
            HighlightNode(nodeId);

            // Check if sequence matches
            if (playerSequence.TrimEnd('-') == correctSequence)
            {
                UnlockVault();
            }
            else if (playerSequence.TrimEnd('-').Length >= correctSequence.Length)
            {
                // Wrong sequence - reset
                ResetPuzzle();
            }
        }

        /// <summary>
        /// Highlight activated rune node.
        /// </summary>
        private void HighlightNode(string nodeId)
        {
            GameObject node = Array.Find(runeNodes, n => n.name == nodeId);
            
            if (node != null)
            {
                // Play glow effect
                if (runeGlowParticles != null)
                {
                    ParticleSystem particles = Instantiate(runeGlowParticles, node.transform.position, Quaternion.identity);
                    particles.Play();
                }

                Debug.Log($"[RunePuzzle] Node highlighted: {nodeId}");
            }
        }

        /// <summary>
        /// Unlock vault with cinematic sequence.
        /// </summary>
        private void UnlockVault()
        {
            puzzleSolved = true;

            Debug.Log("[RunePuzzle] ✅ Vault unlocked!");

            // Open vault door
            if (vaultDoor != null)
            {
                OpenVaultDoor();
            }

            // Play unlock FX
            if (unlockFX != null)
            {
                Instantiate(unlockFX, transform.position, Quaternion.identity);
            }

            // Play success sound
            if (successSound != null)
            {
                AudioSource.PlayClipAtPoint(successSound, transform.position);
            }

            // Record lore
            SoulvanLore.Record("Contributor solved rune puzzle. Vault breached.");

            // Trigger cutscene
            CutsceneTrigger cutscene = FindObjectOfType<CutsceneTrigger>();
            if (cutscene != null)
            {
                cutscene.TriggerVaultBreachCutscene();
            }
        }

        /// <summary>
        /// Open vault door animation.
        /// </summary>
        private void OpenVaultDoor()
        {
            if (vaultDoor == null) return;

            Debug.Log("[RunePuzzle] Opening vault door...");

            // Animate door opening
            // vaultDoor.GetComponent<Animation>().Play("DoorOpen");
            
            // Or simple position movement
            StartCoroutine(AnimateDoorOpen());
        }

        private System.Collections.IEnumerator AnimateDoorOpen()
        {
            Vector3 startPos = vaultDoor.transform.position;
            Vector3 endPos = startPos + Vector3.up * 5f;
            
            float elapsed = 0f;
            float duration = 2f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                vaultDoor.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                yield return null;
            }

            Debug.Log("[RunePuzzle] Vault door opened");
        }

        /// <summary>
        /// Reset puzzle on wrong sequence.
        /// </summary>
        private void ResetPuzzle()
        {
            Debug.Log("[RunePuzzle] ❌ Wrong sequence! Resetting puzzle.");

            playerSequence = "";

            // Play error sound
            // AudioSource.PlayClipAtPoint(errorSound, transform.position);

            // Reset node visuals
            foreach (var node in runeNodes)
            {
                // Reset glow, color, etc.
            }
        }

        /// <summary>
        /// Get puzzle hints (optional).
        /// </summary>
        public string GetHint()
        {
            int currentLength = playerSequence.TrimEnd('-').Length;
            
            if (currentLength == 0)
            {
                return "Start with the first rune...";
            }
            else if (currentLength < correctSequence.Length)
            {
                string[] correct = correctSequence.Split('-');
                return $"Next rune: {correct[currentLength]}";
            }

            return "Puzzle complete!";
        }
    }
}
