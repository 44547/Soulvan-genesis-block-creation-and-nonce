using UnityEngine;
using UnityEngine.VFX;

namespace Soulvan.Wallet
{
    /// <summary>
    /// Renders a deterministic avatar based on wallet address.
    /// Uses hash seeding for consistent visual identity.
    /// Integrates with VFX Graph, particle systems, and motif overlays.
    /// Updates visual identity based on wallet and tier (gray → cyan → magenta → yellow → white).
    /// </summary>
    public class AvatarRenderer : MonoBehaviour
    {
        [Header("Rendering")]
        [SerializeField] private GameObject avatarPrefab;
        [SerializeField] private Transform avatarRoot;
        [SerializeField] private Material avatarMaterial;

        [Header("Motif Mapping")]
        [SerializeField] private Gradient stormGradient;
        [SerializeField] private Gradient calmGradient;
        [SerializeField] private Gradient cosmicGradient;
        [SerializeField] private Gradient oracleGradient;

        [Header("Milestone Thresholds")]
        [SerializeField] private int stormThreshold = 10; // 10 races won
        [SerializeField] private int calmThreshold = 5; // 5 missions completed
        [SerializeField] private int cosmicThreshold = 1; // 1 boss defeated
        [SerializeField] private int oracleThreshold = 1; // 1 vote cast

        private string currentAddress;
        private float currentIntensity;

        public void RenderForAddress(string address, float intensity01)
        {
            currentAddress = address;
            currentIntensity = Mathf.Clamp01(intensity01);

            // Generate deterministic seed from address
            int seed = address.GetHashCode();
            Random.InitState(seed);

            // Select motif based on milestones (stub: would query blockchain)
            Motif motif = DetermineMotif(address);

            // Apply visual effects
            ApplyAvatarVisuals(motif, intensity01);
            ApplyRunePattern(seed);
            ApplyLighting(motif, intensity01);

            Debug.Log($"[AvatarRenderer] Rendered avatar for {address}, motif: {motif}, intensity: {intensity01:F2}");
        }

        private Motif DetermineMotif(string address)
        {
            // Stub: Query blockchain for milestone data
            // Real implementation would check:
            // - Races won (Storm)
            // - Missions completed (Calm)
            // - Bosses defeated (Cosmic)
            // - Votes cast (Oracle)

            // For now, use deterministic random based on address
            int seed = address.GetHashCode();
            Random.InitState(seed);
            float rand = Random.value;

            if (rand < 0.25f) return Motif.Storm;
            if (rand < 0.5f) return Motif.Calm;
            if (rand < 0.75f) return Motif.Cosmic;
            return Motif.Oracle;
        }

        private void ApplyAvatarVisuals(Motif motif, float intensity)
        {
            if (avatarVFX == null) return;

            // Set VFX Graph parameters
            avatarVFX.SetFloat("Intensity", intensity);
            avatarVFX.SetFloat("MotifIndex", (float)motif);

            // Set color gradient based on motif
            Gradient gradient = GetGradientForMotif(motif);
            Color color = gradient.Evaluate(intensity);
            
            avatarVFX.SetVector4("MotifColor", color);
            
            // Set particle emission rate
            float emissionRate = Mathf.Lerp(10f, 100f, intensity);
            avatarVFX.SetFloat("EmissionRate", emissionRate);
        }

        private void ApplyRunePattern(int seed)
        {
            if (runeParticles == null) return;

            // Generate deterministic rune pattern
            Random.InitState(seed);
            
            var main = runeParticles.main;
            main.startColor = new Color(
                Random.value,
                Random.value,
                Random.value,
                1f
            );

            var emission = runeParticles.emission;
            emission.rateOverTime = Mathf.Lerp(5f, 20f, currentIntensity);
        }

        private void ApplyLighting(Motif motif, float intensity)
        {
            if (avatarLight == null) return;

            Gradient gradient = GetGradientForMotif(motif);
            avatarLight.color = gradient.Evaluate(intensity);
            avatarLight.intensity = Mathf.Lerp(0.5f, 2f, intensity);
        }

        private Gradient GetGradientForMotif(Motif motif)
        {
            switch (motif)
            {
                case Motif.Storm: return stormGradient;
                case Motif.Calm: return calmGradient;
                case Motif.Cosmic: return cosmicGradient;
                case Motif.Oracle: return oracleGradient;
                default: return calmGradient;
            }
        }

        public void UpdateMilestone(string milestoneType, int count)
        {
            Debug.Log($"[AvatarRenderer] Milestone updated: {milestoneType} = {count}");
            
            // Trigger milestone celebration effect
            if (runeParticles != null)
            {
                runeParticles.Emit(50);
            }
        }

        /// <summary>
        /// Update avatar material color based on tier progression.
        /// Tier 1 (Street Racer): Gray
        /// Tier 2 (Mission Runner): Cyan
        /// Tier 3 (Arc Champion): Magenta
        /// Tier 4 (DAO Hero): Yellow
        /// Tier 5 (Mythic Legend): White
        /// </summary>
        public void UpdateForTier(int tier)
        {
            if (avatarMaterial == null)
            {
                Debug.LogWarning("[AvatarRenderer] Avatar material not assigned");
                return;
            }

            Color glowColor;
            switch (tier)
            {
                case 1: glowColor = Color.gray; break;
                case 2: glowColor = Color.cyan; break;
                case 3: glowColor = Color.magenta; break;
                case 4: glowColor = Color.yellow; break;
                case 5: glowColor = Color.white; break;
                default: glowColor = Color.gray; break;
            }

            avatarMaterial.SetColor("_GlowColor", glowColor);
            
            // Update emission intensity for higher tiers
            float emissionIntensity = tier * 0.2f; // 0.2 to 1.0
            if (avatarMaterial.HasProperty("_EmissionIntensity"))
            {
                avatarMaterial.SetFloat("_EmissionIntensity", emissionIntensity);
            }

            // Update particle effects for tier
            if (haloParticles != null)
            {
                var main = haloParticles.main;
                main.startColor = glowColor;
            }

            Debug.Log($"[AvatarRenderer] Avatar updated for Tier {tier}: {glowColor}");
        }

        /// <summary>
        /// Trigger Oracle flare effect on DAO vote.
        /// Creates a 2-second visual surge with violet/cyan aura.
        /// </summary>
        public void TriggerOracleFlare()
        {
            if (avatarMaterial == null)
            {
                Debug.LogWarning("[AvatarRenderer] Avatar material not assigned");
                return;
            }

            if (avatarMaterial.HasProperty("_FlareIntensity"))
            {
                avatarMaterial.SetFloat("_FlareIntensity", 1f);
            }
            
            // Apply Oracle motif colors temporarily
            if (avatarMaterial.HasProperty("_FlareColor"))
            {
                avatarMaterial.SetColor("_FlareColor", new Color(0.5f, 0f, 1f)); // Violet
            }
            
            // Emit Oracle particles
            if (runeParticles != null)
            {
                var main = runeParticles.main;
                main.startColor = new Color(0.5f, 0f, 1f, 1f); // Violet
                runeParticles.Emit(100);
            }

            // Reset after 2 seconds
            Invoke(nameof(ResetFlare), 2f);

            Debug.Log("[AvatarRenderer] Oracle flare triggered");
        }

        private void ResetFlare()
        {
            if (avatarMaterial != null && avatarMaterial.HasProperty("_FlareIntensity"))
            {
                avatarMaterial.SetFloat("_FlareIntensity", 0f);
            }
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(currentAddress)) return;

            GUILayout.BeginArea(new Rect(10, 440, 300, 80));
            GUILayout.Label($"Avatar Address: {currentAddress.Substring(0, 10)}...");
            GUILayout.Label($"Intensity: {currentIntensity:F2}");
            GUILayout.EndArea();
        }
    }

    // Stub classes for UI panels
    public class WalletHUD : MonoBehaviour
    {
        public void UpdateBalances(BalanceState balances)
        {
            Debug.Log($"[WalletHUD] SVN: {balances.soulvanCoin}, NFTs: {balances.nftCount}, Voting Power: {balances.votingPower}");
        }

        public void ShowLocked()
        {
            Debug.Log("[WalletHUD] Wallet locked");
        }
    }

    public class GovernancePanel : MonoBehaviour
    {
        public void DisplayProposals(Proposal[] proposals)
        {
            Debug.Log($"[GovernancePanel] Displaying {proposals.Length} proposals");
        }
    }

    public class AssetPanel : MonoBehaviour
    {
        public void DisplayAssets(Nft[] nfts)
        {
            Debug.Log($"[AssetPanel] Displaying {nfts.Length} NFTs");
        }
    }

    public class OffChainCache
    {
        public void AddPendingReward(string rewardId, string metadataUri)
        {
            Debug.Log($"[OffChainCache] Cached reward: {rewardId}");
        }

        public void RemovePendingReward(string rewardId)
        {
            Debug.Log($"[OffChainCache] Removed cached reward: {rewardId}");
        }
    }

    public class ReconcilerService
    {
        private ISoulvanWallet wallet;
        private OffChainCache cache;

        public ReconcilerService(ISoulvanWallet wallet, OffChainCache cache)
        {
            this.wallet = wallet;
            this.cache = cache;
        }

        public async System.Threading.Tasks.Task ReconcileAsync()
        {
            // Stub: Reconcile off-chain cache with on-chain state
            await System.Threading.Tasks.Task.Delay(100);
            Debug.Log("[ReconcilerService] Reconciled off-chain state");
        }
    }
}
