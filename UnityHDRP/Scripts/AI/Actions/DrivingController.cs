using UnityEngine;

namespace Soulvan.AI.Actions
{
    /// <summary>
    /// Physics-based driving controller for AI agents.
    /// Handles steering, acceleration, braking, and drift mechanics.
    /// Integrates with Unity's Rigidbody system for realistic vehicle physics.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class DrivingController : MonoBehaviour
    {
        [Header("Performance")]
        [SerializeField] private float maxSpeedKmh = 240f;
        [SerializeField] private float accelerationForce = 55f;
        [SerializeField] private float turnRateDegPerSec = 2.4f;
        [SerializeField] private float brakeForce = 120f;

        [Header("Drift")]
        [SerializeField] private AnimationCurve driftCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float driftMultiplier = 100f;

        [Header("Physics")]
        [SerializeField] private float downforceMultiplier = 2f; // Keeps car grounded at high speeds

        private Rigidbody rb;
        private float currentSpeed; // Cached for performance

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = new Vector3(0f, -0.5f, 0f); // Lower center of mass for stability
        }

        private void FixedUpdate()
        {
            currentSpeed = rb.velocity.magnitude;
            
            // Apply downforce at high speeds for better handling
            float downforce = currentSpeed * downforceMultiplier;
            rb.AddForce(-transform.up * downforce, ForceMode.Force);
        }

        /// <summary>
        /// Drive towards a waypoint with specified aggression level.
        /// </summary>
        /// <param name="waypoint">Target world position</param>
        /// <param name="aggression01">0 = cautious, 1 = max aggression</param>
        public void DriveTowards(Vector3 waypoint, float aggression01)
        {
            Vector3 targetDir = (waypoint - transform.position);
            targetDir.y = 0f; // Flatten to horizontal plane
            
            if (targetDir.sqrMagnitude < 0.01f) return; // Already at target

            // Calculate steering angle
            Vector3 fwd = transform.forward;
            float turnAngle = Vector3.SignedAngle(fwd, targetDir.normalized, Vector3.up);
            float turnAmount = Mathf.Clamp(turnAngle, -turnRateDegPerSec, turnRateDegPerSec);
            transform.Rotate(0f, turnAmount * Time.fixedDeltaTime * 60f, 0f);

            // Calculate target speed based on aggression
            float targetSpeedKmh = Mathf.Lerp(90f, maxSpeedKmh, aggression01);
            float targetSpeedMs = targetSpeedKmh / 3.6f; // Convert km/h to m/s
            
            // Apply acceleration
            float speedDelta = targetSpeedMs - currentSpeed;
            float accel = Mathf.Clamp(speedDelta, -accelerationForce, accelerationForce);
            rb.AddForce(transform.forward * accel, ForceMode.Acceleration);

            // Apply drift effect based on turn angle and aggression
            float driftIntensity = driftCurve.Evaluate(Mathf.Abs(turnAngle) / 90f) * aggression01;
            float driftDirection = Mathf.Sign(turnAngle);
            rb.AddForce(transform.right * driftDirection * driftIntensity * driftMultiplier, ForceMode.Force);
        }

        /// <summary>
        /// Apply brakes with specified intensity.
        /// </summary>
        /// <param name="intensity01">0 = no braking, 1 = emergency stop</param>
        public void Brake(float intensity01)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, intensity01 * brakeForce * Time.fixedDeltaTime);
        }

        /// <summary>
        /// Execute handbrake turn (drift turn).
        /// </summary>
        public void HandbrakeTurn(float direction)
        {
            rb.angularVelocity = Vector3.up * direction * 5f;
            Brake(0.3f);
        }

        /// <summary>
        /// Get current speed in km/h.
        /// </summary>
        public float GetSpeedKmh()
        {
            return currentSpeed * 3.6f;
        }

        /// <summary>
        /// Apply nitro boost force.
        /// </summary>
        public void ApplyNitro(float boostForce = 200f)
        {
            rb.AddForce(transform.forward * boostForce, ForceMode.Acceleration);
        }

        private void OnDrawGizmos()
        {
            if (!rb) return;
            
            // Draw velocity vector
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, rb.velocity);
            
            // Draw forward direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 5f);
        }
    }
}
