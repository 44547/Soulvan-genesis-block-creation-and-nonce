using System.Collections;
using UnityEngine;

namespace Soulvan.Systems
{
    /// <summary>
    /// Cinematic cutscene trigger for Architect Kit role upgrades.
    /// Features camera sweeps, scroll FX, and Soulvan's approval stamp.
    /// </summary>
    public class ArchitectCutsceneTrigger : MonoBehaviour
    {
        [Header("Cinematic Setup")]
        public Camera cinematicCamera;
        public Transform[] cameraPoints;
        public float transitionSpeed = 2f;

        [Header("Audio")]
        public AudioClip soulvanVoiceLine;
        public AudioClip[] roleUpgradeVoiceLines; // 4 clips for each role

        [Header("FX")]
        public GameObject scrollFX;
        public GameObject approvalStampFX;
        public GameObject roleLadderGlowFX;

        [Header("Upgrade Visuals")]
        public GameObject initiateStamp;
        public GameObject builderStamp;
        public GameObject architectStamp;
        public GameObject oracleStamp;

        /// <summary>
        /// Trigger role upgrade cutscene.
        /// </summary>
        public void TriggerCutscene(ContributorRole newRole)
        {
            StartCoroutine(PlayCutscene(newRole));
        }

        /// <summary>
        /// Play cinematic sequence.
        /// </summary>
        private IEnumerator PlayCutscene(ContributorRole newRole)
        {
            Debug.Log($"[ArchitectCutscene] Playing cutscene for {newRole} upgrade");

            // Camera sweep through points
            for (int i = 0; i < cameraPoints.Length; i++)
            {
                yield return StartCoroutine(TransitionToCamera(cameraPoints[i]));
                yield return new WaitForSeconds(2f);
            }

            // Zoom on scroll
            if (scrollFX != null)
            {
                Instantiate(scrollFX, transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(1f);

            // Pan to Soulvan's approval stamp
            if (approvalStampFX != null)
            {
                GameObject stamp = Instantiate(approvalStampFX, transform.position + Vector3.up * 2f, Quaternion.identity);
                
                // Spawn role-specific stamp
                SpawnRoleStamp(newRole, stamp.transform.position);
            }

            yield return new WaitForSeconds(0.5f);

            // Play voice line
            AudioClip voiceLine = GetRoleVoiceLine(newRole);
            if (voiceLine != null)
            {
                AudioSource.PlayClipAtPoint(voiceLine, transform.position);
            }

            // Role ladder glow
            if (roleLadderGlowFX != null)
            {
                Instantiate(roleLadderGlowFX, transform.position, Quaternion.identity);
            }

            // Record to lore
            SoulvanLore.Record($"Contributor upgraded via SoulvanArchitectKit to {newRole}");

            Debug.Log("[ArchitectCutscene] Cutscene complete");
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
        /// Get voice line for role.
        /// </summary>
        private AudioClip GetRoleVoiceLine(ContributorRole role)
        {
            if (roleUpgradeVoiceLines == null || roleUpgradeVoiceLines.Length < 4)
            {
                return soulvanVoiceLine;
            }

            return roleUpgradeVoiceLines[(int)role];
        }

        /// <summary>
        /// Spawn role-specific approval stamp.
        /// </summary>
        private void SpawnRoleStamp(ContributorRole role, Vector3 position)
        {
            GameObject stamp = null;

            switch (role)
            {
                case ContributorRole.Initiate:
                    stamp = initiateStamp;
                    break;
                case ContributorRole.Builder:
                    stamp = builderStamp;
                    break;
                case ContributorRole.Architect:
                    stamp = architectStamp;
                    break;
                case ContributorRole.Oracle:
                    stamp = oracleStamp;
                    break;
            }

            if (stamp != null)
            {
                Instantiate(stamp, position, Quaternion.identity);
                Debug.Log($"[ArchitectCutscene] Spawned {role} stamp");
            }
        }
    }
}
