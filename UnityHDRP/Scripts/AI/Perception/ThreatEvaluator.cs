using UnityEngine;

namespace Soulvan.AI.Perception
{
    /// <summary>
    /// Evaluates threat level based on rival proximity, police distance, speed risk, and damage.
    /// Outputs normalized threat score (0..1) used by UtilityScorer for goal selection.
    /// </summary>
    public class ThreatEvaluator : MonoBehaviour
    {
        [Header("Threat Weights")]
        [SerializeField] private float rivalWeight = 0.45f;
        [SerializeField] private float policeWeight = 0.35f;
        [SerializeField] private float speedWeight = 0.15f;
        [SerializeField] private float damageWeight = 0.05f;

        [Header("Thresholds")]
        [SerializeField] private float maxSpeedKmh = 220f;
        [SerializeField] private float policeDetectionRadius = 50f;

        /// <summary>
        /// Calculate normalized threat score (0..1).
        /// </summary>
        public float Evaluate(Transform rival, Vector3 policePos, float speedKmh, float damagePct)
        {
            // Inverse proximity scoring (closer = higher threat)
            float rivalProx = rival ? 1f / Mathf.Max(1f, Vector3.Distance(transform.position, rival.position)) : 0f;
            float policeProx = 1f / Mathf.Max(1f, Vector3.Distance(transform.position, policePos));
            
            // Normalize speed risk
            float speedRisk = Mathf.Clamp01(speedKmh / maxSpeedKmh);
            
            // Damage already 0..1
            float damageRisk = Mathf.Clamp01(damagePct);
            
            // Weighted sum
            float threat = rivalWeight * rivalProx + 
                          policeWeight * policeProx + 
                          speedWeight * speedRisk + 
                          damageWeight * damageRisk;
            
            return Mathf.Clamp01(threat);
        }

        /// <summary>
        /// Overload for evaluating multiple police positions.
        /// </summary>
        public float EvaluateMulti(Transform rival, Vector3[] policePositions, float speedKmh, float damagePct)
        {
            float maxPoliceProx = 0f;
            foreach (var pos in policePositions)
            {
                float prox = 1f / Mathf.Max(1f, Vector3.Distance(transform.position, pos));
                if (prox > maxPoliceProx) maxPoliceProx = prox;
            }

            float rivalProx = rival ? 1f / Mathf.Max(1f, Vector3.Distance(transform.position, rival.position)) : 0f;
            float speedRisk = Mathf.Clamp01(speedKmh / maxSpeedKmh);
            float damageRisk = Mathf.Clamp01(damagePct);

            float threat = rivalWeight * rivalProx + 
                          policeWeight * maxPoliceProx + 
                          speedWeight * speedRisk + 
                          damageWeight * damageRisk;

            return Mathf.Clamp01(threat);
        }
    }
}
