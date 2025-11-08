using System;
using UnityEngine;

namespace Soulvan.Missions
{
    /// <summary>
    /// Rune grapple system for parkour and traversal.
    /// Features cinematic grapple swings, wall runs, and epic moments.
    /// </summary>
    public class GrappleSystem : MonoBehaviour
    {
        [Header("Grapple Configuration")]
        public LineRenderer grappleLine;
        public Transform runeEmitter;
        public LayerMask grappleSurfaces;
        public float grappleSpeed = 25f;
        public float grappleRange = 50f;
        public GameObject runeEffect;

        [Header("Rune Visuals")]
        public Material runeTrailMaterial;
        public ParticleSystem runeParticles;
        public Color runeColor = Color.cyan;

        private bool isGrappling = false;
        private Vector3 targetPoint;
        private GameObject player;

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            
            if (grappleLine != null)
            {
                grappleLine.enabled = false;
                grappleLine.startWidth = 0.1f;
                grappleLine.endWidth = 0.05f;
                grappleLine.material = runeTrailMaterial;
            }
        }

        /// <summary>
        /// Enable rune grapple system.
        /// </summary>
        public void EnableRunes()
        {
            Debug.Log("[GrappleSystem] Rune grapples enabled!");
            
            if (runeParticles != null)
            {
                runeParticles.Play();
            }

            // Show HUD prompt
            UIManager.ShowNotification("🔮 Rune Grapples Active - Press G to grapple");
        }

        void Update()
        {
            HandleGrappleInput();
            UpdateGrapple();
        }

        /// <summary>
        /// Handle grapple input.
        /// </summary>
        private void HandleGrappleInput()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                TryGrapple();
            }

            if (Input.GetKeyUp(KeyCode.G) && isGrappling)
            {
                ReleaseGrapple();
            }
        }

        /// <summary>
        /// Attempt to grapple to surface.
        /// </summary>
        private void TryGrapple()
        {
            RaycastHit hit;
            
            if (Physics.Raycast(runeEmitter.position, runeEmitter.forward, out hit, grappleRange, grappleSurfaces))
            {
                StartGrapple(hit.point);
            }
            else
            {
                Debug.Log("[GrappleSystem] No grapple point found");
            }
        }

        /// <summary>
        /// Start grapple to target point.
        /// </summary>
        private void StartGrapple(Vector3 point)
        {
            targetPoint = point;
            isGrappling = true;

            // Enable grapple line
            if (grappleLine != null)
            {
                grappleLine.enabled = true;
                grappleLine.SetPosition(0, runeEmitter.position);
                grappleLine.SetPosition(1, targetPoint);
            }

            // Spawn rune effect at target
            if (runeEffect != null)
            {
                Instantiate(runeEffect, targetPoint, Quaternion.identity);
            }

            // Play grapple sound
            AudioManager.PlayGrappleSound();

            // Trigger cinematic moment (15% chance)
            if (UnityEngine.Random.value < 0.15f)
            {
                TriggerGrappleCinematic();
            }

            Debug.Log($"[GrappleSystem] Grappling to {targetPoint}");
        }

        /// <summary>
        /// Update grapple movement.
        /// </summary>
        private void UpdateGrapple()
        {
            if (!isGrappling) return;

            // Move player toward target
            if (player != null)
            {
                player.transform.position = Vector3.MoveTowards(
                    player.transform.position,
                    targetPoint,
                    grappleSpeed * Time.deltaTime
                );

                // Update grapple line
                if (grappleLine != null)
                {
                    grappleLine.SetPosition(0, runeEmitter.position);
                }

                // Check if reached target
                if (Vector3.Distance(player.transform.position, targetPoint) < 1f)
                {
                    ReleaseGrapple();
                }
            }
        }

        /// <summary>
        /// Release grapple.
        /// </summary>
        private void ReleaseGrapple()
        {
            isGrappling = false;

            if (grappleLine != null)
            {
                grappleLine.enabled = false;
            }

            Debug.Log("[GrappleSystem] Grapple released");
        }

        /// <summary>
        /// Trigger cinematic grapple moment.
        /// </summary>
        private void TriggerGrappleCinematic()
        {
            Debug.Log("[GrappleSystem] 🎬 Epic grapple swing!");
            
            // Slow motion
            Time.timeScale = 0.4f;
            Invoke("ResetTimeScale", 2f);

            // Camera effect
            // CinematicCamera.TriggerWideAngle();
        }

        private void ResetTimeScale()
        {
            Time.timeScale = 1.0f;
        }
    }
}
