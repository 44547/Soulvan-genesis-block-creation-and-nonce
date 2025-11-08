using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Soulvan Remix Forge Cutscene Kit
 * 
 * Cinematic cutscene system for Remix Forge events:
 * - Divergence Reveal: Glowing rune trails showing alternate saga paths
 * - Echo Ripple: Expanding waves across lore-linked contributors
 * - Scroll Constellation: Floating scrolls orbiting remix nodes
 * - Lineage Path: Camera sweep through remix ancestry
 * - DAO Impact: Ripple propagation from DAO vote events
 */

public class RemixForgeCutsceneKit : MonoBehaviour
{
    [Header("Cutscene Cameras")]
    public Camera cinematicCamera;
    public Camera mainCamera;
    public float transitionSpeed = 2f;
    
    [Header("Cutscene Prefabs")]
    public GameObject divergenceRevealFX;
    public GameObject echoRippleFX;
    public GameObject scrollConstellationFX;
    public GameObject lineagePathFX;
    public GameObject daoImpactFX;
    
    [Header("Audio")]
    public AudioClip divergenceVoice; // "Your legend diverges. The vault remembers."
    public AudioClip echoVoice; // "Your remix echoes through the vault."
    public AudioClip scrollVoice; // "Your scrolls orbit the forge."
    public AudioClip lineageVoice; // "The lineage reveals your saga."
    public AudioClip daoVoice; // "The DAO ripples through the remix graph."
    
    [Header("Camera Paths")]
    public Transform[] divergenceCameraPoints;
    public Transform[] echoCameraPoints;
    public Transform[] scrollCameraPoints;
    public Transform[] lineageCameraPoints;
    public Transform[] daoCameraPoints;
    
    private bool isCutscenePlaying = false;
    private RemixForgeGraph graph;

    void Start()
    {
        graph = GetComponent<RemixForgeGraph>();
        
        if (cinematicCamera != null)
        {
            cinematicCamera.gameObject.SetActive(false);
        }
    }

    // ============================
    // Divergence Reveal Cutscene
    // ============================

    public void TriggerDivergenceReveal(string contributorId, string remixId)
    {
        if (isCutscenePlaying) return;
        StartCoroutine(PlayDivergenceReveal(contributorId, remixId));
    }

    IEnumerator PlayDivergenceReveal(string contributorId, string remixId)
    {
        isCutscenePlaying = true;
        
        // Activate cinematic camera
        cinematicCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);
        
        // Get contributor node position
        Vector3 nodePosition = Vector3.zero;
        if (graph != null)
        {
            // Focus camera on contributor node
            graph.FocusOnNode(contributorId);
            yield return new WaitForSeconds(1f);
        }
        
        // Spawn divergence FX
        if (divergenceRevealFX != null)
        {
            GameObject fx = Instantiate(divergenceRevealFX, nodePosition, Quaternion.identity);
            Destroy(fx, 5f);
        }
        
        // Camera sweep through divergence points
        for (int i = 0; i < divergenceCameraPoints.Length; i++)
        {
            yield return StartCoroutine(MoveCameraToPoint(divergenceCameraPoints[i], 2f));
        }
        
        // Play voice line
        if (divergenceVoice != null)
        {
            AudioSource.PlayClipAtPoint(divergenceVoice, cinematicCamera.transform.position);
        }
        
        yield return new WaitForSeconds(2f);
        
        // Return to main camera
        cinematicCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
        
        isCutscenePlaying = false;
        
        SoulvanLore.Record($"Divergence Reveal cutscene: {remixId} by {contributorId}");
    }

    // ============================
    // Echo Ripple Cutscene
    // ============================

    public void TriggerEchoRipple(string sourceNodeId, List<string> echoNodeIds)
    {
        if (isCutscenePlaying) return;
        StartCoroutine(PlayEchoRipple(sourceNodeId, echoNodeIds));
    }

    IEnumerator PlayEchoRipple(string sourceId, List<string> echoIds)
    {
        isCutscenePlaying = true;
        
        cinematicCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);
        
        // Focus on source node
        if (graph != null)
        {
            graph.FocusOnNode(sourceId);
        }
        
        yield return new WaitForSeconds(1.5f);
        
        // Spawn echo ripple FX at source
        Vector3 sourcePosition = Vector3.zero;
        if (echoRippleFX != null)
        {
            GameObject fx = Instantiate(echoRippleFX, sourcePosition, Quaternion.identity);
            Destroy(fx, 8f);
        }
        
        // Pan camera across echo nodes
        for (int i = 0; i < echoCameraPoints.Length && i < echoIds.Count; i++)
        {
            if (graph != null)
            {
                graph.FocusOnNode(echoIds[i]);
            }
            yield return new WaitForSeconds(0.8f);
        }
        
        // Play voice line
        if (echoVoice != null)
        {
            AudioSource.PlayClipAtPoint(echoVoice, cinematicCamera.transform.position);
        }
        
        yield return new WaitForSeconds(2f);
        
        cinematicCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
        
        isCutscenePlaying = false;
        
        SoulvanLore.Record($"Echo Ripple cutscene: {sourceId} → {echoIds.Count} nodes");
    }

    // ============================
    // Scroll Constellation Cutscene
    // ============================

    public void TriggerScrollConstellation(string contributorId)
    {
        if (isCutscenePlaying) return;
        StartCoroutine(PlayScrollConstellation(contributorId));
    }

    IEnumerator PlayScrollConstellation(string contributorId)
    {
        isCutscenePlaying = true;
        
        cinematicCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);
        
        // Get node position
        Vector3 centerPosition = Vector3.zero;
        if (graph != null)
        {
            graph.FocusOnNode(contributorId);
            yield return new WaitForSeconds(1f);
        }
        
        // Spawn scroll constellation FX
        if (scrollConstellationFX != null)
        {
            GameObject fx = Instantiate(scrollConstellationFX, centerPosition, Quaternion.identity);
            Destroy(fx, 10f);
        }
        
        // Orbit camera around node
        float orbitDuration = 4f;
        float elapsed = 0f;
        float orbitRadius = 12f;
        
        while (elapsed < orbitDuration)
        {
            float angle = (elapsed / orbitDuration) * 360f;
            float rad = angle * Mathf.Deg2Rad;
            
            Vector3 orbitPos = centerPosition + new Vector3(
                Mathf.Cos(rad) * orbitRadius,
                8f,
                Mathf.Sin(rad) * orbitRadius
            );
            
            cinematicCamera.transform.position = orbitPos;
            cinematicCamera.transform.LookAt(centerPosition);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Play voice line
        if (scrollVoice != null)
        {
            AudioSource.PlayClipAtPoint(scrollVoice, cinematicCamera.transform.position);
        }
        
        yield return new WaitForSeconds(2f);
        
        cinematicCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
        
        isCutscenePlaying = false;
        
        SoulvanLore.Record($"Scroll Constellation cutscene: {contributorId}");
    }

    // ============================
    // Lineage Path Cutscene
    // ============================

    public void TriggerLineagePath(string contributorId, List<string> ancestorIds)
    {
        if (isCutscenePlaying) return;
        StartCoroutine(PlayLineagePath(contributorId, ancestorIds));
    }

    IEnumerator PlayLineagePath(string contributorId, List<string> ancestorIds)
    {
        isCutscenePlaying = true;
        
        cinematicCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);
        
        // Spawn lineage path FX
        if (lineagePathFX != null && graph != null)
        {
            Vector3 startPos = Vector3.zero; // Contributor node position
            GameObject fx = Instantiate(lineagePathFX, startPos, Quaternion.identity);
            Destroy(fx, 8f);
        }
        
        // Highlight lineage in graph
        if (graph != null)
        {
            graph.TriggerLineageGlow(contributorId, ancestorIds);
        }
        
        // Camera sweep through lineage path
        for (int i = 0; i < lineageCameraPoints.Length && i < ancestorIds.Count; i++)
        {
            yield return StartCoroutine(MoveCameraToPoint(lineageCameraPoints[i], 1.5f));
        }
        
        // Play voice line
        if (lineageVoice != null)
        {
            AudioSource.PlayClipAtPoint(lineageVoice, cinematicCamera.transform.position);
        }
        
        yield return new WaitForSeconds(2f);
        
        cinematicCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
        
        isCutscenePlaying = false;
        
        SoulvanLore.Record($"Lineage Path cutscene: {contributorId} ancestry");
    }

    // ============================
    // DAO Impact Cutscene
    // ============================

    public void TriggerDAOImpact(string proposalId, string sourceNodeId, int votePower)
    {
        if (isCutscenePlaying) return;
        StartCoroutine(PlayDAOImpact(proposalId, sourceNodeId, votePower));
    }

    IEnumerator PlayDAOImpact(string proposalId, string sourceId, int power)
    {
        isCutscenePlaying = true;
        
        cinematicCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);
        
        // Focus on source node
        if (graph != null)
        {
            graph.FocusOnNode(sourceId);
            graph.TriggerDAORipple(proposalId, sourceId, power);
        }
        
        yield return new WaitForSeconds(1f);
        
        // Spawn DAO impact FX
        Vector3 sourcePosition = Vector3.zero;
        if (daoImpactFX != null)
        {
            GameObject fx = Instantiate(daoImpactFX, sourcePosition, Quaternion.identity);
            Destroy(fx, 6f);
        }
        
        // Camera sweep through DAO camera points
        for (int i = 0; i < daoCameraPoints.Length; i++)
        {
            yield return StartCoroutine(MoveCameraToPoint(daoCameraPoints[i], 1.2f));
        }
        
        // Play voice line
        if (daoVoice != null)
        {
            AudioSource.PlayClipAtPoint(daoVoice, cinematicCamera.transform.position);
        }
        
        yield return new WaitForSeconds(2f);
        
        cinematicCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
        
        isCutscenePlaying = false;
        
        SoulvanLore.Record($"DAO Impact cutscene: Proposal {proposalId}");
    }

    // ============================
    // Camera Utilities
    // ============================

    IEnumerator MoveCameraToPoint(Transform targetPoint, float duration)
    {
        if (targetPoint == null) yield break;
        
        Vector3 startPos = cinematicCamera.transform.position;
        Quaternion startRot = cinematicCamera.transform.rotation;
        
        Vector3 endPos = targetPoint.position;
        Quaternion endRot = targetPoint.rotation;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            cinematicCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            cinematicCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        cinematicCamera.transform.position = endPos;
        cinematicCamera.transform.rotation = endRot;
    }

    public void StopCurrentCutscene()
    {
        StopAllCoroutines();
        
        cinematicCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
        
        isCutscenePlaying = false;
    }

    // ============================
    // Public API for External Triggers
    // ============================

    public bool IsPlaying()
    {
        return isCutscenePlaying;
    }

    public void TriggerCutsceneByType(string cutsceneType, Dictionary<string, object> parameters)
    {
        switch (cutsceneType.ToLower())
        {
            case "divergence":
                TriggerDivergenceReveal(
                    parameters["contributorId"] as string,
                    parameters["remixId"] as string
                );
                break;
            
            case "echo":
                TriggerEchoRipple(
                    parameters["sourceNodeId"] as string,
                    parameters["echoNodeIds"] as List<string>
                );
                break;
            
            case "scroll":
                TriggerScrollConstellation(parameters["contributorId"] as string);
                break;
            
            case "lineage":
                TriggerLineagePath(
                    parameters["contributorId"] as string,
                    parameters["ancestorIds"] as List<string>
                );
                break;
            
            case "dao":
                TriggerDAOImpact(
                    parameters["proposalId"] as string,
                    parameters["sourceNodeId"] as string,
                    (int)parameters["votePower"]
                );
                break;
            
            default:
                Debug.LogWarning($"[RemixForgeCutsceneKit] Unknown cutscene type: {cutsceneType}");
                break;
        }
    }
}
