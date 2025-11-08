using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Soulvan.Distribution;

namespace Soulvan.UI
{
    /// <summary>
    /// Download button UI for all platforms (PC, PlayStation, Xbox, Android).
    /// Displays platform availability, pricing, and handles purchase flow.
    /// </summary>
    public class DownloadButtonUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject downloadPanel;
        [SerializeField] private Transform platformButtonContainer;
        [SerializeField] private GameObject platformButtonPrefab;

        [Header("Payment Panel")]
        [SerializeField] private GameObject paymentPanel;
        [SerializeField] private TMP_Text platformNameText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Dropdown paymentMethodDropdown;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button cancelButton;

        [Header("Download Key Panel")]
        [SerializeField] private GameObject downloadKeyPanel;
        [SerializeField] private TMP_Text downloadKeyText;
        [SerializeField] private TMP_Text downloadUrlText;
        [SerializeField] private Button copyKeyButton;
        [SerializeField] private Button downloadButton;

        [Header("Services")]
        [SerializeField] private PlatformDistribution platformDistribution;
        [SerializeField] private SoulvanPaymentGateway paymentGateway;

        private Platform selectedPlatform;
        private float selectedPrice;

        private void Start()
        {
            InitializeUI();
            LoadPlatforms();
        }

        /// <summary>
        /// Initialize UI components.
        /// </summary>
        private void InitializeUI()
        {
            if (platformDistribution == null)
            {
                platformDistribution = FindObjectOfType<PlatformDistribution>();
            }

            if (paymentGateway == null)
            {
                paymentGateway = FindObjectOfType<SoulvanPaymentGateway>();
            }

            // Setup button listeners
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelClicked);
            }

            if (copyKeyButton != null)
            {
                copyKeyButton.onClick.AddListener(OnCopyKeyClicked);
            }

            if (downloadButton != null)
            {
                downloadButton.onClick.AddListener(OnDownloadClicked);
            }

            // Hide panels initially
            if (downloadPanel != null) downloadPanel.SetActive(true);
            if (paymentPanel != null) paymentPanel.SetActive(false);
            if (downloadKeyPanel != null) downloadKeyPanel.SetActive(false);

            Debug.Log("[DownloadButtonUI] Initialized");
        }

        /// <summary>
        /// Load all available platforms and create download buttons.
        /// </summary>
        private void LoadPlatforms()
        {
            if (platformDistribution == null)
            {
                Debug.LogError("[DownloadButtonUI] PlatformDistribution not found");
                return;
            }

            var platforms = platformDistribution.GetAvailablePlatforms();

            Debug.Log($"[DownloadButtonUI] Loading {platforms.Count} platforms");

            foreach (var platformInfo in platforms)
            {
                CreatePlatformButton(platformInfo);
            }
        }

        /// <summary>
        /// Create download button for a platform.
        /// </summary>
        private void CreatePlatformButton(PlatformInfo platformInfo)
        {
            if (platformButtonPrefab == null || platformButtonContainer == null)
            {
                Debug.LogError("[DownloadButtonUI] Button prefab or container not set");
                return;
            }

            GameObject buttonObj = Instantiate(platformButtonPrefab, platformButtonContainer);

            // Set platform name
            TMP_Text nameText = buttonObj.transform.Find("PlatformName")?.GetComponent<TMP_Text>();
            if (nameText != null)
            {
                nameText.text = platformInfo.platformName;
            }

            // Set price
            TMP_Text priceText = buttonObj.transform.Find("PriceText")?.GetComponent<TMP_Text>();
            if (priceText != null)
            {
                priceText.text = $"${platformInfo.priceUSD:F2}";
            }

            // Set platform icon (stub)
            Image iconImage = buttonObj.transform.Find("PlatformIcon")?.GetComponent<Image>();
            if (iconImage != null && !string.IsNullOrEmpty(platformInfo.iconUrl))
            {
                // Load icon texture from URL (stub)
                // iconImage.sprite = LoadIconSprite(platformInfo.iconUrl);
            }

            // Set requirements
            TMP_Text requirementsText = buttonObj.transform.Find("Requirements")?.GetComponent<TMP_Text>();
            if (requirementsText != null)
            {
                requirementsText.text = platformInfo.requirements;
            }

            // Setup button click
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnPlatformSelected(platformInfo));
            }

            Debug.Log($"[DownloadButtonUI] Created button for {platformInfo.platformName}");
        }

        /// <summary>
        /// Handle platform selection.
        /// </summary>
        private void OnPlatformSelected(PlatformInfo platformInfo)
        {
            Debug.Log($"[DownloadButtonUI] Platform selected: {platformInfo.platformName}");

            selectedPlatform = platformInfo.platform;
            selectedPrice = platformInfo.priceUSD;

            // Show payment panel
            ShowPaymentPanel(platformInfo);
        }

        /// <summary>
        /// Show payment panel with selected platform info.
        /// </summary>
        private void ShowPaymentPanel(PlatformInfo platformInfo)
        {
            if (paymentPanel == null) return;

            paymentPanel.SetActive(true);

            if (platformNameText != null)
            {
                platformNameText.text = $"Purchase Soulvan - {platformInfo.platformName}";
            }

            if (priceText != null)
            {
                priceText.text = $"${platformInfo.priceUSD:F2} USD";
            }

            // Populate payment method dropdown
            if (paymentMethodDropdown != null)
            {
                paymentMethodDropdown.ClearOptions();
                paymentMethodDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "Soulvan Coin (SVN)",
                    "Bitcoin (BTC)",
                    "Credit Card",
                    "PayPal"
                });
            }

            Debug.Log($"[DownloadButtonUI] Payment panel shown for {platformInfo.platformName}");
        }

        /// <summary>
        /// Handle purchase button click.
        /// </summary>
        private async void OnPurchaseClicked()
        {
            if (paymentGateway == null)
            {
                Debug.LogError("[DownloadButtonUI] PaymentGateway not found");
                return;
            }

            // Get selected payment method
            PaymentMethod paymentMethod = PaymentMethod.SoulvanCoin;
            if (paymentMethodDropdown != null)
            {
                paymentMethod = paymentMethodDropdown.value switch
                {
                    0 => PaymentMethod.SoulvanCoin,
                    1 => PaymentMethod.Bitcoin,
                    2 => PaymentMethod.CreditCard,
                    3 => PaymentMethod.PayPal,
                    _ => PaymentMethod.SoulvanCoin
                };
            }

            Debug.Log($"[DownloadButtonUI] Processing payment: {selectedPrice} USD via {paymentMethod}");

            // Disable button during processing
            if (purchaseButton != null)
            {
                purchaseButton.interactable = false;
                purchaseButton.GetComponentInChildren<TMP_Text>().text = "Processing...";
            }

            try
            {
                // Process payment
                var result = await platformDistribution.PurchaseGame(selectedPlatform, paymentMethod);

                if (result.success)
                {
                    Debug.Log($"[DownloadButtonUI] Purchase successful: {result.downloadKey}");

                    // Hide payment panel
                    if (paymentPanel != null) paymentPanel.SetActive(false);

                    // Show download key panel
                    ShowDownloadKeyPanel(result);
                }
                else
                {
                    Debug.LogError($"[DownloadButtonUI] Purchase failed: {result.error}");
                    ShowErrorMessage($"Purchase failed: {result.error}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DownloadButtonUI] Payment error: {e.Message}");
                ShowErrorMessage($"Payment error: {e.Message}");
            }
            finally
            {
                // Re-enable button
                if (purchaseButton != null)
                {
                    purchaseButton.interactable = true;
                    purchaseButton.GetComponentInChildren<TMP_Text>().text = "Purchase";
                }
            }
        }

        /// <summary>
        /// Show download key panel after successful purchase.
        /// </summary>
        private void ShowDownloadKeyPanel(PurchaseResult result)
        {
            if (downloadKeyPanel == null) return;

            downloadKeyPanel.SetActive(true);

            if (downloadKeyText != null)
            {
                downloadKeyText.text = $"Download Key: {result.downloadKey}";
            }

            if (downloadUrlText != null)
            {
                downloadUrlText.text = $"Download URL: {result.downloadUrl}";
            }

            Debug.Log($"[DownloadButtonUI] Download key displayed: {result.downloadKey}");
        }

        /// <summary>
        /// Handle copy key button click.
        /// </summary>
        private void OnCopyKeyClicked()
        {
            if (downloadKeyText != null)
            {
                string key = downloadKeyText.text.Replace("Download Key: ", "");
                GUIUtility.systemCopyBuffer = key;
                Debug.Log($"[DownloadButtonUI] Key copied to clipboard: {key}");
                ShowSuccessMessage("Download key copied to clipboard!");
            }
        }

        /// <summary>
        /// Handle download button click.
        /// </summary>
        private void OnDownloadClicked()
        {
            if (downloadUrlText != null)
            {
                string url = downloadUrlText.text.Replace("Download URL: ", "");
                Application.OpenURL(url);
                Debug.Log($"[DownloadButtonUI] Opening download URL: {url}");
            }
        }

        /// <summary>
        /// Handle cancel button click.
        /// </summary>
        private void OnCancelClicked()
        {
            if (paymentPanel != null)
            {
                paymentPanel.SetActive(false);
            }

            Debug.Log("[DownloadButtonUI] Payment cancelled");
        }

        /// <summary>
        /// Show error message to user.
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            // Stub: Display error popup/toast
            Debug.LogError($"[DownloadButtonUI] Error: {message}");
        }

        /// <summary>
        /// Show success message to user.
        /// </summary>
        private void ShowSuccessMessage(string message)
        {
            // Stub: Display success popup/toast
            Debug.Log($"[DownloadButtonUI] Success: {message}");
        }
    }

    /// <summary>
    /// Platform button prefab structure (for reference).
    /// Prefab should contain:
    /// - PlatformIcon (Image)
    /// - PlatformName (TextMeshProUGUI)
    /// - PriceText (TextMeshProUGUI)
    /// - Requirements (TextMeshProUGUI)
    /// - Button component
    /// </summary>
    public class PlatformButtonPrefabStructure
    {
        // This is just a reference class showing the expected prefab structure
        // Actual prefab should be created in Unity Editor with these components:
        
        // GameObject: PlatformButton
        //   ├─ Image: Background
        //   ├─ Image: PlatformIcon
        //   ├─ TextMeshProUGUI: PlatformName
        //   ├─ TextMeshProUGUI: PriceText
        //   ├─ TextMeshProUGUI: Requirements
        //   └─ Button component
    }
}
