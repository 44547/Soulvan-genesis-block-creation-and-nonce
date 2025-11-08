using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/**
 * Soulvan Remix Forge Graph Components
 * 
 * Visual components for the Remix Forge Graph:
 * - RemixNode: Contributor node with tier-based visuals
 * - EchoTrail: Animated connection showing saga remix lineage
 * - ScrollOrbit: Floating lore scrolls orbiting remix nodes
 * - RemixLineagePath: Glowing path showing remix ancestry
 */

// ============================
// Remix Node Component
// ============================

public class RemixNode : MonoBehaviour
{
    [Header("Visual Components")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI tierText;
    public TextMeshProUGUI statsText;
    public GameObject nodeBackground;
    public Light nodeLight;
    public ParticleSystem glowFX;
    
    [Header("Node Properties")]
    public float glowIntensity = 1f;
    public float pulseSpeed = 2f;
    public bool isActive = false;
    
    private ContributorNodeData data;
    private Color tierColor;
    private float baseScale = 1f;
    private float lastActivityTime = 0f;

    void Start()
    {
        baseScale = transform.localScale.x;
    }

    void Update()
    {
        if (isActive)
        {
            AnimateNodePulse();
            UpdateGlowDecay();
        }
    }

    public void SetData(ContributorNodeData nodeData)
    {
        data = nodeData;
        
        if (nameText != null)
            nameText.text = nodeData.name;
        
        if (tierText != null)
            tierText.text = nodeData.tier;
        
        if (statsText != null)
            statsText.text = $"Remixes: {nodeData.remixCount}\nLore: {nodeData.loreLinks}";
        
        isActive = true;
        lastActivityTime = Time.time;
    }

    public ContributorNodeData GetData()
    {
        return data;
    }

    public void SetColor(Color color)
    {
        tierColor = color;
        
        if (nodeBackground != null)
        {
            var renderer = nodeBackground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", color * glowIntensity);
            }
        }
        
        if (nodeLight != null)
        {
            nodeLight.color = color;
            nodeLight.intensity = glowIntensity * 2f;
        }
        
        if (glowFX != null)
        {
            var main = glowFX.main;
            main.startColor = color;
        }
    }

    public void SetGlowIntensity(float intensity)
    {
        glowIntensity = intensity;
        lastActivityTime = Time.time;
        
        if (nodeLight != null)
        {
            nodeLight.intensity = intensity * 2f;
        }
        
        if (nodeBackground != null)
        {
            var renderer = nodeBackground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.SetColor("_EmissionColor", tierColor * intensity);
            }
        }
    }

    void AnimateNodePulse()
    {
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.1f * glowIntensity;
        transform.localScale = Vector3.one * baseScale * pulse;
    }

    void UpdateGlowDecay()
    {
        // Decay glow intensity over 1 hour (3600 seconds)
        float timeSinceActivity = Time.time - lastActivityTime;
        float decayedIntensity = Mathf.Max(0.2f, 1f - (timeSinceActivity / 3600f));
        
        if (glowIntensity > decayedIntensity)
        {
            glowIntensity = Mathf.Lerp(glowIntensity, decayedIntensity, Time.deltaTime * 0.1f);
            SetGlowIntensity(glowIntensity);
        }
    }

    public void AnimatePulse()
    {
        StartCoroutine(PulseAnimation());
    }

    IEnumerator PulseAnimation()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        float originalIntensity = glowIntensity;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float intensity = originalIntensity + Mathf.Sin(t * Mathf.PI) * 1f;
            SetGlowIntensity(intensity);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        SetGlowIntensity(originalIntensity);
    }

    public void HighlightNode(bool highlight)
    {
        if (highlight)
        {
            SetGlowIntensity(2f);
            if (glowFX != null && !glowFX.isPlaying)
                glowFX.Play();
        }
        else
        {
            SetGlowIntensity(1f);
            if (glowFX != null && glowFX.isPlaying)
                glowFX.Stop();
        }
    }
}

// ============================
// Echo Trail Component
// ============================

public class EchoTrail : MonoBehaviour
{
    [Header("Trail Properties")]
    public LineRenderer lineRenderer;
    public float flowSpeed = 2f;
    public float pulseWidth = 0.2f;
    public string intensity = "medium";
    
    [Header("Visual Effects")]
    public Gradient trailGradient;
    public AnimationCurve widthCurve;
    
    private Transform fromNode;
    private Transform toNode;
    private string context;
    private float flowOffset = 0f;

    void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
        }
        
        ConfigureLineRenderer();
    }

    void ConfigureLineRenderer()
    {
        lineRenderer.positionCount = 20;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = GetIntensityColor();
        
        if (trailGradient != null)
        {
            lineRenderer.colorGradient = trailGradient;
        }
    }

    void Update()
    {
        if (fromNode != null && toNode != null)
        {
            UpdateLinkPosition();
            AnimateFlow();
        }
    }

    public void SetConnection(Transform from, Transform to, string intensityLevel)
    {
        fromNode = from;
        toNode = to;
        intensity = intensityLevel;
        context = $"Echo: {from.name} → {to.name}";
        
        lineRenderer.material.color = GetIntensityColor();
    }

    void UpdateLinkPosition()
    {
        if (fromNode == null || toNode == null) return;
        
        Vector3 start = fromNode.position;
        Vector3 end = toNode.position;
        
        // Create curved path using Bezier curve
        Vector3 midPoint = (start + end) / 2f;
        Vector3 perpendicular = Vector3.Cross((end - start).normalized, Vector3.up);
        Vector3 controlPoint = midPoint + perpendicular * 5f;
        
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            float t = i / (float)(lineRenderer.positionCount - 1);
            Vector3 point = CalculateBezierPoint(t, start, controlPoint, end);
            lineRenderer.SetPosition(i, point);
        }
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        
        Vector3 point = uu * p0;
        point += 2 * u * t * p1;
        point += tt * p2;
        
        return point;
    }

    void AnimateFlow()
    {
        flowOffset += Time.deltaTime * flowSpeed;
        
        // Animate line width with flowing pulse
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            float t = (i / (float)lineRenderer.positionCount + flowOffset) % 1f;
            float width = 0.1f + Mathf.Sin(t * Mathf.PI * 2f) * pulseWidth;
            lineRenderer.widthMultiplier = width;
        }
        
        // Reset offset to prevent overflow
        if (flowOffset > 100f) flowOffset = 0f;
    }

    Color GetIntensityColor()
    {
        switch (intensity.ToLower())
        {
            case "low":
                return new Color(0.5f, 0.5f, 1f, 0.3f); // Faint blue
            case "medium":
                return new Color(0.5f, 1f, 1f, 0.6f); // Cyan
            case "high":
                return new Color(1f, 1f, 0f, 0.9f); // Gold
            default:
                return new Color(0.5f, 1f, 1f, 0.6f);
        }
    }

    public void PulseTrail()
    {
        StartCoroutine(PulseAnimation());
    }

    IEnumerator PulseAnimation()
    {
        float duration = 1f;
        float elapsed = 0f;
        float originalWidth = lineRenderer.widthMultiplier;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float width = originalWidth + Mathf.Sin(t * Mathf.PI) * 0.3f;
            lineRenderer.widthMultiplier = width;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        lineRenderer.widthMultiplier = originalWidth;
    }
}

// ============================
// Scroll Orbit Component
// ============================

public class ScrollOrbit : MonoBehaviour
{
    [Header("Scroll Properties")]
    public TextMeshProUGUI scrollTitle;
    public GameObject scrollModel;
    public Light scrollGlow;
    
    [Header("Orbit Settings")]
    public float orbitRadius = 3f;
    public float orbitSpeed = 30f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;
    
    private ScrollData data;
    private float orbitAngle = 0f;
    private Vector3 centerPosition;
    private bool isInteractive = false;

    void Start()
    {
        centerPosition = transform.parent != null ? transform.parent.position : transform.position;
        orbitAngle = Random.Range(0f, 360f);
    }

    void Update()
    {
        AnimateOrbit();
        AnimateBob();
        
        if (isInteractive)
        {
            transform.Rotate(Vector3.up, 50f * Time.deltaTime);
        }
    }

    public void SetData(ScrollData scrollData)
    {
        data = scrollData;
        
        if (scrollTitle != null)
        {
            scrollTitle.text = scrollData.title;
        }
        
        if (scrollGlow != null)
        {
            scrollGlow.color = GetFormatColor(scrollData.format);
        }
        
        isInteractive = true;
    }

    public void SetOrbitRadius(float radius)
    {
        orbitRadius = radius;
    }

    void AnimateOrbit()
    {
        orbitAngle += orbitSpeed * Time.deltaTime;
        if (orbitAngle >= 360f) orbitAngle -= 360f;
        
        float rad = orbitAngle * Mathf.Deg2Rad;
        float x = Mathf.Cos(rad) * orbitRadius;
        float z = Mathf.Sin(rad) * orbitRadius;
        
        transform.localPosition = new Vector3(x, transform.localPosition.y, z);
    }

    void AnimateBob()
    {
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        Vector3 localPos = transform.localPosition;
        localPos.y = bobOffset;
        transform.localPosition = localPos;
    }

    Color GetFormatColor(string format)
    {
        switch (format.ToLower())
        {
            case "scroll":
                return new Color(1f, 1f, 0.8f); // Parchment
            case "nft":
                return new Color(1f, 0.5f, 1f); // Magenta
            case "bundle":
                return new Color(0.5f, 1f, 0.5f); // Green
            default:
                return Color.white;
        }
    }

    void OnMouseEnter()
    {
        if (scrollGlow != null)
        {
            scrollGlow.intensity = 3f;
        }
        
        if (scrollModel != null)
        {
            scrollModel.transform.localScale = Vector3.one * 1.2f;
        }
    }

    void OnMouseExit()
    {
        if (scrollGlow != null)
        {
            scrollGlow.intensity = 1f;
        }
        
        if (scrollModel != null)
        {
            scrollModel.transform.localScale = Vector3.one;
        }
    }

    void OnMouseDown()
    {
        Debug.Log($"[ScrollOrbit] Clicked: {data.title}");
        // Trigger scroll preview or export
    }
}

// ============================
// Remix Lineage Path Component
// ============================

public class RemixLineagePath : MonoBehaviour
{
    [Header("Path Properties")]
    public LineRenderer pathRenderer;
    public ParticleSystem ancestryFX;
    public Color lineageColor = new Color(1f, 1f, 0f, 0.8f);
    
    [Header("Animation")]
    public float glowPulseSpeed = 1f;
    public bool animateGlow = true;
    
    private List<Transform> ancestorNodes = new List<Transform>();

    void Start()
    {
        if (pathRenderer == null)
        {
            pathRenderer = GetComponent<LineRenderer>();
        }
        
        ConfigurePath();
    }

    void Update()
    {
        if (animateGlow)
        {
            AnimateLineageGlow();
        }
    }

    void ConfigurePath()
    {
        pathRenderer.startWidth = 0.3f;
        pathRenderer.endWidth = 0.3f;
        pathRenderer.material = new Material(Shader.Find("Sprites/Default"));
        pathRenderer.material.color = lineageColor;
        pathRenderer.material.EnableKeyword("_EMISSION");
        pathRenderer.material.SetColor("_EmissionColor", lineageColor * 2f);
    }

    public void SetAncestors(List<Transform> ancestors)
    {
        ancestorNodes = ancestors;
        pathRenderer.positionCount = ancestors.Count;
        
        for (int i = 0; i < ancestors.Count; i++)
        {
            pathRenderer.SetPosition(i, ancestors[i].position);
        }
    }

    void AnimateLineageGlow()
    {
        float glow = 1f + Mathf.Sin(Time.time * glowPulseSpeed) * 0.5f;
        pathRenderer.material.SetColor("_EmissionColor", lineageColor * glow * 2f);
    }

    public void TriggerAncestryFX()
    {
        if (ancestryFX != null)
        {
            ancestryFX.Play();
        }
    }
}

// ============================
// Remix Forge Cutscene Trigger
// ============================

public class RemixForgeGraphCutsceneTrigger : MonoBehaviour
{
    [Header("Cutscene Configuration")]
    public Camera cinematicCamera;
    public Transform[] cameraPoints;
    public AudioClip soulvanVoiceLine;
    public GameObject remixFX;
    
    [Header("Voice Lines")]
    public AudioClip divergenceVoice;
    public AudioClip echoVoice;
    public AudioClip scrollVoice;

    public void TriggerDivergenceCutscene()
    {
        StartCoroutine(PlayDivergenceCutscene());
    }

    IEnumerator PlayDivergenceCutscene()
    {
        for (int i = 0; i < cameraPoints.Length; i++)
        {
            cinematicCamera.transform.position = cameraPoints[i].position;
            cinematicCamera.transform.rotation = cameraPoints[i].rotation;
            yield return new WaitForSeconds(2f);
        }

        if (divergenceVoice != null)
        {
            AudioSource.PlayClipAtPoint(divergenceVoice, transform.position);
        }
        
        if (remixFX != null)
        {
            Instantiate(remixFX, transform.position, Quaternion.identity);
        }
        
        SoulvanLore.Record("Remix Forge divergence cutscene triggered");
    }

    public void TriggerEchoCutscene(Vector3 sourcePos, List<Vector3> echoPositions)
    {
        StartCoroutine(PlayEchoCutscene(sourcePos, echoPositions));
    }

    IEnumerator PlayEchoCutscene(Vector3 source, List<Vector3> echoes)
    {
        // Focus on source
        cinematicCamera.transform.position = source + new Vector3(0, 10f, -15f);
        cinematicCamera.transform.LookAt(source);
        yield return new WaitForSeconds(1.5f);
        
        // Pan across echo nodes
        foreach (var echoPos in echoes)
        {
            cinematicCamera.transform.LookAt(echoPos);
            yield return new WaitForSeconds(0.8f);
        }
        
        if (echoVoice != null)
        {
            AudioSource.PlayClipAtPoint(echoVoice, source);
        }
        
        SoulvanLore.Record("Remix Forge echo cutscene triggered");
    }

    public void TriggerScrollConstellationCutscene(Transform centerNode)
    {
        StartCoroutine(PlayScrollConstellationCutscene(centerNode));
    }

    IEnumerator PlayScrollConstellationCutscene(Transform center)
    {
        Vector3 startPos = cinematicCamera.transform.position;
        Vector3 orbitPos = center.position + new Vector3(0, 8f, -12f);
        
        float duration = 3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            cinematicCamera.transform.position = Vector3.Lerp(startPos, orbitPos, t);
            cinematicCamera.transform.LookAt(center.position);
            
            // Orbit around center
            cinematicCamera.transform.RotateAround(center.position, Vector3.up, 60f * Time.deltaTime);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (scrollVoice != null)
        {
            AudioSource.PlayClipAtPoint(scrollVoice, center.position);
        }
        
        SoulvanLore.Record("Scroll constellation cutscene triggered");
    }
}
