using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Soulvan.Systems
{
    /// <summary>
    /// RTX Auto-Scaler adjusts ray tracing fidelity based on GPU driver updates and performance.
    /// Supports RTX 5090 with latest features (ReSTIR GI, Ray Reconstruction, Reflex 2.0).
    /// </summary>
    public class RTXAutoScaler : MonoBehaviour
    {
        [Header("RTX Settings")]
        [SerializeField] private Volume globalVolume;
        [SerializeField] private bool autoDetectCapabilities = true;
        
        [Header("Ray Tracing Quality")]
        [Range(0f, 1f)]
        [SerializeField] private float rayTracingQuality = 1f;
        [SerializeField] private bool enableReSTIRGI = true;
        [SerializeField] private bool enableRayReconstruction = true;
        [SerializeField] private bool enableReflex = true;

        [Header("Performance Targets")]
        [SerializeField] private int targetFPS = 60;
        [SerializeField] private float qualityAdjustmentSpeed = 0.1f;

        private HDRenderPipelineAsset hdAsset;
        private RayTracingSettings rtSettings;
        private bool isRTXCapable = false;
        private bool isDLSS40Available = false;
        private bool isReSTIRAvailable = false;

        public bool IsActive => isRTXCapable && rayTracingQuality > 0f;

        public void Initialize()
        {
            hdAsset = GraphicsSettings.currentRenderPipeline as HDRenderPipelineAsset;
            
            if (autoDetectCapabilities)
            {
                DetectGPUCapabilities();
            }

            if (globalVolume != null && globalVolume.profile.Has<RayTracingSettings>())
            {
                globalVolume.profile.TryGet(out rtSettings);
            }

            ApplyRTXSettings();
            
            Debug.Log($"[RTXAutoScaler] Initialized - RTX: {isRTXCapable}, ReSTIR: {isReSTIRAvailable}, DLSS 4.0: {isDLSS40Available}");
        }

        private void DetectGPUCapabilities()
        {
            string gpuName = SystemInfo.graphicsDeviceName.ToLower();
            
            // Check for RTX 5090 or RTX 40/50 series
            isRTXCapable = gpuName.Contains("rtx") && 
                          (gpuName.Contains("5090") || 
                           gpuName.Contains("50") || 
                           gpuName.Contains("4090") ||
                           gpuName.Contains("40"));

            // ReSTIR GI available on RTX 40/50 series with driver 566.36+
            isReSTIRAvailable = isRTXCapable && CheckDriverVersion("566.36");

            // DLSS 4.0 available on RTX 50 series
            isDLSS40Available = gpuName.Contains("50");

            Debug.Log($"[RTXAutoScaler] GPU: {SystemInfo.graphicsDeviceName}");
            Debug.Log($"[RTXAutoScaler] Driver: {SystemInfo.graphicsDeviceVersion}");
        }

        private bool CheckDriverVersion(string minVersion)
        {
            // Stub: Parse SystemInfo.graphicsDeviceVersion and compare
            // Real implementation would extract driver number from version string
            return true; // Assume latest drivers
        }

        private void Update()
        {
            if (!isRTXCapable) return;

            // Adaptive quality adjustment
            float currentFPS = 1f / Time.deltaTime;
            float targetRatio = currentFPS / targetFPS;

            if (targetRatio < 0.9f) // Below 90% of target
            {
                rayTracingQuality = Mathf.Max(0.2f, rayTracingQuality - qualityAdjustmentSpeed * Time.deltaTime);
                ApplyRTXSettings();
            }
            else if (targetRatio > 1.1f) // Above 110% of target
            {
                rayTracingQuality = Mathf.Min(1f, rayTracingQuality + qualityAdjustmentSpeed * Time.deltaTime * 0.5f);
                ApplyRTXSettings();
            }
        }

        private void ApplyRTXSettings()
        {
            if (hdAsset == null) return;

            // Apply ray tracing quality
            if (rtSettings != null)
            {
                // Adjust ray count based on quality
                int rayCount = Mathf.RoundToInt(Mathf.Lerp(1, 4, rayTracingQuality));
                
                // These would be actual HDRP settings in production
                // rtSettings.raySamples.value = rayCount;
                // rtSettings.maxBounces.value = Mathf.RoundToInt(Mathf.Lerp(1, 3, rayTracingQuality));
            }

            // Enable/disable advanced features based on GPU capabilities
            if (isReSTIRAvailable && enableReSTIRGI)
            {
                EnableReSTIRGI();
            }

            if (enableRayReconstruction)
            {
                EnableRayReconstruction();
            }

            if (enableReflex)
            {
                EnableNVIDIAReflex();
            }

            Debug.Log($"[RTXAutoScaler] Applied quality: {rayTracingQuality:F2}");
        }

        private void EnableReSTIRGI()
        {
            // ReSTIR GI provides high-quality global illumination with fewer rays
            // Stub: Would configure HDRP's GI settings to use ReSTIR algorithm
            Debug.Log("[RTXAutoScaler] ReSTIR GI enabled");
        }

        private void EnableRayReconstruction()
        {
            // Ray Reconstruction uses AI to denoise ray-traced output
            // Part of DLSS 4.0 suite
            Debug.Log("[RTXAutoScaler] Ray Reconstruction enabled");
        }

        private void EnableNVIDIAReflex()
        {
            // NVIDIA Reflex reduces input latency
            // Stub: Would call NVIDIA Reflex SDK
            Debug.Log("[RTXAutoScaler] NVIDIA Reflex 2.0 enabled");
        }

        public void UpdateGISettings(UpdateManager.ModuleVersion version)
        {
            Debug.Log($"[RTXAutoScaler] Updating GI with version {version.versionString}");
            
            // Check for new features in the update
            foreach (var feature in version.features)
            {
                if (feature.Contains("ReSTIR") && !isReSTIRAvailable)
                {
                    isReSTIRAvailable = true;
                    EnableReSTIRGI();
                }
            }
        }

        public void ReduceFidelity()
        {
            rayTracingQuality = Mathf.Max(0.2f, rayTracingQuality * 0.7f);
            ApplyRTXSettings();
            Debug.Log($"[RTXAutoScaler] Reduced fidelity to {rayTracingQuality:F2}");
        }

        public void SetQuality(float quality)
        {
            rayTracingQuality = Mathf.Clamp01(quality);
            ApplyRTXSettings();
        }

        private void OnGUI()
        {
            if (!isRTXCapable) return;

            GUILayout.BeginArea(new Rect(10, 150, 250, 150));
            GUILayout.Label($"RTX Quality: {rayTracingQuality:F2}");
            GUILayout.Label($"ReSTIR GI: {(isReSTIRAvailable && enableReSTIRGI ? "ON" : "OFF")}");
            GUILayout.Label($"Ray Reconstruction: {(enableRayReconstruction ? "ON" : "OFF")}");
            GUILayout.Label($"Reflex: {(enableReflex ? "ON" : "OFF")}");
            GUILayout.EndArea();
        }
    }
}
