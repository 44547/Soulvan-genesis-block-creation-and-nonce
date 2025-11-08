using UnityEngine;

namespace Soulvan.AI.Actions
{
    /// <summary>
    /// Combat actions for boss battles and aggressive encounters.
    /// Handles ramming, shooting (if equipped), and defensive maneuvers.
    /// </summary>
    public class CombatActions : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float ramDamage = 25f;
        [SerializeField] private float ramCooldown = 3f;
        [SerializeField] private float evasionForce = 50f;

        [Header("Weapon Settings")]
        [SerializeField] private bool hasWeapons = false;
        [SerializeField] private Transform[] weaponMounts;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float fireRate = 0.5f;

        private float lastRamTime;
        private float lastFireTime;
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Execute ram attack on target.
        /// </summary>
        public bool RamTarget(Transform target)
        {
            if (Time.time - lastRamTime < ramCooldown) return false;
            if (!target) return false;

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > 10f) return false;

            // Apply ram damage (integrate with health system)
            var healthComp = target.GetComponent<HealthComponent>();
            if (healthComp)
            {
                healthComp.TakeDamage(ramDamage);
            }

            // Apply physics impulse
            var targetRb = target.GetComponent<Rigidbody>();
            if (targetRb)
            {
                Vector3 dir = (target.position - transform.position).normalized;
                targetRb.AddForce(dir * 500f, ForceMode.Impulse);
            }

            lastRamTime = Time.time;
            Debug.Log($"[CombatActions] {gameObject.name} rammed {target.name}!");
            return true;
        }

        /// <summary>
        /// Fire weapon at target (if equipped).
        /// </summary>
        public bool Fire Weapon(Transform target)
        {
            if (!hasWeapons) return false;
            if (Time.time - lastFireTime < fireRate) return false;
            if (!target) return false;
            if (weaponMounts.Length == 0) return false;

            foreach (var mount in weaponMounts)
            {
                if (!mount) continue;

                Vector3 dir = (target.position - mount.position).normalized;
                
                if (projectilePrefab)
                {
                    var projectile = Instantiate(projectilePrefab, mount.position, Quaternion.LookRotation(dir));
                    var projRb = projectile.GetComponent<Rigidbody>();
                    if (projRb)
                    {
                        projRb.velocity = dir * 100f;
                    }
                }
            }

            lastFireTime = Time.time;
            return true;
        }

        /// <summary>
        /// Execute evasive maneuver (quick dodge).
        /// </summary>
        public void Evade(Vector3 threatDirection)
        {
            if (!rb) return;

            // Dodge perpendicular to threat
            Vector3 dodgeDir = Vector3.Cross(threatDirection.normalized, Vector3.up);
            
            // Randomly choose left or right
            if (Random.value > 0.5f) dodgeDir = -dodgeDir;

            rb.AddForce(dodgeDir * evasionForce, ForceMode.Impulse);
            Debug.Log($"[CombatActions] {gameObject.name} evading!");
        }

        /// <summary>
        /// Check if target is in firing arc.
        /// </summary>
        public bool IsTargetInArc(Transform target, float arcAngle = 30f)
        {
            if (!target) return false;

            Vector3 dir = (target.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dir);
            return angle <= arcAngle;
        }
    }

    /// <summary>
    /// Simple health component for damage tracking.
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        private float currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                OnDeath();
            }
        }

        public float GetHealthPct() => currentHealth / maxHealth;
        public float GetDamagePct() => 1f - GetHealthPct();

        private void OnDeath()
        {
            Debug.Log($"[Health] {gameObject.name} destroyed!");
            // Trigger death effects, respawn, etc.
        }
    }
}
