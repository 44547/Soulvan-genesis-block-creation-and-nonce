using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Networking;

/**
 * Soulvan Remix Forge Graph
 * 
 * Interactive 3D graph visualization showing:
 * - Contributor nodes with remix lineage
 * - Echo trails connecting remixed missions
 * - Scroll orbits around remix nodes
 * - DAO ripple waves propagating across graph
 * 
 * Real-time sync with SoulvanRemixForgeGraphAPI
 */

public class RemixForgeGraph : MonoBehaviour
{
    [Header("Graph Configuration")]
    public GameObject remixNodePrefab;
    public GameObject echoTrailPrefab;
    public GameObject scrollOrbitPrefab;
    public GameObject daoRipplePrefab;
    
    [Header("Graph Properties")]
    public int maxRemixNodes = 100;
    public float graphRadius = 50f;
    public float nodeVerticalSpacing = 5f;
    public bool autoRotate = true;
    public float rotationSpeed = 10f;
    
    [Header("API Configuration")]
    public string apiBaseUrl = "http://localhost:3003";
    public float syncInterval = 5f;
    
    [Header("Visual Effects")]
    public GameObject remixBurstFX;
    public GameObject tierGlowFX;
    public ParticleSystem daoRippleFX;
    
    [Header("Camera")]
    public Camera cinematicCamera;
    public float cameraOrbitSpeed = 5f;
    
    // Internal state
    private Dictionary<string, RemixNode> activeNodes = new Dictionary<string, RemixNode>();
    private List<EchoTrail> activeTrails = new List<EchoTrail>();
    private List<ScrollOrbit> activeScrolls = new List<ScrollOrbit>();
    private string authToken;

    void Start()
    {
        StartCoroutine(InitializeGraph());
        StartCoroutine(SyncWithAPI());
        
        if (autoRotate)
        {
            StartCoroutine(RotateCamera());
        }
    }

    // ============================
    // Graph Initialization
    // ============================

    IEnumerator InitializeGraph()
    {
        yield return StartCoroutine(AuthenticateAPI());
        yield return StartCoroutine(LoadRemixNodes());
        yield return StartCoroutine(LoadEchoTrails());
        yield return StartCoroutine(LoadScrollOrbits());
        
        SoulvanLore.Record("RemixForgeGraph initialized with real-time sync");
    }

    IEnumerator AuthenticateAPI()
    {
        string url = $"{apiBaseUrl}/auth/init";
        string jsonBody = "{\"wallet\":\"0xREMIXFORGE\",\"name\":\"RemixForgeGraph\"}";
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
                authToken = response.accessToken;
                Debug.Log($"[RemixForgeGraph] Authenticated: {response.contributorId}");
            }
            else
            {
                Debug.LogError($"[RemixForgeGraph] Auth failed: {request.error}");
            }
        }
    }

    // ============================
    // Node Management
    // ============================

    IEnumerator LoadRemixNodes()
    {
        string url = $"{apiBaseUrl}/remix/graph/contributors";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ContributorsResponse>(request.downloadHandler.text);
                
                int index = 0;
                foreach (var nodeData in response.contributors)
                {
                    CreateRemixNode(nodeData, index);
                    index++;
                }
                
                Debug.Log($"[RemixForgeGraph] Loaded {activeNodes.Count} remix nodes");
            }
        }
    }

    void CreateRemixNode(ContributorNodeData data, int index)
    {
        if (activeNodes.ContainsKey(data.id)) return;
        
        // Calculate position in circular layout with vertical tier spacing
        float angle = (360f / maxRemixNodes) * index;
        float rad = angle * Mathf.Deg2Rad;
        
        // Vertical offset based on tier
        float yOffset = GetTierVerticalOffset(data.tier);
        
        Vector3 position = new Vector3(
            Mathf.Cos(rad) * graphRadius,
            yOffset,
            Mathf.Sin(rad) * graphRadius
        );
        
        GameObject nodeObj = Instantiate(remixNodePrefab, position, Quaternion.identity, transform);
        RemixNode node = nodeObj.GetComponent<RemixNode>();
        
        if (node != null)
        {
            node.SetData(data);
            node.SetColor(GetTierColor(data.tier));
            activeNodes[data.id] = node;
        }
    }

    float GetTierVerticalOffset(string tier)
    {
        switch (tier)
        {
            case "Initiate": return 0f;
            case "Builder": return nodeVerticalSpacing;
            case "Architect": return nodeVerticalSpacing * 2;
            case "Oracle": return nodeVerticalSpacing * 3;
            case "Operative": return nodeVerticalSpacing * 2.5f;
            case "Legend": return nodeVerticalSpacing * 4;
            default: return 0f;
        }
    }

    Color GetTierColor(string tier)
    {
        switch (tier)
        {
            case "Initiate": return new Color(0.5f, 0.5f, 1f); // Blue
            case "Builder": return new Color(0.5f, 1f, 0.5f); // Green
            case "Architect": return new Color(1f, 0.5f, 1f); // Magenta
            case "Oracle": return new Color(0.5f, 1f, 1f); // Cyan
            case "Operative": return new Color(1f, 1f, 0.5f); // Yellow
            case "Legend": return new Color(1f, 1f, 0f); // Gold
            default: return Color.white;
        }
    }

    // ============================
    // Echo Trail Management
    // ============================

    IEnumerator LoadEchoTrails()
    {
        foreach (var nodeId in activeNodes.Keys)
        {
            yield return StartCoroutine(LoadNodeEchoTrails(nodeId));
        }
        
        Debug.Log($"[RemixForgeGraph] Loaded {activeTrails.Count} echo trails");
    }

    IEnumerator LoadNodeEchoTrails(string nodeId)
    {
        string url = $"{apiBaseUrl}/remix/echo/{nodeId}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<EchoResponse>(request.downloadHandler.text);
                
                foreach (var echoNodeId in response.echoNodes)
                {
                    if (activeNodes.ContainsKey(nodeId) && activeNodes.ContainsKey(echoNodeId))
                    {
                        CreateEchoTrail(nodeId, echoNodeId, response.intensity);
                    }
                }
            }
        }
    }

    void CreateEchoTrail(string fromId, string toId, string intensity)
    {
        if (!activeNodes.ContainsKey(fromId) || !activeNodes.ContainsKey(toId)) return;
        
        GameObject trailObj = Instantiate(echoTrailPrefab, transform);
        EchoTrail trail = trailObj.GetComponent<EchoTrail>();
        
        if (trail != null)
        {
            trail.SetConnection(
                activeNodes[fromId].transform,
                activeNodes[toId].transform,
                intensity
            );
            activeTrails.Add(trail);
        }
    }

    // ============================
    // Scroll Orbit Management
    // ============================

    IEnumerator LoadScrollOrbits()
    {
        foreach (var nodeId in activeNodes.Keys)
        {
            yield return StartCoroutine(LoadNodeScrolls(nodeId));
        }
        
        Debug.Log($"[RemixForgeGraph] Loaded {activeScrolls.Count} scroll orbits");
    }

    IEnumerator LoadNodeScrolls(string nodeId)
    {
        string url = $"{apiBaseUrl}/remix/scrolls/${nodeId}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ScrollsResponse>(request.downloadHandler.text);
                
                foreach (var scrollData in response.scrolls)
                {
                    CreateScrollOrbit(nodeId, scrollData);
                }
            }
        }
    }

    void CreateScrollOrbit(string nodeId, ScrollData scrollData)
    {
        if (!activeNodes.ContainsKey(nodeId)) return;
        
        GameObject scrollObj = Instantiate(scrollOrbitPrefab, activeNodes[nodeId].transform);
        ScrollOrbit scroll = scrollObj.GetComponent<ScrollOrbit>();
        
        if (scroll != null)
        {
            scroll.SetData(scrollData);
            scroll.SetOrbitRadius(3f);
            activeScrolls.Add(scroll);
        }
    }

    // ============================
    // DAO Ripple Effects
    // ============================

    public void TriggerDAORipple(string proposalId, string sourceNodeId, int votePower)
    {
        if (!activeNodes.ContainsKey(sourceNodeId)) return;
        
        StartCoroutine(AnimateDAORipple(proposalId, sourceNodeId, votePower));
        SoulvanLore.Record($"DAO ripple triggered: Proposal {proposalId} from {sourceNodeId}");
    }

    IEnumerator AnimateDAORipple(string proposalId, string sourceNodeId, int votePower)
    {
        Vector3 sourcePos = activeNodes[sourceNodeId].transform.position;
        
        // Spawn ripple FX at source
        if (daoRippleFX != null)
        {
            ParticleSystem ripple = Instantiate(daoRippleFX, sourcePos, Quaternion.identity);
            ripple.Play();
        }
        
        // Calculate affected nodes by distance
        float maxRippleRadius = votePower * 2f;
        List<string> affectedNodes = new List<string>();
        
        foreach (var kvp in activeNodes)
        {
            float distance = Vector3.Distance(sourcePos, kvp.Value.transform.position);
            if (distance <= maxRippleRadius && kvp.Key != sourceNodeId)
            {
                affectedNodes.Add(kvp.Key);
                
                // Animate node pulse
                float delay = distance / maxRippleRadius * 2f;
                StartCoroutine(PulseNodeAfterDelay(kvp.Key, delay));
            }
        }
        
        yield return new WaitForSeconds(2f);
        
        Debug.Log($"[RemixForgeGraph] DAO ripple affected {affectedNodes.Count} nodes");
    }

    IEnumerator PulseNodeAfterDelay(string nodeId, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (activeNodes.ContainsKey(nodeId))
        {
            activeNodes[nodeId].AnimatePulse();
        }
    }

    // ============================
    // Remix Event Handling
    // ============================

    public void TriggerRemixBurst(string remixId, string contributorId)
    {
        if (!activeNodes.ContainsKey(contributorId)) return;
        
        Vector3 nodePos = activeNodes[contributorId].transform.position;
        
        if (remixBurstFX != null)
        {
            GameObject burst = Instantiate(remixBurstFX, nodePos, Quaternion.identity);
            Destroy(burst, 3f);
        }
        
        activeNodes[contributorId].SetGlowIntensity(1.5f);
        
        SoulvanLore.Record($"Remix burst: {remixId} by {contributorId}");
    }

    public void TriggerLineageGlow(string contributorId, List<string> ancestorIds)
    {
        if (!activeNodes.ContainsKey(contributorId)) return;
        
        // Highlight contributor node
        activeNodes[contributorId].SetGlowIntensity(2f);
        
        // Highlight ancestor trail
        foreach (var ancestorId in ancestorIds)
        {
            if (activeNodes.ContainsKey(ancestorId))
            {
                activeNodes[ancestorId].SetGlowIntensity(1.2f);
            }
        }
    }

    // ============================
    // Camera Control
    // ============================

    IEnumerator RotateCamera()
    {
        while (autoRotate)
        {
            if (cinematicCamera != null)
            {
                cinematicCamera.transform.RotateAround(
                    transform.position,
                    Vector3.up,
                    rotationSpeed * Time.deltaTime
                );
                cinematicCamera.transform.LookAt(transform.position);
            }
            
            yield return null;
        }
    }

    public void FocusOnNode(string nodeId)
    {
        if (!activeNodes.ContainsKey(nodeId)) return;
        
        StartCoroutine(AnimateCameraToNode(nodeId));
    }

    IEnumerator AnimateCameraToNode(string nodeId)
    {
        Vector3 targetPos = activeNodes[nodeId].transform.position + new Vector3(0, 5f, -10f);
        float duration = 2f;
        float elapsed = 0f;
        
        Vector3 startPos = cinematicCamera.transform.position;
        
        while (elapsed < duration)
        {
            cinematicCamera.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            cinematicCamera.transform.LookAt(activeNodes[nodeId].transform.position);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // ============================
    // Real-time Sync
    // ============================

    IEnumerator SyncWithAPI()
    {
        while (true)
        {
            yield return new WaitForSeconds(syncInterval);
            
            // Refresh node data
            yield return StartCoroutine(LoadRemixNodes());
            
            // Check for new echo trails
            yield return StartCoroutine(LoadEchoTrails());
            
            // Update scroll orbits
            yield return StartCoroutine(LoadScrollOrbits());
        }
    }

    // ============================
    // Data Export
    // ============================

    public void ExportGraphData()
    {
        var graphData = new GraphExportData
        {
            contributors = new List<ContributorNodeData>(),
            echoTrails = new List<EchoTrailData>(),
            scrollOrbits = new List<ScrollData>(),
            timestamp = System.DateTime.UtcNow.ToString("o")
        };
        
        foreach (var node in activeNodes.Values)
        {
            graphData.contributors.Add(node.GetData());
        }
        
        string json = JsonUtility.ToJson(graphData, true);
        Debug.Log($"[RemixForgeGraph] Exported graph data:\n{json}");
        
        SoulvanLore.Record("RemixForgeGraph data exported");
    }
}

// ============================
// Data Structures
// ============================

[System.Serializable]
public class AuthResponse
{
    public string contributorId;
    public string accessToken;
    public int expiresIn;
}

[System.Serializable]
public class ContributorsResponse
{
    public ContributorNodeData[] contributors;
}

[System.Serializable]
public class ContributorNodeData
{
    public string id;
    public string name;
    public string tier;
    public int remixCount;
    public int loreLinks;
}

[System.Serializable]
public class EchoResponse
{
    public string remixId;
    public string[] echoNodes;
    public string intensity;
}

[System.Serializable]
public class EchoTrailData
{
    public string fromId;
    public string toId;
    public string intensity;
}

[System.Serializable]
public class ScrollsResponse
{
    public ScrollData[] scrolls;
}

[System.Serializable]
public class ScrollData
{
    public string title;
    public string format;
    public string url;
}

[System.Serializable]
public class GraphExportData
{
    public List<ContributorNodeData> contributors;
    public List<EchoTrailData> echoTrails;
    public List<ScrollData> scrollOrbits;
    public string timestamp;
}
