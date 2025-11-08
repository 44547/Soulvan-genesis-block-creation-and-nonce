using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soulvan.Systems
{
    /// <summary>
    /// Interactive badge vault graph visualization.
    /// Displays contributor nodes connected by lore links with DAO ripple effects.
    /// </summary>
    public class BadgeVaultGraph : MonoBehaviour
    {
        [Header("Graph Configuration")]
        public int maxContributorNodes = 100;
        public float nodeSpacing = 5f;
        public float graphRadius = 50f;

        [Header("Contributor Nodes")]
        public GameObject contributorNodePrefab;
        public Transform nodeContainer;
        public Dictionary<string, ContributorNode> activeNodes = new Dictionary<string, ContributorNode>();

        [Header("Lore Links")]
        public GameObject loreLinkPrefab;
        public Transform linkContainer;
        public List<LoreLink> activeLinks = new List<LoreLink>();

        [Header("DAO Ripples")]
        public GameObject daoRippleFX;
        public float rippleSpeed = 2f;
        public Color rippleColor = new Color(0f, 1f, 1f, 0.5f);

        [Header("Badge Tier Pulses")]
        public ParticleSystem tierPulseFX;
        public Dictionary<string, Color> tierColors = new Dictionary<string, Color>();

        [Header("Camera")]
        public Camera graphCamera;
        public float cameraRotationSpeed = 10f;
        public bool autoRotate = true;

        [Header("Cinematic")]
        public BadgeVaultGraphCutsceneTrigger cutsceneTrigger;

        private List<ContributorNodeData> contributorData = new List<ContributorNodeData>();

        private void Start()
        {
            InitializeGraph();
            LoadContributorNodes();
            LoadLoreLinks();
            SetupTierColors();
        }

        private void Update()
        {
            if (autoRotate && graphCamera != null)
            {
                graphCamera.transform.RotateAround(transform.position, Vector3.up, cameraRotationSpeed * Time.deltaTime);
            }

            UpdateNodeGlow();
        }

        /// <summary>
        /// Initialize badge vault graph.
        /// </summary>
        private void InitializeGraph()
        {
            Debug.Log("[BadgeVaultGraph] Initializing contributor graph visualization");

            SoulvanLore.Record("Badge Vault Graph initialized");
        }

        /// <summary>
        /// Setup tier colors.
        /// </summary>
        private void SetupTierColors()
        {
            tierColors["Initiate"] = new Color(0.5f, 0.5f, 0.5f); // Gray
            tierColors["Builder"] = new Color(0f, 0.8f, 1f); // Cyan
            tierColors["Architect"] = new Color(0.8f, 0f, 1f); // Purple
            tierColors["Oracle"] = new Color(1f, 0.8f, 0f); // Gold
            tierColors["Operative"] = new Color(1f, 0f, 0f); // Red
            tierColors["Legend"] = new Color(1f, 1f, 0f); // Yellow
        }

        /// <summary>
        /// Load contributor nodes from API/database.
        /// </summary>
        private void LoadContributorNodes()
        {
            // Example contributor data - in production, load from API
            contributorData.Add(new ContributorNodeData
            {
                contributorId = "C001",
                contributorName = "Brian",
                badgeTier = "Architect",
                loreCount = 12,
                daoPower = 25,
                recentActivity = Time.time
            });

            contributorData.Add(new ContributorNodeData
            {
                contributorId = "C002",
                contributorName = "Alice",
                badgeTier = "Oracle",
                loreCount = 45,
                daoPower = 100,
                recentActivity = Time.time - 3600f
            });

            contributorData.Add(new ContributorNodeData
            {
                contributorId = "C003",
                contributorName = "Charlie",
                badgeTier = "Builder",
                loreCount = 8,
                daoPower = 5,
                recentActivity = Time.time - 7200f
            });

            // Create visual nodes
            for (int i = 0; i < contributorData.Count; i++)
            {
                CreateContributorNode(contributorData[i], i);
            }

            Debug.Log($"[BadgeVaultGraph] Loaded {contributorData.Count} contributor nodes");
        }

        /// <summary>
        /// Create visual contributor node.
        /// </summary>
        private void CreateContributorNode(ContributorNodeData data, int index)
        {
            if (contributorNodePrefab == null || nodeContainer == null) return;

            // Calculate position in circular layout
            float angle = (360f / maxContributorNodes) * index;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                Mathf.Cos(rad) * graphRadius,
                0f,
                Mathf.Sin(rad) * graphRadius
            );

            // Instantiate node
            GameObject nodeObj = Instantiate(contributorNodePrefab, position, Quaternion.identity, nodeContainer);
            ContributorNode node = nodeObj.GetComponent<ContributorNode>();

            if (node != null)
            {
                node.SetData(data);
                node.SetColor(tierColors[data.badgeTier]);
                activeNodes[data.contributorId] = node;
            }

            Debug.Log($"[BadgeVaultGraph] Created node: {data.contributorName} ({data.badgeTier})");
        }

        /// <summary>
        /// Load lore links between contributors.
        /// </summary>
        private void LoadLoreLinks()
        {
            // Example lore connections
            CreateLoreLink("C001", "C002", "Vault Breach collaboration");
            CreateLoreLink("C002", "C003", "DAO proposal co-sponsorship");
            CreateLoreLink("C001", "C003", "Seasonal quest party");

            Debug.Log($"[BadgeVaultGraph] Loaded {activeLinks.Count} lore links");
        }

        /// <summary>
        /// Create visual lore link.
        /// </summary>
        private void CreateLoreLink(string fromId, string toId, string loreContext)
        {
            if (!activeNodes.ContainsKey(fromId) || !activeNodes.ContainsKey(toId)) return;
            if (loreLinkPrefab == null || linkContainer == null) return;

            ContributorNode fromNode = activeNodes[fromId];
            ContributorNode toNode = activeNodes[toId];

            GameObject linkObj = Instantiate(loreLinkPrefab, linkContainer);
            LoreLink link = linkObj.GetComponent<LoreLink>();

            if (link != null)
            {
                link.SetConnection(fromNode.transform, toNode.transform, loreContext);
                activeLinks.Add(link);
            }
        }

        /// <summary>
        /// Trigger DAO ripple effect from vote.
        /// </summary>
        public void TriggerDAORipple(string proposalId, string contributorId, int votePower)
        {
            if (!activeNodes.ContainsKey(contributorId)) return;

            Debug.Log($"[BadgeVaultGraph] ⚡ DAO ripple triggered: {proposalId} by {contributorId} (Power: {votePower})");

            ContributorNode sourceNode = activeNodes[contributorId];
            StartCoroutine(AnimateDAORipple(sourceNode.transform, votePower));

            // Record to lore
            SoulvanLore.Record($"DAO ripple propagated from {contributorId} across badge vault graph");
        }

        /// <summary>
        /// Animate DAO ripple wave.
        /// </summary>
        private System.Collections.IEnumerator AnimateDAORipple(Transform source, int power)
        {
            if (daoRippleFX == null) yield break;

            GameObject ripple = Instantiate(daoRippleFX, source.position, Quaternion.identity);
            float elapsed = 0f;
            float duration = 2f;
            float maxRadius = graphRadius;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float currentRadius = Mathf.Lerp(0f, maxRadius, t);

                // Scale ripple
                ripple.transform.localScale = Vector3.one * currentRadius;

                // Fade out
                if (ripple.TryGetComponent<Renderer>(out Renderer renderer))
                {
                    Color color = rippleColor;
                    color.a = Mathf.Lerp(1f, 0f, t);
                    renderer.material.color = color;
                }

                yield return null;
            }

            Destroy(ripple);
        }

        /// <summary>
        /// Trigger badge tier pulse effect.
        /// </summary>
        public void TriggerTierPulse(string contributorId, string newTier)
        {
            if (!activeNodes.ContainsKey(contributorId)) return;

            Debug.Log($"[BadgeVaultGraph] 🌟 Badge tier pulse: {contributorId} → {newTier}");

            ContributorNode node = activeNodes[contributorId];

            // Update node color
            if (tierColors.ContainsKey(newTier))
            {
                node.SetColor(tierColors[newTier]);
            }

            // Play pulse FX
            if (tierPulseFX != null)
            {
                ParticleSystem pulse = Instantiate(tierPulseFX, node.transform.position, Quaternion.identity);
                pulse.Play();
                Destroy(pulse.gameObject, 3f);
            }

            // Trigger cutscene
            TriggerGraphCutscene();
        }

        /// <summary>
        /// Update node glow based on recent activity.
        /// </summary>
        private void UpdateNodeGlow()
        {
            foreach (var kvp in activeNodes)
            {
                ContributorNodeData data = contributorData.Find(d => d.contributorId == kvp.Key);
                if (data == null) continue;

                float timeSinceActivity = Time.time - data.recentActivity;
                float glowIntensity = Mathf.Max(0f, 1f - (timeSinceActivity / 3600f)); // Fade over 1 hour

                kvp.Value.SetGlowIntensity(glowIntensity);
            }
        }

        /// <summary>
        /// Get contributor node data.
        /// </summary>
        public ContributorNodeData GetNodeData(string contributorId)
        {
            return contributorData.Find(d => d.contributorId == contributorId);
        }

        /// <summary>
        /// Get lore links for contributor.
        /// </summary>
        public List<LoreLink> GetLoreLinks(string contributorId)
        {
            return activeLinks.FindAll(link => 
                link.fromId == contributorId || link.toId == contributorId
            );
        }

        /// <summary>
        /// Trigger graph cutscene.
        /// </summary>
        private void TriggerGraphCutscene()
        {
            if (cutsceneTrigger != null)
            {
                cutsceneTrigger.TriggerCutscene();
            }
        }

        /// <summary>
        /// Export graph data as JSON.
        /// </summary>
        public string ExportGraphData()
        {
            GraphExportData exportData = new GraphExportData
            {
                timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                contributorCount = contributorData.Count,
                loreLinksCount = activeLinks.Count,
                contributors = contributorData,
                links = activeLinks.ConvertAll(link => new LinkExportData
                {
                    from = link.fromId,
                    to = link.toId,
                    loreContext = link.loreContext
                })
            };

            return JsonUtility.ToJson(exportData, true);
        }
    }

    /// <summary>
    /// Contributor node data.
    /// </summary>
    [System.Serializable]
    public class ContributorNodeData
    {
        public string contributorId;
        public string contributorName;
        public string badgeTier;
        public int loreCount;
        public int daoPower;
        public float recentActivity;
    }

    /// <summary>
    /// Graph export data.
    /// </summary>
    [System.Serializable]
    public class GraphExportData
    {
        public string timestamp;
        public int contributorCount;
        public int loreLinksCount;
        public List<ContributorNodeData> contributors;
        public List<LinkExportData> links;
    }

    /// <summary>
    /// Link export data.
    /// </summary>
    [System.Serializable]
    public class LinkExportData
    {
        public string from;
        public string to;
        public string loreContext;
    }
}
