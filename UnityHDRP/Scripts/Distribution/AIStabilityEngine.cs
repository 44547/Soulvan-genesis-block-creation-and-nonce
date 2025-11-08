using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Soulvan.Distribution
{
    /// <summary>
    /// AI-driven stability engine for Soulvan Coin.
    /// Uses collected fees to perform self-buying to maintain and increase SVN value.
    /// Implements algorithmic market making and price stabilization.
    /// </summary>
    public class AIStabilityEngine : MonoBehaviour
    {
        [Header("Stability Pool")]
        [SerializeField] private float stabilityPoolSVN = 0f; // Accumulated fees in SVN
        [SerializeField] private float stabilityPoolUSD = 0f; // USD reserves
        [SerializeField] private float targetPriceSVN = 0.50f; // Target $0.50 per SVN

        [Header("AI Configuration")]
        [SerializeField] private float buyThresholdPercent = 5f; // Buy when price drops 5% below target
        [SerializeField] private float sellThresholdPercent = 10f; // Sell when price rises 10% above target
        [SerializeField] private float maxBuyPercentage = 25f; // Use max 25% of pool per buy
        [SerializeField] private float minReservePercent = 20f; // Keep 20% reserve always

        [Header("Market Monitoring")]
        [SerializeField] private float currentPriceSVN = 0.50f;
        [SerializeField] private float priceChangePercent24h = 0f;
        [SerializeField] private float marketCapUSD = 500000000f; // $500M market cap

        [Header("AI Stats")]
        public int totalBuys = 0;
        public int totalSells = 0;
        public float totalVolumeBought = 0f;
        public float totalVolumeSold = 0f;
        public float profitGenerated = 0f;

        private bool isRunning = false;

        private void Start()
        {
            // Start AI monitoring loop
            StartAIStabilityLoop();
        }

        /// <summary>
        /// Add fees collected from payments to stability pool.
        /// Fees are used by AI to buy SVN and stabilize price.
        /// </summary>
        public async Task AddFees(float feesSVN)
        {
            stabilityPoolSVN += feesSVN;

            Debug.Log($"[AIStabilityEngine] Fees added: {feesSVN} SVN (Total pool: {stabilityPoolSVN} SVN)");

            // Trigger immediate stability check
            await CheckAndExecuteStabilityAction();
        }

        /// <summary>
        /// Start AI stability monitoring loop.
        /// Runs every 5 minutes to check market conditions.
        /// </summary>
        private async void StartAIStabilityLoop()
        {
            if (isRunning) return;

            isRunning = true;

            Debug.Log("[AIStabilityEngine] AI stability loop started");

            while (isRunning)
            {
                await UpdateMarketData();
                await CheckAndExecuteStabilityAction();
                await Task.Delay(300000); // 5 minutes
            }
        }

        /// <summary>
        /// Update market data from oracle (price, volume, market cap).
        /// </summary>
        private async Task UpdateMarketData()
        {
            try
            {
                // Stub: Fetch from Chainlink oracle or DEX API
                await Task.Delay(500);

                // Simulate market data
                float previousPrice = currentPriceSVN;
                currentPriceSVN = UnityEngine.Random.Range(0.45f, 0.55f);
                priceChangePercent24h = ((currentPriceSVN - previousPrice) / previousPrice) * 100f;

                Debug.Log($"[AIStabilityEngine] Market data updated: SVN=${currentPriceSVN} ({priceChangePercent24h:F2}% 24h)");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AIStabilityEngine] Market data update failed: {e.Message}");
            }
        }

        /// <summary>
        /// Check market conditions and execute buy/sell actions.
        /// AI decision-making based on price deviation from target.
        /// </summary>
        private async Task CheckAndExecuteStabilityAction()
        {
            float priceDeviationPercent = ((currentPriceSVN - targetPriceSVN) / targetPriceSVN) * 100f;

            Debug.Log($"[AIStabilityEngine] Price deviation: {priceDeviationPercent:F2}% (Target: ${targetPriceSVN})");

            // Price is below target - BUY to support price
            if (priceDeviationPercent < -buyThresholdPercent)
            {
                await ExecuteBuy(priceDeviationPercent);
            }
            // Price is above target - SELL to take profits
            else if (priceDeviationPercent > sellThresholdPercent)
            {
                await ExecuteSell(priceDeviationPercent);
            }
            else
            {
                Debug.Log($"[AIStabilityEngine] Price stable, no action needed");
            }
        }

        /// <summary>
        /// Execute buy order to support SVN price.
        /// Uses fees from stability pool to buy SVN from market.
        /// </summary>
        private async Task ExecuteBuy(float priceDeviationPercent)
        {
            // Calculate buy amount based on price deviation
            float buyIntensity = Mathf.Clamp01(Mathf.Abs(priceDeviationPercent) / 10f); // 0-1 intensity
            float buyAmount = stabilityPoolSVN * (maxBuyPercentage / 100f) * buyIntensity;

            // Ensure we keep minimum reserve
            float availableFunds = stabilityPoolSVN * (1f - minReservePercent / 100f);
            buyAmount = Mathf.Min(buyAmount, availableFunds);

            if (buyAmount < 1f)
            {
                Debug.Log($"[AIStabilityEngine] Insufficient funds for buy (Pool: {stabilityPoolSVN} SVN)");
                return;
            }

            Debug.Log($"[AIStabilityEngine] Executing BUY: {buyAmount} SVN at ${currentPriceSVN}");

            try
            {
                // Stub: Execute buy order on DEX (Uniswap, PancakeSwap, etc.)
                await Task.Delay(2000);

                // Deduct from pool
                stabilityPoolSVN -= buyAmount;

                // Add to USD reserves (simulate buying SVN increases our SVN holdings)
                float usdSpent = buyAmount * currentPriceSVN;
                stabilityPoolUSD += usdSpent;

                // Update stats
                totalBuys++;
                totalVolumeBought += buyAmount;

                // Simulate price increase from buy pressure (0.5-2% increase)
                float priceIncrease = UnityEngine.Random.Range(0.005f, 0.02f);
                currentPriceSVN += currentPriceSVN * priceIncrease;

                Debug.Log($"[AIStabilityEngine] BUY executed: {buyAmount} SVN, New price: ${currentPriceSVN}");
                Debug.Log($"[AIStabilityEngine] Pool remaining: {stabilityPoolSVN} SVN, ${stabilityPoolUSD} USD");

                // Emit buy event
                EventBus.EmitStabilityBuy(buyAmount, currentPriceSVN);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AIStabilityEngine] Buy execution failed: {e.Message}");
            }
        }

        /// <summary>
        /// Execute sell order to take profits when price is high.
        /// Sells SVN to accumulate fees for future buying.
        /// </summary>
        private async Task ExecuteSell(float priceDeviationPercent)
        {
            // Calculate sell amount based on price deviation
            float sellIntensity = Mathf.Clamp01(priceDeviationPercent / 20f); // 0-1 intensity
            float sellAmount = stabilityPoolUSD / currentPriceSVN * 0.5f * sellIntensity; // Sell up to 50% of holdings

            if (sellAmount < 1f)
            {
                Debug.Log($"[AIStabilityEngine] Insufficient holdings for sell");
                return;
            }

            Debug.Log($"[AIStabilityEngine] Executing SELL: {sellAmount} SVN at ${currentPriceSVN}");

            try
            {
                // Stub: Execute sell order on DEX
                await Task.Delay(2000);

                // Add proceeds to stability pool
                float usdReceived = sellAmount * currentPriceSVN;
                stabilityPoolSVN += usdReceived / currentPriceSVN;
                stabilityPoolUSD -= usdReceived;

                // Update stats
                totalSells++;
                totalVolumeSold += sellAmount;

                // Calculate profit (difference between buy and sell price)
                float profit = (currentPriceSVN - targetPriceSVN) * sellAmount;
                profitGenerated += profit;

                // Simulate price decrease from sell pressure (0.5-2% decrease)
                float priceDecrease = UnityEngine.Random.Range(0.005f, 0.02f);
                currentPriceSVN -= currentPriceSVN * priceDecrease;

                Debug.Log($"[AIStabilityEngine] SELL executed: {sellAmount} SVN, Profit: ${profit:F2}");
                Debug.Log($"[AIStabilityEngine] Pool balance: {stabilityPoolSVN} SVN, ${stabilityPoolUSD} USD");

                // Emit sell event
                EventBus.EmitStabilitySell(sellAmount, currentPriceSVN);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AIStabilityEngine] Sell execution failed: {e.Message}");
            }
        }

        /// <summary>
        /// Get current stability pool status.
        /// </summary>
        public StabilityPoolStatus GetPoolStatus()
        {
            return new StabilityPoolStatus
            {
                svnBalance = stabilityPoolSVN,
                usdBalance = stabilityPoolUSD,
                currentPrice = currentPriceSVN,
                targetPrice = targetPriceSVN,
                priceDeviation = ((currentPriceSVN - targetPriceSVN) / targetPriceSVN) * 100f,
                totalBuys = totalBuys,
                totalSells = totalSells,
                profitGenerated = profitGenerated
            };
        }

        /// <summary>
        /// Stop AI stability loop.
        /// </summary>
        private void OnDestroy()
        {
            isRunning = false;
        }
    }

    /// <summary>
    /// Stability pool status data structure.
    /// </summary>
    [Serializable]
    public class StabilityPoolStatus
    {
        public float svnBalance;
        public float usdBalance;
        public float currentPrice;
        public float targetPrice;
        public float priceDeviation;
        public int totalBuys;
        public int totalSells;
        public float profitGenerated;
    }

    /// <summary>
    /// Extended EventBus with stability events.
    /// </summary>
    public static partial class EventBus
    {
        public static event Action<float, float> OnStabilityBuy; // amount, price
        public static event Action<float, float> OnStabilitySell; // amount, price

        public static void EmitStabilityBuy(float amount, float price)
        {
            OnStabilityBuy?.Invoke(amount, price);
            Debug.Log($"[EventBus] Stability buy emitted: {amount} SVN @ ${price}");
        }

        public static void EmitStabilitySell(float amount, float price)
        {
            OnStabilitySell?.Invoke(amount, price);
            Debug.Log($"[EventBus] Stability sell emitted: {amount} SVN @ ${price}");
        }
    }
}
