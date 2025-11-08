using System;
using System.Collections;
using UnityEngine;

namespace Soulvan.Systems
{
    /// <summary>
    /// Coin hologram display in Soulvan's command chamber.
    /// Shows orbiting SoulvanCoin with glow, spin, and pulse effects.
    /// </summary>
    public class CoinHologram : MonoBehaviour
    {
        [Header("Display")]
        public TextMesh amountText;
        public GameObject coinModel;

        [Header("Animation")]
        public float rotationSpeed = 50f;
        public float pulseSpeed = 2f;
        public float orbitRadius = 2f;
        public float orbitSpeed = 30f;

        [Header("Effects")]
        public ParticleSystem glowParticles;
        public Light hologramLight;
        public Color hologramColor = Color.cyan;

        private float baseScale = 1f;
        private float orbitAngle = 0f;

        private void Start()
        {
            baseScale = transform.localScale.x;
            orbitAngle = UnityEngine.Random.Range(0f, 360f);

            // Setup hologram light
            if (hologramLight != null)
            {
                hologramLight.color = hologramColor;
                hologramLight.intensity = 2f;
            }

            // Play glow particles
            if (glowParticles != null)
            {
                glowParticles.Play();
            }
        }

        /// <summary>
        /// Set coin amount displayed.
        /// </summary>
        public void SetAmount(float amount)
        {
            if (amountText != null)
            {
                amountText.text = $"{amount:F2} SoulvanCoin";
                amountText.color = hologramColor;
            }

            Debug.Log($"[CoinHologram] Displaying: {amount:F2} SVN");
        }

        private void Update()
        {
            AnimateHologram();
        }

        /// <summary>
        /// Animate hologram with spin, pulse, and orbit.
        /// </summary>
        private void AnimateHologram()
        {
            // Spin coin model
            if (coinModel != null)
            {
                coinModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }

            // Pulse scale
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.1f;
            transform.localScale = Vector3.one * baseScale * pulse;

            // Orbit around spawn point
            orbitAngle += orbitSpeed * Time.deltaTime;
            if (orbitAngle > 360f) orbitAngle -= 360f;

            float x = Mathf.Cos(orbitAngle * Mathf.Deg2Rad) * orbitRadius;
            float z = Mathf.Sin(orbitAngle * Mathf.Deg2Rad) * orbitRadius;

            Vector3 orbitPosition = transform.parent.position + new Vector3(x, 0, z);
            transform.position = Vector3.Lerp(transform.position, orbitPosition, Time.deltaTime * 2f);

            // Pulse light
            if (hologramLight != null)
            {
                hologramLight.intensity = 2f + Mathf.Sin(Time.time * pulseSpeed) * 0.5f;
            }
        }
    }
}
