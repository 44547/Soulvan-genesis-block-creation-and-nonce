using UnityEngine;

namespace Soulvan.Systems
{
    /// <summary>
    /// DLSS Controller manages NVIDIA DLSS evolution (3.7 → 4.0).
    /// Supports Balanced, Quality, Ultra Quality, Performance, and Frame Generation modes.
    /// </summary>
    public class DLSSController : MonoBehaviour
    {
        [Header("DLSS Settings")]
        [SerializeField] private DLSSMode currentMode = DLSSMode.Quality;
        [SerializeField] private bool enableFrameGeneration = false;
        [SerializeField] private bool enableRayReconstruction = false;
        [SerializeField] private bool enableDLAA = false; // Deep Learning Anti-Aliasing

        [Header("Auto Mode")]
        [SerializeField] private bool autoSelectMode = true;
        [SerializeField] private int targetFPS = 60;
        [SerializeField] private int targetResolution = 1440; // 1080p, 1440p, 2160p (4K), 4320p (8K)

        private bool isDLSSAvailable = false;
        private bool isDLSS40 = false;
        private float[] qualityScales = { 0.5f, 0.58f, 0.67f, 0.77f, 1.0f }; // Perf, Balanced, Quality, Ultra, Native

        public string CurrentMode => enableFrameGeneration ? $"{currentMode} + FG" : currentMode.ToString();

        public void Initialize()
        {
            DetectDLSSCapabilities();
            
            if (isDLSSAvailable)
            {
                if (autoSelectMode)
                {
                    SelectOptimalMode();
                }
                
                ApplyDLSSSettings();
            }
            
            Debug.Log($"[DLSSController] Initialized - DLSS: {isDLSSAvailable}, DLSS 4.0: {isDLSS40}, Mode: {CurrentMode}");
        }

        private void DetectDLSSCapabilities()
        {
            string gpuName = SystemInfo.graphicsDeviceName.ToLower();
            
            // DLSS available on RTX 20/30/40/50 series
            isDLSSAvailable = gpuName.Contains("rtx");
            
            // DLSS 4.0 with Frame Generation on RTX 50 series
            isDLSS40 = gpuName.Contains("50");
            
            // Enable frame generation only on RTX 50 series
            if (!isDLSS40)
            {
                enableFrameGeneration = false;
            }
        }

        private void SelectOptimalMode()
        {
            // Auto-select DLSS mode based on resolution and target FPS
            if (targetResolution >= 4320) // 8K
            {
                currentMode = DLSSMode.Performance;
                enableFrameGeneration = isDLSS40; // Use FG at 8K
            }
            else if (targetResolution >= 2160) // 4K
            {
                currentMode = DLSSMode.Balanced;
                enableFrameGeneration = isDLSS40 && targetFPS >= 120;
            }
            else if (targetResolution >= 1440) // 1440p
            {
                currentMode = DLSSMode.Quality;
                enableFrameGeneration = isDLSS40 && targetFPS >= 144;
            }
            else // 1080p
            {
                currentMode = DLSSMode.UltraQuality;
                enableFrameGeneration = false; // Native-like quality at 1080p
            }

            Debug.Log($"[DLSSController] Auto-selected mode: {CurrentMode} for {targetResolution}p @ {targetFPS} FPS");
        }

        private void ApplyDLSSSettings()
        {
            if (!isDLSSAvailable) return;

            // Get quality scale for current mode
            float scale = GetQualityScale(currentMode);
            
            // Apply render scale
            // In production, this would call NVIDIA DLSS SDK or Unity's DLSS integration
            // Example: DLSSCommandBuffer.SetDLSSMode(currentMode, enableFrameGeneration);
            
            Debug.Log($"[DLSSController] Applied DLSS mode: {currentMode}, Scale: {scale:F2}, Frame Gen: {enableFrameGeneration}");
        }

        private float GetQualityScale(DLSSMode mode)
        {
            switch (mode)
            {
                case DLSSMode.Performance: return qualityScales[0];
                case DLSSMode.Balanced: return qualityScales[1];
                case DLSSMode.Quality: return qualityScales[2];
                case DLSSMode.UltraQuality: return qualityScales[3];
                case DLSSMode.Native: return qualityScales[4];
                default: return qualityScales[2];
            }
        }

        public void EnableFrameGeneration(bool enable)
        {
            if (!isDLSS40)
            {
                Debug.LogWarning("[DLSSController] Frame Generation requires RTX 50 series GPU");
                return;
            }

            enableFrameGeneration = enable;
            ApplyDLSSSettings();
            Debug.Log($"[DLSSController] Frame Generation: {(enable ? "ON" : "OFF")}");
        }

        public void EnableRayReconstruction(bool enable)
        {
            if (!isDLSS40)
            {
                Debug.LogWarning("[DLSSController] Ray Reconstruction requires DLSS 4.0");
                return;
            }

            enableRayReconstruction = enable;
            ApplyDLSSSettings();
            Debug.Log($"[DLSSController] Ray Reconstruction: {(enable ? "ON" : "OFF")}");
        }

        public void SetMode(string modeName)
        {
            switch (modeName.ToLower())
            {
                case "performance":
                    currentMode = DLSSMode.Performance;
                    break;
                case "balanced":
                    currentMode = DLSSMode.Balanced;
                    break;
                case "quality":
                    currentMode = DLSSMode.Quality;
                    break;
                case "ultra":
                case "ultra quality":
                    currentMode = DLSSMode.UltraQuality;
                    break;
                case "native":
                case "off":
                    currentMode = DLSSMode.Native;
                    break;
            }

            ApplyDLSSSettings();
        }

        public void RefreshQualityPresets()
        {
            // Called when DLSS updates to new version
            Debug.Log("[DLSSController] Refreshing quality presets");
            
            if (isDLSS40)
            {
                // DLSS 4.0 has improved quality scales
                qualityScales[0] = 0.5f;  // Performance
                qualityScales[1] = 0.58f; // Balanced
                qualityScales[2] = 0.67f; // Quality
                qualityScales[3] = 0.77f; // Ultra Quality
            }

            ApplyDLSSSettings();
        }

        private void Update()
        {
            if (!isDLSSAvailable || !autoSelectMode) return;

            // Monitor FPS and adjust mode if needed
            float currentFPS = 1f / Time.deltaTime;
            
            if (currentFPS < targetFPS * 0.8f) // Below 80% of target
            {
                // Step down quality
                if (currentMode == DLSSMode.UltraQuality)
                    SetMode("quality");
                else if (currentMode == DLSSMode.Quality)
                    SetMode("balanced");
                else if (currentMode == DLSSMode.Balanced)
                    SetMode("performance");
                else if (!enableFrameGeneration && isDLSS40)
                    EnableFrameGeneration(true);
            }
            else if (currentFPS > targetFPS * 1.5f) // Well above target
            {
                // Step up quality
                if (enableFrameGeneration)
                    EnableFrameGeneration(false);
                else if (currentMode == DLSSMode.Performance)
                    SetMode("balanced");
                else if (currentMode == DLSSMode.Balanced)
                    SetMode("quality");
                else if (currentMode == DLSSMode.Quality)
                    SetMode("ultra quality");
            }
        }

        private void OnGUI()
        {
            if (!isDLSSAvailable) return;

            GUILayout.BeginArea(new Rect(10, 310, 250, 120));
            GUILayout.Label($"DLSS Mode: {CurrentMode}");
            GUILayout.Label($"Quality Scale: {GetQualityScale(currentMode):F2}");
            GUILayout.Label($"Ray Reconstruction: {(enableRayReconstruction ? "ON" : "OFF")}");
            GUILayout.Label($"DLAA: {(enableDLAA ? "ON" : "OFF")}");
            GUILayout.EndArea();
        }

        public enum DLSSMode
        {
            Performance,
            Balanced,
            Quality,
            UltraQuality,
            Native
        }
    }
}
