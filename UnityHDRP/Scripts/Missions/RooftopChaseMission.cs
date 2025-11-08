using System;
using System.Collections.Generic;
using UnityEngine;
using Soulvan.Systems;
using Soulvan.Lore;
using Soulvan.AI;

namespace Soulvan.Missions
{
    /// <summary>
    /// "Skyfire Pursuit" - Epic rooftop chase mission after DAO vault heist.
    /// Features AI-generated battle scenes, cinematic moments, drone combat,
    /// rune grapples, and extraction sequence with lore integration.
    /// </summary>
    public class RooftopChaseMission : MonoBehaviour
    {
        [Header("Mission Configuration")]
        public string missionId = "skyfire_pursuit";
        public string missionName = "Skyfire Pursuit";

        [Header("Player & Checkpoints")]
        public GameObject player;
        public Transform[] rooftopCheckpoints;
        public Transform extractionPoint;

        [Header("Combat Systems")]
        public GameObject[] drones;
        public GameObject[] sniperBots;
        public GameObject[] flameTurrets;
        public WeaponSystem weaponSystem;
        public GrappleSystem grappleSystem;

        [Header("Cinematic Systems")]
        public CutsceneTrigger cutsceneTrigger;
        public CinematicCameraSystem cinematicCamera;
        public AIBattleSceneGenerator battleSceneGenerator;

        [Header("Mission Progression")]
        private int currentCheckpoint = 0;
        private bool missionStarted = false;
        private bool extractionTriggered = false;
        private float missionTimer = 0f;
        private int enemiesDefeated = 0;

        [Header("Epic Battle Scenes")]
        [SerializeField] private List<BattleScene> epicScenes = new List<BattleScene>();
        [SerializeField] private int currentBattlePhase = 0;

        void Start()
        {
            InitializeMission();
        }

        /// <summary>
        /// Initialize mission with epic battle scenes.
        /// </summary>
        private void InitializeMission()
        {
            Debug.Log($"[RooftopChaseMission] Starting {missionName}");

            // Enable combat systems
            weaponSystem.EnableGuns();
            grappleSystem.EnableRunes();

            // Initialize AI battle scene generator
            if (battleSceneGenerator == null)
            {
                battleSceneGenerator = gameObject.AddComponent<AIBattleSceneGenerator>();
            }

            // Generate epic battle scenes
            GenerateEpicBattleScenes();

            // Start mission
            missionStarted = true;
            EventBus.EmitMissionStart(missionId);

            // Play intro cutscene
            cutsceneTrigger.TriggerCutscene("Skyfire_VaultEscape_Intro");

            Debug.Log("[RooftopChaseMission] Mission initialized with epic battle scenes!");
        }

        /// <summary>
        /// Generate epic battle scenes using AI.
        /// </summary>
        private void GenerateEpicBattleScenes()
        {
            epicScenes = new List<BattleScene>
            {
                // Phase 1: Vault Exit - Initial Escape
                new BattleScene
                {
                    phaseName = "Vault Exit",
                    checkpointIndex = 0,
                    description = "Explosive vault breach! Escape through collapsing corridors!",
                    enemyTypes = new[] { "SecurityDrone", "LaserGrid" },
                    enemyCount = 5,
                    cinematicMoments = new[]
                    {
                        "Slow-motion dive through closing blast doors",
                        "Rune grapple swing over collapsing floor",
                        "Explosion behind player as they leap to safety"
                    },
                    musicIntensity = 0.7f,
                    weatherEffect = "Smoke and Embers"
                },

                // Phase 2: Rooftop Assault - Drone Swarm
                new BattleScene
                {
                    phaseName = "Drone Swarm",
                    checkpointIndex = 2,
                    description = "6 combat drones engage! Aerial dogfight on the rooftops!",
                    enemyTypes = new[] { "CombatDrone", "SniperDrone" },
                    enemyCount = 8,
                    cinematicMoments = new[]
                    {
                        "Matrix-style bullet time as drones surround player",
                        "Dual-wielding gunfire while grappling between buildings",
                        "Drone explodes in slow-motion, player backflips over debris"
                    },
                    musicIntensity = 0.9f,
                    weatherEffect = "Neon Rain"
                },

                // Phase 3: Sniper Alley - Precision Combat
                new BattleScene
                {
                    phaseName = "Sniper Alley",
                    checkpointIndex = 4,
                    description = "Laser sights everywhere! Navigate sniper kill zones!",
                    enemyTypes = new[] { "SniperBot", "MineField" },
                    enemyCount = 6,
                    cinematicMoments = new[]
                    {
                        "Dodge sniper shots in bullet time",
                        "Slide under laser sight, return fire mid-slide",
                        "Grapple up to sniper position, dramatic takedown"
                    },
                    musicIntensity = 0.85f,
                    weatherEffect = "Lightning Storm"
                },

                // Phase 4: Flame Turret Gauntlet
                new BattleScene
                {
                    phaseName = "Inferno Gauntlet",
                    checkpointIndex = 6,
                    description = "Flame turrets activate! Run the gauntlet of fire!",
                    enemyTypes = new[] { "FlameTurret", "IncendiaryDrone" },
                    enemyCount = 10,
                    cinematicMoments = new[]
                    {
                        "Leap through wall of flames in slow motion",
                        "Grapple swing over fire jets, coat catches flame",
                        "Destroy turret control panel, chain explosion"
                    },
                    musicIntensity = 1.0f,
                    weatherEffect = "Fire and Ash"
                },

                // Phase 5: Boss Fight - Enforcer Titan
                new BattleScene
                {
                    phaseName = "Enforcer Titan",
                    checkpointIndex = 8,
                    description = "BOSS BATTLE! Enforcer Titan mech deploys!",
                    enemyTypes = new[] { "EnforcerTitan", "SupportDrone" },
                    enemyCount = 1,
                    cinematicMoments = new[]
                    {
                        "Titan crashes through building, dramatic reveal",
                        "Grapple onto titan's back, plant explosive",
                        "Final shot - titan explodes, player walks away in slow-motion"
                    },
                    musicIntensity = 1.0f,
                    weatherEffect = "Thunder and Lightning",
                    isBossFight = true
                },

                // Phase 6: Final Extraction
                new BattleScene
                {
                    phaseName = "Extraction Sprint",
                    checkpointIndex = 10,
                    description = "Final sprint to extraction! Helicopter incoming!",
                    enemyTypes = new[] { "ChaseDrone" },
                    enemyCount = 12,
                    cinematicMoments = new[]
                    {
                        "Helicopter spotlight illuminates player",
                        "Final grapple swing onto helicopter skid",
                        "Leap of faith onto moving helicopter"
                    },
                    musicIntensity = 0.95f,
                    weatherEffect = "Storm Winds"
                }
            };

            Debug.Log($"[RooftopChaseMission] Generated {epicScenes.Count} epic battle scenes");
        }

        void Update()
        {
            if (!missionStarted) return;

            missionTimer += Time.deltaTime;

            // Check checkpoint progression
            CheckCheckpointProgress();

            // Update battle scenes
            UpdateBattleScene();

            // Manage enemies
            ManageEnemyAI();

            // Check for extraction
            CheckExtractionTrigger();
        }

        /// <summary>
        /// Check player progress through checkpoints.
        /// </summary>
        private void CheckCheckpointProgress()
        {
            if (currentCheckpoint >= rooftopCheckpoints.Length) return;

            float distanceToCheckpoint = Vector3.Distance(
                player.transform.position,
                rooftopCheckpoints[currentCheckpoint].position
            );

            if (distanceToCheckpoint < 5f)
            {
                OnCheckpointReached();
            }
        }

        /// <summary>
        /// Handle checkpoint reached event.
        /// </summary>
        private void OnCheckpointReached()
        {
            Debug.Log($"[RooftopChaseMission] Checkpoint {currentCheckpoint + 1}/{rooftopCheckpoints.Length} reached");

            currentCheckpoint++;

            // Check if new battle phase should trigger
            CheckBattlePhaseTransition();

            // Play checkpoint sound/effect
            EventBus.EmitCheckpointReached(missionId, currentCheckpoint);

            // Check if mission complete
            if (currentCheckpoint >= rooftopCheckpoints.Length)
            {
                TriggerExtraction();
            }
        }

        /// <summary>
        /// Check if new battle phase should start.
        /// </summary>
        private void CheckBattlePhaseTransition()
        {
            if (currentBattlePhase >= epicScenes.Count) return;

            var scene = epicScenes[currentBattlePhase];

            if (currentCheckpoint >= scene.checkpointIndex)
            {
                StartBattleScene(scene);
                currentBattlePhase++;
            }
        }

        /// <summary>
        /// Start epic battle scene with cinematic moments.
        /// </summary>
        private void StartBattleScene(BattleScene scene)
        {
            Debug.Log($"[RooftopChaseMission] ⚔️ BATTLE PHASE: {scene.phaseName}");
            Debug.Log($"[RooftopChaseMission] {scene.description}");

            // Trigger cinematic camera
            if (cinematicCamera != null)
            {
                cinematicCamera.SetIntensity(scene.musicIntensity);
                cinematicCamera.TriggerCinematicMoment(scene.cinematicMoments[0]);
            }

            // Spawn enemies
            SpawnEnemies(scene);

            // Set weather/environment
            SetEnvironmentEffect(scene.weatherEffect);

            // Play battle music
            AudioManager.PlayBattleMusic(scene.musicIntensity);

            // Show HUD notification
            UIManager.ShowNotification($"⚔️ {scene.phaseName}: {scene.description}");

            // Boss fight special handling
            if (scene.isBossFight)
            {
                InitiateBossFight(scene);
            }
        }

        /// <summary>
        /// Update current battle scene with dynamic events.
        /// </summary>
        private void UpdateBattleScene()
        {
            if (currentBattlePhase == 0 || currentBattlePhase > epicScenes.Count) return;

            var scene = epicScenes[currentBattlePhase - 1];

            // Trigger random cinematic moments during battle
            if (UnityEngine.Random.value < 0.01f) // 1% chance per frame
            {
                TriggerRandomCinematicMoment(scene);
            }

            // AI generates additional battle events
            if (battleSceneGenerator != null)
            {
                battleSceneGenerator.UpdateBattleScene(scene, enemiesDefeated);
            }
        }

        /// <summary>
        /// Trigger random cinematic moment from current scene.
        /// </summary>
        private void TriggerRandomCinematicMoment(BattleScene scene)
        {
            if (scene.cinematicMoments.Length == 0) return;

            string moment = scene.cinematicMoments[UnityEngine.Random.Range(0, scene.cinematicMoments.Length)];

            if (cinematicCamera != null)
            {
                cinematicCamera.TriggerCinematicMoment(moment);
            }

            Debug.Log($"[RooftopChaseMission] 🎬 Cinematic: {moment}");
        }

        /// <summary>
        /// Spawn enemies for battle scene.
        /// </summary>
        private void SpawnEnemies(BattleScene scene)
        {
            Debug.Log($"[RooftopChaseMission] Spawning {scene.enemyCount} enemies: {string.Join(", ", scene.enemyTypes)}");

            for (int i = 0; i < scene.enemyCount; i++)
            {
                string enemyType = scene.enemyTypes[UnityEngine.Random.Range(0, scene.enemyTypes.Length)];
                SpawnEnemy(enemyType);
            }
        }

        /// <summary>
        /// Spawn specific enemy type.
        /// </summary>
        private void SpawnEnemy(string enemyType)
        {
            Vector3 spawnPosition = player.transform.position + UnityEngine.Random.insideUnitSphere * 30f;
            spawnPosition.y = player.transform.position.y + UnityEngine.Random.Range(5f, 15f);

            GameObject enemy = null;

            switch (enemyType)
            {
                case "SecurityDrone":
                case "CombatDrone":
                case "SniperDrone":
                case "ChaseDrone":
                case "IncendiaryDrone":
                    if (drones != null && drones.Length > 0)
                    {
                        enemy = Instantiate(drones[0], spawnPosition, Quaternion.identity);
                    }
                    break;

                case "SniperBot":
                    if (sniperBots != null && sniperBots.Length > 0)
                    {
                        enemy = Instantiate(sniperBots[0], spawnPosition, Quaternion.identity);
                    }
                    break;

                case "FlameTurret":
                    if (flameTurrets != null && flameTurrets.Length > 0)
                    {
                        enemy = Instantiate(flameTurrets[0], spawnPosition, Quaternion.identity);
                    }
                    break;
            }

            if (enemy != null)
            {
                var ai = enemy.GetComponent<DroneAI>();
                if (ai != null)
                {
                    ai.player = player.transform;
                    ai.OnEnemyDefeated += OnEnemyDefeated;
                }
            }
        }

        /// <summary>
        /// Set environment effect for battle scene.
        /// </summary>
        private void SetEnvironmentEffect(string effect)
        {
            Debug.Log($"[RooftopChaseMission] 🌦️ Environment: {effect}");

            // Stub: Would trigger weather/particle systems
            switch (effect)
            {
                case "Smoke and Embers":
                    // Enable smoke particles, ember effects
                    break;
                case "Neon Rain":
                    // Enable rain with neon reflections
                    break;
                case "Lightning Storm":
                    // Enable lightning effects, dark clouds
                    break;
                case "Fire and Ash":
                    // Enable fire particles, ash falling
                    break;
                case "Thunder and Lightning":
                    // Enable thunder audio, lightning strikes
                    break;
                case "Storm Winds":
                    // Enable wind effects, debris flying
                    break;
            }
        }

        /// <summary>
        /// Initiate boss fight with special mechanics.
        /// </summary>
        private void InitiateBossFight(BattleScene scene)
        {
            Debug.Log($"[RooftopChaseMission] 💀 BOSS FIGHT: {scene.phaseName}");

            // Trigger boss intro cutscene
            cutsceneTrigger.TriggerCutscene("Skyfire_BossIntro");

            // Spawn boss enemy
            Vector3 bossSpawn = player.transform.position + player.transform.forward * 50f;
            // Spawn Enforcer Titan here

            // Play boss music
            AudioManager.PlayBossMusic();

            // Show boss health bar
            UIManager.ShowBossHealthBar("Enforcer Titan", 1000f);
        }

        /// <summary>
        /// Manage enemy AI behavior.
        /// </summary>
        private void ManageEnemyAI()
        {
            foreach (var drone in drones)
            {
                if (drone == null) continue;

                var ai = drone.GetComponent<DroneAI>();
                if (ai != null && ai.IsPlayerDetected(player.transform))
                {
                    ai.EngagePlayer(player.transform);
                }
            }

            foreach (var sniper in sniperBots)
            {
                if (sniper == null) continue;

                var ai = sniper.GetComponent<SniperBotAI>();
                if (ai != null && ai.HasLineOfSight(player.transform))
                {
                    ai.AimAtPlayer(player.transform);
                }
            }
        }

        /// <summary>
        /// Handle enemy defeated event.
        /// </summary>
        private void OnEnemyDefeated()
        {
            enemiesDefeated++;
            Debug.Log($"[RooftopChaseMission] Enemy defeated! Total: {enemiesDefeated}");

            // Award XP/SVN
            EventBus.EmitEnemyDefeated(missionId, enemiesDefeated);
        }

        /// <summary>
        /// Check if extraction should trigger.
        /// </summary>
        private void CheckExtractionTrigger()
        {
            if (extractionTriggered) return;
            if (currentCheckpoint < rooftopCheckpoints.Length) return;

            float distanceToExtraction = Vector3.Distance(player.transform.position, extractionPoint.position);

            if (distanceToExtraction < 10f)
            {
                TriggerExtraction();
            }
        }

        /// <summary>
        /// Trigger final extraction sequence.
        /// </summary>
        private void TriggerExtraction()
        {
            if (extractionTriggered) return;

            extractionTriggered = true;

            Debug.Log("[RooftopChaseMission] 🚁 EXTRACTION TRIGGERED!");

            // Play extraction cutscene
            cutsceneTrigger.TriggerCutscene("Skyfire_Extraction");

            // Complete mission
            CompleteMission();
        }

        /// <summary>
        /// Complete mission with rewards and lore export.
        /// </summary>
        private void CompleteMission()
        {
            Debug.Log($"[RooftopChaseMission] ✅ MISSION COMPLETE!");
            Debug.Log($"[RooftopChaseMission] Time: {missionTimer:F1}s, Enemies defeated: {enemiesDefeated}");

            // Calculate rewards
            float svnReward = 500f + (enemiesDefeated * 10f);
            int xpReward = 1000 + (enemiesDefeated * 50);

            // Award rewards
            EventBus.EmitMissionComplete(missionId);
            // RewardService.AwardSVN(svnReward);
            // RewardService.AwardXP(xpReward);

            // Export mission lore
            // LoreExporter.ExportMissionLore(missionId, missionTimer, enemiesDefeated);

            // Mint replay NFT
            // BadgeMintService.MintReplayNFT(missionId, "Epic");

            missionStarted = false;

            Debug.Log($"[RooftopChaseMission] Rewards: {svnReward} SVN, {xpReward} XP");
        }
    }

    #region Data Structures

    /// <summary>
    /// Battle scene configuration with AI-generated epic moments.
    /// </summary>
    [Serializable]
    public class BattleScene
    {
        public string phaseName;
        public int checkpointIndex;
        public string description;
        public string[] enemyTypes;
        public int enemyCount;
        public string[] cinematicMoments;
        public float musicIntensity;
        public string weatherEffect;
        public bool isBossFight;
    }

    #endregion
}
