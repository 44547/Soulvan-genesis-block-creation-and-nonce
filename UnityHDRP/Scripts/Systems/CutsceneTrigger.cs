using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soulvan.Systems
{
    /// <summary>
    /// Cinematic cutscene trigger system for Soulvan's command chamber,
    /// vault rooms, lore terminals, and mission tributes.
    /// Handles camera movements, voice overlays, and FX synchronization.
    /// </summary>
    public class CutsceneTrigger : MonoBehaviour
    {
        [Header("Cinematic Camera")]
        public Camera cinematicCamera;
        public Transform[] cameraPoints;
        public float cameraTransitionSpeed = 2f;

        [Header("Audio")]
        public AudioClip[] soulvanVoiceLines;
        public AudioSource audioSource;

        [Header("FX")]
        public GameObject coinFX;
        public GameObject loreFX;
        public GameObject badgeFX;
        public GameObject nftMintFX;
        public GameObject runeFX;

        private bool cutscenePlaying = false;

        /// <summary>
        /// Trigger mission tribute cutscene when operative reports to Soulvan.
        /// </summary>
        public void TriggerMissionTributeCutscene(MissionReport report, float cut)
        {
            if (cutscenePlaying) return;

            StartCoroutine(PlayMissionTributeCutscene(report, cut));
        }

        private IEnumerator PlayMissionTributeCutscene(MissionReport report, float cut)
        {
            cutscenePlaying = true;

            Debug.Log("[CutsceneTrigger] 🎬 Playing mission tribute cutscene");

            // Enable cinematic camera
            if (cinematicCamera != null)
            {
                cinematicCamera.gameObject.SetActive(true);
            }

            // Camera sequence: Throne room pan
            yield return StartCoroutine(CameraSequence());

            // Spawn coin FX
            if (coinFX != null)
            {
                Instantiate(coinFX, transform.position, Quaternion.identity);
            }

            // Play Soulvan voice line
            PlayVoiceLine($"Mission complete: {report.missionName}. Tribute received: {cut:F2} SoulvanCoin.");

            yield return new WaitForSeconds(3f);

            // Record lore
            SoulvanLore.Record($"Operative {report.operativeName} completed {report.missionName}. Soulvan's cut: {cut:F2} SVN.");

            // Disable cinematic camera
            if (cinematicCamera != null)
            {
                cinematicCamera.gameObject.SetActive(false);
            }

            cutscenePlaying = false;

            Debug.Log("[CutsceneTrigger] Cutscene complete");
        }

        /// <summary>
        /// Trigger vault breach cutscene.
        /// </summary>
        public void TriggerVaultBreachCutscene()
        {
            if (cutscenePlaying) return;

            StartCoroutine(PlayVaultBreachCutscene());
        }

        private IEnumerator PlayVaultBreachCutscene()
        {
            cutscenePlaying = true;

            Debug.Log("[CutsceneTrigger] 🎬 Playing vault breach cutscene");

            // Enable cinematic camera
            if (cinematicCamera != null)
            {
                cinematicCamera.gameObject.SetActive(true);
            }

            // Camera sequence
            yield return StartCoroutine(CameraSequence());

            // Spawn lore FX
            if (loreFX != null)
            {
                Instantiate(loreFX, transform.position, Quaternion.identity);
            }

            // Play voice line
            PlayVoiceLine("The vault is breached. Lore awaits.");

            yield return new WaitForSeconds(3f);

            // Record lore
            SoulvanLore.Record("Vault breach cutscene triggered.");

            // Disable cinematic camera
            if (cinematicCamera != null)
            {
                cinematicCamera.gameObject.SetActive(false);
            }

            cutscenePlaying = false;
        }

        /// <summary>
        /// Trigger badge mint cutscene.
        /// </summary>
        public void TriggerBadgeMintCutscene(string contributorId, string badgeType)
        {
            if (cutscenePlaying) return;

            StartCoroutine(PlayBadgeMintCutscene(contributorId, badgeType));
        }

        private IEnumerator PlayBadgeMintCutscene(string contributorId, string badgeType)
        {
            cutscenePlaying = true;

            Debug.Log($"[CutsceneTrigger] 🎬 Playing badge mint cutscene for {contributorId}");

            // Enable cinematic camera
            if (cinematicCamera != null)
            {
                cinematicCamera.gameObject.SetActive(true);
            }

            // Camera sequence
            yield return StartCoroutine(CameraSequence());

            // Spawn badge FX
            if (badgeFX != null)
            {
                Instantiate(badgeFX, transform.position, Quaternion.identity);
            }

            // Play voice line
            PlayVoiceLine($"Badge earned: {badgeType}. Your legend grows, {contributorId}.");

            yield return new WaitForSeconds(3f);

            // Record lore
            SoulvanLore.Record($"Badge minted: {badgeType} for {contributorId}");

            // Disable cinematic camera
            if (cinematicCamera != null)
            {
                cinematicCamera.gameObject.SetActive(false);
            }

            cutscenePlaying = false;
        }

        /// <summary>
        /// Trigger replay NFT mint cutscene.
        /// </summary>
        public void TriggerReplayNFTCutscene(string missionName)
        {
            if (cutscenePlaying) return;

            StartCoroutine(PlayReplayNFTCutscene(missionName));
        }

        private IEnumerator PlayReplayNFTCutscene(string missionName)
        {
            cutscenePlaying = true;

            Debug.Log($"[CutsceneTrigger] 🎬 Playing replay NFT mint cutscene for {missionName}");

            // Enable cinematic camera
            if (cinematicCamera != null)
            {
                cinematicCamera.gameObject.SetActive(true);
            }

            // Camera sequence
            yield return StartCoroutine(CameraSequence());

            // Spawn NFT mint FX
            if (nftMintFX != null)
            {
                Instantiate(nftMintFX, transform.position, Quaternion.identity);
            }

            // Play voice line
            PlayVoiceLine($"Mission replay minted: {missionName}. Your saga is immortalized.");

            yield return new WaitForSeconds(3f);

            // Record lore
            SoulvanLore.Record($"Replay NFT minted for mission: {missionName}");

            // Disable cinematic camera
            if (cinematicCamera != null)
            {
                cinematicCamera.gameObject.SetActive(false);
            }

            cutscenePlaying = false;
        }

        /// <summary>
        /// Trigger contributor dashboard cutscene.
        /// </summary>
        public void TriggerDashboardCutscene(string contributorId)
        {
            if (cutscenePlaying) return;

            StartCoroutine(PlayDashboardCutscene(contributorId));
        }

        private IEnumerator PlayDashboardCutscene(string contributorId)
        {
            cutscenePlaying = true;

            Debug.Log($"[CutsceneTrigger] 🎬 Playing dashboard cutscene for {contributorId}");

            // Enable cinematic camera
            if (cinematicCamera != null)
            {
                cinematicCamera.gameObject.SetActive(true);
            }

            // Camera sequence
            yield return StartCoroutine(CameraSequence());

            // Spawn rune FX
            if (runeFX != null)
            {
                Instantiate(runeFX, transform.position, Quaternion.identity);
            }

            // Play voice line
            PlayVoiceLine($"Welcome, {contributorId}. Your saga awaits.");

            yield return new WaitForSeconds(3f);

            // Record lore
            SoulvanLore.Record($"Contributor {contributorId} accessed dashboard.");

            // Disable cinematic camera
            if (cinematicCamera != null)
            {
                cinematicCamera.gameObject.SetActive(false);
            }

            cutscenePlaying = false;
        }

        /// <summary>
        /// Execute camera movement sequence through camera points.
        /// </summary>
        private IEnumerator CameraSequence()
        {
            if (cinematicCamera == null || cameraPoints == null || cameraPoints.Length == 0)
            {
                yield break;
            }

            for (int i = 0; i < cameraPoints.Length; i++)
            {
                if (cameraPoints[i] == null) continue;

                // Smooth transition to camera point
                float elapsed = 0f;
                Vector3 startPos = cinematicCamera.transform.position;
                Quaternion startRot = cinematicCamera.transform.rotation;
                Vector3 targetPos = cameraPoints[i].position;
                Quaternion targetRot = cameraPoints[i].rotation;

                while (elapsed < cameraTransitionSpeed)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / cameraTransitionSpeed;

                    cinematicCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
                    cinematicCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

                    yield return null;
                }

                // Hold at camera point
                yield return new WaitForSeconds(2f);
            }
        }

        /// <summary>
        /// Play Soulvan voice line.
        /// </summary>
        private void PlayVoiceLine(string text)
        {
            Debug.Log($"[CutsceneTrigger] 🗣️ Soulvan: \"{text}\"");

            if (audioSource != null && soulvanVoiceLines != null && soulvanVoiceLines.Length > 0)
            {
                AudioClip clip = soulvanVoiceLines[UnityEngine.Random.Range(0, soulvanVoiceLines.Length)];
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Generic cutscene trigger by name.
        /// </summary>
        public void TriggerCutscene(string cutsceneName)
        {
            Debug.Log($"[CutsceneTrigger] Triggering cutscene: {cutsceneName}");

            switch (cutsceneName)
            {
                case "Skyfire_VaultEscape_Intro":
                case "Skyfire_BossIntro":
                case "Skyfire_Extraction":
                    StartCoroutine(PlayGenericCutscene(cutsceneName));
                    break;
                default:
                    Debug.LogWarning($"[CutsceneTrigger] Unknown cutscene: {cutsceneName}");
                    break;
            }
        }

        private IEnumerator PlayGenericCutscene(string name)
        {
            Debug.Log($"[CutsceneTrigger] 🎬 Playing: {name}");

            if (cinematicCamera != null)
            {
                cinematicCamera.gameObject.SetActive(true);
            }

            yield return StartCoroutine(CameraSequence());

            PlayVoiceLine($"Cutscene: {name}");

            yield return new WaitForSeconds(3f);

            if (cinematicCamera != null)
            {
                cinematicCamera.gameObject.SetActive(false);
            }

            SoulvanLore.Record($"Cutscene played: {name}");
        }
    }
}
