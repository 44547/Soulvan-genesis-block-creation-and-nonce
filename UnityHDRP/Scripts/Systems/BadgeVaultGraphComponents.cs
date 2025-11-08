using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soulvan.Systems
{
    /// <summary>
    /// Visual contributor node in badge vault graph.
    /// </summary>
    public class ContributorNode : MonoBehaviour
    {
        [Header("Node Display")]
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI tierText;
        public TextMeshProUGUI statsText;
        public Image nodeBackground;

        [Header("FX")]
        public ParticleSystem glowFX;
        public Light nodeLight;
        public float pulseSpeed = 2f;

        private ContributorNodeData nodeData;
        private float glowIntensity = 1f;

        private void Update()
        {
            AnimateNodePulse();
        }

        /// <summary>
        /// Set node data.
        /// </summary>
        public void SetData(ContributorNodeData data)
        {
            nodeData = data;

            if (nameText != null)
                nameText.text = data.contributorName;

            if (tierText != null)
                tierText.text = data.badgeTier;

            if (statsText != null)
            {
                statsText.text = $"Lore: {data.loreCount}\nDAO: {data.daoPower}";
            }
        }

        /// <summary>
        /// Set node color based on badge tier.
        /// </summary>
        public void SetColor(Color color)
        {
            if (nodeBackground != null)
                nodeBackground.color = color;

            if (nodeLight != null)
                nodeLight.color = color;

            if (glowFX != null)
            {
                var main = glowFX.main;
                main.startColor = color;
            }
        }

        /// <summary>
        /// Set glow intensity based on recent activity.
        /// </summary>
        public void SetGlowIntensity(float intensity)
        {
            glowIntensity = Mathf.Clamp01(intensity);

            if (nodeLight != null)
                nodeLight.intensity = glowIntensity * 2f;
        }

        /// <summary>
        /// Animate node pulse effect.
        /// </summary>
        private void AnimateNodePulse()
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.1f;
            transform.localScale = Vector3.one * pulse * glowIntensity;
        }
    }

    /// <summary>
    /// Visual lore link between contributors.
    /// </summary>
    public class LoreLink : MonoBehaviour
    {
        [Header("Link Configuration")]
        public Transform fromNode;
        public Transform toNode;
        public string loreContext;
        
        [HideInInspector]
        public string fromId;
        [HideInInspector]
        public string toId;

        [Header("Visual")]
        public LineRenderer lineRenderer;
        public Color linkColor = new Color(0f, 1f, 1f, 0.3f);
        public float linkWidth = 0.1f;

        [Header("Animation")]
        public bool animated = true;
        public float flowSpeed = 1f;

        private void Start()
        {
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                lineRenderer.startWidth = linkWidth;
                lineRenderer.endWidth = linkWidth;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = linkColor;
                lineRenderer.endColor = linkColor;
            }
        }

        private void Update()
        {
            if (fromNode != null && toNode != null)
            {
                UpdateLinkPosition();
                
                if (animated)
                {
                    AnimateFlow();
                }
            }
        }

        /// <summary>
        /// Set connection between nodes.
        /// </summary>
        public void SetConnection(Transform from, Transform to, string context)
        {
            fromNode = from;
            toNode = to;
            loreContext = context;

            fromId = from.GetComponent<ContributorNode>()?.nodeData?.contributorId;
            toId = to.GetComponent<ContributorNode>()?.nodeData?.contributorId;
        }

        /// <summary>
        /// Update link position.
        /// </summary>
        private void UpdateLinkPosition()
        {
            if (lineRenderer == null) return;

            lineRenderer.SetPosition(0, fromNode.position);
            lineRenderer.SetPosition(1, toNode.position);
        }

        /// <summary>
        /// Animate flow effect.
        /// </summary>
        private void AnimateFlow()
        {
            if (lineRenderer == null) return;

            // Pulse width
            float pulse = linkWidth * (1f + Mathf.Sin(Time.time * flowSpeed) * 0.2f);
            lineRenderer.startWidth = pulse;
            lineRenderer.endWidth = pulse;
        }
    }

    /// <summary>
    /// Badge vault graph cutscene trigger.
    /// </summary>
    public class BadgeVaultGraphCutsceneTrigger : MonoBehaviour
    {
        public Camera cinematicCamera;
        public Transform[] cameraPoints;
        public AudioClip soulvanVoiceLine;
        public GameObject rippleFX;

        public void TriggerCutscene()
        {
            StartCoroutine(PlayCutscene());
        }

        System.Collections.IEnumerator PlayCutscene()
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
            
            SoulvanLore.Record("SoulvanBadgeVaultGraphFX cutscene triggered");
        }
    }
}
