using UnityEngine;
using Soulvan.AI.Perception;
using Soulvan.AI.Decision;
using Soulvan.AI.Actions;
using Soulvan.Systems;

namespace Soulvan.AI
{
    /// <summary>
    /// Main AI controller integrating perception, decision, and action systems.
    /// Implements hybrid state machine + utility AI for racing, missions, and combat.
    /// </summary>
    [RequireComponent(typeof(DrivingController))]
    [RequireComponent(typeof(VisionSensor))]
    [RequireComponent(typeof(ThreatEvaluator))]
    public class AgentBrain : MonoBehaviour
    {
        [Header("Blackboard")]
        public AgentBlackboard bb = new AgentBlackboard();

        [Header("Waypoints")]
        public Transform[] racePath;
        private int currentWp;

        [Header("References")]
        private VisionSensor sensor;
        private ThreatEvaluator threat;
        private DrivingController drive;
        private MissionActions mission;
        private CombatActions combat;
        private MotifAPI motif;
        private RewardService rewards;
        private PerformanceScaler perf;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private AgentGoal currentGoal;

        private void Awake()
        {
            // Get required components
            sensor = GetComponent<VisionSensor>();
            threat = GetComponent<ThreatEvaluator>();
            drive = GetComponent<DrivingController>();
            mission = GetComponent<MissionActions>();
            combat = GetComponent<CombatActions>();

            // Find global services
            motif = FindFirstObjectByType<MotifAPI>();
            rewards = FindFirstObjectByType<RewardService>();
            perf = FindFirstObjectByType<PerformanceScaler>();

            // Initialize blackboard
            bb.Reset();
            bb.missionActive = true;
        }

        private void FixedUpdate()
        {
            UpdatePerception();
            UpdateDecision();
            ExecuteGoal();
        }

        /// <summary>
        /// Update perception and threat evaluation.
        /// </summary>
        private void UpdatePerception()
        {
            // Get current speed
            float speedKmh = drive.GetSpeedKmh();

            // Update threat level
            bb.threatLevel = threat.Evaluate(bb.rival, bb.lastSeenThreatPos, speedKmh, bb.damagePct);

            // Scan for rivals
            if (bb.rival && !sensor.SeeTarget(bb.rival))
            {
                bb.rival = null; // Lost sight
            }

            // Performance-scaled motif intensity
            float perfScalar = perf ? perf.GetQualityScalar() : 1f;
            bb.motifIntensity = Mathf.Clamp01(0.4f + bb.threatLevel * 0.6f) * perfScalar;
        }

        /// <summary>
        /// Select goal using utility AI.
        /// </summary>
        private void UpdateDecision()
        {
            currentGoal = UtilityScorer.Select(bb);
        }

        /// <summary>
        /// Execute behavior for selected goal.
        /// </summary>
        private void ExecuteGoal()
        {
            switch (currentGoal)
            {
                case AgentGoal.Race:
                    motif?.SetMotif(Motif.Storm, bb.motifIntensity);
                    RaceBehaviour();
                    break;

                case AgentGoal.StealthDeliver:
                    motif?.SetMotif(Motif.Calm, bb.motifIntensity * 0.7f);
                    StealthBehaviour();
                    break;

                case AgentGoal.Flee:
                    motif?.SetMotif(Motif.Storm, 1f);
                    FleeBehaviour();
                    break;

                case AgentGoal.BossDuel:
                    motif?.SetMotif(Motif.Cosmic, bb.motifIntensity);
                    BossBehaviour();
                    break;

                case AgentGoal.Recover:
                    motif?.SetMotif(Motif.Oracle, 0.5f);
                    RecoverBehaviour();
                    break;
            }
        }

        #region Behavior Implementations

        private void RaceBehaviour()
        {
            if (racePath == null || racePath.Length == 0)
            {
                Debug.LogWarning($"[AgentBrain] {name} has no race path assigned!");
                return;
            }

            var wp = racePath[currentWp];
            float aggression = Mathf.Lerp(0.6f, 1f, bb.threatLevel);
            drive.DriveTowards(wp.position, aggression);

            // Advance waypoint when close
            if (Vector3.Distance(transform.position, wp.position) < 8f)
            {
                currentWp = (currentWp + 1) % racePath.Length;
                
                // Completed lap?
                if (currentWp == 0)
                {
                    Debug.Log($"[AgentBrain] {name} completed lap!");
                    rewards?.MintSVNReward("WALLET_STUB", 50f, RewardType.SVNCoin);
                }
            }
        }

        private void StealthBehaviour()
        {
            if (!bb.targetWaypoint)
            {
                Debug.LogWarning($"[AgentBrain] {name} in stealth mode but no target waypoint!");
                return;
            }

            // Slow, careful driving
            drive.DriveTowards(bb.targetWaypoint.position, aggression01: 0.35f);

            // Check for drone detection
            var drones = GameObject.FindGameObjectsWithTag("Drone");
            foreach (var drone in drones)
            {
                if (sensor.SeeTarget(drone.transform))
                {
                    bb.isInStealthZone = false;
                    bb.threatLevel = Mathf.Max(bb.threatLevel, 0.7f);
                    break;
                }
            }

            // Delivery complete?
            if (bb.hasCargo && Vector3.Distance(transform.position, bb.targetWaypoint.position) < 6f)
            {
                mission.DropCargo(bb);
                rewards?.GrantSeasonalBadge("calm_delivery", "WALLET_STUB");
                bb.missionActive = false;
                Infra.EventBus.EmitMissionCompleted("stealth_delivery");
            }
        }

        private void FleeBehaviour()
        {
            // Brake-turn, then accelerate away from threat
            Vector3 away = (transform.position - bb.lastSeenThreatPos).normalized;
            Vector3 escape = transform.position + away * 40f;
            drive.DriveTowards(escape, aggression01: 1f);

            // Check if safe distance reached
            if (Vector3.Distance(transform.position, bb.lastSeenThreatPos) > 60f)
            {
                bb.threatLevel *= 0.5f; // Reduce threat
            }
        }

        private void BossBehaviour()
        {
            if (!bb.rival)
            {
                Debug.LogWarning($"[AgentBrain] {name} in boss mode but no rival assigned!");
                return;
            }

            // Aggressive pursuit
            Vector3 interceptPoint = bb.rival.position + bb.rival.forward * 12f;
            drive.DriveTowards(interceptPoint, aggression01: 0.9f);

            // Attempt ram if close
            float distance = Vector3.Distance(transform.position, bb.rival.position);
            if (distance < 10f && combat)
            {
                combat.RamTarget(bb.rival);
            }

            // Check if boss defeated
            var bossHealth = bb.rival.GetComponent<Actions.HealthComponent>();
            if (bossHealth && bossHealth.GetHealthPct() <= 0f)
            {
                Debug.Log($"[AgentBrain] {name} defeated boss!");
                bb.bossEngaged = false;
                rewards?.MintSVNReward("WALLET_STUB", 200f, RewardType.GovernanceToken);
                Infra.EventBus.EmitBossDefeated(bb.rival.name);
            }
        }

        private void RecoverBehaviour()
        {
            // Slow down and find repair station
            drive.Brake(0.5f);

            // TODO: Navigate to pitstop waypoint
            // TODO: Trigger repair animation/state

            Debug.Log($"[AgentBrain] {name} recovering... Damage: {bb.damagePct:F2}, Fuel: {bb.fuelPct:F2}");

            // Simulate repair over time
            bb.damagePct = Mathf.Max(0f, bb.damagePct - Time.fixedDeltaTime * 0.1f);
            bb.fuelPct = Mathf.Min(1f, bb.fuelPct + Time.fixedDeltaTime * 0.05f);

            // Return to racing when recovered
            if (bb.damagePct < 0.3f && bb.fuelPct > 0.5f)
            {
                bb.missionActive = true;
            }
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            // Draw current waypoint
            if (racePath != null && racePath.Length > 0 && currentWp < racePath.Length)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, racePath[currentWp].position);
                Gizmos.DrawWireSphere(racePath[currentWp].position, 2f);
            }

            // Draw rival connection
            if (bb.rival)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, bb.rival.position);
            }

            // Draw threat position
            if (bb.lastSeenThreatPos != Vector3.zero)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(bb.lastSeenThreatPos, 5f);
            }

            // Draw cargo connection
            if (bb.hasCargo && bb.cargo)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, bb.cargo.position);
            }
        }

        private void OnGUI()
        {
            if (!Debug.isDebugBuild) return;
            if (!showDebugGizmos) return;

            // Debug HUD
            GUILayout.BeginArea(new Rect(10, 100, 300, 200));
            GUILayout.Label($"Agent: {name}");
            GUILayout.Label($"Goal: {currentGoal}");
            GUILayout.Label($"Threat: {bb.threatLevel:F2}");
            GUILayout.Label($"Speed: {drive.GetSpeedKmh():F0} km/h");
            GUILayout.Label($"Damage: {bb.damagePct:F2}");
            GUILayout.Label($"Fuel: {bb.fuelPct:F2}");
            GUILayout.Label($"Has Cargo: {bb.hasCargo}");
            GUILayout.EndArea();
        }

        #endregion
    }
}
