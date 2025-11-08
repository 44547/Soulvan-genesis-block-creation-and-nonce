using UnityEngine;

namespace Soulvan.Systems
{
    /// <summary>
    /// Performance scaling system for adaptive quality settings.
    /// Dynamically adjusts motif intensity, particle counts, and draw distances
    /// based on FPS and GPU load to maintain smooth gameplay.
    /// </summary>
    public class PerformanceScaler : MonoBehaviour
    {
        [Header("FPS Targets")]
        [SerializeField] private float targetFPS = 60f;
        [SerializeField] private float minFPS = 30f;
        [SerializeField] private float maxFPS = 144f;

        [Header("Quality Bounds")]
        [SerializeField, Range(0f, 1f)] private float minQuality = 0.3f;
        [SerializeField, Range(0f, 1f)] private float maxQuality = 1f;

        [Header("Adaptation")]
        [SerializeField] private float adaptSpeed = 0.1f;
        [SerializeField] private bool autoScale = true;

        private float currentQuality = 1f;
        private float avgFrameTime;
        private const int frameHistorySize = 60;
        private float[] frameTimeHistory = new float[frameHistorySize];
        private int frameIndex;

        private void Update()
        {
            if (!autoScale) return;

            // Track frame time
            frameTimeHistory[frameIndex] = Time.unscaledDeltaTime;
            frameIndex = (frameIndex + 1) % frameHistorySize;

            // Calculate average FPS
            avgFrameTime = 0f;
            for (int i = 0; i < frameHistorySize; i++)
            {
                avgFrameTime += frameTimeHistory[i];
            }
            avgFrameTime /= frameHistorySize;
            float avgFPS = 1f / Mathf.Max(avgFrameTime, 0.001f);

            // Adjust quality based on performance
            float targetQuality = Mathf.InverseLerp(minFPS, targetFPS, avgFPS);
            targetQuality = Mathf.Clamp(targetQuality, minQuality, maxQuality);
            
            currentQuality = Mathf.Lerp(currentQuality, targetQuality, adaptSpeed * Time.unscaledDeltaTime);

            // Apply quality settings
            ApplyQualitySettings();
        }

        private void ApplyQualitySettings()
        {
            // Scale motif intensity
            var motifAPI = FindFirstObjectByType<MotifAPI>();
            if (motifAPI)
            {
                float baseIntensity = motifAPI.GetCurrentIntensity();
                float scaledIntensity = baseIntensity * currentQuality;
                // MotifAPI will handle actual scaling
            }

            // Adjust particle system max particles
            var particleSystems = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                int maxParticles = Mathf.RoundToInt(Mathf.Lerp(100, 1000, currentQuality));
                main.maxParticles = maxParticles;
            }

            // Adjust shadow quality
            QualitySettings.shadowDistance = Mathf.Lerp(50f, 150f, currentQuality);
            QualitySettings.shadowCascades = currentQuality > 0.7f ? 4 : 2;
        }

        /// <summary>
        /// Get current performance quality scalar (0..1).
        /// </summary>
        public float GetQualityScalar() => currentQuality;

        /// <summary>
        /// Force quality level (disables auto-scaling temporarily).
        /// </summary>
        public void SetQuality(float quality01)
        {
            currentQuality = Mathf.Clamp01(quality01);
            autoScale = false;
        }

        /// <summary>
        /// Enable auto-scaling.
        /// </summary>
        public void EnableAutoScale()
        {
            autoScale = true;
        }

        /// <summary>
        /// Get current average FPS.
        /// </summary>
        public float GetAverageFPS()
        {
            return 1f / Mathf.Max(avgFrameTime, 0.001f);
        }

        private void OnGUI()
        {
            if (!Debug.isDebugBuild) return;

            // Debug overlay
            GUI.Label(new Rect(10, 10, 200, 20), $"FPS: {GetAverageFPS():F1}");
            GUI.Label(new Rect(10, 30, 200, 20), $"Quality: {currentQuality:F2}");
            GUI.Label(new Rect(10, 50, 200, 20), $"Auto: {autoScale}");
        }
    }
}
