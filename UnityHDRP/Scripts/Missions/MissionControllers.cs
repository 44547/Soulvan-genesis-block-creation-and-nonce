using UnityEngine;
using Soulvan.Physics;

namespace Soulvan.Missions
{
    /// <summary>
    /// Driving mission controller for high-speed races and pursuits.
    /// Tracks waypoints, rivals, and time challenges.
    /// </summary>
    public class DrivingMission : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float waypointRadius = 20f;
        
        [Header("Time Challenge")]
        [SerializeField] private float timeLimit = 180f; // seconds
        [SerializeField] private bool hasTimeLimit = false;

        [Header("Rivals")]
        [SerializeField] private GameObject[] rivalVehicles;
        [SerializeField] private int rivalsToDefeat = 0;

        private MissionData missionData;
        private int currentWaypointIndex = 0;
        private float elapsedTime = 0f;
        private int rivalsDefeated = 0;
        private bool missionActive = false;

        public void Initialize(MissionData data)
        {
            missionData = data;
        }

        public void Begin()
        {
            missionActive = true;
            currentWaypointIndex = 0;
            elapsedTime = 0f;
            rivalsDefeated = 0;

            // Spawn rival vehicles
            foreach (var rival in rivalVehicles)
            {
                if (rival != null)
                    rival.SetActive(true);
            }

            Debug.Log($"[DrivingMission] Started: {missionData?.name}");
        }

        private void Update()
        {
            if (!missionActive) return;

            // Update elapsed time
            elapsedTime += Time.deltaTime;

            // Check time limit
            if (hasTimeLimit && elapsedTime > timeLimit)
            {
                FailMission("Time limit exceeded");
                return;
            }

            // Check waypoint progress
            if (waypoints.Length > 0)
            {
                CheckWaypointProgress();
            }

            // Check mission completion
            CheckCompletion();
        }

        private void CheckWaypointProgress()
        {
            if (currentWaypointIndex >= waypoints.Length) return;

            Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) return;

            float distance = Vector3.Distance(player.position, waypoints[currentWaypointIndex].position);
            
            if (distance < waypointRadius)
            {
                currentWaypointIndex++;
                Debug.Log($"[DrivingMission] Waypoint {currentWaypointIndex}/{waypoints.Length} reached");
                
                // Trigger waypoint VFX/audio
                EventBus.EmitWaypointReached(currentWaypointIndex);
            }
        }

        private void CheckCompletion()
        {
            bool waypointsComplete = currentWaypointIndex >= waypoints.Length;
            bool rivalsComplete = rivalsDefeated >= rivalsToDefeat;

            if (waypointsComplete && rivalsComplete)
            {
                CompleteMission();
            }
        }

        private void CompleteMission()
        {
            missionActive = false;
            
            Debug.Log($"[DrivingMission] Completed in {elapsedTime:F2}s");
            EventBus.EmitMissionCompleted(missionData?.id);
        }

        private void FailMission(string reason)
        {
            missionActive = false;
            
            Debug.LogWarning($"[DrivingMission] Failed: {reason}");
            EventBus.EmitMissionFailed(missionData?.id, reason);
        }

        public void OnRivalDefeated()
        {
            rivalsDefeated++;
            Debug.Log($"[DrivingMission] Rivals defeated: {rivalsDefeated}/{rivalsToDefeat}");
        }

        private void OnDrawGizmos()
        {
            if (waypoints == null) return;

            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;

                // Draw waypoint sphere
                Gizmos.DrawWireSphere(waypoints[i].position, waypointRadius);

                // Draw path line
                if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }
    }

    /// <summary>
    /// Stealth mission controller for infiltration and cargo delivery.
    /// Tracks detection status and hacking progress.
    /// </summary>
    public class StealthMission : MonoBehaviour
    {
        [Header("Stealth Zones")]
        [SerializeField] private Transform[] stealthZones;
        [SerializeField] private float zoneRadius = 30f;

        [Header("Objectives")]
        [SerializeField] private Transform[] hackTargets;
        [SerializeField] private Transform deliveryPoint;
        [SerializeField] private float interactionRange = 3f;

        [Header("Detection")]
        [SerializeField] private Transform[] detectionSources; // Drones, cameras
        [SerializeField] private int maxDetections = 3;
        [SerializeField] private float detectionCooldown = 5f;

        private MissionData missionData;
        private int targetsHacked = 0;
        private int detectionsCount = 0;
        private float lastDetectionTime = 0f;
        private bool cargoDelivered = false;
        private bool missionActive = false;

        public void Initialize(MissionData data)
        {
            missionData = data;
        }

        public void Begin()
        {
            missionActive = true;
            targetsHacked = 0;
            detectionsCount = 0;
            cargoDelivered = false;

            Debug.Log($"[StealthMission] Started: {missionData?.name}");
        }

        private void Update()
        {
            if (!missionActive) return;

            CheckHackingInteraction();
            CheckDeliveryInteraction();
            CheckDetection();
            CheckCompletion();
        }

        private void CheckHackingInteraction()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            foreach (var target in hackTargets)
            {
                if (target == null) continue;

                float distance = Vector3.Distance(player.transform.position, target.position);
                
                if (distance < interactionRange && Input.GetKeyDown(KeyCode.E))
                {
                    StartHacking(target);
                }
            }
        }

        private void StartHacking(Transform target)
        {
            Debug.Log($"[StealthMission] Hacking {target.name}...");
            
            // Start hacking coroutine
            StartCoroutine(HackSequence(target));
        }

        private System.Collections.IEnumerator HackSequence(Transform target)
        {
            float hackTime = 3f;
            float elapsed = 0f;

            while (elapsed < hackTime)
            {
                // Check if player moved away
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player == null || Vector3.Distance(player.transform.position, target.position) > interactionRange)
                {
                    Debug.Log("[StealthMission] Hacking interrupted");
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            targetsHacked++;
            Debug.Log($"[StealthMission] Target hacked: {targetsHacked}/{hackTargets.Length}");
            
            // Disable hacked target
            target.gameObject.SetActive(false);
        }

        private void CheckDeliveryInteraction()
        {
            if (deliveryPoint == null || cargoDelivered) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            float distance = Vector3.Distance(player.transform.position, deliveryPoint.position);
            
            if (distance < interactionRange && Input.GetKeyDown(KeyCode.E))
            {
                cargoDelivered = true;
                Debug.Log("[StealthMission] Cargo delivered");
            }
        }

        private void CheckDetection()
        {
            if (Time.time - lastDetectionTime < detectionCooldown) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            foreach (var source in detectionSources)
            {
                if (source == null) continue;

                // Simple line-of-sight check
                Vector3 direction = player.transform.position - source.position;
                if (Physics.Raycast(source.position, direction.normalized, out RaycastHit hit, 50f))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        OnDetected();
                        break;
                    }
                }
            }
        }

        private void OnDetected()
        {
            detectionsCount++;
            lastDetectionTime = Time.time;

            Debug.LogWarning($"[StealthMission] Detected! {detectionsCount}/{maxDetections}");
            EventBus.EmitPlayerDetected();

            if (detectionsCount >= maxDetections)
            {
                FailMission("Too many detections");
            }
        }

        private void CheckCompletion()
        {
            bool hackingComplete = targetsHacked >= hackTargets.Length;
            bool deliveryComplete = deliveryPoint == null || cargoDelivered;

            if (hackingComplete && deliveryComplete)
            {
                CompleteMission();
            }
        }

        private void CompleteMission()
        {
            missionActive = false;
            
            Debug.Log($"[StealthMission] Completed with {detectionsCount} detections");
            EventBus.EmitMissionCompleted(missionData?.id);
        }

        private void FailMission(string reason)
        {
            missionActive = false;
            
            Debug.LogWarning($"[StealthMission] Failed: {reason}");
            EventBus.EmitMissionFailed(missionData?.id, reason);
        }
    }

    /// <summary>
    /// Boss battle controller for mythic encounters.
    /// Tracks boss health, phases, and cinematic sequences.
    /// </summary>
    public class BossBattle : MonoBehaviour
    {
        [Header("Boss Configuration")]
        [SerializeField] private GameObject bossObject;
        [SerializeField] private string bossName = "Flame Samurai";
        [SerializeField] private float bossMaxHealth = 1000f;

        [Header("Phases")]
        [SerializeField] private float[] phaseThresholds = { 0.75f, 0.5f, 0.25f }; // Health percentages
        [SerializeField] private int currentPhase = 0;

        [Header("Arena")]
        [SerializeField] private Transform arenaCenter;
        [SerializeField] private float arenaRadius = 50f;

        private MissionData missionData;
        private float bossCurrentHealth;
        private bool missionActive = false;

        public void Initialize(MissionData data)
        {
            missionData = data;
        }

        public void Begin()
        {
            missionActive = true;
            bossCurrentHealth = bossMaxHealth;
            currentPhase = 0;

            if (bossObject != null)
            {
                bossObject.SetActive(true);
            }

            Debug.Log($"[BossBattle] Started: {bossName}");
        }

        public void ApplyDamage(float amount)
        {
            if (!missionActive) return;

            bossCurrentHealth -= amount;
            bossCurrentHealth = Mathf.Max(0f, bossCurrentHealth);

            float healthPercent = bossCurrentHealth / bossMaxHealth;
            Debug.Log($"[BossBattle] Boss health: {healthPercent:P0}");

            CheckPhaseTransition(healthPercent);

            if (bossCurrentHealth <= 0f)
            {
                DefeatBoss();
            }
        }

        private void CheckPhaseTransition(float healthPercent)
        {
            for (int i = currentPhase; i < phaseThresholds.Length; i++)
            {
                if (healthPercent <= phaseThresholds[i])
                {
                    currentPhase = i + 1;
                    TriggerPhaseTransition(currentPhase);
                    break;
                }
            }
        }

        private void TriggerPhaseTransition(int phase)
        {
            Debug.Log($"[BossBattle] Phase {phase} triggered");
            
            // Trigger cinematic sequence, new attack patterns, motif surge
            EventBus.EmitBossPhaseChange(bossName, phase);

            // Apply cosmic motif surge
            FindObjectOfType<MotifAPI>()?.SetMotif(Motif.Cosmic, 1.0f);
        }

        private void DefeatBoss()
        {
            missionActive = false;

            if (bossObject != null)
            {
                bossObject.SetActive(false);
            }

            Debug.Log($"[BossBattle] {bossName} defeated!");
            EventBus.EmitBossDefeated(bossName);
            EventBus.EmitMissionCompleted(missionData?.id);
        }

        private void OnDrawGizmos()
        {
            if (arenaCenter == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(arenaCenter.position, arenaRadius);
        }
    }

    /// <summary>
    /// DAO ritual controller for governance voting and lore council.
    /// Displays proposals and handles cinematic vote sequences.
    /// </summary>
    public class DaoRitual : MonoBehaviour
    {
        [Header("Ritual Configuration")]
        [SerializeField] private Transform ritualCenter;
        [SerializeField] private GameObject ritualVFX;

        [Header("UI")]
        [SerializeField] private GameObject ritualPanel;
        [SerializeField] private UnityEngine.UI.Text proposalText;
        [SerializeField] private UnityEngine.UI.Button voteForButton;
        [SerializeField] private UnityEngine.UI.Button voteAgainstButton;

        private MissionData missionData;
        private string currentProposalId;
        private bool missionActive = false;

        public void Initialize(MissionData data)
        {
            missionData = data;
        }

        public void Begin()
        {
            missionActive = true;

            // Fetch current proposal from DAO governance client
            currentProposalId = "proposal_season_transition";

            ShowRitualUI();

            if (ritualVFX != null)
            {
                ritualVFX.SetActive(true);
            }

            // Apply oracle motif
            FindObjectOfType<MotifAPI>()?.SetMotif(Motif.Oracle, 1.0f);

            Debug.Log($"[DaoRitual] Started: {missionData?.name}");
        }

        private void ShowRitualUI()
        {
            if (ritualPanel != null)
            {
                ritualPanel.SetActive(true);
            }

            if (proposalText != null)
            {
                proposalText.text = "Proposal: Transition to Cosmic Season\n\nVote to evolve the saga.";
            }

            if (voteForButton != null)
            {
                voteForButton.onClick.AddListener(() => CastVote(1));
            }

            if (voteAgainstButton != null)
            {
                voteAgainstButton.onClick.AddListener(() => CastVote(0));
            }
        }

        public void CastVote(int choice)
        {
            if (!missionActive) return;

            Debug.Log($"[DaoRitual] Vote cast: {choice} on {currentProposalId}");

            // Trigger haptic oracle pulse
            FindObjectOfType<SensoryImmersion>()?.TriggerHapticBurst(0.8f, 1.5f);

            // Submit vote to wallet
            var wallet = FindObjectOfType<WalletController>();
            if (wallet != null)
            {
                wallet.VoteOnProposal(currentProposalId, choice);
            }

            CompleteMission();
        }

        private void CompleteMission()
        {
            missionActive = false;

            if (ritualPanel != null)
            {
                ritualPanel.SetActive(false);
            }

            if (ritualVFX != null)
            {
                ritualVFX.SetActive(false);
            }

            Debug.Log($"[DaoRitual] Completed");
            EventBus.EmitDaoVoteCast(currentProposalId);
            EventBus.EmitMissionCompleted(missionData?.id);
        }
    }
}
