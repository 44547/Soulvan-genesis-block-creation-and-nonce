using System;
using UnityEngine;

namespace Soulvan.Missions
{
    /// <summary>
    /// Advanced drone AI with detection, engagement, and cinematic combat behaviors.
    /// </summary>
    public class DroneAI : MonoBehaviour
    {
        [Header("AI Configuration")]
        public Transform player;
        public float detectionRange = 20f;
        public float attackRange = 15f;
        public float flightSpeed = 10f;
        public float rotationSpeed = 5f;

        [Header("Combat")]
        public GameObject projectilePrefab;
        public Transform firePoint;
        public float fireRate = 1f;
        public float projectileSpeed = 30f;
        public float health = 100f;

        [Header("AI Behavior")]
        public DroneType droneType = DroneType.Combat;
        public bool isAggressive = true;

        private float lastFireTime = 0f;
        private bool playerDetected = false;
        private Vector3 patrolTarget;

        public event Action OnEnemyDefeated;

        private void Start()
        {
            SetRandomPatrolTarget();
        }

        void Update()
        {
            if (player == null) return;

            // Check player detection
            playerDetected = IsPlayerDetected(player);

            if (playerDetected)
            {
                EngagePlayer(player);
            }
            else
            {
                Patrol();
            }

            // Check if destroyed
            if (health <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Check if player is detected.
        /// </summary>
        public bool IsPlayerDetected(Transform target)
        {
            if (target == null) return false;

            float distance = Vector3.Distance(transform.position, target.position);
            
            if (distance > detectionRange) return false;

            // Check line of sight
            RaycastHit hit;
            Vector3 direction = target.position - transform.position;
            
            if (Physics.Raycast(transform.position, direction, out hit, detectionRange))
            {
                return hit.transform == target;
            }

            return false;
        }

        /// <summary>
        /// Engage player with attacks.
        /// </summary>
        public void EngagePlayer(Transform target)
        {
            if (target == null) return;

            // Move toward player
            Vector3 targetPosition = target.position;
            targetPosition.y += 5f; // Maintain height above player

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                flightSpeed * Time.deltaTime
            );

            // Look at player
            Vector3 lookDirection = target.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Attack if in range
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= attackRange && CanFire())
            {
                Attack(target.position);
            }
        }

        /// <summary>
        /// Patrol behavior when player not detected.
        /// </summary>
        private void Patrol()
        {
            // Move toward patrol target
            transform.position = Vector3.MoveTowards(
                transform.position,
                patrolTarget,
                flightSpeed * 0.5f * Time.deltaTime
            );

            // Check if reached patrol target
            if (Vector3.Distance(transform.position, patrolTarget) < 2f)
            {
                SetRandomPatrolTarget();
            }
        }

        /// <summary>
        /// Set random patrol target.
        /// </summary>
        private void SetRandomPatrolTarget()
        {
            patrolTarget = transform.position + new Vector3(
                UnityEngine.Random.Range(-20f, 20f),
                UnityEngine.Random.Range(-5f, 5f),
                UnityEngine.Random.Range(-20f, 20f)
            );
        }

        /// <summary>
        /// Check if drone can fire.
        /// </summary>
        private bool CanFire()
        {
            return Time.time - lastFireTime >= fireRate;
        }

        /// <summary>
        /// Attack target position.
        /// </summary>
        private void Attack(Vector3 targetPos)
        {
            lastFireTime = Time.time;

            ShootProjectile(targetPos);

            // Play attack sound
            AudioManager.PlayDroneAttack();
        }

        /// <summary>
        /// Shoot projectile at target.
        /// </summary>
        public void ShootProjectile(Vector3 targetPos)
        {
            if (projectilePrefab == null) return;

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (targetPos - spawnPos).normalized;
                rb.velocity = direction * projectileSpeed;
            }

            Destroy(proj, 10f);
        }

        /// <summary>
        /// Take damage.
        /// </summary>
        public void TakeDamage(float damage)
        {
            health -= damage;

            if (health <= 0)
            {
                Die();
            }
            else
            {
                // Play damage effect
                // ParticleSystem.Play("DamageEffect");
            }
        }

        /// <summary>
        /// Die with cinematic explosion.
        /// </summary>
        private void Die()
        {
            Debug.Log($"[DroneAI] {droneType} drone destroyed!");

            // Trigger explosion effect
            // ParticleSystem.Play("DroneExplosion");

            // Play explosion sound
            AudioManager.PlayExplosion();

            // Chance for cinematic death (10%)
            if (UnityEngine.Random.value < 0.1f)
            {
                TriggerCinematicDeath();
            }

            // Notify mission
            OnEnemyDefeated?.Invoke();

            // Destroy drone
            Destroy(gameObject);
        }

        /// <summary>
        /// Trigger cinematic death moment.
        /// </summary>
        private void TriggerCinematicDeath()
        {
            Debug.Log("[DroneAI] 🎬 Cinematic explosion!");
            
            // Slow motion
            Time.timeScale = 0.4f;
            // Will be reset by mission system
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Check if hit by bullet
            if (collision.gameObject.CompareTag("Bullet"))
            {
                TakeDamage(25f);
                Destroy(collision.gameObject);
            }
        }
    }

    /// <summary>
    /// Sniper bot AI with precision targeting.
    /// </summary>
    public class SniperBotAI : MonoBehaviour
    {
        [Header("Sniper Configuration")]
        public Transform player;
        public float sightRange = 50f;
        public float aimTime = 2f;
        public GameObject laserSight;
        public GameObject bulletPrefab;
        public Transform firePoint;

        private bool isAiming = false;
        private float aimTimer = 0f;

        public event Action OnEnemyDefeated;

        void Update()
        {
            if (player == null) return;

            if (HasLineOfSight(player))
            {
                AimAtPlayer(player);
            }
        }

        /// <summary>
        /// Check if has line of sight to player.
        /// </summary>
        public bool HasLineOfSight(Transform target)
        {
            if (target == null) return false;

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > sightRange) return false;

            RaycastHit hit;
            Vector3 direction = target.position - transform.position;
            
            if (Physics.Raycast(transform.position, direction, out hit, sightRange))
            {
                return hit.transform == target;
            }

            return false;
        }

        /// <summary>
        /// Aim at player with laser sight.
        /// </summary>
        public void AimAtPlayer(Transform target)
        {
            // Look at player
            transform.LookAt(target);

            // Show laser sight
            if (laserSight != null && !isAiming)
            {
                laserSight.SetActive(true);
                isAiming = true;
                aimTimer = 0f;
            }

            // Increment aim timer
            if (isAiming)
            {
                aimTimer += Time.deltaTime;

                // Fire when aim complete
                if (aimTimer >= aimTime)
                {
                    Fire(target.position);
                    isAiming = false;
                    aimTimer = 0f;
                    
                    if (laserSight != null)
                    {
                        laserSight.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Fire sniper shot.
        /// </summary>
        private void Fire(Vector3 targetPos)
        {
            Debug.Log("[SniperBotAI] Sniper shot fired!");

            if (bulletPrefab != null && firePoint != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = (targetPos - firePoint.position).normalized * 100f;
                }
                Destroy(bullet, 5f);
            }

            AudioManager.PlaySniperShot();
        }
    }

    /// <summary>
    /// Drone type enum.
    /// </summary>
    public enum DroneType
    {
        Security,
        Combat,
        Sniper,
        Chase,
        Incendiary
    }

    public static partial class AudioManager
    {
        public static void PlayDroneAttack()
        {
            Debug.Log("[AudioManager] Drone attack sound");
        }

        public static void PlayExplosion()
        {
            Debug.Log("[AudioManager] BOOM!");
        }

        public static void PlaySniperShot()
        {
            Debug.Log("[AudioManager] Sniper rifle shot!");
        }
    }
}
