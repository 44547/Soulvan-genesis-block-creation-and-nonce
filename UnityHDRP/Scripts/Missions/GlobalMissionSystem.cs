using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Soulvan.Missions
{
    /// <summary>
    /// Global mission system with real-world city locations.
    /// Players can race through iconic cities around the world.
    /// Supports online multiplayer with location-based matchmaking.
    /// </summary>
    public class GlobalMissionSystem : MonoBehaviour
    {
        [Header("Global Cities")]
        [SerializeField] private List<CityMissionHub> cities = new List<CityMissionHub>();

        [Header("Current Session")]
        [SerializeField] private CityMissionHub currentCity;
        [SerializeField] private bool isMultiplayerSession = false;
        [SerializeField] private List<string> connectedPlayers = new List<string>();

        private void Start()
        {
            InitializeCities();
            Debug.Log($"[GlobalMissionSystem] Initialized {cities.Count} city hubs");
        }

        /// <summary>
        /// Initialize all city mission hubs with their unique characteristics.
        /// </summary>
        private void InitializeCities()
        {
            cities = new List<CityMissionHub>
            {
                // Asia-Pacific
                new CityMissionHub
                {
                    cityName = "Tokyo",
                    country = "Japan",
                    continent = "Asia",
                    timezone = "JST",
                    weatherConditions = new[] { "Clear", "Rainy", "Neon Night" },
                    trackTypes = new[] { "Urban Street", "Highway", "Shibuya Circuit", "Tokyo Drift" },
                    iconicLandmarks = new[] { "Tokyo Tower", "Shibuya Crossing", "Rainbow Bridge", "Akihabara" },
                    difficulty = 3,
                    unlockTier = 1,
                    rewardMultiplier = 1.2f,
                    coordinates = new Vector2(35.6762f, 139.6503f)
                },
                new CityMissionHub
                {
                    cityName = "Dubai",
                    country = "UAE",
                    continent = "Asia",
                    timezone = "GST",
                    weatherConditions = new[] { "Clear", "Sandstorm", "Sunset" },
                    trackTypes = new[] { "Desert Highway", "Downtown Circuit", "Palm Island", "Burj Khalifa Sprint" },
                    iconicLandmarks = new[] { "Burj Khalifa", "Palm Jumeirah", "Dubai Marina", "Gold Souk" },
                    difficulty = 4,
                    unlockTier = 2,
                    rewardMultiplier = 1.5f,
                    coordinates = new Vector2(25.2048f, 55.2708f)
                },
                new CityMissionHub
                {
                    cityName = "Singapore",
                    country = "Singapore",
                    continent = "Asia",
                    timezone = "SGT",
                    weatherConditions = new[] { "Tropical", "Rain", "Night" },
                    trackTypes = new[] { "Marina Bay Circuit", "Gardens Sprint", "Sentosa Island", "Orchard Road" },
                    iconicLandmarks = new[] { "Marina Bay Sands", "Gardens by the Bay", "Merlion", "Sentosa" },
                    difficulty = 3,
                    unlockTier = 2,
                    rewardMultiplier = 1.3f,
                    coordinates = new Vector2(1.3521f, 103.8198f)
                },
                new CityMissionHub
                {
                    cityName = "Seoul",
                    country = "South Korea",
                    continent = "Asia",
                    timezone = "KST",
                    weatherConditions = new[] { "Clear", "Snow", "Cherry Blossom" },
                    trackTypes = new[] { "Gangnam Circuit", "Han River Drive", "N Seoul Tower Sprint", "Dongdaemun Night" },
                    iconicLandmarks = new[] { "N Seoul Tower", "Gangnam District", "Han River", "Gyeongbokgung Palace" },
                    difficulty = 3,
                    unlockTier = 1,
                    rewardMultiplier = 1.2f,
                    coordinates = new Vector2(37.5665f, 126.9780f)
                },

                // Europe
                new CityMissionHub
                {
                    cityName = "London",
                    country = "United Kingdom",
                    continent = "Europe",
                    timezone = "GMT",
                    weatherConditions = new[] { "Rainy", "Foggy", "Clear" },
                    trackTypes = new[] { "Thames Circuit", "Westminster Sprint", "Tower Bridge Race", "Camden Night" },
                    iconicLandmarks = new[] { "Big Ben", "Tower Bridge", "London Eye", "Buckingham Palace" },
                    difficulty = 3,
                    unlockTier = 1,
                    rewardMultiplier = 1.3f,
                    coordinates = new Vector2(51.5074f, -0.1278f)
                },
                new CityMissionHub
                {
                    cityName = "Paris",
                    country = "France",
                    continent = "Europe",
                    timezone = "CET",
                    weatherConditions = new[] { "Clear", "Rainy", "Sunset" },
                    trackTypes = new[] { "Champs-Élysées Sprint", "Eiffel Tower Circuit", "Arc de Triomphe Loop", "Seine River Drive" },
                    iconicLandmarks = new[] { "Eiffel Tower", "Arc de Triomphe", "Louvre", "Notre-Dame" },
                    difficulty = 3,
                    unlockTier = 1,
                    rewardMultiplier = 1.3f,
                    coordinates = new Vector2(48.8566f, 2.3522f)
                },
                new CityMissionHub
                {
                    cityName = "Monaco",
                    country = "Monaco",
                    continent = "Europe",
                    timezone = "CET",
                    weatherConditions = new[] { "Clear", "Rainy" },
                    trackTypes = new[] { "Monte Carlo Circuit", "Casino Square Sprint", "Harbor Drive", "Grand Prix Track" },
                    iconicLandmarks = new[] { "Casino Monte Carlo", "Prince's Palace", "Port Hercules", "Circuit de Monaco" },
                    difficulty = 5,
                    unlockTier = 3,
                    rewardMultiplier = 2.0f,
                    coordinates = new Vector2(43.7384f, 7.4246f)
                },
                new CityMissionHub
                {
                    cityName = "Berlin",
                    country = "Germany",
                    continent = "Europe",
                    timezone = "CET",
                    weatherConditions = new[] { "Clear", "Rainy", "Snow" },
                    trackTypes = new[] { "Brandenburg Gate Sprint", "Autobahn Circuit", "Berlin Wall Memorial", "Alexanderplatz Night" },
                    iconicLandmarks = new[] { "Brandenburg Gate", "Berlin Wall", "Reichstag", "TV Tower" },
                    difficulty = 3,
                    unlockTier = 2,
                    rewardMultiplier = 1.3f,
                    coordinates = new Vector2(52.5200f, 13.4050f)
                },

                // North America
                new CityMissionHub
                {
                    cityName = "New York",
                    country = "USA",
                    continent = "North America",
                    timezone = "EST",
                    weatherConditions = new[] { "Clear", "Rainy", "Snow", "Sunset" },
                    trackTypes = new[] { "Manhattan Circuit", "Brooklyn Bridge Sprint", "Times Square Night", "Central Park Loop" },
                    iconicLandmarks = new[] { "Statue of Liberty", "Empire State Building", "Times Square", "Brooklyn Bridge" },
                    difficulty = 4,
                    unlockTier = 2,
                    rewardMultiplier = 1.4f,
                    coordinates = new Vector2(40.7128f, -74.0060f)
                },
                new CityMissionHub
                {
                    cityName = "Los Angeles",
                    country = "USA",
                    continent = "North America",
                    timezone = "PST",
                    weatherConditions = new[] { "Clear", "Sunset", "Night" },
                    trackTypes = new[] { "Hollywood Hills Sprint", "Venice Beach Circuit", "Downtown LA Night", "Santa Monica Pier" },
                    iconicLandmarks = new[] { "Hollywood Sign", "Santa Monica Pier", "Venice Beach", "Griffith Observatory" },
                    difficulty = 3,
                    unlockTier = 1,
                    rewardMultiplier = 1.2f,
                    coordinates = new Vector2(34.0522f, -118.2437f)
                },
                new CityMissionHub
                {
                    cityName = "Miami",
                    country = "USA",
                    continent = "North America",
                    timezone = "EST",
                    weatherConditions = new[] { "Clear", "Tropical Storm", "Sunset" },
                    trackTypes = new[] { "Ocean Drive Circuit", "South Beach Sprint", "Biscayne Bay Loop", "Art Deco Night" },
                    iconicLandmarks = new[] { "South Beach", "Ocean Drive", "Art Deco District", "Biscayne Bay" },
                    difficulty = 3,
                    unlockTier = 2,
                    rewardMultiplier = 1.3f,
                    coordinates = new Vector2(25.7617f, -80.1918f)
                },
                new CityMissionHub
                {
                    cityName = "Las Vegas",
                    country = "USA",
                    continent = "North America",
                    timezone = "PST",
                    weatherConditions = new[] { "Clear", "Desert Night" },
                    trackTypes = new[] { "Strip Circuit", "Downtown Vegas Sprint", "Red Rock Canyon", "Fremont Street Night" },
                    iconicLandmarks = new[] { "The Strip", "Bellagio Fountains", "Fremont Street", "Luxor Pyramid" },
                    difficulty = 4,
                    unlockTier = 2,
                    rewardMultiplier = 1.5f,
                    coordinates = new Vector2(36.1699f, -115.1398f)
                },

                // Middle East
                new CityMissionHub
                {
                    cityName = "Abu Dhabi",
                    country = "UAE",
                    continent = "Asia",
                    timezone = "GST",
                    weatherConditions = new[] { "Clear", "Sandstorm", "Night" },
                    trackTypes = new[] { "Yas Marina Circuit", "Sheikh Zayed Mosque Sprint", "Corniche Drive", "Desert Highway" },
                    iconicLandmarks = new[] { "Sheikh Zayed Grand Mosque", "Yas Island", "Emirates Palace", "Corniche" },
                    difficulty = 4,
                    unlockTier = 3,
                    rewardMultiplier = 1.6f,
                    coordinates = new Vector2(24.4539f, 54.3773f)
                },

                // South America
                new CityMissionHub
                {
                    cityName = "Rio de Janeiro",
                    country = "Brazil",
                    continent = "South America",
                    timezone = "BRT",
                    weatherConditions = new[] { "Clear", "Rainy", "Carnival" },
                    trackTypes = new[] { "Copacabana Beach Circuit", "Christ the Redeemer Sprint", "Ipanema Loop", "Sugarloaf Mountain" },
                    iconicLandmarks = new[] { "Christ the Redeemer", "Copacabana Beach", "Sugarloaf Mountain", "Maracanã Stadium" },
                    difficulty = 3,
                    unlockTier = 2,
                    rewardMultiplier = 1.3f,
                    coordinates = new Vector2(-22.9068f, -43.1729f)
                },

                // Australia
                new CityMissionHub
                {
                    cityName = "Sydney",
                    country = "Australia",
                    continent = "Oceania",
                    timezone = "AEST",
                    weatherConditions = new[] { "Clear", "Rainy", "Sunset" },
                    trackTypes = new[] { "Harbour Bridge Circuit", "Opera House Sprint", "Bondi Beach Loop", "City Night Race" },
                    iconicLandmarks = new[] { "Sydney Opera House", "Harbour Bridge", "Bondi Beach", "Darling Harbour" },
                    difficulty = 3,
                    unlockTier = 2,
                    rewardMultiplier = 1.3f,
                    coordinates = new Vector2(-33.8688f, 151.2093f)
                }
            };

            Debug.Log($"[GlobalMissionSystem] Loaded {cities.Count} cities across {GetContinents().Count} continents");
        }

        /// <summary>
        /// Get all available cities for the player's current tier.
        /// </summary>
        public List<CityMissionHub> GetAvailableCities(int playerTier)
        {
            return cities.Where(c => c.unlockTier <= playerTier).ToList();
        }

        /// <summary>
        /// Get cities by continent.
        /// </summary>
        public List<CityMissionHub> GetCitiesByContinent(string continent)
        {
            return cities.Where(c => c.continent == continent).ToList();
        }

        /// <summary>
        /// Get all continents.
        /// </summary>
        public List<string> GetContinents()
        {
            return cities.Select(c => c.continent).Distinct().ToList();
        }

        /// <summary>
        /// Select a city for racing.
        /// </summary>
        public void SelectCity(string cityName, bool enableMultiplayer = false)
        {
            currentCity = cities.FirstOrDefault(c => c.cityName == cityName);

            if (currentCity == null)
            {
                Debug.LogError($"[GlobalMissionSystem] City not found: {cityName}");
                return;
            }

            isMultiplayerSession = enableMultiplayer;

            Debug.Log($"[GlobalMissionSystem] City selected: {currentCity.cityName}, {currentCity.country}");
            Debug.Log($"[GlobalMissionSystem] Available tracks: {string.Join(", ", currentCity.trackTypes)}");

            if (enableMultiplayer)
            {
                Debug.Log($"[GlobalMissionSystem] Multiplayer enabled for {cityName}");
            }
        }

        /// <summary>
        /// Get missions for current city.
        /// </summary>
        public List<Mission> GetCityMissions(string cityName)
        {
            var city = cities.FirstOrDefault(c => c.cityName == cityName);
            if (city == null) return new List<Mission>();

            var missions = new List<Mission>();

            // Generate missions for each track type
            for (int i = 0; i < city.trackTypes.Length; i++)
            {
                missions.Add(new Mission
                {
                    id = $"{cityName}_{i}",
                    title = $"{city.trackTypes[i]} Challenge",
                    description = $"Race through {city.trackTypes[i]} in {cityName}",
                    location = cityName,
                    trackType = city.trackTypes[i],
                    difficulty = city.difficulty,
                    rewardSVN = 100f * city.rewardMultiplier * city.difficulty,
                    rewardXP = 500 * city.difficulty,
                    timeLimit = 300f, // 5 minutes
                    requiresTier = city.unlockTier
                });
            }

            return missions;
        }

        /// <summary>
        /// Add player to multiplayer session.
        /// </summary>
        public void AddPlayer(string playerId)
        {
            if (!connectedPlayers.Contains(playerId))
            {
                connectedPlayers.Add(playerId);
                Debug.Log($"[GlobalMissionSystem] Player joined: {playerId} ({connectedPlayers.Count} total)");
            }
        }

        /// <summary>
        /// Remove player from multiplayer session.
        /// </summary>
        public void RemovePlayer(string playerId)
        {
            if (connectedPlayers.Remove(playerId))
            {
                Debug.Log($"[GlobalMissionSystem] Player left: {playerId} ({connectedPlayers.Count} remaining)");
            }
        }

        /// <summary>
        /// Get city info by name.
        /// </summary>
        public CityMissionHub GetCity(string cityName)
        {
            return cities.FirstOrDefault(c => c.cityName == cityName);
        }
    }

    /// <summary>
    /// City mission hub data structure.
    /// </summary>
    [Serializable]
    public class CityMissionHub
    {
        public string cityName;
        public string country;
        public string continent;
        public string timezone;
        public string[] weatherConditions;
        public string[] trackTypes;
        public string[] iconicLandmarks;
        public int difficulty; // 1-5
        public int unlockTier; // Tier required to unlock
        public float rewardMultiplier; // Reward multiplier for this city
        public Vector2 coordinates; // Lat/Long for map display
    }

    /// <summary>
    /// Mission data structure (extends existing).
    /// </summary>
    [Serializable]
    public class Mission
    {
        public string id;
        public string title;
        public string description;
        public string location; // City name
        public string trackType;
        public int difficulty;
        public float rewardSVN;
        public int rewardXP;
        public float timeLimit;
        public int requiresTier;
    }
}
