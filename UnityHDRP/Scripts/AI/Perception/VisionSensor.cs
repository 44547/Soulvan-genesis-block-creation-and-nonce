using UnityEngine;

namespace Soulvan.AI.Perception
{
    /// <summary>
    /// Raycasting vision sensor for line-of-sight checks.
    /// Used by agents to detect rivals, cargo, drones, and obstacles.
    /// </summary>
    public class VisionSensor : MonoBehaviour
    {
        [SerializeField] private float maxDistance = 80f;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private float fovAngle = 120f; // Field of view in degrees

        /// <summary>
        /// Check if target is visible within FOV and not occluded.
        /// </summary>
        public bool SeeTarget(Transform t)
        {
            if (!t) return false;
            
            Vector3 dir = (t.position - transform.position).normalized;
            
            // Check FOV
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle > fovAngle * 0.5f) return false;
            
            // Raycast to check occlusion
            if (Physics.Raycast(transform.position, dir, out var hit, maxDistance, obstacleMask | targetMask))
            {
                return hit.transform == t;
            }
            return false;
        }

        /// <summary>
        /// Check if position has line of sight (no obstacles blocking).
        /// </summary>
        public bool LineOfSight(Vector3 pos)
        {
            Vector3 dir = (pos - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, pos);
            return !Physics.Raycast(transform.position, dir, distance, obstacleMask);
        }

        /// <summary>
        /// Find all visible targets within range.
        /// </summary>
        public Transform[] FindVisibleTargets(string tag)
        {
            var candidates = GameObject.FindGameObjectsWithTag(tag);
            var visible = new System.Collections.Generic.List<Transform>();
            
            foreach (var candidate in candidates)
            {
                if (SeeTarget(candidate.transform))
                {
                    visible.Add(candidate.transform);
                }
            }
            
            return visible.ToArray();
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize FOV cone in editor
            Gizmos.color = Color.yellow;
            Vector3 fovLine1 = Quaternion.AngleAxis(fovAngle * 0.5f, transform.up) * transform.forward * maxDistance;
            Vector3 fovLine2 = Quaternion.AngleAxis(-fovAngle * 0.5f, transform.up) * transform.forward * maxDistance;
            
            Gizmos.DrawRay(transform.position, fovLine1);
            Gizmos.DrawRay(transform.position, fovLine2);
            Gizmos.DrawRay(transform.position, transform.forward * maxDistance);
        }
    }
}
