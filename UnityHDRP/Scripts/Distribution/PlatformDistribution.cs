using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Soulvan.Distribution
{
    /// <summary>
    /// Multi-platform distribution manager for Soulvan.
    /// Supports PC, PlayStation, Xbox, Android with payment integration.
    /// All fees paid add to Soulvan Coin to stabilize value through AI self-buying.
    /// </summary>
    public class PlatformDistribution : MonoBehaviour
    {
        [Header("Platform Configuration")]
        [SerializeField] private bool enablePC = true;
        [SerializeField] private bool enablePlayStation = true;
        [SerializeField] private bool enableXbox = true;
        [SerializeField] private bool enableAndroid = true;

        [Header("Download Links")]
        [SerializeField] private string pcDownloadUrl = "https://soulvan.io/download/pc";
        [SerializeField] private string playstationStoreUrl = "https://store.playstation.com/soulvan";
        [SerializeField] private string xboxStoreUrl = "https://microsoft.com/store/soulvan";
        [SerializeField] private string androidPlayStoreUrl = "https://play.google.com/store/apps/soulvan";

        [Header("Payment Integration")]
        [SerializeField] private SoulvanPaymentGateway paymentGateway;
        [SerializeField] private AIStabilityEngine stabilityEngine;

        [Header("Pricing (USD)")]
        [SerializeField] private float basePriceUSD = 59.99f;
        [SerializeField] private float playstationPriceUSD = 59.99f;
        [SerializeField] private float xboxPriceUSD = 59.99f;
        [SerializeField] private float androidPriceUSD = 39.99f;

        private void Awake()
        {
            if (paymentGateway == null)
            {
                paymentGateway = FindObjectOfType<SoulvanPaymentGateway>();
            }

            if (stabilityEngine == null)
            {
                stabilityEngine = FindObjectOfType<AIStabilityEngine>();
            }
        }

        /// <summary>
        /// Get available platforms for download.
        /// </summary>
        public List<PlatformInfo> GetAvailablePlatforms()
        {
            List<PlatformInfo> platforms = new List<PlatformInfo>();

            if (enablePC)
            {
                platforms.Add(new PlatformInfo
                {
                    platform = Platform.PC,
                    name = "PC (Windows/Linux/Mac)",
                    downloadUrl = pcDownloadUrl,
                    priceUSD = basePriceUSD,
                    requirements = "Windows 10/11, RTX 3060+, 16GB RAM, 100GB Storage",
                    icon = "pc_icon"
                });
            }

            if (enablePlayStation)
            {
                platforms.Add(new PlatformInfo
                {
                    platform = Platform.PlayStation,
                    name = "PlayStation 5",
                    downloadUrl = playstationStoreUrl,
                    priceUSD = playstationPriceUSD,
                    requirements = "PlayStation 5, 100GB Storage",
                    icon = "ps5_icon"
                });
            }

            if (enableXbox)
            {
                platforms.Add(new PlatformInfo
                {
                    platform = Platform.Xbox,
                    name = "Xbox Series X|S",
                    downloadUrl = xboxStoreUrl,
                    priceUSD = xboxPriceUSD,
                    requirements = "Xbox Series X|S, 100GB Storage",
                    icon = "xbox_icon"
                });
            }

            if (enableAndroid)
            {
                platforms.Add(new PlatformInfo
                {
                    platform = Platform.Android,
                    name = "Android",
                    downloadUrl = androidPlayStoreUrl,
                    priceUSD = androidPriceUSD,
                    requirements = "Android 11+, Snapdragon 888+, 8GB RAM, 20GB Storage",
                    icon = "android_icon"
                });
            }

            return platforms;
        }

        /// <summary>
        /// Purchase game for specific platform.
        /// Routes payment to SoulvanPaymentGateway.
        /// </summary>
        public async Task<PurchaseResult> PurchaseGame(Platform platform, PaymentMethod paymentMethod)
        {
            float priceUSD = GetPriceForPlatform(platform);

            Debug.Log($"[PlatformDistribution] Initiating purchase: {platform}, ${priceUSD}, {paymentMethod}");

            try
            {
                // Process payment
                var paymentResult = await paymentGateway.ProcessPayment(priceUSD, paymentMethod);

                if (paymentResult.success)
                {
                    // Generate download key
                    string downloadKey = GenerateDownloadKey(platform);

                    // Add fees to stability pool
                    await stabilityEngine.AddFees(paymentResult.feesCollected);

                    Debug.Log($"[PlatformDistribution] Purchase successful: {downloadKey}");

                    return new PurchaseResult
                    {
                        success = true,
                        downloadKey = downloadKey,
                        platform = platform,
                        transactionHash = paymentResult.transactionHash
                    };
                }
                else
                {
                    Debug.LogError($"[PlatformDistribution] Payment failed: {paymentResult.errorMessage}");

                    return new PurchaseResult
                    {
                        success = false,
                        errorMessage = paymentResult.errorMessage
                    };
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlatformDistribution] Purchase error: {e.Message}");

                return new PurchaseResult
                {
                    success = false,
                    errorMessage = e.Message
                };
            }
        }

        /// <summary>
        /// Get price for specific platform.
        /// </summary>
        private float GetPriceForPlatform(Platform platform)
        {
            switch (platform)
            {
                case Platform.PC: return basePriceUSD;
                case Platform.PlayStation: return playstationPriceUSD;
                case Platform.Xbox: return xboxPriceUSD;
                case Platform.Android: return androidPriceUSD;
                default: return basePriceUSD;
            }
        }

        /// <summary>
        /// Generate unique download key for platform.
        /// </summary>
        private string GenerateDownloadKey(Platform platform)
        {
            string prefix = platform.ToString().Substring(0, 2).ToUpper();
            string guid = Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();
            return $"{prefix}-{guid.Substring(0, 4)}-{guid.Substring(4, 4)}-{guid.Substring(8, 4)}-{guid.Substring(12, 4)}";
        }

        /// <summary>
        /// Verify download key for specific platform.
        /// </summary>
        public async Task<bool> VerifyDownloadKey(string downloadKey, Platform platform)
        {
            // Stub: Verify with backend server
            await Task.Delay(500);

            string prefix = platform.ToString().Substring(0, 2).ToUpper();
            bool valid = downloadKey.StartsWith(prefix);

            Debug.Log($"[PlatformDistribution] Download key verification: {downloadKey} → {valid}");

            return valid;
        }
    }

    /// <summary>
    /// Platform enumeration.
    /// </summary>
    public enum Platform
    {
        PC,
        PlayStation,
        Xbox,
        Android
    }

    /// <summary>
    /// Platform information data structure.
    /// </summary>
    [Serializable]
    public class PlatformInfo
    {
        public Platform platform;
        public string name;
        public string downloadUrl;
        public float priceUSD;
        public string requirements;
        public string icon;
    }

    /// <summary>
    /// Purchase result data structure.
    /// </summary>
    [Serializable]
    public class PurchaseResult
    {
        public bool success;
        public string downloadKey;
        public Platform platform;
        public string transactionHash;
        public string errorMessage;
    }
}
