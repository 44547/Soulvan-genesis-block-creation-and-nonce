using System;
using UnityEngine;

namespace Soulvan.Missions
{
    /// <summary>
    /// Weapon system with dual-wielding, muzzle flash, and cinematic combat.
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Weapon Configuration")]
        public GameObject muzzleFlash;
        public GameObject bulletPrefab;
        public Transform firePoint;
        public Transform firePointLeft; // For dual wielding

        [Header("Combat Stats")]
        public int ammoCount = 100;
        public int maxAmmo = 100;
        public float fireRate = 0.1f;
        public float damage = 25f;
        public bool dualWieldEnabled = false;

        private float lastFireTime = 0f;
        private bool isEnabled = false;

        /// <summary>
        /// Enable weapon system.
        /// </summary>
        public void EnableGuns()
        {
            isEnabled = true;
            Debug.Log("[WeaponSystem] Weapons enabled!");

            // Show HUD
            UIManager.ShowNotification("🔫 Weapons Active - LMB to fire");
            UIManager.ShowAmmoCounter(ammoCount, maxAmmo);
        }

        /// <summary>
        /// Enable dual wielding mode.
        /// </summary>
        public void EnableDualWield()
        {
            dualWieldEnabled = true;
            Debug.Log("[WeaponSystem] 🔫🔫 DUAL WIELD MODE ACTIVATED!");
            
            UIManager.ShowNotification("⚡ DUAL WIELD MODE!");
            
            // Trigger cinematic moment
            TriggerDualWieldCinematic();
        }

        void Update()
        {
            if (!isEnabled) return;

            HandleWeaponInput();
        }

        /// <summary>
        /// Handle weapon firing input.
        /// </summary>
        private void HandleWeaponInput()
        {
            if (Input.GetMouseButton(0) && CanFire())
            {
                Fire();

                if (dualWieldEnabled)
                {
                    FireLeft();
                }
            }

            // Reload
            if (Input.GetKeyDown(KeyCode.R))
            {
                Reload();
            }
        }

        /// <summary>
        /// Check if weapon can fire.
        /// </summary>
        private bool CanFire()
        {
            return Time.time - lastFireTime >= fireRate && ammoCount > 0;
        }

        /// <summary>
        /// Fire right weapon.
        /// </summary>
        private void Fire()
        {
            lastFireTime = Time.time;
            ammoCount--;

            // Muzzle flash
            if (muzzleFlash != null && firePoint != null)
            {
                GameObject flash = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation);
                Destroy(flash, 0.1f);
            }

            // Spawn bullet
            if (bulletPrefab != null && firePoint != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = firePoint.forward * 50f;
                }
                Destroy(bullet, 5f);
            }

            // Play fire sound
            AudioManager.PlayGunfire();

            // Update HUD
            UIManager.ShowAmmoCounter(ammoCount, maxAmmo);

            // Chance for cinematic moment
            if (UnityEngine.Random.value < 0.05f) // 5% chance
            {
                TriggerFireCinematic();
            }
        }

        /// <summary>
        /// Fire left weapon (dual wield).
        /// </summary>
        private void FireLeft()
        {
            if (firePointLeft == null) return;

            // Muzzle flash
            if (muzzleFlash != null)
            {
                GameObject flash = Instantiate(muzzleFlash, firePointLeft.position, firePointLeft.rotation);
                Destroy(flash, 0.1f);
            }

            // Spawn bullet
            if (bulletPrefab != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, firePointLeft.position, firePointLeft.rotation);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = firePointLeft.forward * 50f;
                }
                Destroy(bullet, 5f);
            }
        }

        /// <summary>
        /// Reload weapon.
        /// </summary>
        private void Reload()
        {
            Debug.Log("[WeaponSystem] Reloading...");
            ammoCount = maxAmmo;
            UIManager.ShowAmmoCounter(ammoCount, maxAmmo);
            AudioManager.PlayReload();
        }

        /// <summary>
        /// Trigger cinematic fire moment.
        /// </summary>
        private void TriggerFireCinematic()
        {
            Debug.Log("[WeaponSystem] 🎬 Cinematic gunfire!");
            
            // Slow motion
            Time.timeScale = 0.5f;
            Invoke("ResetTimeScale", 1f);
        }

        /// <summary>
        /// Trigger dual wield cinematic.
        /// </summary>
        private void TriggerDualWieldCinematic()
        {
            Debug.Log("[WeaponSystem] 🎬 Epic dual wield moment!");
            
            // Slow motion
            Time.timeScale = 0.3f;
            Invoke("ResetTimeScale", 3f);

            // Camera effect
            // CinematicCamera.TriggerDutchAngle();
        }

        private void ResetTimeScale()
        {
            Time.timeScale = 1.0f;
        }
    }

    public static partial class AudioManager
    {
        public static void PlayGunfire()
        {
            Debug.Log("[AudioManager] Bang!");
        }

        public static void PlayReload()
        {
            Debug.Log("[AudioManager] Reload sound");
        }

        public static void PlayGrappleSound()
        {
            Debug.Log("[AudioManager] Grapple whoosh!");
        }
    }

    public static partial class UIManager
    {
        public static void ShowAmmoCounter(int current, int max)
        {
            Debug.Log($"[UIManager] Ammo: {current}/{max}");
        }
    }
}
