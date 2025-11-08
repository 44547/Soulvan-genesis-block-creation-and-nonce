using System;
using UnityEngine;

namespace Soulvan.Physics
{
    /// <summary>
    /// Advanced vehicle physics system with per-hypercar torque curves, aerodynamics, and damage modeling.
    /// Integrates with NVIDIA PhysX for realistic tire grip, suspension, and collision forces.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VehiclePhysics : MonoBehaviour
    {
        [Header("Vehicle Configuration")]
        [SerializeField] private VehicleProfile vehicleProfile;
        [SerializeField] private WheelCollider[] wheelColliders;
        [SerializeField] private Transform[] wheelMeshes;

        [Header("Engine & Transmission")]
        [SerializeField] private float currentRPM = 1000f;
        [SerializeField] private float maxRPM = 8000f;
        [SerializeField] private int currentGear = 1;
        [SerializeField] private float[] gearRatios = { 3.5f, 2.5f, 1.8f, 1.3f, 1.0f, 0.8f, 0.6f };

        [Header("Aerodynamics")]
        [SerializeField] private float currentDownforce = 0f;
        [SerializeField] private float activeSpoilerAngle = 0f;
        [SerializeField] private bool activeSpoilerDeployed = false;

        [Header("Damage & Wear")]
        [SerializeField] private float engineHeat = 20f; // Celsius
        [SerializeField] private float[] tireDegradation = { 1f, 1f, 1f, 1f }; // 1.0 = new, 0.0 = worn
        [SerializeField] private float[] brakeFade = { 1f, 1f, 1f, 1f }; // 1.0 = fresh, 0.0 = faded
        [SerializeField] private float structuralDamage = 0f; // 0 = pristine, 100 = totaled

        [Header("Input")]
        [SerializeField] private float throttleInput = 0f;
        [SerializeField] private float brakeInput = 0f;
        [SerializeField] private float steeringInput = 0f;
        [SerializeField] private bool nitroActive = false;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;

        private Rigidbody rb;
        private float currentSpeed; // km/h
        private float wheelRPM;
        private Vector3 localVelocity;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = vehicleProfile.centerOfMass;
            
            InitializeWheels();
            Debug.Log($"[VehiclePhysics] Initialized {vehicleProfile.name} with {vehicleProfile.horsepower} HP");
        }

        private void FixedUpdate()
        {
            if (vehicleProfile == null) return;

            UpdatePhysics();
            UpdateEngine();
            UpdateAerodynamics();
            UpdateDamageAndWear();
            ApplyForces();
            UpdateWheelMeshes();
        }

        #region Physics Simulation

        private void UpdatePhysics()
        {
            // Calculate local velocity and speed
            localVelocity = transform.InverseTransformDirection(rb.velocity);
            currentSpeed = localVelocity.z * 3.6f; // m/s to km/h
            wheelRPM = currentSpeed / (vehicleProfile.wheelRadius * 2f * Mathf.PI * 60f / 1000f);
        }

        private void UpdateEngine()
        {
            // Calculate engine torque from curve
            float torque = vehicleProfile.GetTorqueAtRPM(currentRPM);
            
            // Apply nitro boost
            if (nitroActive)
            {
                torque *= vehicleProfile.nitroMultiplier;
                engineHeat += 5f * Time.fixedDeltaTime; // Nitro heats engine
            }

            // Calculate wheel torque through transmission
            float gearRatio = gearRatios[currentGear];
            float wheelTorque = torque * gearRatio * vehicleProfile.finalDriveRatio;

            // Apply throttle
            wheelTorque *= throttleInput;

            // Distribute torque to wheels based on drivetrain
            ApplyWheelTorque(wheelTorque);

            // Update RPM based on wheel speed and gear
            float targetRPM = wheelRPM * gearRatio * vehicleProfile.finalDriveRatio;
            currentRPM = Mathf.Lerp(currentRPM, targetRPM, Time.fixedDeltaTime * 5f);
            currentRPM = Mathf.Clamp(currentRPM, 1000f, maxRPM);

            // Auto gear shifting
            if (currentRPM > maxRPM * 0.9f && currentGear < gearRatios.Length - 1)
            {
                currentGear++;
            }
            else if (currentRPM < maxRPM * 0.3f && currentGear > 0)
            {
                currentGear--;
            }

            // Engine heat dissipation
            engineHeat = Mathf.Lerp(engineHeat, 90f + (throttleInput * 30f), Time.fixedDeltaTime * 0.1f);
        }

        private void ApplyWheelTorque(float torque)
        {
            // Apply torque based on drivetrain type
            switch (vehicleProfile.drivetrain)
            {
                case DrivetrainType.RWD:
                    wheelColliders[2].motorTorque = torque * 0.5f;
                    wheelColliders[3].motorTorque = torque * 0.5f;
                    break;

                case DrivetrainType.FWD:
                    wheelColliders[0].motorTorque = torque * 0.5f;
                    wheelColliders[1].motorTorque = torque * 0.5f;
                    break;

                case DrivetrainType.AWD:
                    float frontTorque = torque * vehicleProfile.frontTorqueDistribution;
                    float rearTorque = torque * (1f - vehicleProfile.frontTorqueDistribution);
                    wheelColliders[0].motorTorque = frontTorque * 0.5f;
                    wheelColliders[1].motorTorque = frontTorque * 0.5f;
                    wheelColliders[2].motorTorque = rearTorque * 0.5f;
                    wheelColliders[3].motorTorque = rearTorque * 0.5f;
                    break;
            }
        }

        private void UpdateAerodynamics()
        {
            float speed = rb.velocity.magnitude;
            
            // Calculate downforce (increases with speed squared)
            currentDownforce = vehicleProfile.downforceCoefficient * speed * speed;
            
            // Active spoiler deployment
            if (vehicleProfile.hasActiveSpoiler)
            {
                if (speed > 100f && !activeSpoilerDeployed)
                {
                    activeSpoilerDeployed = true;
                    activeSpoilerAngle = 15f;
                    currentDownforce *= 1.5f;
                }
                else if (speed < 60f && activeSpoilerDeployed)
                {
                    activeSpoilerDeployed = false;
                    activeSpoilerAngle = 0f;
                }
            }

            // Apply downforce
            rb.AddForce(-transform.up * currentDownforce, ForceMode.Force);

            // Calculate drag force
            float dragForce = vehicleProfile.dragCoefficient * speed * speed;
            rb.AddForce(-rb.velocity.normalized * dragForce, ForceMode.Force);
        }

        private void UpdateDamageAndWear()
        {
            // Tire degradation based on slip
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                WheelHit hit;
                if (wheelColliders[i].GetGroundHit(out hit))
                {
                    // Slip causes tire wear
                    float slip = Mathf.Abs(hit.forwardSlip) + Mathf.Abs(hit.sidewaysSlip);
                    tireDegradation[i] -= slip * 0.0001f * Time.fixedDeltaTime;
                    tireDegradation[i] = Mathf.Clamp01(tireDegradation[i]);

                    // Update wheel friction based on wear
                    WheelFrictionCurve forwardFriction = wheelColliders[i].forwardFriction;
                    forwardFriction.stiffness = Mathf.Lerp(0.5f, 1.5f, tireDegradation[i]);
                    wheelColliders[i].forwardFriction = forwardFriction;
                }
            }

            // Brake fade from heat
            if (brakeInput > 0.5f)
            {
                for (int i = 0; i < brakeFade.Length; i++)
                {
                    brakeFade[i] -= 0.01f * brakeInput * Time.fixedDeltaTime;
                    brakeFade[i] = Mathf.Clamp01(brakeFade[i]);
                }
            }
            else
            {
                // Brakes cool down when not used
                for (int i = 0; i < brakeFade.Length; i++)
                {
                    brakeFade[i] = Mathf.Lerp(brakeFade[i], 1f, Time.fixedDeltaTime * 0.1f);
                }
            }

            // Engine overheat damage
            if (engineHeat > 110f)
            {
                structuralDamage += 0.1f * Time.fixedDeltaTime;
            }

            structuralDamage = Mathf.Clamp(structuralDamage, 0f, 100f);
        }

        private void ApplyForces()
        {
            // Apply braking with fade consideration
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                float effectiveBrakeForce = vehicleProfile.brakeForce * brakeInput * brakeFade[i];
                wheelColliders[i].brakeTorque = effectiveBrakeForce;
            }

            // Apply steering
            float steerAngle = steeringInput * vehicleProfile.maxSteeringAngle;
            wheelColliders[0].steerAngle = steerAngle;
            wheelColliders[1].steerAngle = steerAngle;
        }

        #endregion

        #region Wheel Management

        private void InitializeWheels()
        {
            foreach (var wheel in wheelColliders)
            {
                WheelFrictionCurve forwardFriction = wheel.forwardFriction;
                forwardFriction.stiffness = vehicleProfile.tireLateralGrip;
                wheel.forwardFriction = forwardFriction;

                WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
                sidewaysFriction.stiffness = vehicleProfile.tireLongitudinalGrip;
                wheel.sidewaysFriction = sidewaysFriction;

                wheel.suspensionDistance = vehicleProfile.suspensionTravel;
                
                JointSpring spring = wheel.suspensionSpring;
                spring.spring = vehicleProfile.suspensionStiffness;
                spring.damper = vehicleProfile.suspensionDamping;
                wheel.suspensionSpring = spring;
            }
        }

        private void UpdateWheelMeshes()
        {
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                if (wheelMeshes[i] == null) continue;

                Vector3 position;
                Quaternion rotation;
                wheelColliders[i].GetWorldPose(out position, out rotation);

                wheelMeshes[i].position = position;
                wheelMeshes[i].rotation = rotation;
            }
        }

        #endregion

        #region Public API

        public void SetThrottle(float value) => throttleInput = Mathf.Clamp01(value);
        public void SetBrake(float value) => brakeInput = Mathf.Clamp01(value);
        public void SetSteering(float value) => steeringInput = Mathf.Clamp(value, -1f, 1f);
        public void SetNitro(bool active) => nitroActive = active;

        public float GetSpeed() => currentSpeed;
        public float GetRPM() => currentRPM;
        public int GetGear() => currentGear;
        public float GetEngineHeat() => engineHeat;
        public float GetAverageTireWear() => (tireDegradation[0] + tireDegradation[1] + tireDegradation[2] + tireDegradation[3]) / 4f;
        public float GetStructuralDamage() => structuralDamage;

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!showDebugGizmos) return;

            GUILayout.BeginArea(new Rect(10, 100, 250, 200));
            GUILayout.Label($"=== {vehicleProfile?.name} ===");
            GUILayout.Label($"Speed: {currentSpeed:F1} km/h");
            GUILayout.Label($"RPM: {currentRPM:F0}");
            GUILayout.Label($"Gear: {currentGear + 1}/{gearRatios.Length}");
            GUILayout.Label($"Downforce: {currentDownforce:F0} N");
            GUILayout.Label($"Engine Heat: {engineHeat:F1}°C");
            GUILayout.Label($"Tire Wear: {GetAverageTireWear():F2}");
            GUILayout.Label($"Damage: {structuralDamage:F1}%");
            GUILayout.EndArea();
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || rb == null) return;

            // Draw velocity vector
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + rb.velocity);

            // Draw downforce vector
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position - transform.up * (currentDownforce * 0.01f));
        }

        #endregion
    }

    #region Vehicle Profile

    [Serializable]
    public enum DrivetrainType { RWD, FWD, AWD }

    [CreateAssetMenu(fileName = "VehicleProfile", menuName = "Soulvan/Vehicle Profile")]
    public class VehicleProfile : ScriptableObject
    {
        [Header("Identity")]
        public string vehicleName = "Bugatti Bolide";
        public string manufacturer = "Bugatti";

        [Header("Engine")]
        public float horsepower = 1600f;
        public float torque = 1600f; // Nm
        public AnimationCurve torqueCurve = AnimationCurve.Linear(0f, 0.5f, 1f, 1f); // RPM normalized to torque multiplier
        public float idleRPM = 1000f;
        public float maxRPM = 8000f;
        public float nitroMultiplier = 1.5f;

        [Header("Transmission")]
        public DrivetrainType drivetrain = DrivetrainType.AWD;
        public float frontTorqueDistribution = 0.3f; // For AWD only
        public float finalDriveRatio = 3.5f;

        [Header("Aerodynamics")]
        public float dragCoefficient = 0.3f;
        public float downforceCoefficient = 50f;
        public bool hasActiveSpoiler = true;

        [Header("Suspension")]
        public float suspensionTravel = 0.3f; // meters
        public float suspensionStiffness = 35000f;
        public float suspensionDamping = 4500f;

        [Header("Tires")]
        public float wheelRadius = 0.35f; // meters
        public float tireLateralGrip = 1.5f;
        public float tireLongitudinalGrip = 1.5f;

        [Header("Brakes")]
        public float brakeForce = 8000f;

        [Header("Mass & Center of Mass")]
        public float mass = 1240f; // kg
        public Vector3 centerOfMass = new Vector3(0f, -0.5f, 0f);

        [Header("Steering")]
        public float maxSteeringAngle = 35f;

        public float GetTorqueAtRPM(float rpm)
        {
            float normalizedRPM = Mathf.InverseLerp(idleRPM, maxRPM, rpm);
            return torque * torqueCurve.Evaluate(normalizedRPM);
        }
    }

    #endregion
}
