using System.Collections;
using UnityEngine;

namespace Soulvan.Systems
{
    /// <summary>
    /// Cinematic cutscene trigger for DAO Hall voting moments.
    /// Features sweeps across podiums, zoom on vote FX, pan to Soulvan's throne.
    /// </summary>
    public class DAOHallCutsceneTrigger : MonoBehaviour
    {
        [Header("Cinematic Setup")]
        public Camera cinematicCamera;
        public Transform[] cameraPoints;
        public float transitionSpeed = 2f;

        [Header("Audio")]
        public AudioClip soulvanVoiceLine;
        public AudioClip[] voteVoiceLines;

        [Header("FX")]
        public GameObject voteFX;
        public GameObject proposalPassFX;
        public GameObject runeFlareFX;

        /// <summary>
        /// Trigger DAO vote cutscene.
        /// </summary>
        public void TriggerCutscene()
        {
            StartCoroutine(PlayCutscene());
        }

        /// <summary>
        /// Play cinematic sequence.
        /// </summary>
        private IEnumerator PlayCutscene()
        {
            Debug.Log("[DAOHallCutscene] Playing vote cutscene");

            // Sweep across podiums
            for (int i = 0; i < cameraPoints.Length; i++)
            {
                yield return StartCoroutine(TransitionToCamera(cameraPoints[i]));
                yield return new WaitForSeconds(2f);
            }

            // Zoom on vote FX
            if (voteFX != null)
            {
                Instantiate(voteFX, transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(1f);

            // Pan to Soulvan's throne
            if (cameraPoints.Length > 0)
            {
                yield return StartCoroutine(TransitionToCamera(cameraPoints[cameraPoints.Length - 1]));
            }

            // Play voice line
            if (soulvanVoiceLine != null)
            {
                AudioSource.PlayClipAtPoint(soulvanVoiceLine, transform.position);
            }

            // Rune flare
            if (runeFlareFX != null)
            {
                Instantiate(runeFlareFX, transform.position, Quaternion.identity);
            }

            // Record to lore
            SoulvanLore.Record("SoulvanCityDAOHub vote cutscene triggered");

            yield return new WaitForSeconds(2f);
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
    }
}
