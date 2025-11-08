using UnityEngine;

namespace Soulvan.AI.Decision
{
    /// <summary>
    /// Agent memory store for perception, state, and decision-making data.
    /// Updated by sensors and consumed by UtilityScorer and AgentBrain.
    /// </summary>
    [System.Serializable]
    public class AgentBlackboard
    {
        [Header("Navigation")]
        public Transform targetWaypoint;
        public Transform rival;
        public Transform cargo;
        
        [Header("Threat Tracking")]
        public Vector3 lastSeenThreatPos;
        public float threatLevel; // 0..1 normalized threat score
        
        [Header("Agent State")]
        public float fuelPct; // 0..1
        public float damagePct; // 0..1
        public bool hasCargo;
        public bool isInStealthZone;
        public bool missionActive;
        public bool bossEngaged;
        
        [Header("Visual/Audio/Haptic")]
        public float motifIntensity; // 0..1 scaled by PerformanceScaler
        
        /// <summary>
        /// Reset blackboard state for new mission or respawn.
        /// </summary>
        public void Reset()
        {
            targetWaypoint = null;
            rival = null;
            cargo = null;
            lastSeenThreatPos = Vector3.zero;
            threatLevel = 0f;
            fuelPct = 1f;
            damagePct = 0f;
            hasCargo = false;
            isInStealthZone = false;
            missionActive = false;
            bossEngaged = false;
            motifIntensity = 0f;
        }
    }
}
