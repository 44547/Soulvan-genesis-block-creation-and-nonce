using System;
using System.Collections.Generic;
using UnityEngine;
using Soulvan.Systems;
using Soulvan.Wallet;

namespace Soulvan.Missions
{
    /// <summary>
    /// Central mission management system supporting driving, stealth, boss battles, and DAO rituals.
    /// Integrates with blockchain wallet for reward minting and chronicle logging.
    /// Now supports global city missions and online multiplayer.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        [Header("Mission Configuration")]
        [SerializeField] private MissionType currentMissionType;
        [SerializeField] private MissionData[] availableMissions;

        [Header("Mission Controllers")]
        [SerializeField] private DrivingMission drivingMission;
        [SerializeField] private StealthMission stealthMission;
        [SerializeField] private BossBattle bossBattle;
        [SerializeField] private DaoRitual daoRitual;

        [Header("Player References")]
        [SerializeField] private DriverController driverController;
        [SerializeField] private OnFootController onFootController;

        [Header("Systems")]
        [SerializeField] private MotifAPI motifAPI;
        [SerializeField] private WalletController walletController;
        [SerializeField] private RewardService rewardService;

        [Header("Global Mission System")]
        [SerializeField] private GlobalMissionSystem globalMissionSystem;
        [SerializeField] private string currentCity;

        private MissionData activeMission;
        private Dictionary<string, MissionData> missionDatabase;
        private HashSet<string> completedMissions;

        private void Start()
        {
            InitializeMissionDatabase();
            LoadCompletedMissions();

            // Initialize global mission system
            if (globalMissionSystem == null)
            {
                globalMissionSystem = FindObjectOfType<GlobalMissionSystem>();
            }

            // Subscribe to mission events
            EventBus.OnMissionCompleted += OnMissionCompleted;
            EventBus.OnBossDefeated += OnBossDefeated;
            EventBus.OnDaoVoteCast += OnDaoVoteCast;
        }

        private void OnDestroy()
        {
            EventBus.OnMissionCompleted -= OnMissionCompleted;
            EventBus.OnBossDefeated -= OnBossDefeated;
            EventBus.OnDaoVoteCast -= OnDaoVoteCast;
        }

        #region Initialization

        private void InitializeMissionDatabase()
        {
            missionDatabase = new Dictionary<string, MissionData>();
            
            foreach (var mission in availableMissions)
            {
                missionDatabase[mission.id] = mission;
            }

            Debug.Log($"[MissionManager] Initialized with {missionDatabase.Count} missions");
        }

        private void LoadCompletedMissions()
        {
            completedMissions = new HashSet<string>();
            
            // Load from PlayerPrefs or blockchain Chronicle
            string completed = PlayerPrefs.GetString("CompletedMissions", "");
            if (!string.IsNullOrEmpty(completed))
            {
                string[] ids = completed.Split(',');
                foreach (var id in ids)
                {
                    completedMissions.Add(id);
                }
            }

            Debug.Log($"[MissionManager] Loaded {completedMissions.Count} completed missions");
        }

        #endregion

        #region Mission Control

        public void StartMission(string missionId)
        {
            if (!missionDatabase.ContainsKey(missionId))
            {
                Debug.LogError($"[MissionManager] Mission not found: {missionId}");
                return;
            }

            activeMission = missionDatabase[missionId];

            // Apply motif overlay for mission
            if (motifAPI != null)
            {
                motifAPI.SetMotif(activeMission.primaryMotif, activeMission.motifIntensity);
            }

            // Start appropriate mission type
            switch (activeMission.type)
            {
                case MissionType.Driving:
                    StartDrivingMission();
                    break;

                case MissionType.Stealth:
                    StartStealthMission();
                    break;

                case MissionType.Boss:
                    StartBossBattle();
                    break;

                case MissionType.Dao:
                    StartDaoRitual();
                    break;
            }

            Debug.Log($"[MissionManager] Started mission: {activeMission.name}");
            EventBus.EmitMissionStarted(missionId);
        }

        private void StartDrivingMission()
        {
            if (drivingMission != null)
            {
                drivingMission.Initialize(activeMission);
                drivingMission.Begin();
            }

            if (driverController != null)
            {
                driverController.enabled = true;
            }
        }

        private void StartStealthMission()
        {
            if (stealth Mission != null)
            {
                stealthMission.Initialize(activeMission);
                stealthMission.Begin();
            }

            if (onFootController != null)
            {
                onFootController.enabled = true;
                onFootController.EnterStealthMode();
            }
        }

        private void StartBossBattle()
        {
            if (bossBattle != null)
            {
                bossBattle.Initialize(activeMission);
                bossBattle.Begin();
            }

            if (onFootController != null)
            {
                onFootController.enabled = true;
            }
        }

        private void StartDaoRitual()
        {
            if (daoRitual != null)
            {
                daoRitual.Initialize(activeMission);
                daoRitual.Begin();
            }
        }

        public void CompleteMission(string missionId)
        {
            if (activeMission == null || activeMission.id != missionId)
            {
                Debug.LogWarning($"[MissionManager] Tried to complete inactive mission: {missionId}");
                return;
            }

            // Mark as completed
            completedMissions.Add(missionId);
            SaveCompletedMissions();

            // Award rewards
            AwardMissionRewards(activeMission);

            // Clear active mission
            activeMission = null;

            Debug.Log($"[MissionManager] Completed mission: {missionId}");
            EventBus.EmitMissionCompleted(missionId);
        }

        private void AwardMissionRewards(MissionData mission)
        {
            if (rewardService == null) return;

            // Award SVN tokens
            if (mission.svnReward > 0)
            {
                rewardService.MintSVNReward(walletController.Wallet.Address, mission.svnReward);
            }

            // Award NFT if specified
            if (!string.IsNullOrEmpty(mission.nftMetadataUri))
            {
                rewardService.MintNFTReward(walletController.Wallet.Address, mission.nftMetadataUri);
            }

            // Log to Chronicle
            // Chronicle logging would happen in RewardService
        }

        private void SaveCompletedMissions()
        {
            string completed = string.Join(",", completedMissions);
            PlayerPrefs.SetString("CompletedMissions", completed);
            PlayerPrefs.Save();
        }

        #endregion

        #region Mission Queries

        /// <summary>
        /// Get available missions for player's current tier.
        /// Now includes city-based missions from GlobalMissionSystem.
        /// </summary>
        public MissionData[] GetAvailableMissions(int playerTier)
        {
            var available = new List<MissionData>();

            foreach (var mission in missionDatabase.Values)
            {
                // Check if mission is unlocked for player tier
                if (mission.requiredTier <= playerTier && !completedMissions.Contains(mission.id))
                {
                    available.Add(mission);
                }
            }

            // Add global city missions
            if (globalMissionSystem != null)
            {
                var cities = globalMissionSystem.GetAvailableCities(playerTier);
                foreach (var city in cities)
                {
                    var cityMissions = globalMissionSystem.GetCityMissions(city.cityName);
                    foreach (var cityMission in cityMissions)
                    {
                        if (!completedMissions.Contains(cityMission.id))
                        {
                            // Convert to MissionData format
                            var missionData = new MissionData
                            {
                                id = cityMission.id,
                                name = cityMission.title,
                                type = MissionType.Driving,
                                requiredTier = cityMission.requiresTier,
                                description = cityMission.description,
                                svnReward = cityMission.rewardSVN,
                                sceneName = cityMission.location,
                                primaryMotif = GetCityMotif(cityMission.location)
                            };

                            available.Add(missionData);
                        }
                    }
                }
            }

            return available.ToArray();
        }

        /// <summary>
        /// Get missions for a specific city.
        /// </summary>
        public Mission[] GetCityMissions(string cityName)
        {
            if (globalMissionSystem == null) return new Mission[0];

            return globalMissionSystem.GetCityMissions(cityName).ToArray();
        }

        /// <summary>
        /// Select a city for missions.
        /// </summary>
        public void SelectCity(string cityName, bool enableMultiplayer = false)
        {
            currentCity = cityName;

            if (globalMissionSystem != null)
            {
                globalMissionSystem.SelectCity(cityName, enableMultiplayer);
            }

            Debug.Log($"[MissionManager] Selected city: {cityName} (Multiplayer: {enableMultiplayer})");
        }

        /// <summary>
        /// Get motif for a city (for atmospheric consistency).
        /// </summary>
        private Motif GetCityMotif(string cityName)
        {
            return cityName switch
            {
                "Tokyo" => Motif.Storm,
                "Dubai" => Motif.Desert,
                "London" => Motif.Fog,
                "Paris" => Motif.Sunset,
                "Monaco" => Motif.Luxury,
                "New York" => Motif.Urban,
                "Los Angeles" => Motif.Sunset,
                "Miami" => Motif.Tropical,
                "Las Vegas" => Motif.Neon,
                "Rio de Janeiro" => Motif.Carnival,
                "Sydney" => Motif.Ocean,
                _ => Motif.Storm
            };
        }

        public bool IsMissionCompleted(string missionId)
        {
            return completedMissions.Contains(missionId);
        }

        public int GetCompletedMissionCount(MissionType type)
        {
            int count = 0;
            foreach (var missionId in completedMissions)
            {
                if (missionDatabase.ContainsKey(missionId) && missionDatabase[missionId].type == type)
                {
                    count++;
                }
            }
            return count;
        }

        #endregion

        #region Event Handlers

        private void OnMissionCompleted(string missionId)
        {
            CompleteMission(missionId);
        }

        private void OnBossDefeated(string bossName)
        {
            // Boss battles trigger mission completion
            if (activeMission != null && activeMission.type == MissionType.Boss)
            {
                CompleteMission(activeMission.id);
            }
        }

        private void OnDaoVoteCast(string proposalId)
        {
            // DAO rituals trigger mission completion
            if (activeMission != null && activeMission.type == MissionType.Dao)
            {
                CompleteMission(activeMission.id);
            }
        }

        #endregion
    }

    #region Data Structures

    public enum MissionType
    {
        Driving,
        Stealth,
        Boss,
        Dao
    }

    public enum Motif
    {
        Storm,
        Desert,
        Fog,
        Sunset,
        Luxury,
        Urban,
        Tropical,
        Neon,
        Carnival,
        Ocean
    }

    [Serializable]
    public class MissionData
    {
        [Header("Identity")]
        public string id = "mission_001";
        public string name = "Neon City Sprint";
        public MissionType type = MissionType.Driving;

        [Header("Requirements")]
        public int requiredTier = 1; // 1=Street Racer, 2=Mission Runner, etc.

        [Header("Motif")]
        public Motif primaryMotif = Motif.Storm;
        public float motifIntensity = 0.8f;

        [Header("Objectives")]
        [TextArea(3, 6)]
        public string description = "Outrun rival agents through neon tunnels.";
        public ObjectiveData[] objectives;

        [Header("Rewards")]
        public float svnReward = 100f;
        public string nftMetadataUri = "";
        public int tierUnlock = 0; // 0 = no tier unlock

        [Header("Environment")]
        public string sceneName = "NeonCity";
        public Vector3 startPosition;
        public Quaternion startRotation;
    }

    [Serializable]
    public class ObjectiveData
    {
        public string description;
        public ObjectiveType type;
        public float targetValue; // Distance, time, count, etc.
        public bool completed;
    }

    public enum ObjectiveType
    {
        ReachWaypoint,
        DefeatRival,
        StayUndetected,
        HackTerminal,
        DefeatBoss,
        CastVote
    }

    #endregion
}
