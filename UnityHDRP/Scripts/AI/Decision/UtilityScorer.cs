using UnityEngine;

namespace Soulvan.AI.Decision
{
    /// <summary>
    /// Goal options for hybrid state machine + utility AI.
    /// </summary>
    public enum AgentGoal
    {
        Race,
        StealthDeliver,
        Flee,
        BossDuel,
        Recover
    }

    /// <summary>
    /// Utility-based goal selection system.
    /// Evaluates blackboard state and returns highest-scoring goal.
    /// Uses softmax-like normalization for smooth transitions.
    /// </summary>
    public static class UtilityScorer
    {
        /// <summary>
        /// Select the best goal based on current blackboard state.
        /// </summary>
        public static AgentGoal Select(AgentBlackboard bb)
        {
            // Base utility scores
            float raceU = bb.missionActive && !bb.hasCargo ? 0.7f : 0.4f;
            float stealthU = bb.hasCargo ? 0.8f : 0.2f;
            float fleeU = bb.threatLevel > 0.65f ? 0.9f : 0.1f;
            float bossU = bb.bossEngaged ? 0.85f : 0.1f;
            float recoverU = (bb.damagePct > 0.6f || bb.fuelPct < 0.2f) ? 0.75f : 0.15f;

            // Softmax-like normalization
            float sum = raceU + stealthU + fleeU + bossU + recoverU;
            if (sum < 0.01f) return AgentGoal.Race; // Fallback

            float r = raceU / sum;
            float s = stealthU / sum;
            float f = fleeU / sum;
            float b = bossU / sum;
            float rc = recoverU / sum;

            // Select highest normalized score
            float max = Mathf.Max(r, s, f, b, rc);
            
            if (Mathf.Approximately(max, f)) return AgentGoal.Flee;
            if (Mathf.Approximately(max, s)) return AgentGoal.StealthDeliver;
            if (Mathf.Approximately(max, b)) return AgentGoal.BossDuel;
            if (Mathf.Approximately(max, rc)) return AgentGoal.Recover;
            
            return AgentGoal.Race;
        }

        /// <summary>
        /// Get utility score for a specific goal (for debugging/tuning).
        /// </summary>
        public static float GetUtility(AgentGoal goal, AgentBlackboard bb)
        {
            switch (goal)
            {
                case AgentGoal.Race:
                    return bb.missionActive && !bb.hasCargo ? 0.7f : 0.4f;
                case AgentGoal.StealthDeliver:
                    return bb.hasCargo ? 0.8f : 0.2f;
                case AgentGoal.Flee:
                    return bb.threatLevel > 0.65f ? 0.9f : 0.1f;
                case AgentGoal.BossDuel:
                    return bb.bossEngaged ? 0.85f : 0.1f;
                case AgentGoal.Recover:
                    return (bb.damagePct > 0.6f || bb.fuelPct < 0.2f) ? 0.75f : 0.15f;
                default:
                    return 0f;
            }
        }
    }
}
