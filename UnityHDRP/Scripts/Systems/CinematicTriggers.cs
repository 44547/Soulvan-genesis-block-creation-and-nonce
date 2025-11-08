using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soulvan.Systems
{
    /// <summary>
    /// Consolidated cutscene triggers for all Soulvan systems.
    /// Handles cinematic sequences with camera sweeps, FX, and voice overlays.
    /// </summary>

    // SEASONAL LORE PACK CUTSCENE
    public class SeasonalCutsceneTrigger : MonoBehaviour
    {
        public Camera cinematicCamera;
        public Transform[] cameraPoints;
        public AudioClip soulvanVoiceLine;
        public GameObject seasonalFX;

        public void TriggerCutscene()
        {
            StartCoroutine(PlayCutscene());
        }

        IEnumerator PlayCutscene()
        {
            for (int i = 0; i < cameraPoints.Length; i++)
            {
                yield return StartCoroutine(TransitionCamera(cameraPoints[i]));
                yield return new WaitForSeconds(2f);
            }

            if (soulvanVoiceLine != null)
                AudioSource.PlayClipAtPoint(soulvanVoiceLine, transform.position);
            
            if (seasonalFX != null)
                Instantiate(seasonalFX, transform.position, Quaternion.identity);
            
            SoulvanLore.Record("SoulvanSeasonalLorePack cutscene triggered");
        }

        IEnumerator TransitionCamera(Transform target)
        {
            float elapsed = 0f;
            float duration = 0.5f;
            Vector3 startPos = cinematicCamera.transform.position;
            Quaternion startRot = cinematicCamera.transform.rotation;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                cinematicCamera.transform.position = Vector3.Lerp(startPos, target.position, t);
                cinematicCamera.transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);
                yield return null;
            }
        }
    }

    // MULTIVERSE BRIDGE CUTSCENE
    public class MultiverseCutsceneTrigger : MonoBehaviour
    {
        public Camera cinematicCamera;
        public Transform[] cameraPoints;
        public AudioClip soulvanVoiceLine;
        public GameObject portalFX;

        public void TriggerCutscene()
        {
            StartCoroutine(PlayCutscene());
        }

        IEnumerator PlayCutscene()
        {
            for (int i = 0; i < cameraPoints.Length; i++)
            {
                cinematicCamera.transform.position = cameraPoints[i].position;
                cinematicCamera.transform.rotation = cameraPoints[i].rotation;
                yield return new WaitForSeconds(2f);
            }

            if (soulvanVoiceLine != null)
                AudioSource.PlayClipAtPoint(soulvanVoiceLine, transform.position);
            
            if (portalFX != null)
                Instantiate(portalFX, transform.position, Quaternion.identity);
            
            SoulvanLore.Record("SoulvanMultiverseBridge cutscene triggered");
        }
    }

    // CHRONO FORGE CUTSCENE
    public class ChronoCutsceneTrigger : MonoBehaviour
    {
        public Camera cinematicCamera;
        public Transform[] cameraPoints;
        public AudioClip soulvanVoiceLine;
        public GameObject rippleFX;

        public void TriggerCutscene()
        {
            StartCoroutine(PlayCutscene());
        }

        IEnumerator PlayCutscene()
        {
            for (int i = 0; i < cameraPoints.Length; i++)
            {
                cinematicCamera.transform.position = cameraPoints[i].position;
                cinematicCamera.transform.rotation = cameraPoints[i].rotation;
                yield return new WaitForSeconds(2f);
            }

            if (soulvanVoiceLine != null)
                AudioSource.PlayClipAtPoint(soulvanVoiceLine, transform.position);
            
            if (rippleFX != null)
                Instantiate(rippleFX, transform.position, Quaternion.identity);
            
            SoulvanLore.Record("SoulvanChronoForge cutscene triggered");
        }
    }

    // LEGEND MINT CUTSCENE
    public class LegendMintCutsceneTrigger : MonoBehaviour
    {
        public Camera cinematicCamera;
        public Transform[] cameraPoints;
        public AudioClip soulvanVoiceLine;
        public GameObject scrollFX;

        public void TriggerCutscene()
        {
            StartCoroutine(PlayCutscene());
        }

        IEnumerator PlayCutscene()
        {
            for (int i = 0; i < cameraPoints.Length; i++)
            {
                cinematicCamera.transform.position = cameraPoints[i].position;
                cinematicCamera.transform.rotation = cameraPoints[i].rotation;
                yield return new WaitForSeconds(2f);
            }

            if (soulvanVoiceLine != null)
                AudioSource.PlayClipAtPoint(soulvanVoiceLine, transform.position);
            
            if (scrollFX != null)
                Instantiate(scrollFX, transform.position, Quaternion.identity);
            
            SoulvanLore.Record("SoulvanLegendMint cutscene triggered");
        }
    }

    // CONTRIBUTOR FORGE CUTSCENE
    public class ContributorForgeCutsceneTrigger : MonoBehaviour
    {
        public Camera cinematicCamera;
        public Transform[] cameraPoints;
        public AudioClip soulvanVoiceLine;
        public GameObject forgeFX;

        public void TriggerCutscene()
        {
            StartCoroutine(PlayCutscene());
        }

        IEnumerator PlayCutscene()
        {
            for (int i = 0; i < cameraPoints.Length; i++)
            {
                cinematicCamera.transform.position = cameraPoints[i].position;
                cinematicCamera.transform.rotation = cameraPoints[i].rotation;
                yield return new WaitForSeconds(2f);
            }

            if (soulvanVoiceLine != null)
                AudioSource.PlayClipAtPoint(soulvanVoiceLine, transform.position);
            
            if (forgeFX != null)
                Instantiate(forgeFX, transform.position, Quaternion.identity);
            
            SoulvanLore.Record("SoulvanContributorForge cutscene triggered");
        }
    }

    // FORGE NETWORK CUTSCENE
    public class ForgeNetworkCutsceneTrigger : MonoBehaviour
    {
        public Camera cinematicCamera;
        public Transform[] cameraPoints;
        public AudioClip soulvanVoiceLine;
        public GameObject pulseFX;

        public void TriggerCutscene()
        {
            StartCoroutine(PlayCutscene());
        }

        IEnumerator PlayCutscene()
        {
            for (int i = 0; i < cameraPoints.Length; i++)
            {
                cinematicCamera.transform.position = cameraPoints[i].position;
                cinematicCamera.transform.rotation = cameraPoints[i].rotation;
                yield return new WaitForSeconds(2f);
            }

            if (soulvanVoiceLine != null)
                AudioSource.PlayClipAtPoint(soulvanVoiceLine, transform.position);
            
            if (pulseFX != null)
                Instantiate(pulseFX, transform.position, Quaternion.identity);
            
            SoulvanLore.Record("SoulvanForgeNetwork cutscene triggered");
        }
    }

    // MYTHIC REPLAY ENGINE CUTSCENE
    public class MythicReplayCutsceneTrigger : MonoBehaviour
    {
        public Camera cinematicCamera;
        public Transform[] cameraPoints;
        public AudioClip soulvanVoiceLine;
        public GameObject replayFX;

        public void TriggerCutscene()
        {
            StartCoroutine(PlayCutscene());
        }

        IEnumerator PlayCutscene()
        {
            for (int i = 0; i < cameraPoints.Length; i++)
            {
                cinematicCamera.transform.position = cameraPoints[i].position;
                cinematicCamera.transform.rotation = cameraPoints[i].rotation;
                yield return new WaitForSeconds(2f);
            }

            if (soulvanVoiceLine != null)
                AudioSource.PlayClipAtPoint(soulvanVoiceLine, transform.position);
            
            if (replayFX != null)
                Instantiate(replayFX, transform.position, Quaternion.identity);
            
            SoulvanLore.Record("SoulvanMythicReplayEngine cutscene triggered");
        }
    }

    // LORE VAULT CUTSCENE
    public class LoreVaultCutsceneTrigger : MonoBehaviour
    {
        public Camera cinematicCamera;
        public Transform[] cameraPoints;
        public AudioClip soulvanVoiceLine;
        public GameObject archiveFX;

        public void TriggerCutscene()
        {
            StartCoroutine(PlayCutscene());
        }

        IEnumerator PlayCutscene()
        {
            for (int i = 0; i < cameraPoints.Length; i++)
            {
                cinematicCamera.transform.position = cameraPoints[i].position;
                cinematicCamera.transform.rotation = cameraPoints[i].rotation;
                yield return new WaitForSeconds(2f);
            }

            if (soulvanVoiceLine != null)
                AudioSource.PlayClipAtPoint(soulvanVoiceLine, transform.position);
            
            if (archiveFX != null)
                Instantiate(archiveFX, transform.position, Quaternion.identity);
            
            SoulvanLore.Record("SoulvanLoreVault cutscene triggered");
        }
    }

    // SAGA REMIX LAB CUTSCENE
    public class RemixLabCutsceneTrigger : MonoBehaviour
    {
        public Camera cinematicCamera;
        public Transform[] cameraPoints;
        public AudioClip soulvanVoiceLine;
        public GameObject remixFX;

        public void TriggerCutscene()
        {
            StartCoroutine(PlayCutscene());
        }

        IEnumerator PlayCutscene()
        {
            for (int i = 0; i < cameraPoints.Length; i++)
            {
                cinematicCamera.transform.position = cameraPoints[i].position;
                cinematicCamera.transform.rotation = cameraPoints[i].rotation;
                yield return new WaitForSeconds(2f);
            }

            if (soulvanVoiceLine != null)
                AudioSource.PlayClipAtPoint(soulvanVoiceLine, transform.position);
            
            if (remixFX != null)
                Instantiate(remixFX, transform.position, Quaternion.identity);
            
            SoulvanLore.Record("SoulvanSagaRemixLab cutscene triggered");
        }
    }

    // BADGE UNLOCK CUTSCENE
    public class BadgeUnlockCutsceneTrigger : MonoBehaviour
    {
        public Camera cinematicCamera;
        public Transform[] cameraPoints;
        public AudioClip soulvanVoiceLine;
        public GameObject badgeFX;

        public void TriggerCutscene()
        {
            StartCoroutine(PlayCutscene());
        }

        IEnumerator PlayCutscene()
        {
            for (int i = 0; i < cameraPoints.Length; i++)
            {
                cinematicCamera.transform.position = cameraPoints[i].position;
                cinematicCamera.transform.rotation = cameraPoints[i].rotation;
                yield return new WaitForSeconds(2f);
            }

            if (soulvanVoiceLine != null)
                AudioSource.PlayClipAtPoint(soulvanVoiceLine, transform.position);
            
            if (badgeFX != null)
                Instantiate(badgeFX, transform.position, Quaternion.identity);
            
            SoulvanLore.Record("SoulvanContributorBadgeSystem cutscene triggered");
        }
    }
}
