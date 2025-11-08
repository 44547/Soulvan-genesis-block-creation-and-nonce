using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soulvan.Systems
{
    /// <summary>
    /// Cinematic cutscene trigger for Saga Export Pack.
    /// Features camera sweeps for lore, badge, replay, and artifact exports.
    /// Includes Soulvan's approval stamp and artifact FX.
    /// </summary>
    public class ExportCutsceneTrigger : MonoBehaviour
    {
        [Header("Cinematic Setup")]
        public Camera cinematicCamera;
        public Transform[] cameraPoints;
        public float transitionSpeed = 2f;

        [Header("Audio")]
        public AudioClip soulvanVoiceLine;
        public AudioClip[] exportVoiceLines; // Different clips for each export type

        [Header("FX")]
        public GameObject artifactFX;
        public GameObject approvalStampFX;
        public GameObject loreFX;
        public GameObject badgeFX;
        public GameObject replayFX;

        [Header("Export Visuals")]
        public GameObject sagaScrollModel;
        public GameObject badgeGalleryModel;
        public GameObject replayVaultModel;
        public GameObject artifactModel;

        /// <summary>
        /// Trigger saga scroll export cutscene.
        /// </summary>
        public void TriggerSagaScrollExport(List<LoreEntry> loreEntries)
        {
            StartCoroutine(PlaySagaScrollCutscene(loreEntries));
        }

        /// <summary>
        /// Trigger badge export cutscene.
        /// </summary>
        public void TriggerBadgeExport(string contributorId, ContributorRole role)
        {
            StartCoroutine(PlayBadgeExportCutscene(contributorId, role));
        }

        /// <summary>
        /// Trigger replay export cutscene.
        /// </summary>
        public void TriggerReplayExport(List<MissionReplay> replays)
        {
            StartCoroutine(PlayReplayExportCutscene(replays));
        }

        /// <summary>
        /// Trigger artifact export cutscene.
        /// </summary>
        public void TriggerArtifactExport(int artifactPower)
        {
            StartCoroutine(PlayArtifactExportCutscene(artifactPower));
        }

        /// <summary>
        /// Play saga scroll export cutscene.
        /// </summary>
        private IEnumerator PlaySagaScrollCutscene(List<LoreEntry> loreEntries)
        {
            Debug.Log("[ExportCutscene] Playing saga scroll export cutscene");

            // Camera sweep
            yield return StartCoroutine(CameraSweep());

            // Spawn scroll model
            if (sagaScrollModel != null)
            {
                GameObject scroll = Instantiate(sagaScrollModel, transform.position, Quaternion.identity);
                yield return new WaitForSeconds(1f);
            }

            // Spawn lore FX
            if (loreFX != null)
            {
                Instantiate(loreFX, transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.5f);

            // Approval stamp
            if (approvalStampFX != null)
            {
                Instantiate(approvalStampFX, transform.position + Vector3.up * 2f, Quaternion.identity);
            }

            // Voice line
            PlayVoiceLine(0);

            // Record to lore
            SoulvanLore.Record("SoulvanSagaExportPack: Saga scroll minted");

            yield return new WaitForSeconds(2f);
        }

        /// <summary>
        /// Play badge export cutscene.
        /// </summary>
        private IEnumerator PlayBadgeExportCutscene(string contributorId, ContributorRole role)
        {
            Debug.Log("[ExportCutscene] Playing badge export cutscene");

            // Camera sweep
            yield return StartCoroutine(CameraSweep());

            // Spawn badge gallery
            if (badgeGalleryModel != null)
            {
                GameObject gallery = Instantiate(badgeGalleryModel, transform.position, Quaternion.identity);
                yield return new WaitForSeconds(1f);
            }

            // Spawn badge FX
            if (badgeFX != null)
            {
                Instantiate(badgeFX, transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.5f);

            // Approval stamp
            if (approvalStampFX != null)
            {
                Instantiate(approvalStampFX, transform.position + Vector3.up * 2f, Quaternion.identity);
            }

            // Voice line
            PlayVoiceLine(1);

            // Record to lore
            SoulvanLore.Record($"SoulvanSagaExportPack: Badges exported for {contributorId} ({role})");

            yield return new WaitForSeconds(2f);
        }

        /// <summary>
        /// Play replay export cutscene.
        /// </summary>
        private IEnumerator PlayReplayExportCutscene(List<MissionReplay> replays)
        {
            Debug.Log("[ExportCutscene] Playing replay export cutscene");

            // Camera sweep
            yield return StartCoroutine(CameraSweep());

            // Spawn replay vault
            if (replayVaultModel != null)
            {
                GameObject vault = Instantiate(replayVaultModel, transform.position, Quaternion.identity);
                yield return new WaitForSeconds(1f);
            }

            // Spawn replay FX
            if (replayFX != null)
            {
                Instantiate(replayFX, transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.5f);

            // Approval stamp
            if (approvalStampFX != null)
            {
                Instantiate(approvalStampFX, transform.position + Vector3.up * 2f, Quaternion.identity);
            }

            // Voice line
            PlayVoiceLine(2);

            // Record to lore
            SoulvanLore.Record($"SoulvanSagaExportPack: {replays.Count} mission replays exported");

            yield return new WaitForSeconds(2f);
        }

        /// <summary>
        /// Play artifact export cutscene.
        /// </summary>
        private IEnumerator PlayArtifactExportCutscene(int artifactPower)
        {
            Debug.Log("[ExportCutscene] Playing artifact export cutscene");

            // Camera sweep (longer for artifact)
            yield return StartCoroutine(CameraSweep());

            // Spawn artifact model
            if (artifactModel != null)
            {
                GameObject artifact = Instantiate(artifactModel, transform.position, Quaternion.identity);
                
                // Rotate artifact
                StartCoroutine(RotateArtifact(artifact.transform));
                
                yield return new WaitForSeconds(1.5f);
            }

            // Spawn artifact FX (glowing rune-bound)
            if (artifactFX != null)
            {
                Instantiate(artifactFX, transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(1f);

            // Pan to approval stamp
            if (approvalStampFX != null)
            {
                Instantiate(approvalStampFX, transform.position + Vector3.up * 3f, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.5f);

            // Voice line
            PlayVoiceLine(3);

            // Record to lore
            SoulvanLore.Record($"SoulvanSagaExportPack artifact minted with power {artifactPower}");

            yield return new WaitForSeconds(3f);
        }

        /// <summary>
        /// Camera sweep through all points.
        /// </summary>
        private IEnumerator CameraSweep()
        {
            for (int i = 0; i < cameraPoints.Length; i++)
            {
                yield return StartCoroutine(TransitionToCamera(cameraPoints[i]));
                yield return new WaitForSeconds(2f);
            }
        }

        /// <summary>
        /// Smooth camera transition.
        /// </summary>
        private IEnumerator TransitionToCamera(Transform targetCamera)
        {
            float elapsed = 0f;
            float duration = 1f / transitionSpeed;

            Vector3 startPos = cinematicCamera.transform.position;
            Quaternion startRot = cinematicCamera.transform.rotation;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                cinematicCamera.transform.position = Vector3.Lerp(startPos, targetCamera.position, t);
                cinematicCamera.transform.rotation = Quaternion.Slerp(startRot, targetCamera.rotation, t);

                yield return null;
            }

            cinematicCamera.transform.position = targetCamera.position;
            cinematicCamera.transform.rotation = targetCamera.rotation;
        }

        /// <summary>
        /// Rotate artifact model.
        /// </summary>
        private IEnumerator RotateArtifact(Transform artifact)
        {
            float elapsed = 0f;
            float duration = 3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                artifact.Rotate(Vector3.up, 60f * Time.deltaTime);
                
                // Pulse scale
                float pulse = 1f + Mathf.Sin(elapsed * 3f) * 0.1f;
                artifact.localScale = Vector3.one * pulse;

                yield return null;
            }
        }

        /// <summary>
        /// Play voice line by index.
        /// </summary>
        private void PlayVoiceLine(int index)
        {
            AudioClip clip = soulvanVoiceLine;

            if (exportVoiceLines != null && index < exportVoiceLines.Length && exportVoiceLines[index] != null)
            {
                clip = exportVoiceLines[index];
            }

            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }
    }
}
