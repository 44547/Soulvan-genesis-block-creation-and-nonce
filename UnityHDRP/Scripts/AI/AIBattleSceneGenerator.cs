using System;
using System.Collections.Generic;
using UnityEngine;

namespace Soulvan.AI
{
    /// <summary>
    /// AI system that generates dynamic epic battle scenes during missions.
    /// Creates cinematic moments, spawns reinforcements, adjusts difficulty,
    /// and ensures every battle feels unique and epic.
    /// </summary>
    public class AIBattleSceneGenerator : MonoBehaviour
    {
        [Header("AI Configuration")]
        [SerializeField] private float epicMomentChance = 0.15f; // 15% chance per update
        [SerializeField] private float reinforcementThreshold = 0.5f; // Spawn reinforcements at 50% enemy defeat
        [SerializeField] private bool dynamicDifficultyEnabled = true;

        [Header("Battle Analytics")]
        [SerializeField] private int totalEnemiesSpawned = 0;
        [SerializeField] private int totalCinematicMoments = 0;
        [SerializeField] private float battleIntensity = 0.5f;

        private Dictionary<string, EpicMomentTemplate> epicMomentLibrary = new Dictionary<string, EpicMomentTemplate>();
        private float lastReinforcementTime = 0f;
        private float lastEpicMomentTime = 0f;

        private void Start()
        {
            InitializeEpicMomentLibrary();
        }

        /// <summary>
        /// Initialize library of epic cinematic moments.
        /// </summary>
        private void InitializeEpicMomentLibrary()
        {
            epicMomentLibrary = new Dictionary<string, EpicMomentTemplate>
            {
                ["ExplosionDive"] = new EpicMomentTemplate
                {
                    name = "Explosion Dive",
                    description = "Dive through explosion in slow motion",
                    slowMotionDuration = 2f,
                    cameraEffect = "ExplosionBloom",
                    particleEffects = new[] { "Explosion", "Debris", "Smoke" }
                },
                ["BulletTime"] = new EpicMomentTemplate
                {
                    name = "Bullet Time",
                    description = "Dodge bullets in Matrix-style slow motion",
                    slowMotionDuration = 3f,
                    cameraEffect = "BulletTrails",
                    particleEffects = new[] { "BulletTrails", "ShellCasings" }
                },
                ["GrappleSwing"] = new EpicMomentTemplate
                {
                    name = "Epic Grapple Swing",
                    description = "Cinematic grapple swing between buildings",
                    slowMotionDuration = 2.5f,
                    cameraEffect = "WideAngle",
                    particleEffects = new[] { "RuneTrail", "WindGust" }
                },
                ["DualWield"] = new EpicMomentTemplate
                {
                    name = "Dual Wield Assault",
                    description = "Dual-wielding gunfire with cinematic camera",
                    slowMotionDuration = 2f,
                    cameraEffect = "DutchAngle",
                    particleEffects = new[] { "MuzzleFlash", "BulletImpacts", "ShellCasings" }
                },
                ["Backflip"] = new EpicMomentTemplate
                {
                    name = "Backflip Over Explosion",
                    description = "Backflip over exploding enemy",
                    slowMotionDuration = 2.5f,
                    cameraEffect = "SlowOrbit",
                    particleEffects = new[] { "Explosion", "Shrapnel", "Fire" }
                },
                ["SlideShoot"] = new EpicMomentTemplate
                {
                    name = "Slide and Shoot",
                    description = "Slide under cover while returning fire",
                    slowMotionDuration = 1.5f,
                    cameraEffect = "LowAngle",
                    particleEffects = new[] { "Dust", "MuzzleFlash", "Sparks" }
                },
                ["LeapOfFaith"] = new EpicMomentTemplate
                {
                    name = "Leap of Faith",
                    description = "Jump between buildings in slow motion",
                    slowMotionDuration = 3f,
                    cameraEffect = "WideAngle",
                    particleEffects = new[] { "WindGust", "MotionBlur" }
                },
                ["ChainExplosion"] = new EpicMomentTemplate
                {
                    name = "Chain Explosion",
                    description = "Trigger chain of explosions, walk away slowly",
                    slowMotionDuration = 4f,
                    cameraEffect = "BackCamera",
                    particleEffects = new[] { "MultiExplosion", "Fire", "Smoke", "Debris" }
                }
            };

            Debug.Log($"[AIBattleSceneGenerator] Loaded {epicMomentLibrary.Count} epic moment templates");
        }

        /// <summary>
        /// Update battle scene with dynamic AI-generated events.
        /// </summary>
        public void UpdateBattleScene(BattleScene scene, int enemiesDefeated)
        {
            // Update battle intensity based on enemies defeated
            UpdateBattleIntensity(scene, enemiesDefeated);

            // Generate epic moments dynamically
            TryGenerateEpicMoment(scene);

            // Spawn reinforcements if needed
            TrySpawnReinforcements(scene, enemiesDefeated);

            // Adjust difficulty dynamically
            if (dynamicDifficultyEnabled)
            {
                AdjustDifficulty(scene, enemiesDefeated);
            }
        }

        /// <summary>
        /// Update battle intensity meter.
        /// </summary>
        private void UpdateBattleIntensity(BattleScene scene, int enemiesDefeated)
        {
            float defeatRatio = (float)enemiesDefeated / Mathf.Max(1f, scene.enemyCount);
            battleIntensity = Mathf.Lerp(scene.musicIntensity, 1.0f, defeatRatio);
        }

        /// <summary>
        /// Try to generate random epic moment during battle.
        /// </summary>
        private void TryGenerateEpicMoment(BattleScene scene)
        {
            // Cooldown check (at least 10 seconds between epic moments)
            if (Time.time - lastEpicMomentTime < 10f) return;

            // Random chance check
            if (UnityEngine.Random.value > epicMomentChance) return;

            // Generate epic moment
            GenerateRandomEpicMoment(scene);
        }

        /// <summary>
        /// Generate random epic cinematic moment.
        /// </summary>
        private void GenerateRandomEpicMoment(BattleScene scene)
        {
            // Select random epic moment from library
            var momentKeys = new List<string>(epicMomentLibrary.Keys);
            string randomKey = momentKeys[UnityEngine.Random.Range(0, momentKeys.Count)];
            var moment = epicMomentLibrary[randomKey];

            Debug.Log($"[AIBattleSceneGenerator] 🎬 Generating epic moment: {moment.name}");
            Debug.Log($"[AIBattleSceneGenerator] {moment.description}");

            // Trigger cinematic effects
            TriggerSlowMotion(moment.slowMotionDuration);
            TriggerCameraEffect(moment.cameraEffect);
            TriggerParticleEffects(moment.particleEffects);

            totalCinematicMoments++;
            lastEpicMomentTime = Time.time;

            // Play epic sound effect
            AudioManager.PlayEpicMoment(moment.name);
        }

        /// <summary>
        /// Trigger slow motion effect.
        /// </summary>
        private void TriggerSlowMotion(float duration)
        {
            Debug.Log($"[AIBattleSceneGenerator] ⏱️ Slow motion: {duration}s");
            // Time.timeScale = 0.3f; // Slow down to 30% speed
            // Invoke("ResetTimeScale", duration);
        }

        private void ResetTimeScale()
        {
            Time.timeScale = 1.0f;
        }

        /// <summary>
        /// Trigger cinematic camera effect.
        /// </summary>
        private void TriggerCameraEffect(string effect)
        {
            Debug.Log($"[AIBattleSceneGenerator] 📷 Camera effect: {effect}");

            switch (effect)
            {
                case "ExplosionBloom":
                    // Enable bloom, lens flare
                    break;
                case "BulletTrails":
                    // Enable bullet trail rendering
                    break;
                case "WideAngle":
                    // Switch to wide angle lens
                    break;
                case "DutchAngle":
                    // Tilt camera for dramatic effect
                    break;
                case "SlowOrbit":
                    // Orbit camera around player
                    break;
                case "LowAngle":
                    // Low angle shot from ground
                    break;
                case "BackCamera":
                    // Camera behind player facing away
                    break;
            }
        }

        /// <summary>
        /// Trigger particle effects.
        /// </summary>
        private void TriggerParticleEffects(string[] effects)
        {
            foreach (var effect in effects)
            {
                Debug.Log($"[AIBattleSceneGenerator] ✨ Particle effect: {effect}");
                // ParticleSystem.Play(effect);
            }
        }

        /// <summary>
        /// Try to spawn enemy reinforcements.
        /// </summary>
        private void TrySpawnReinforcements(BattleScene scene, int enemiesDefeated)
        {
            // Cooldown check (at least 15 seconds between reinforcements)
            if (Time.time - lastReinforcementTime < 15f) return;

            // Check if enough enemies defeated
            float defeatRatio = (float)enemiesDefeated / Mathf.Max(1f, scene.enemyCount);
            if (defeatRatio < reinforcementThreshold) return;

            // Spawn reinforcements
            SpawnReinforcements(scene);
        }

        /// <summary>
        /// Spawn enemy reinforcements with dramatic entrance.
        /// </summary>
        private void SpawnReinforcements(BattleScene scene)
        {
            int reinforcementCount = Mathf.CeilToInt(scene.enemyCount * 0.3f); // 30% of original enemies

            Debug.Log($"[AIBattleSceneGenerator] 🚨 REINFORCEMENTS INCOMING! Spawning {reinforcementCount} enemies");

            // Play warning sound
            AudioManager.PlayReinforcementWarning();

            // Show HUD warning
            UIManager.ShowWarning("⚠️ REINFORCEMENTS INCOMING!");

            // Spawn enemies with dramatic effect
            for (int i = 0; i < reinforcementCount; i++)
            {
                string enemyType = scene.enemyTypes[UnityEngine.Random.Range(0, scene.enemyTypes.Length)];
                // Spawn enemy with drop pod/portal effect
                Debug.Log($"[AIBattleSceneGenerator] Spawning reinforcement: {enemyType}");
            }

            totalEnemiesSpawned += reinforcementCount;
            lastReinforcementTime = Time.time;
        }

        /// <summary>
        /// Dynamically adjust battle difficulty based on player performance.
        /// </summary>
        private void AdjustDifficulty(BattleScene scene, int enemiesDefeated)
        {
            // Calculate player performance
            float timeInBattle = Time.time - lastReinforcementTime;
            float killRate = enemiesDefeated / Mathf.Max(1f, timeInBattle);

            if (killRate > 2f) // Player killing enemies too fast
            {
                Debug.Log("[AIBattleSceneGenerator] 📈 Increasing difficulty - Player performing well");
                // Increase enemy health, damage, accuracy
            }
            else if (killRate < 0.5f) // Player struggling
            {
                Debug.Log("[AIBattleSceneGenerator] 📉 Decreasing difficulty - Player needs help");
                // Decrease enemy health, damage, accuracy
                // Spawn health pickups
            }
        }

        /// <summary>
        /// Generate custom epic moment for specific situation.
        /// </summary>
        public void GenerateCustomEpicMoment(string situation)
        {
            Debug.Log($"[AIBattleSceneGenerator] Generating custom epic moment for: {situation}");

            EpicMomentTemplate customMoment = situation switch
            {
                "boss_defeated" => new EpicMomentTemplate
                {
                    name = "Boss Defeated",
                    description = "Boss explodes, player walks away in slow motion",
                    slowMotionDuration = 5f,
                    cameraEffect = "BackCamera",
                    particleEffects = new[] { "MassiveExplosion", "Shockwave", "Debris" }
                },
                "narrow_escape" => new EpicMomentTemplate
                {
                    name = "Narrow Escape",
                    description = "Escape collapsing building by inches",
                    slowMotionDuration = 3f,
                    cameraEffect = "WideAngle",
                    particleEffects = new[] { "Dust", "Debris", "Smoke" }
                },
                "perfect_headshot" => new EpicMomentTemplate
                {
                    name = "Perfect Headshot",
                    description = "Sniper headshot from extreme range",
                    slowMotionDuration = 2f,
                    cameraEffect = "BulletCam",
                    particleEffects = new[] { "BulletTrail", "BloodSpray" }
                },
                _ => epicMomentLibrary["BulletTime"]
            };

            // Trigger the custom moment
            TriggerSlowMotion(customMoment.slowMotionDuration);
            TriggerCameraEffect(customMoment.cameraEffect);
            TriggerParticleEffects(customMoment.particleEffects);

            totalCinematicMoments++;
        }

        /// <summary>
        /// Get AI battle scene stats.
        /// </summary>
        public BattleSceneStats GetStats()
        {
            return new BattleSceneStats
            {
                totalEnemiesSpawned = totalEnemiesSpawned,
                totalCinematicMoments = totalCinematicMoments,
                currentBattleIntensity = battleIntensity,
                epicMomentLibrarySize = epicMomentLibrary.Count
            };
        }
    }

    #region Data Structures

    /// <summary>
    /// Epic cinematic moment template.
    /// </summary>
    [Serializable]
    public class EpicMomentTemplate
    {
        public string name;
        public string description;
        public float slowMotionDuration;
        public string cameraEffect;
        public string[] particleEffects;
    }

    /// <summary>
    /// Battle scene generation stats.
    /// </summary>
    [Serializable]
    public class BattleSceneStats
    {
        public int totalEnemiesSpawned;
        public int totalCinematicMoments;
        public float currentBattleIntensity;
        public int epicMomentLibrarySize;
    }

    #endregion

    #region Stub Classes

    public static class AudioManager
    {
        public static void PlayBattleMusic(float intensity)
        {
            Debug.Log($"[AudioManager] Playing battle music (intensity: {intensity})");
        }

        public static void PlayBossMusic()
        {
            Debug.Log("[AudioManager] Playing boss music");
        }

        public static void PlayEpicMoment(string momentName)
        {
            Debug.Log($"[AudioManager] Playing epic moment sound: {momentName}");
        }

        public static void PlayReinforcementWarning()
        {
            Debug.Log("[AudioManager] Playing reinforcement warning");
        }
    }

    public static class UIManager
    {
        public static void ShowNotification(string message)
        {
            Debug.Log($"[UIManager] Notification: {message}");
        }

        public static void ShowBossHealthBar(string bossName, float health)
        {
            Debug.Log($"[UIManager] Boss health bar: {bossName} ({health} HP)");
        }

        public static void ShowWarning(string message)
        {
            Debug.Log($"[UIManager] Warning: {message}");
        }
    }

    #endregion
}
