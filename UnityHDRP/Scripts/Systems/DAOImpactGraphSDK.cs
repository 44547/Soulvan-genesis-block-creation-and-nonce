using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// DAOImpactGraphSDK: Integration layer connecting DAO overlay with RemixForgeGraph
/// for real-time vote visualization, contributor node updates, and governance trails.
/// </summary>
public class DAOImpactGraphSDK : MonoBehaviour
{
    public static DAOImpactGraphSDK Instance { get; private set; }

    [Header("Component References")]
    public DAOOverlayFXManager daoOverlay;
    public RemixForgeGraph remixGraph;

    [Header("Sync Configuration")]
    [Tooltip("Auto-sync interval in seconds")]
    public float autoSyncInterval = 5f;

    [Tooltip("Enable real-time proposal sync")]
    public bool enableProposalSync = true;

    [Tooltip("Enable real-time vote ripple propagation")]
    public bool enableRipplePropagation = true;

    [Header("Visual Configuration")]
    [Tooltip("Vote ripple radius multiplier for graph nodes")]
    public float graphRippleMultiplier = 2f;

    [Tooltip("Tier upgrade pulse intensity")]
    public float tierUpgradePulseIntensity = 3f;

    [Tooltip("Governance trail fade time")]
    public float governanceTrailFadeTime = 10f;

    // Internal state
    private float _syncTimer = 0f;
    private Dictionary<string, ProposalNodeData> _activeProposals = new Dictionary<string, ProposalNodeData>();
    private Dictionary<string, List<GovernanceTrailSegment>> _governanceTrails = new Dictionary<string, List<GovernanceTrailSegment>>();
    private ServerRpcClient _rpcClient;

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
            return;
        }

        _rpcClient = ServerRpcClient.Instance;

        // Find components if not assigned
        if (daoOverlay == null)
        {
            daoOverlay = FindObjectOfType<DAOOverlayFXManager>();
        }

        if (remixGraph == null)
        {
            remixGraph = FindObjectOfType<RemixForgeGraph>();
        }

        if (daoOverlay == null || remixGraph == null)
        {
            Debug.LogError("DAOImpactGraphSDK: Required components not found!");
        }
    }

    void Start()
    {
        StartCoroutine(InitializeSync());
    }

    void Update()
    {
        if (!enableProposalSync) return;

        _syncTimer += Time.deltaTime;
        if (_syncTimer >= autoSyncInterval)
        {
            _syncTimer = 0f;
            SyncProposals();
        }
    }

    /// <summary>
    /// Initialize synchronization with DAO API
    /// </summary>
    IEnumerator InitializeSync()
    {
        Debug.Log("DAOImpactGraphSDK: Initializing sync...");

        // Wait for RPC client to be ready
        while (_rpcClient == null || string.IsNullOrEmpty(_rpcClient.authToken))
        {
            yield return new WaitForSeconds(0.5f);
            _rpcClient = ServerRpcClient.Instance;
        }

        // Initial proposal sync
        yield return SyncProposalsCoroutine();

        Debug.Log("DAOImpactGraphSDK: Sync initialized");
    }

    /// <summary>
    /// Sync proposals from DAO API
    /// </summary>
    void SyncProposals()
    {
        StartCoroutine(SyncProposalsCoroutine());
    }

    /// <summary>
    /// Sync proposals coroutine
    /// </summary>
    IEnumerator SyncProposalsCoroutine()
    {
        if (_rpcClient == null) yield break;

        bool success = false;
        string response = null;

        yield return _rpcClient.GetDaoProposals((s, r) =>
        {
            success = s;
            response = r;
        });

        if (success && !string.IsNullOrEmpty(response))
        {
            ProcessProposalSync(response);
        }
    }

    /// <summary>
    /// Process proposal sync response
    /// </summary>
    void ProcessProposalSync(string json)
    {
        try
        {
            var proposalsResponse = JsonUtility.FromJson<ProposalsResponse>("{\"proposals\":" + json + "}");

            foreach (var proposal in proposalsResponse.proposals)
            {
                // Check if proposal is new or updated
                if (!_activeProposals.ContainsKey(proposal.id))
                {
                    // New proposal - create visualization
                    CreateProposalVisualization(proposal);
                }
                else
                {
                    // Existing proposal - update votes
                    UpdateProposalVisualization(proposal);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DAOImpactGraphSDK: Failed to process proposal sync: {e.Message}");
        }
    }

    /// <summary>
    /// Create proposal visualization on graph
    /// </summary>
    void CreateProposalVisualization(ProposalData proposal)
    {
        if (daoOverlay == null || remixGraph == null) return;

        // Find position near center of graph
        Vector3 position = CalculateProposalPosition(proposal.id);

        // Create proposal node in DAO overlay
        daoOverlay.CreateProposalNode(proposal.id, proposal.title, position);

        // Update votes
        daoOverlay.UpdateProposalVotes(proposal.id, proposal.votes.yes, proposal.votes.no, proposal.votes.abstain);

        // Store proposal data
        _activeProposals[proposal.id] = new ProposalNodeData
        {
            id = proposal.id,
            position = position,
            totalVotes = proposal.votes.yes + proposal.votes.no + proposal.votes.abstain
        };

        Debug.Log($"DAOImpactGraphSDK: Created proposal visualization for {proposal.id}");
    }

    /// <summary>
    /// Update proposal visualization
    /// </summary>
    void UpdateProposalVisualization(ProposalData proposal)
    {
        if (daoOverlay == null) return;

        daoOverlay.UpdateProposalVotes(proposal.id, proposal.votes.yes, proposal.votes.no, proposal.votes.abstain);

        // Update stored vote count
        if (_activeProposals.ContainsKey(proposal.id))
        {
            _activeProposals[proposal.id].totalVotes = proposal.votes.yes + proposal.votes.no + proposal.votes.abstain;
        }
    }

    /// <summary>
    /// Calculate position for proposal node
    /// </summary>
    Vector3 CalculateProposalPosition(string proposalId)
    {
        // Hash proposal ID to get consistent position
        int hash = proposalId.GetHashCode();
        float angle = (hash % 360) * Mathf.Deg2Rad;
        float radius = 15f; // Proposals orbit around center

        return new Vector3(
            Mathf.Cos(angle) * radius,
            Random.Range(-2f, 2f), // Small Y variation
            Mathf.Sin(angle) * radius
        );
    }

    /// <summary>
    /// Trigger vote ripple propagation across graph
    /// </summary>
    public void PropagateVoteRipple(string proposalId, string contributorId, string vote, float votePower)
    {
        if (!enableRipplePropagation || daoOverlay == null || remixGraph == null) return;

        // Trigger ripple in DAO overlay
        daoOverlay.TriggerVoteRipple(proposalId, contributorId, vote, votePower);

        // Propagate to RemixForge graph nodes
        StartCoroutine(PropagateToGraphNodes(proposalId, contributorId, vote, votePower));

        Debug.Log($"DAOImpactGraphSDK: Propagating vote ripple for {proposalId} by {contributorId}");
    }

    /// <summary>
    /// Propagate ripple to affected graph nodes
    /// </summary>
    IEnumerator PropagateToGraphNodes(string proposalId, string contributorId, string vote, float votePower)
    {
        // Get affected nodes from API
        if (_rpcClient == null) yield break;

        bool success = false;
        string response = null;

        yield return _rpcClient.GetVoteRipples(proposalId, (s, r) =>
        {
            success = s;
            response = r;
        });

        if (success && !string.IsNullOrEmpty(response))
        {
            ProcessRipplePropagation(response, vote);
        }
    }

    /// <summary>
    /// Process ripple propagation to graph nodes
    /// </summary>
    void ProcessRipplePropagation(string json, string vote)
    {
        try
        {
            var rippleResponse = JsonUtility.FromJson<VoteRipplesResponse>(json);

            Color rippleColor = GetVoteColor(vote);
            float radius = rippleResponse.rippleRadius * graphRippleMultiplier;

            // Pulse affected contributor nodes in RemixForge graph
            foreach (string nodeId in rippleResponse.rippleNodes)
            {
                if (remixGraph != null)
                {
                    remixGraph.PulseNode(nodeId, rippleColor, radius);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DAOImpactGraphSDK: Failed to process ripple propagation: {e.Message}");
        }
    }

    /// <summary>
    /// Trigger tier upgrade visualization across both systems
    /// </summary>
    public void VisualizeTierUpgrade(string contributorId, string fromTier, string toTier)
    {
        if (daoOverlay == null || remixGraph == null) return;

        // Get contributor position from RemixForge graph
        Vector3 position = remixGraph.GetNodePosition(contributorId);

        // Trigger tier burst in DAO overlay
        daoOverlay.TriggerTierUpgradeBurst(contributorId, fromTier, toTier, position);

        // Pulse node in RemixForge graph with tier color
        Color tierColor = GetTierColor(toTier);
        remixGraph.PulseNode(contributorId, tierColor, tierUpgradePulseIntensity);

        Debug.Log($"DAOImpactGraphSDK: Visualized tier upgrade for {contributorId}: {fromTier} -> {toTier}");
    }

    /// <summary>
    /// Create governance trail connecting contributor to proposals
    /// </summary>
    public void CreateGovernanceTrail(string contributorId, List<string> proposalIds)
    {
        if (daoOverlay == null || remixGraph == null) return;

        List<Vector3> positions = new List<Vector3>();

        // Start from contributor node
        Vector3 contributorPos = remixGraph.GetNodePosition(contributorId);
        positions.Add(contributorPos);

        // Add proposal positions
        foreach (string proposalId in proposalIds)
        {
            if (_activeProposals.ContainsKey(proposalId))
            {
                positions.Add(_activeProposals[proposalId].position);
            }
        }

        if (positions.Count >= 2)
        {
            // Create trail in DAO overlay
            daoOverlay.CreateGovernanceTrail(contributorId, proposalIds.ToArray(), positions.ToArray());

            // Store trail data for fading
            _governanceTrails[contributorId] = new List<GovernanceTrailSegment>();
            for (int i = 0; i < proposalIds.Count; i++)
            {
                _governanceTrails[contributorId].Add(new GovernanceTrailSegment
                {
                    proposalId = proposalIds[i],
                    createdTime = Time.time
                });
            }

            Debug.Log($"DAOImpactGraphSDK: Created governance trail for {contributorId} with {proposalIds.Count} proposals");
        }
    }

    /// <summary>
    /// Get vote color
    /// </summary>
    Color GetVoteColor(string vote)
    {
        switch (vote.ToUpperInvariant())
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

    /// <summary>
    /// Get tier color
    /// </summary>
    Color GetTierColor(string tier)
    {
        switch (tier.ToLowerInvariant())
        {
            case "initiate":
                return new Color(0.6f, 0.6f, 0.7f); // Gray
            case "builder":
                return new Color(0.3f, 0.8f, 0.3f); // Green
            case "architect":
                return new Color(0.3f, 0.5f, 1f); // Blue
            case "oracle":
                return new Color(0.7f, 0.3f, 1f); // Purple
            case "legend":
                return new Color(1f, 0.84f, 0f); // Gold
            default:
                return Color.cyan;
        }
    }

    /// <summary>
    /// Process DAO impact response and trigger visualizations
    /// </summary>
    public void ProcessDAOImpact(string impactResponseJson)
    {
        if (daoOverlay == null) return;

        daoOverlay.OnImpactResponse(impactResponseJson);
    }

    /// <summary>
    /// Get active proposal count
    /// </summary>
    public int GetActiveProposalCount()
    {
        return _activeProposals.Count;
    }

    /// <summary>
    /// Get proposal position
    /// </summary>
    public Vector3 GetProposalPosition(string proposalId)
    {
        if (_activeProposals.ContainsKey(proposalId))
        {
            return _activeProposals[proposalId].position;
        }
        return Vector3.zero;
    }
}

/// <summary>
/// Proposal node data
/// </summary>
public class ProposalNodeData
{
    public string id;
    public Vector3 position;
    public int totalVotes;
}

/// <summary>
/// Governance trail segment
/// </summary>
public class GovernanceTrailSegment
{
    public string proposalId;
    public float createdTime;
}

/// <summary>
/// Proposals API response
/// </summary>
[System.Serializable]
public class ProposalsResponse
{
    public ProposalData[] proposals;
}

/// <summary>
/// Proposal data
/// </summary>
[System.Serializable]
public class ProposalData
{
    public string id;
    public string title;
    public string status;
    public VoteCount votes;
    public long createdAt;
    public long endAt;
}

/// <summary>
/// Vote count
/// </summary>
[System.Serializable]
public class VoteCount
{
    public int yes;
    public int no;
    public int abstain;
}

/// <summary>
/// Vote ripples API response
/// </summary>
[System.Serializable]
public class VoteRipplesResponse
{
    public string[] rippleNodes;
    public float intensity;
    public float rippleRadius;
}
