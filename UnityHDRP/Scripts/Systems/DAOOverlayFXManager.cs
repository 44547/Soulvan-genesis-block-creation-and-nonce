using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/**
 * Soulvan DAO Overlay FX Manager
 * 
 * Visual overlay system for DAO governance events:
 * - ProposalNode: Visual nodes for active DAO proposals
 * - VoteRipple: Expanding waves from proposal votes across contributors
 * - TierUpgradeBurst: Cinematic burst when contributors level up from votes
 * - GovernanceTrail: Glowing paths showing contributor influence
 * 
 * Integrates with RemixForgeGraph for real-time vote visualization
 */

public class DAOOverlayFXManager : MonoBehaviour
{
    public static DAOOverlayFXManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject proposalNodePrefab;
    public GameObject voteRipplePrefab;
    public GameObject tierUpgradeBurstPrefab;
    public GameObject governanceTrailPrefab;
    
    [Header("Audio")]
    public AudioClip tierUpgradeVoice; // "You rise. The vault remembers."
    public AudioClip voteRippleSound;
    public AudioClip proposalCreatedSound;
    
    [Header("Graph Integration")]
    public RemixForgeGraph remixGraph;
    
    [Header("Configuration")]
    public float ripplePropagationSpeed = 20f;
    public float tierBurstDuration = 3f;
    public bool autoConnectToGraph = true;
    
    // Internal state
    private Dictionary<string, ProposalNode> activeProposals = new Dictionary<string, ProposalNode>();
    private List<VoteRipple> activeRipples = new List<VoteRipple>();
    private List<GovernanceTrail> activeTrails = new List<GovernanceTrail>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (autoConnectToGraph && remixGraph == null)
        {
            remixGraph = FindObjectOfType<RemixForgeGraph>();
        }
        
        SoulvanLore.Record("DAOOverlayFXManager initialized");
    }

    // ============================
    // Proposal Node Management
    // ============================

    public void CreateProposalNode(string proposalId, string title, Vector3 position)
    {
        if (activeProposals.ContainsKey(proposalId)) return;
        
        GameObject nodeObj = Instantiate(proposalNodePrefab, position, Quaternion.identity, transform);
        ProposalNode node = nodeObj.GetComponent<ProposalNode>();
        
        if (node != null)
        {
            node.SetData(proposalId, title);
            activeProposals[proposalId] = node;
            
            if (proposalCreatedSound != null)
            {
                AudioSource.PlayClipAtPoint(proposalCreatedSound, position);
            }
            
            SoulvanLore.Record($"DAO Proposal created: {proposalId} - {title}");
        }
    }

    public void UpdateProposalVotes(string proposalId, int yesVotes, int noVotes, int abstainVotes)
    {
        if (!activeProposals.ContainsKey(proposalId)) return;
        
        ProposalNode node = activeProposals[proposalId];
        node.UpdateVotes(yesVotes, noVotes, abstainVotes);
        
        // Update glow intensity based on vote activity
        float totalVotes = yesVotes + noVotes + abstainVotes;
        float glowIntensity = Mathf.Clamp(totalVotes / 100f, 0.5f, 3f);
        node.SetGlowIntensity(glowIntensity);
    }

    public void CloseProposal(string proposalId, string outcome)
    {
        if (!activeProposals.ContainsKey(proposalId)) return;
        
        ProposalNode node = activeProposals[proposalId];
        node.SetOutcome(outcome);
        
        StartCoroutine(FadeOutProposalNode(proposalId, 5f));
    }

    IEnumerator FadeOutProposalNode(string proposalId, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (activeProposals.ContainsKey(proposalId))
        {
            Destroy(activeProposals[proposalId].gameObject);
            activeProposals.Remove(proposalId);
        }
    }

    // ============================
    // Vote Ripple Management
    // ============================

    public void TriggerVoteRipple(string proposalId, string contributorId, string vote, int votePower)
    {
        if (!activeProposals.ContainsKey(proposalId)) return;
        
        Vector3 sourcePos = activeProposals[proposalId].transform.position;
        Color rippleColor = GetVoteColor(vote);
        
        StartCoroutine(AnimateVoteRipple(proposalId, sourcePos, rippleColor, votePower));
        
        // Propagate to RemixForgeGraph if available
        if (remixGraph != null && !string.IsNullOrEmpty(contributorId))
        {
            remixGraph.TriggerDAORipple(proposalId, contributorId, votePower);
        }
        
        if (voteRippleSound != null)
        {
            AudioSource.PlayClipAtPoint(voteRippleSound, sourcePos);
        }
        
        SoulvanLore.Record($"Vote ripple: {proposalId} - {vote} ({votePower} power)");
    }

    IEnumerator AnimateVoteRipple(string proposalId, Vector3 sourcePos, Color color, int votePower)
    {
        GameObject rippleObj = Instantiate(voteRipplePrefab, sourcePos, Quaternion.identity, transform);
        VoteRipple ripple = rippleObj.GetComponent<VoteRipple>();
        
        if (ripple != null)
        {
            ripple.Initialize(sourcePos, color, votePower);
            activeRipples.Add(ripple);
            
            float maxRadius = votePower * 2f;
            float duration = maxRadius / ripplePropagationSpeed;
            
            yield return new WaitForSeconds(duration);
            
            activeRipples.Remove(ripple);
            Destroy(rippleObj);
        }
    }

    Color GetVoteColor(string vote)
    {
        switch (vote.ToUpper())
        {
            case "YES":
                return new Color(1f, 0.84f, 0f); // Gold
            case "NO":
                return new Color(1f, 0f, 0f); // Red
            case "ABSTAIN":
                return new Color(0.5f, 0.5f, 1f); // Blue
            default:
                return Color.white;
        }
    }

    // ============================
    // Tier Upgrade Burst
    // ============================

    public void TriggerTierUpgradeBurst(string contributorId, string fromTier, string toTier, Vector3 position)
    {
        StartCoroutine(AnimateTierUpgrade(contributorId, fromTier, toTier, position));
    }

    IEnumerator AnimateTierUpgrade(string contributorId, string fromTier, string toTier, Vector3 position)
    {
        // Spawn tier upgrade burst FX
        GameObject burstObj = Instantiate(tierUpgradeBurstPrefab, position, Quaternion.identity, transform);
        
        // Configure burst color based on new tier
        ParticleSystem burstPS = burstObj.GetComponent<ParticleSystem>();
        if (burstPS != null)
        {
            var main = burstPS.main;
            main.startColor = GetTierColor(toTier);
            burstPS.Play();
        }
        
        // Play voice line
        if (tierUpgradeVoice != null)
        {
            AudioSource.PlayClipAtPoint(tierUpgradeVoice, position);
        }
        
        // Trigger rune flare on RemixForgeGraph if available
        if (remixGraph != null)
        {
            remixGraph.TriggerRemixBurst($"TIER_UPGRADE_{contributorId}", contributorId);
        }
        
        SoulvanLore.Record($"Tier upgrade: {contributorId} from {fromTier} to {toTier}");
        
        yield return new WaitForSeconds(tierBurstDuration);
        
        Destroy(burstObj);
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
    // Governance Trail Management
    // ============================

    public void CreateGovernanceTrail(string contributorId, List<string> proposalIds, List<Vector3> positions)
    {
        GameObject trailObj = Instantiate(governanceTrailPrefab, transform);
        GovernanceTrail trail = trailObj.GetComponent<GovernanceTrail>();
        
        if (trail != null)
        {
            trail.SetContributor(contributorId);
            trail.SetPath(positions);
            activeTrails.Add(trail);
            
            SoulvanLore.Record($"Governance trail created: {contributorId} across {proposalIds.Count} proposals");
        }
    }

    public void ClearGovernanceTrail(string contributorId)
    {
        for (int i = activeTrails.Count - 1; i >= 0; i--)
        {
            if (activeTrails[i].GetContributorId() == contributorId)
            {
                Destroy(activeTrails[i].gameObject);
                activeTrails.RemoveAt(i);
            }
        }
    }

    // ============================
    // API Response Handler
    // ============================

    public void OnImpactResponse(string jsonResponse)
    {
        try
        {
            var response = JsonUtility.FromJson<DAOImpactResponse>(jsonResponse);
            
            // Trigger ripple effects for impacted nodes
            if (response.rippleNodes != null && response.rippleNodes.Length > 0)
            {
                foreach (string nodeId in response.rippleNodes)
                {
                    if (remixGraph != null)
                    {
                        // Pulse affected contributor nodes
                        StartCoroutine(PulseContributorNode(nodeId, 0.5f));
                    }
                }
            }
            
            Debug.Log($"[DAOOverlayFX] Processed impact: {response.impactId}, rippled {response.rippleNodes.Length} nodes");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DAOOverlayFX] Failed to parse impact response: {e.Message}");
        }
    }

    IEnumerator PulseContributorNode(string nodeId, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // This would call into RemixForgeGraph to pulse the specific node
        // remixGraph.PulseNode(nodeId);
    }

    // ============================
    // Cleanup
    // ============================

    void OnDestroy()
    {
        foreach (var ripple in activeRipples)
        {
            if (ripple != null) Destroy(ripple.gameObject);
        }
        
        foreach (var trail in activeTrails)
        {
            if (trail != null) Destroy(trail.gameObject);
        }
        
        foreach (var proposal in activeProposals.Values)
        {
            if (proposal != null) Destroy(proposal.gameObject);
        }
        
        activeRipples.Clear();
        activeTrails.Clear();
        activeProposals.Clear();
    }
}

// ============================
// Component Classes
// ============================

public class ProposalNode : MonoBehaviour
{
    [Header("Visual Components")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI voteCountText;
    public GameObject nodeGlow;
    public Light proposalLight;
    public ParticleSystem voteActivityFX;
    
    private string proposalId;
    private string title;
    private int yesVotes;
    private int noVotes;
    private int abstainVotes;
    private float glowIntensity = 1f;

    public void SetData(string id, string proposalTitle)
    {
        proposalId = id;
        title = proposalTitle;
        
        if (titleText != null)
        {
            titleText.text = proposalTitle;
        }
        
        UpdateVisuals();
    }

    public void UpdateVotes(int yes, int no, int abstain)
    {
        yesVotes = yes;
        noVotes = no;
        abstainVotes = abstain;
        
        if (voteCountText != null)
        {
            voteCountText.text = $"YES: {yes} | NO: {no} | ABSTAIN: {abstain}";
        }
        
        UpdateVisuals();
        
        if (voteActivityFX != null && !voteActivityFX.isPlaying)
        {
            voteActivityFX.Play();
        }
    }

    public void SetGlowIntensity(float intensity)
    {
        glowIntensity = intensity;
        UpdateVisuals();
    }

    public void SetOutcome(string outcome)
    {
        if (titleText != null)
        {
            titleText.text = $"{title}\n<color={(outcome == "Passed" ? "green" : "red")}>{outcome}</color>";
        }
    }

    void UpdateVisuals()
    {
        if (proposalLight != null)
        {
            proposalLight.intensity = glowIntensity * 2f;
            
            // Color based on vote leading
            if (yesVotes > noVotes && yesVotes > abstainVotes)
            {
                proposalLight.color = new Color(1f, 0.84f, 0f); // Gold
            }
            else if (noVotes > yesVotes)
            {
                proposalLight.color = new Color(1f, 0f, 0f); // Red
            }
            else
            {
                proposalLight.color = new Color(0.5f, 0.5f, 1f); // Blue
            }
        }
        
        if (nodeGlow != null)
        {
            var renderer = nodeGlow.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.SetColor("_EmissionColor", proposalLight.color * glowIntensity);
            }
        }
    }

    void Update()
    {
        // Gentle rotation
        transform.Rotate(Vector3.up, 10f * Time.deltaTime);
        
        // Pulse glow
        float pulse = 1f + Mathf.Sin(Time.time * 2f) * 0.1f;
        if (proposalLight != null)
        {
            proposalLight.intensity = glowIntensity * 2f * pulse;
        }
    }
}

public class VoteRipple : MonoBehaviour
{
    public ParticleSystem rippleParticles;
    public Light rippleLight;
    
    private Vector3 sourcePosition;
    private Color rippleColor;
    private float currentRadius = 0f;
    private float maxRadius = 50f;
    private float speed = 20f;

    public void Initialize(Vector3 source, Color color, int votePower)
    {
        sourcePosition = source;
        rippleColor = color;
        maxRadius = votePower * 2f;
        
        if (rippleParticles != null)
        {
            var main = rippleParticles.main;
            main.startColor = color;
            rippleParticles.Play();
        }
        
        if (rippleLight != null)
        {
            rippleLight.color = color;
        }
    }

    void Update()
    {
        currentRadius += speed * Time.deltaTime;
        
        // Expand particle system
        if (rippleParticles != null)
        {
            var shape = rippleParticles.shape;
            shape.radius = currentRadius;
        }
        
        // Fade light as ripple expands
        if (rippleLight != null)
        {
            float t = currentRadius / maxRadius;
            rippleLight.intensity = Mathf.Lerp(3f, 0f, t);
        }
    }
}

public class GovernanceTrail : MonoBehaviour
{
    public LineRenderer trailRenderer;
    public ParticleSystem influenceFX;
    public Color trailColor = new Color(1f, 0.84f, 0f, 0.6f);
    
    private string contributorId;
    private List<Vector3> pathPositions = new List<Vector3>();

    public void SetContributor(string id)
    {
        contributorId = id;
    }

    public string GetContributorId()
    {
        return contributorId;
    }

    public void SetPath(List<Vector3> positions)
    {
        pathPositions = positions;
        
        if (trailRenderer != null && positions.Count > 1)
        {
            trailRenderer.positionCount = positions.Count;
            trailRenderer.SetPositions(positions.ToArray());
            trailRenderer.startColor = trailColor;
            trailRenderer.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
            trailRenderer.startWidth = 0.3f;
            trailRenderer.endWidth = 0.1f;
        }
        
        if (influenceFX != null)
        {
            influenceFX.transform.position = positions[0];
            influenceFX.Play();
        }
    }

    void Update()
    {
        // Animate trail glow
        if (trailRenderer != null)
        {
            float glow = 1f + Mathf.Sin(Time.time * 1.5f) * 0.3f;
            trailRenderer.material.SetColor("_EmissionColor", trailColor * glow * 2f);
        }
    }
}

// ============================
// Data Structures
// ============================

[System.Serializable]
public class DAOImpactResponse
{
    public string impactId;
    public string[] rippleNodes;
    public int badgeUpgrades;
    public string message;
}

[System.Serializable]
public class ProposalData
{
    public string id;
    public string title;
    public string status;
    public VoteCount votes;
}

[System.Serializable]
public class VoteCount
{
    public int yes;
    public int no;
    public int abstain;
}
