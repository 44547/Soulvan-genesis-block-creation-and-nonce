using System;
using System.Collections.Generic;
using UnityEngine;
using Soulvan.Systems;

namespace Soulvan.Garage
{
    /// <summary>
    /// 2025 Hypercar Garage System with mythic motif mappings.
    /// Integrates latest hypercars with Storm, Calm, Cosmic, and DAO Ascension overlays.
    /// </summary>
    public class HypercarGarage : MonoBehaviour
    {
        [Header("Garage Configuration")]
        [SerializeField] private MotifAPI motifAPI;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Light spotLight;

        [Header("Hypercar Database")]
        [SerializeField] private HypercarData[] hypercars;

        private Dictionary<string, HypercarData> carDatabase;
        private HypercarData currentCar;

        private void Start()
        {
            InitializeGarage();
            LoadHypercarDatabase();
        }

        private void InitializeGarage()
        {
            carDatabase = new Dictionary<string, HypercarData>();
            
            // Initialize 2025 hypercar lineup
            AddHypercar(CreateBugattiBolide());
            AddHypercar(CreateMcLarenW1());
            AddHypercar(CreateFerrariSF90XX());
            AddHypercar(CreatePaganiUtopia());
            AddHypercar(CreateAstonMartinValhalla());
            AddHypercar(CreatePorsche992_2());
            AddHypercar(CreateRimacNevera());
            AddHypercar(CreateLongbowSpeedster());

            Debug.Log($"[HypercarGarage] Initialized with {carDatabase.Count} hypercars");
        }

        private void LoadHypercarDatabase()
        {
            // In production, load from AssetBundle or Addressables
            Debug.Log("[HypercarGarage] Hypercar database loaded");
        }

        #region 2025 Hypercar Definitions

        private HypercarData CreateBugattiBolide()
        {
            return new HypercarData
            {
                id = "bugatti_bolide",
                name = "Bugatti Bolide",
                manufacturer = "Bugatti",
                year = 2025,
                
                // Performance
                horsepower = 1600,
                torque = 1600,
                weight = 1240, // kg
                topSpeed = 500, // km/h
                acceleration0to100 = 2.17f,
                
                // Specifications
                engine = "8.0L Quad-Turbo W16",
                drivetrain = "AWD",
                transmission = "7-Speed Dual-Clutch",
                
                // Pricing & Rarity
                priceUSD = 4500000,
                rarity = HypercarRarity.Legendary,
                productionCount = 40,
                
                // Motif Mapping
                primaryMotif = Motif.Storm,
                motifIntensity = 1.0f,
                motifDescription = "Storm Surge - Raw unbridled power, track-only monster with extreme aerodynamics. Thunderous W16 roar.",
                
                // Visual Configuration
                colorways = new[] { "Bugatti Blue", "Le Mans Black", "Volcanic Orange" },
                defaultColorway = "Bugatti Blue",
                
                // Gameplay Stats
                handling = 8.5f,
                acceleration = 10f,
                braking = 9f,
                drift = 7f,
                
                // Unlock Requirements
                unlockRequirement = "Win 50 Storm Season races",
                seasonUnlock = 1, // Storm season
                
                // NFT Metadata
                nftMetadataUri = "https://soulvan.io/nft/cars/bugatti-bolide"
            };
        }

        private HypercarData CreateMcLarenW1()
        {
            return new HypercarData
            {
                id = "mclaren_w1",
                name = "McLaren W1",
                manufacturer = "McLaren",
                year = 2025,
                
                horsepower = 1300, // Hybrid system
                torque = 1200,
                weight = 1399, // kg
                topSpeed = 400,
                acceleration0to100 = 2.5f,
                
                engine = "Hybrid V8 + Electric",
                drivetrain = "RWD",
                transmission = "8-Speed Dual-Clutch",
                
                priceUSD = 3000000,
                rarity = HypercarRarity.Mythic,
                productionCount = 399,
                
                primaryMotif = Motif.Oracle, // DAO Ascension
                motifIntensity = 0.95f,
                motifDescription = "DAO Ascension - Successor to F1 and P1 lineage. Halo hypercar representing governance and wisdom.",
                
                colorways = new[] { "McLaren Orange", "Stealth Grey", "Championship Gold" },
                defaultColorway = "McLaren Orange",
                
                handling = 10f,
                acceleration = 9.5f,
                braking = 10f,
                drift = 8.5f,
                
                unlockRequirement = "Reach DAO Hero status + vote in 10 proposals",
                seasonUnlock = 4, // DAO Ascension season
                
                nftMetadataUri = "https://soulvan.io/nft/cars/mclaren-w1"
            };
        }

        private HypercarData CreateFerrariSF90XX()
        {
            return new HypercarData
            {
                id = "ferrari_sf90xx",
                name = "Ferrari SF90 XX Stradale",
                manufacturer = "Ferrari",
                year = 2025,
                
                horsepower = 1030,
                torque = 900,
                weight = 1560,
                topSpeed = 350,
                acceleration0to100 = 2.4f,
                
                engine = "4.0L Twin-Turbo V8 + Electric",
                drivetrain = "AWD",
                transmission = "8-Speed Dual-Clutch",
                
                priceUSD = 1500000,
                rarity = HypercarRarity.Epic,
                productionCount = 799,
                
                primaryMotif = Motif.Calm,
                motifIntensity = 0.8f,
                motifDescription = "Calm Restoration - Hybrid balance of combustion and electric. Track-focused but road-legal elegance.",
                
                colorways = new[] { "Rosso Corsa", "Nero Daytona", "Bianco Italia" },
                defaultColorway = "Rosso Corsa",
                
                handling = 9.5f,
                acceleration = 9f,
                braking = 9.5f,
                drift = 8f,
                
                unlockRequirement = "Complete 20 stealth delivery missions",
                seasonUnlock = 2, // Calm season
                
                nftMetadataUri = "https://soulvan.io/nft/cars/ferrari-sf90xx"
            };
        }

        private HypercarData CreatePaganiUtopia()
        {
            return new HypercarData
            {
                id = "pagani_utopia",
                name = "Pagani Utopia Roadster",
                manufacturer = "Pagani",
                year = 2025,
                
                horsepower = 864,
                torque = 1100,
                weight = 1280,
                topSpeed = 350,
                acceleration0to100 = 2.7f,
                
                engine = "6.0L Twin-Turbo V12",
                drivetrain = "RWD",
                transmission = "7-Speed Manual / Auto",
                
                priceUSD = 3200000,
                rarity = HypercarRarity.Mythic,
                productionCount = 99,
                
                primaryMotif = Motif.Cosmic,
                motifIntensity = 1.0f,
                motifDescription = "Cosmic Prophecy - Analog driving purity meets mythic artistry. Bespoke handcrafted masterpiece.",
                
                colorways = new[] { "Utopia Silver", "Cosmic Purple", "Nebula Blue" },
                defaultColorway = "Utopia Silver",
                
                handling = 9f,
                acceleration = 8.5f,
                braking = 9f,
                drift = 9.5f, // Manual transmission drift king
                
                unlockRequirement = "Defeat 3 mythic bosses",
                seasonUnlock = 3, // Cosmic season
                
                nftMetadataUri = "https://soulvan.io/nft/cars/pagani-utopia"
            };
        }

        private HypercarData CreateAstonMartinValhalla()
        {
            return new HypercarData
            {
                id = "aston_martin_valhalla",
                name = "Aston Martin Valhalla",
                manufacturer = "Aston Martin",
                year = 2025,
                
                horsepower = 1000,
                torque = 1000,
                weight = 1550,
                topSpeed = 360,
                acceleration0to100 = 2.5f,
                
                engine = "Hybrid V8 + Electric",
                drivetrain = "AWD",
                transmission = "8-Speed DCT",
                
                priceUSD = 1000000,
                rarity = HypercarRarity.Rare,
                productionCount = 999,
                
                primaryMotif = Motif.Storm,
                motifIntensity = 0.7f,
                motifDescription = "Storm Surge - F1-inspired aero between Valkyrie and mainstream lineup. Norse warrior spirit.",
                
                colorways = new[] { "Aston Green", "Valhalla Silver", "Odin Black" },
                defaultColorway = "Aston Green",
                
                handling = 9f,
                acceleration = 9f,
                braking = 8.5f,
                drift = 7.5f,
                
                unlockRequirement = "Win 10 races + complete 5 missions",
                seasonUnlock = 1,
                
                nftMetadataUri = "https://soulvan.io/nft/cars/aston-martin-valhalla"
            };
        }

        private HypercarData CreatePorsche992_2()
        {
            return new HypercarData
            {
                id = "porsche_992_2",
                name = "Porsche 911 Carrera (992.2)",
                manufacturer = "Porsche",
                year = 2025,
                
                horsepower = 480, // Mild hybrid
                torque = 570,
                weight = 1550,
                topSpeed = 308,
                acceleration0to100 = 3.2f,
                
                engine = "3.0L Twin-Turbo Flat-6 + Mild Hybrid",
                drivetrain = "RWD / AWD",
                transmission = "8-Speed PDK",
                
                priceUSD = 150000,
                rarity = HypercarRarity.Common,
                productionCount = 10000,
                
                primaryMotif = Motif.Calm,
                motifIntensity = 0.5f,
                motifDescription = "Calm Restoration - Mild hybrid efficiency meets performance. Subtle facelift, improved aero.",
                
                colorways = new[] { "GT Silver", "Racing Yellow", "Guards Red" },
                defaultColorway = "GT Silver",
                
                handling = 10f, // 911 handling perfection
                acceleration = 7.5f,
                braking = 9f,
                drift = 9f,
                
                unlockRequirement = "Starter car",
                seasonUnlock = 0,
                
                nftMetadataUri = "https://soulvan.io/nft/cars/porsche-992-2"
            };
        }

        private HypercarData CreateRimacNevera()
        {
            return new HypercarData
            {
                id = "rimac_nevera",
                name = "Rimac Nevera",
                manufacturer = "Rimac",
                year = 2025,
                
                horsepower = 1914,
                torque = 2360,
                weight = 2150,
                topSpeed = 412,
                acceleration0to100 = 1.81f, // World record
                
                engine = "Quad Electric Motors",
                drivetrain = "AWD",
                transmission = "Single-Speed",
                
                priceUSD = 2400000,
                rarity = HypercarRarity.Legendary,
                productionCount = 150,
                
                primaryMotif = Motif.Cosmic, // Electric prophecy
                motifIntensity = 1.0f,
                motifDescription = "Mythic Legends - Pure EV hypercar. Silent thunder. Electric prophecy made manifest.",
                
                colorways = new[] { "Nevera Blue", "Lightning White", "Thunder Grey" },
                defaultColorway = "Nevera Blue",
                
                handling = 9f,
                acceleration = 10f, // Fastest 0-100
                braking = 10f,
                drift = 6f, // Heavy but torque vectoring
                
                unlockRequirement = "Reach Mythic Legend status",
                seasonUnlock = 5, // Mythic Legends
                
                nftMetadataUri = "https://soulvan.io/nft/cars/rimac-nevera"
            };
        }

        private HypercarData CreateLongbowSpeedster()
        {
            return new HypercarData
            {
                id = "longbow_speedster",
                name = "Longbow Speedster",
                manufacturer = "Longbow",
                year = 2025,
                
                horsepower = 800, // EV estimate
                torque = 1500,
                weight = 900, // Ultra lightweight
                topSpeed = 350,
                acceleration0to100 = 2.3f,
                
                engine = "Dual Electric Motors",
                drivetrain = "RWD",
                transmission = "Single-Speed",
                
                priceUSD = 500000,
                rarity = HypercarRarity.Rare,
                productionCount = 500,
                
                primaryMotif = Motif.Calm,
                motifIntensity = 0.6f,
                motifDescription = "Calm Restoration - British EV startup. Lightweight composite body. Ex-Lucid/McLaren/Tesla DNA.",
                
                colorways = new[] { "British Green", "Speedster Silver", "Carbon Black" },
                defaultColorway = "British Green",
                
                handling = 9.5f,
                acceleration = 9f,
                braking = 8.5f,
                drift = 8f,
                
                unlockRequirement = "Complete EV-only missions",
                seasonUnlock = 2,
                
                nftMetadataUri = "https://soulvan.io/nft/cars/longbow-speedster"
            };
        }

        #endregion

        #region Garage Operations

        private void AddHypercar(HypercarData car)
        {
            carDatabase[car.id] = car;
            Debug.Log($"[HypercarGarage] Added {car.manufacturer} {car.name}");
        }

        public HypercarData GetHypercar(string carId)
        {
            if (carDatabase.ContainsKey(carId))
            {
                return carDatabase[carId];
            }

            Debug.LogWarning($"[HypercarGarage] Car not found: {carId}");
            return null;
        }

        public HypercarData[] GetHypercarsByMotif(Motif motif)
        {
            var cars = new List<HypercarData>();
            
            foreach (var car in carDatabase.Values)
            {
                if (car.primaryMotif == motif)
                {
                    cars.Add(car);
                }
            }

            return cars.ToArray();
        }

        public HypercarData[] GetUnlockedHypercars(int currentSeason, string[] completedRequirements)
        {
            var cars = new List<HypercarData>();
            
            foreach (var car in carDatabase.Values)
            {
                if (car.seasonUnlock <= currentSeason)
                {
                    // Check unlock requirements
                    // Stub: would parse requirement string and check against completedRequirements
                    cars.Add(car);
                }
            }

            return cars.ToArray();
        }

        public void SpawnHypercar(string carId)
        {
            var car = GetHypercar(carId);
            if (car == null) return;

            currentCar = car;

            // Spawn car prefab at spawn point
            // Stub: would load from AssetBundle and instantiate
            Debug.Log($"[HypercarGarage] Spawning {car.name}");

            // Apply motif overlay
            if (motifAPI != null)
            {
                motifAPI.SetMotif(car.primaryMotif, car.motifIntensity);
            }

            // Configure lighting
            if (spotLight != null)
            {
                spotLight.color = GetMotifColor(car.primaryMotif);
            }

            // Play engine sound
            PlayEngineSound(car);
        }

        private void PlayEngineSound(HypercarData car)
        {
            // Stub: would play engine audio clips
            Debug.Log($"[HypercarGarage] Playing {car.engine} sound");
        }

        private Color GetMotifColor(Motif motif)
        {
            switch (motif)
            {
                case Motif.Storm: return new Color(0.8f, 0.2f, 0.2f); // Red
                case Motif.Calm: return new Color(0.2f, 0.6f, 0.8f); // Blue
                case Motif.Cosmic: return new Color(0.6f, 0.2f, 0.8f); // Purple
                case Motif.Oracle: return new Color(0.8f, 0.8f, 0.2f); // Gold
                default: return Color.white;
            }
        }

        #endregion

        #region Debug UI

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 500));
            GUILayout.Label("=== HYPERCAR GARAGE ===");
            
            foreach (var car in carDatabase.Values)
            {
                if (GUILayout.Button($"{car.manufacturer} {car.name}"))
                {
                    SpawnHypercar(car.id);
                }
            }

            if (currentCar != null)
            {
                GUILayout.Label("");
                GUILayout.Label($"Current: {currentCar.name}");
                GUILayout.Label($"HP: {currentCar.horsepower} | Weight: {currentCar.weight}kg");
                GUILayout.Label($"0-100: {currentCar.acceleration0to100}s | Top: {currentCar.topSpeed}km/h");
                GUILayout.Label($"Motif: {currentCar.primaryMotif} ({currentCar.motifIntensity:F2})");
                GUILayout.Label($"Price: ${currentCar.priceUSD:N0}");
            }

            GUILayout.EndArea();
        }

        #endregion
    }

    #region Data Structures

    [Serializable]
    public class HypercarData
    {
        // Identity
        public string id;
        public string name;
        public string manufacturer;
        public int year;

        // Performance
        public int horsepower;
        public int torque;
        public int weight; // kg
        public int topSpeed; // km/h
        public float acceleration0to100; // seconds

        // Specifications
        public string engine;
        public string drivetrain;
        public string transmission;

        // Pricing & Rarity
        public int priceUSD;
        public HypercarRarity rarity;
        public int productionCount;

        // Motif Mapping
        public Motif primaryMotif;
        public float motifIntensity;
        public string motifDescription;

        // Visual
        public string[] colorways;
        public string defaultColorway;

        // Gameplay Stats (0-10 scale)
        public float handling;
        public float acceleration;
        public float braking;
        public float drift;

        // Unlock System
        public string unlockRequirement;
        public int seasonUnlock; // 0=always, 1=Storm, 2=Calm, etc.

        // NFT Integration
        public string nftMetadataUri;
    }

    public enum HypercarRarity
    {
        Common,
        Rare,
        Epic,
        Legendary,
        Mythic
    }

    #endregion
}
