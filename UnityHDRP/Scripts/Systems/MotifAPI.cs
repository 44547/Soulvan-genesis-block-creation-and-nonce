using UnityEngine;

namespace Soulvan.Systems
{
    /// <summary>
    /// Motif types matching seasonal arcs.
    /// </summary>
    public enum Motif
    {
        Storm,   // Neon city racing, volatility
        Calm,    // Stealth missions, restoration
        Cosmic,  // Boss battles, prophecy
        Oracle   // Governance rituals
    }

    /// <summary>
    /// Cinematic motif system controlling visual/audio/haptic overlays.
    /// Integrates with VFX Graph, particle systems, and audio buses (FMOD/Wwise).
    /// Performance-scaled for adaptive quality.
    /// </summary>
    public class MotifAPI : MonoBehaviour
    {
        [Header("Visual FX - Particle Systems")]
        [SerializeField] private ParticleSystem stormRain;
        [SerializeField] private ParticleSystem calmFog;
        [SerializeField] private ParticleSystem cosmicAurora;
        [SerializeField] private ParticleSystem oracleRunes;

        [Header("Visual FX - VFX Graph (Optional)")]
        [SerializeField] private UnityEngine.VFX.VisualEffect stormVFX;
        [SerializeField] private UnityEngine.VFX.VisualEffect calmVFX;
        [SerializeField] private UnityEngine.VFX.VisualEffect cosmicVFX;

        [Header("Audio")]
        [SerializeField] private AudioSource musicBus;
        [SerializeField] private AudioClip stormMusic;
        [SerializeField] private AudioClip calmMusic;
        [SerializeField] private AudioClip cosmicMusic;
        [SerializeField] private AudioClip oracleMusic;

        [Header("Post-Processing")]
        [SerializeField] private UnityEngine.Rendering.Volume postProcessVolume;

        private Motif currentMotif = Motif.Storm;
        private float currentIntensity = 0.5f;

        /// <summary>
        /// Set active motif and intensity.
        /// </summary>
        /// <param name="motif">Target motif type</param>
        /// <param name="intensity01">0 = minimal, 1 = maximum intensity</param>
        public void SetMotif(Motif motif, float intensity01)
        {
            currentMotif = motif;
            currentIntensity = Mathf.Clamp01(intensity01);

            // Update particle systems
            UpdateParticleSystems(motif, intensity01);

            // Update VFX Graph (if using Unity VFX Graph)
            UpdateVFXGraph(motif, intensity01);

            // Update audio
            UpdateAudio(motif, intensity01);

            // Update post-processing
            UpdatePostProcessing(motif, intensity01);

            // Emit event for other systems
            Infra.EventBus.EmitMotif(motif.ToString());
        }

        private void UpdateParticleSystems(Motif motif, float intensity)
        {
            // Activate/deactivate systems
            SetActive(stormRain, motif == Motif.Storm);
            SetActive(calmFog, motif == Motif.Calm);
            SetActive(cosmicAurora, motif == Motif.Cosmic);
            SetActive(oracleRunes, motif == Motif.Oracle);

            // Scale emission rates
            float baseRate = Mathf.Lerp(10f, 200f, intensity);
            SetEmissionRate(stormRain, baseRate, motif == Motif.Storm);
            SetEmissionRate(calmFog, baseRate * 0.5f, motif == Motif.Calm);
            SetEmissionRate(cosmicAurora, baseRate * 0.8f, motif == Motif.Cosmic);
            SetEmissionRate(oracleRunes, baseRate * 0.6f, motif == Motif.Oracle);
        }

        private void UpdateVFXGraph(Motif motif, float intensity)
        {
            if (stormVFX) stormVFX.enabled = motif == Motif.Storm;
            if (calmVFX) calmVFX.enabled = motif == Motif.Calm;
            if (cosmicVFX) cosmicVFX.enabled = motif == Motif.Cosmic;

            // Set intensity parameters
            if (stormVFX && motif == Motif.Storm)
            {
                stormVFX.SetFloat("Intensity", intensity);
                stormVFX.SetFloat("EmissionRate", Mathf.Lerp(50f, 500f, intensity));
            }
        }

        private void UpdateAudio(Motif motif, float intensity)
        {
            if (!musicBus) return;

            // Crossfade music tracks
            AudioClip targetClip = motif switch
            {
                Motif.Storm => stormMusic,
                Motif.Calm => calmMusic,
                Motif.Cosmic => cosmicMusic,
                Motif.Oracle => oracleMusic,
                _ => stormMusic
            };

            if (targetClip && musicBus.clip != targetClip)
            {
                musicBus.clip = targetClip;
                musicBus.Play();
            }

            // Adjust pitch based on intensity
            musicBus.pitch = Mathf.Lerp(0.95f, 1.08f, intensity);
            musicBus.volume = Mathf.Lerp(0.6f, 1f, intensity);
        }

        private void UpdatePostProcessing(Motif motif, float intensity)
        {
            if (!postProcessVolume) return;

            // Adjust post-processing weight based on motif
            postProcessVolume.weight = Mathf.Lerp(0.5f, 1f, intensity);
            
            // TODO: Set specific volume profile per motif
            // Example: Storm = high contrast, Calm = soft bloom, Cosmic = lens flares
        }

        private void SetActive(ParticleSystem ps, bool active)
        {
            if (!ps) return;
            if (active && !ps.isPlaying) ps.Play();
            else if (!active && ps.isPlaying) ps.Stop();
        }

        private void SetEmissionRate(ParticleSystem ps, float rate, bool isActive)
        {
            if (!ps || !isActive) return;
            var emission = ps.emission;
            emission.rateOverTime = rate;
        }

        /// <summary>
        /// Get current active motif.
        /// </summary>
        public Motif GetCurrentMotif() => currentMotif;

        /// <summary>
        /// Get current intensity.
        /// </summary>
        public float GetCurrentIntensity() => currentIntensity;

        /// <summary>
        /// Quick transition to next seasonal arc.
        /// </summary>
        public void TransitionToNextSeason()
        {
            Motif next = currentMotif switch
            {
                Motif.Storm => Motif.Calm,
                Motif.Calm => Motif.Cosmic,
                Motif.Cosmic => Motif.Storm,
                _ => Motif.Storm
            };
            SetMotif(next, 0.7f);
        }
    }
}
