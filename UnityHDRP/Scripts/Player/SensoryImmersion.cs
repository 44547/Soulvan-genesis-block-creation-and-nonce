using UnityEngine;
using UnityEngine.InputSystem;
using Soulvan.Systems;

namespace Soulvan.Player
{
    /// <summary>
    /// Sensory immersion system with force feedback, haptics, adaptive audio, and HUD motifs.
    /// Syncs feedback to driving state, motif overlays, and mission events.
    /// </summary>
    public class SensoryImmersion : MonoBehaviour
    {
        [Header("Force Feedback")]
        [SerializeField] private bool forceFeedbackEnabled = true;
        [SerializeField] private float resistanceStrength = 0.8f;
        [SerializeField] private float bumpIntensity = 1.0f;

        [Header("Haptic Controllers")]
        [SerializeField] private bool hapticsEnabled = true;
        [SerializeField] private float stormPulseIntensity = 0.9f;
        [SerializeField] private float calmGlowIntensity = 0.3f;
        [SerializeField] private float cosmicSurgeIntensity = 1.0f;

        [Header("Adaptive Audio")]
        [SerializeField] private AudioSource engineAudioSource;
        [SerializeField] private AudioClip[] engineSounds; // Different RPM ranges
        [SerializeField] private AudioSource ambientAudioSource;
        [SerializeField] private AudioClip stormAmbient;
        [SerializeField] private AudioClip calmAmbient;
        [SerializeField] private AudioClip cosmicAmbient;
        [SerializeField] private float audioTransitionSpeed = 2f;

        [Header("HUD Motifs")]
        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private UnityEngine.UI.Image stormOverlay;
        [SerializeField] private UnityEngine.UI.Image calmOverlay;
        [SerializeField] private UnityEngine.UI.Image cosmicOverlay;
        [SerializeField] private UnityEngine.UI.Image oracleOverlay;

        [Header("References")]
        [SerializeField] private Soulvan.Physics.VehiclePhysics vehiclePhysics;
        [SerializeField] private MotifAPI motifAPI;

        private Gamepad gamepad;
        private Motif currentMotif = Motif.Storm;
        private float currentMotifIntensity = 0f;
        private float enginePitch = 1f;
        private float engineVolume = 0.5f;

        private void Start()
        {
            gamepad = Gamepad.current;
            
            if (engineAudioSource != null && engineSounds.Length > 0)
            {
                engineAudioSource.clip = engineSounds[0];
                engineAudioSource.loop = true;
                engineAudioSource.Play();
            }

            EventBus.OnMotifChanged += OnMotifChanged;
            EventBus.OnMissionCompleted += OnMissionCompleted;
            EventBus.OnBossDefeated += OnBossDefeated;
            EventBus.OnDaoVoteCast += OnDaoVoteCast;
        }

        private void OnDestroy()
        {
            EventBus.OnMotifChanged -= OnMotifChanged;
            EventBus.OnMissionCompleted -= OnMissionCompleted;
            EventBus.OnBossDefeated -= OnBossDefeated;
            EventBus.OnDaoVoteCast -= OnDaoVoteCast;
        }

        private void Update()
        {
            UpdateForceFeedback();
            UpdateHaptics();
            UpdateAdaptiveAudio();
            UpdateHUDMotifs();
        }

        #region Force Feedback

        private void UpdateForceFeedback()
        {
            if (!forceFeedbackEnabled || vehiclePhysics == null) return;

            // Simulate steering wheel resistance based on speed and grip
            float speed = vehiclePhysics.GetSpeed();
            float resistance = Mathf.Lerp(0.1f, resistanceStrength, speed / 200f);

            // Increase resistance during drifts (lateral forces)
            Vector3 localVelocity = transform.InverseTransformDirection(vehiclePhysics.GetComponent<Rigidbody>().velocity);
            float lateralSlip = Mathf.Abs(localVelocity.x);
            resistance += lateralSlip * 0.5f;

            // Apply to steering device (conceptual - would use SDK in production)
            // SteeringWheelSDK.SetResistance(resistance);
        }

        #endregion

        #region Haptics

        private void UpdateHaptics()
        {
            if (!hapticsEnabled || gamepad == null) return;

            // Base rumble from engine RPM
            float rpm = vehiclePhysics != null ? vehiclePhysics.GetRPM() : 1000f;
            float baseRumble = Mathf.InverseLerp(1000f, 8000f, rpm) * 0.3f;

            // Motif-specific haptics
            float leftMotor = baseRumble;
            float rightMotor = baseRumble;

            switch (currentMotif)
            {
                case Motif.Storm:
                    // Storm: Pulsing intensity in triggers, full rumble
                    leftMotor += Mathf.Sin(Time.time * 3f) * stormPulseIntensity * currentMotifIntensity;
                    rightMotor += Mathf.Sin(Time.time * 3f + Mathf.PI) * stormPulseIntensity * currentMotifIntensity;
                    break;

                case Motif.Calm:
                    // Calm: Subtle vibrations, soft glow pattern
                    leftMotor += Mathf.Sin(Time.time * 0.5f) * calmGlowIntensity * currentMotifIntensity;
                    rightMotor = leftMotor;
                    break;

                case Motif.Cosmic:
                    // Cosmic: Full rumble surges, synced to aurora pulses
                    float cosmicPulse = Mathf.Sin(Time.time * 2f) * 0.5f + 0.5f;
                    leftMotor += cosmicPulse * cosmicSurgeIntensity * currentMotifIntensity;
                    rightMotor += cosmicPulse * cosmicSurgeIntensity * currentMotifIntensity;
                    break;

                case Motif.Oracle:
                    // Oracle: Rhythmic pulses during vote rituals
                    float oraclePulse = Mathf.PingPong(Time.time * 1.5f, 1f);
                    leftMotor += oraclePulse * 0.7f * currentMotifIntensity;
                    rightMotor += oraclePulse * 0.7f * currentMotifIntensity;
                    break;
            }

            // Clamp and apply
            leftMotor = Mathf.Clamp01(leftMotor);
            rightMotor = Mathf.Clamp01(rightMotor);
            
            gamepad.SetMotorSpeeds(leftMotor, rightMotor);
        }

        public void TriggerHapticBurst(float intensity, float duration)
        {
            if (!hapticsEnabled || gamepad == null) return;

            StartCoroutine(HapticBurstCoroutine(intensity, duration));
        }

        private System.Collections.IEnumerator HapticBurstCoroutine(float intensity, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float curve = Mathf.Sin(t * Mathf.PI); // Fade in-out curve
                
                gamepad.SetMotorSpeeds(intensity * curve, intensity * curve);
                
                elapsed += Time.deltaTime;
                yield return null;
            }

            gamepad.SetMotorSpeeds(0f, 0f);
        }

        #endregion

        #region Adaptive Audio

        private void UpdateAdaptiveAudio()
        {
            if (engineAudioSource == null || vehiclePhysics == null) return;

            // Engine audio pitch and volume based on RPM
            float rpm = vehiclePhysics.GetRPM();
            enginePitch = Mathf.Lerp(0.8f, 2.0f, Mathf.InverseLerp(1000f, 8000f, rpm));
            engineVolume = Mathf.Lerp(0.3f, 1.0f, Mathf.InverseLerp(1000f, 8000f, rpm));

            engineAudioSource.pitch = enginePitch;
            engineAudioSource.volume = engineVolume;

            // Switch ambient audio based on motif
            UpdateAmbientAudio();
        }

        private void UpdateAmbientAudio()
        {
            if (ambientAudioSource == null) return;

            AudioClip targetClip = null;
            float targetVolume = currentMotifIntensity;

            switch (currentMotif)
            {
                case Motif.Storm:
                    targetClip = stormAmbient;
                    // Storm: loud, thunderous
                    break;

                case Motif.Calm:
                    targetClip = calmAmbient;
                    // Calm: muted, muffled (stealth mode)
                    targetVolume *= 0.5f;
                    break;

                case Motif.Cosmic:
                    targetClip = cosmicAmbient;
                    // Cosmic: synth swells, orchestral crescendo
                    break;

                case Motif.Oracle:
                    targetClip = cosmicAmbient; // Reuse cosmic for oracle
                    // Oracle: chanting, sacred geometry harmonics
                    break;
            }

            // Crossfade to target clip
            if (targetClip != null && ambientAudioSource.clip != targetClip)
            {
                StartCoroutine(CrossfadeAudio(targetClip, targetVolume));
            }
            else
            {
                ambientAudioSource.volume = Mathf.Lerp(ambientAudioSource.volume, targetVolume, Time.deltaTime * audioTransitionSpeed);
            }
        }

        private System.Collections.IEnumerator CrossfadeAudio(AudioClip newClip, float targetVolume)
        {
            float elapsed = 0f;
            float duration = 1f / audioTransitionSpeed;
            float startVolume = ambientAudioSource.volume;

            // Fade out
            while (elapsed < duration * 0.5f)
            {
                ambientAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration * 0.5f));
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Swap clip
            ambientAudioSource.clip = newClip;
            ambientAudioSource.Play();

            // Fade in
            elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                ambientAudioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / (duration * 0.5f));
                elapsed += Time.deltaTime;
                yield return null;
            }

            ambientAudioSource.volume = targetVolume;
        }

        #endregion

        #region HUD Motifs

        private void UpdateHUDMotifs()
        {
            if (hudCanvas == null) return;

            // Fade all overlays out by default
            FadeOverlay(stormOverlay, currentMotif == Motif.Storm ? currentMotifIntensity : 0f);
            FadeOverlay(calmOverlay, currentMotif == Motif.Calm ? currentMotifIntensity : 0f);
            FadeOverlay(cosmicOverlay, currentMotif == Motif.Cosmic ? currentMotifIntensity : 0f);
            FadeOverlay(oracleOverlay, currentMotif == Motif.Oracle ? currentMotifIntensity : 0f);

            // Add driving-state effects
            if (vehiclePhysics != null)
            {
                float speed = vehiclePhysics.GetSpeed();

                // Storm: Lightning flashes at high speed
                if (currentMotif == Motif.Storm && speed > 200f)
                {
                    float flash = Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f;
                    FadeOverlay(stormOverlay, currentMotifIntensity * flash);
                }

                // Calm: Fog layers intensify at low speed
                if (currentMotif == Motif.Calm && speed < 50f)
                {
                    float fog = Mathf.InverseLerp(50f, 10f, speed);
                    FadeOverlay(calmOverlay, currentMotifIntensity * fog);
                }

                // Cosmic: Aurora ribbons sync to drift angle
                if (currentMotif == Motif.Cosmic)
                {
                    Vector3 localVelocity = transform.InverseTransformDirection(vehiclePhysics.GetComponent<Rigidbody>().velocity);
                    float driftAngle = Mathf.Abs(localVelocity.x) / (localVelocity.z + 0.1f);
                    FadeOverlay(cosmicOverlay, currentMotifIntensity * Mathf.Clamp01(driftAngle * 2f));
                }
            }
        }

        private void FadeOverlay(UnityEngine.UI.Image overlay, float targetAlpha)
        {
            if (overlay == null) return;

            Color color = overlay.color;
            color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * 5f);
            overlay.color = color;
        }

        #endregion

        #region Event Handlers

        private void OnMotifChanged(Motif newMotif, float intensity)
        {
            currentMotif = newMotif;
            currentMotifIntensity = intensity;

            Debug.Log($"[SensoryImmersion] Motif changed: {newMotif}, Intensity: {intensity:F2}");

            // Trigger haptic feedback for motif change
            TriggerHapticBurst(intensity, 0.5f);
        }

        private void OnMissionCompleted(string missionId)
        {
            // Calm glow on mission complete
            TriggerHapticBurst(calmGlowIntensity, 1f);
        }

        private void OnBossDefeated(string bossName)
        {
            // Cosmic surge on boss defeat
            TriggerHapticBurst(cosmicSurgeIntensity, 2f);
        }

        private void OnDaoVoteCast(string proposalId)
        {
            // Oracle pulse on vote
            TriggerHapticBurst(0.8f, 1.5f);
        }

        #endregion
    }
}
