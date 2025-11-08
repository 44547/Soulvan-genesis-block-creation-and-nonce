using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Soulvan.Marketing
{
    /// <summary>
    /// AI-powered marketing agent for Soulvan game and blockchain.
    /// Handles social media campaigns, influencer outreach, community engagement,
    /// content generation, and promotional activities across multiple platforms.
    /// </summary>
    public class AIMarketingAgent : MonoBehaviour
    {
        [Header("Campaign Configuration")]
        [SerializeField] private bool autoMarketingEnabled = true;
        [SerializeField] private float campaignBudgetUSD = 50000f;
        [SerializeField] private float dailyBudgetUSD = 500f;

        [Header("Social Media Platforms")]
        [SerializeField] private bool twitterEnabled = true;
        [SerializeField] private bool discordEnabled = true;
        [SerializeField] private bool redditEnabled = true;
        [SerializeField] private bool youtubeEnabled = true;
        [SerializeField] private bool tiktokEnabled = true;
        [SerializeField] private bool instagramEnabled = true;

        [Header("Content Strategy")]
        [SerializeField] private ContentType[] contentTypes;
        [SerializeField] private int postsPerDay = 5;
        [SerializeField] private string[] targetHashtags;

        [Header("Influencer Marketing")]
        [SerializeField] private List<InfluencerProfile> activeInfluencers = new List<InfluencerProfile>();
        [SerializeField] private float influencerBudget = 20000f;

        [Header("Campaign Stats")]
        [SerializeField] private int totalPosts = 0;
        [SerializeField] private int totalEngagements = 0;
        [SerializeField] private int totalReach = 0;
        [SerializeField] private float totalSpent = 0f;
        [SerializeField] private int newPlayers = 0;
        [SerializeField] private float totalRevenueGenerated = 0f; // Revenue from acquired players

        [Header("Stability Engine Integration")]
        [SerializeField] private AIStabilityEngine stabilityEngine;
        [SerializeField] private float marketingFeesCollected = 0f; // Fees collected from marketing-acquired players

        private Queue<MarketingCampaign> campaignQueue = new Queue<MarketingCampaign>();
        private Dictionary<string, SocialMediaAPI> platformAPIs = new Dictionary<string, SocialMediaAPI>();

        private void Start()
        {
            InitializeMarketingAgent();
            StartAutomatedMarketing();
        }

        /// <summary>
        /// Initialize AI marketing agent with default campaigns.
        /// </summary>
        private void InitializeMarketingAgent()
        {
            Debug.Log("[AIMarketingAgent] Initializing marketing automation");

            // Initialize stability engine connection
            if (stabilityEngine == null)
            {
                stabilityEngine = FindObjectOfType<AIStabilityEngine>();
            }

            // Initialize platform APIs (stubs for now)
            if (twitterEnabled) platformAPIs["Twitter"] = new TwitterAPI();
            if (discordEnabled) platformAPIs["Discord"] = new DiscordAPI();
            if (redditEnabled) platformAPIs["Reddit"] = new RedditAPI();
            if (youtubeEnabled) platformAPIs["YouTube"] = new YouTubeAPI();
            if (tiktokEnabled) platformAPIs["TikTok"] = new TikTokAPI();
            if (instagramEnabled) platformAPIs["Instagram"] = new InstagramAPI();

            // Set default hashtags
            if (targetHashtags == null || targetHashtags.Length == 0)
            {
                targetHashtags = new[]
                {
                    "#Soulvan", "#Web3Gaming", "#PlayToEarn", "#NFTGaming",
                    "#CryptoGaming", "#BlockchainGames", "#GameFi", "#MetaverseGaming",
                    "#SoulvanCoin", "#SVN", "#HypercarRacing", "#RTXGaming"
                };
            }

            // Load default campaigns
            LoadDefaultCampaigns();

            Debug.Log($"[AIMarketingAgent] Initialized with {platformAPIs.Count} platforms");
        }

        /// <summary>
        /// Load default marketing campaigns.
        /// </summary>
        private void LoadDefaultCampaigns()
        {
            campaignQueue.Enqueue(new MarketingCampaign
            {
                id = "launch_campaign",
                name = "Soulvan Launch Campaign",
                description = "Multi-platform launch campaign for Soulvan",
                budget = 10000f,
                duration = 30, // days
                targetPlatforms = new[] { "Twitter", "Discord", "Reddit", "YouTube" },
                contentTypes = new[] { ContentType.Trailer, ContentType.Screenshot, ContentType.Community },
                status = CampaignStatus.Active
            });

            campaignQueue.Enqueue(new MarketingCampaign
            {
                id = "influencer_campaign",
                name = "Gaming Influencer Partnership",
                description = "Partner with top gaming influencers",
                budget = 20000f,
                duration = 60,
                targetPlatforms = new[] { "YouTube", "Twitch", "TikTok" },
                contentTypes = new[] { ContentType.InfluencerReview, ContentType.Gameplay },
                status = CampaignStatus.Planned
            });

            campaignQueue.Enqueue(new MarketingCampaign
            {
                id = "blockchain_campaign",
                name = "Web3 Community Engagement",
                description = "Engage crypto/blockchain communities",
                budget = 5000f,
                duration = 90,
                targetPlatforms = new[] { "Twitter", "Reddit", "Discord" },
                contentTypes = new[] { ContentType.Educational, ContentType.Community, ContentType.AMA },
                status = CampaignStatus.Active
            });
        }

        /// <summary>
        /// Start automated marketing loop.
        /// Runs continuously to post content, engage community, and monitor campaigns.
        /// </summary>
        private async void StartAutomatedMarketing()
        {
            if (!autoMarketingEnabled) return;

            Debug.Log("[AIMarketingAgent] Starting automated marketing");

            while (autoMarketingEnabled)
            {
                // Generate and post content
                await GenerateDailyContent();

                // Engage with community
                await EngageWithCommunity();

                // Monitor campaign performance
                await MonitorCampaigns();

                // Reach out to influencers
                await InfluencerOutreach();

                // Wait 4 hours before next cycle
                await Task.Delay(14400000);
            }
        }

        #region Content Generation

        /// <summary>
        /// Generate and post daily marketing content.
        /// AI creates game highlights, community updates, and promotional posts.
        /// </summary>
        private async Task GenerateDailyContent()
        {
            Debug.Log("[AIMarketingAgent] Generating daily content");

            for (int i = 0; i < postsPerDay; i++)
            {
                var contentType = GetNextContentType();
                var content = await GenerateContent(contentType);

                if (content != null)
                {
                    await PostToSocialMedia(content);
                    totalPosts++;
                }

                // Delay between posts (2-4 hours)
                await Task.Delay(UnityEngine.Random.Range(7200000, 14400000));
            }
        }

        /// <summary>
        /// Generate marketing content using AI.
        /// </summary>
        private async Task<MarketingContent> GenerateContent(ContentType type)
        {
            Debug.Log($"[AIMarketingAgent] Generating {type} content");

            await Task.Delay(1000); // Simulate AI generation

            var content = new MarketingContent
            {
                type = type,
                timestamp = DateTime.Now
            };

            switch (type)
            {
                case ContentType.Trailer:
                    content.text = GenerateTrailerPost();
                    content.mediaUrl = "https://cdn.soulvan.com/trailers/latest.mp4";
                    content.platforms = new[] { "Twitter", "YouTube", "Instagram" };
                    break;

                case ContentType.Screenshot:
                    content.text = GenerateScreenshotPost();
                    content.mediaUrl = "https://cdn.soulvan.com/screenshots/gameplay.jpg";
                    content.platforms = new[] { "Twitter", "Instagram", "Discord" };
                    break;

                case ContentType.Gameplay:
                    content.text = GenerateGameplayPost();
                    content.mediaUrl = "https://cdn.soulvan.com/gameplay/highlights.mp4";
                    content.platforms = new[] { "YouTube", "TikTok", "Twitter" };
                    break;

                case ContentType.Community:
                    content.text = GenerateCommunityPost();
                    content.platforms = new[] { "Discord", "Reddit", "Twitter" };
                    break;

                case ContentType.Educational:
                    content.text = GenerateEducationalPost();
                    content.platforms = new[] { "Twitter", "Reddit", "Discord" };
                    break;

                case ContentType.AMA:
                    content.text = GenerateAMAPost();
                    content.platforms = new[] { "Reddit", "Discord", "Twitter" };
                    break;

                case ContentType.InfluencerReview:
                    content.text = GenerateInfluencerPost();
                    content.platforms = new[] { "YouTube", "Twitter" };
                    break;
            }

            // Add hashtags
            content.hashtags = GetRelevantHashtags(type);

            return content;
        }

        /// <summary>
        /// Post content to social media platforms.
        /// </summary>
        private async Task PostToSocialMedia(MarketingContent content)
        {
            foreach (var platform in content.platforms)
            {
                if (platformAPIs.ContainsKey(platform))
                {
                    try
                    {
                        await platformAPIs[platform].Post(content);
                        Debug.Log($"[AIMarketingAgent] Posted to {platform}: {content.text.Substring(0, Math.Min(50, content.text.Length))}...");

                        // Simulate engagement
                        int engagement = UnityEngine.Random.Range(100, 5000);
                        totalEngagements += engagement;
                        totalReach += engagement * UnityEngine.Random.Range(5, 20);

                        // Track spending
                        float postCost = UnityEngine.Random.Range(5f, 50f);
                        totalSpent += postCost;

                        // Simulate player acquisition (conversion rate: 0.5-2%)
                        int conversions = (int)(engagement * UnityEngine.Random.Range(0.005f, 0.02f));
                        if (conversions > 0)
                        {
                            await OnPlayersAcquired(conversions);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[AIMarketingAgent] Failed to post to {platform}: {e.Message}");
                    }
                }
            }

            await Task.Delay(500);
        }

        #endregion

        #region Content Templates

        private string GenerateTrailerPost()
        {
            var templates = new[]
            {
                "🎮 Experience the future of racing in #Soulvan! RTX-powered graphics meet Web3 gaming. Watch the trailer: ",
                "⚡ Hypercars. Blockchain. Epic missions. #Soulvan is here! Check out our latest trailer: ",
                "🚀 Ready to race? #Soulvan combines AAA gaming with true ownership. Trailer drops now: ",
                "🔥 The most anticipated Web3 game is here! #Soulvan gameplay trailer: "
            };

            return templates[UnityEngine.Random.Range(0, templates.Length)];
        }

        private string GenerateScreenshotPost()
        {
            var templates = new[]
            {
                "📸 RTX Ray Tracing makes #Soulvan look absolutely stunning! What's your favorite city? 🌃",
                "🎨 Every frame is a masterpiece. #Soulvan powered by NVIDIA RTX and DLSS 3. ",
                "🌆 Tokyo at night in #Soulvan hits different. Where should we race next? ",
                "✨ Photo mode captures from today's race. #Soulvan #Web3Gaming "
            };

            return templates[UnityEngine.Random.Range(0, templates.Length)];
        }

        private string GenerateGameplayPost()
        {
            var templates = new[]
            {
                "🏎️ 20 minutes of pure adrenaline! Watch our latest #Soulvan gameplay: ",
                "⚡ Dubai desert race at 200mph! Full gameplay: ",
                "🎮 Boss battle highlights from yesterday's stream! #Soulvan ",
                "🔥 This is what next-gen racing looks like! #Soulvan gameplay: "
            };

            return templates[UnityEngine.Random.Range(0, templates.Length)];
        }

        private string GenerateCommunityPost()
        {
            var templates = new[]
            {
                "🎉 Welcome to all new Soulvan racers! Join our Discord to connect with the community: discord.gg/soulvan",
                "💬 Community poll: Which hypercar should we add next? Vote now! #Soulvan",
                "🏆 Congrats to this week's top racers! Keep grinding for those SVN rewards! #Soulvan",
                "📢 Community update: New city missions drop this week! Stay tuned! #Soulvan"
            };

            return templates[UnityEngine.Random.Range(0, templates.Length)];
        }

        private string GenerateEducationalPost()
        {
            var templates = new[]
            {
                "🧠 What is Play-to-Earn? Learn how #Soulvan rewards you with $SVN tokens: ",
                "💡 NFT Car Skins explained: True ownership of your in-game assets. #Soulvan #Web3Gaming",
                "📚 How does #Soulvan integrate blockchain? A deep dive into our tokenomics: ",
                "🔐 Your wallet, your assets. Here's how blockchain gaming protects your progress: #Soulvan"
            };

            return templates[UnityEngine.Random.Range(0, templates.Length)];
        }

        private string GenerateAMAPost()
        {
            var templates = new[]
            {
                "🎤 AMA with the Soulvan dev team happening NOW! Ask us anything: reddit.com/r/soulvan",
                "💬 Join our Discord AMA today at 3PM EST! Questions about gameplay, tokenomics, roadmap? We got you!",
                "📢 AMA Announcement: Our lead developer will answer your questions tomorrow! Submit them here: ",
                "🎮 Community AMA recap: Here's what we discussed about the future of #Soulvan: "
            };

            return templates[UnityEngine.Random.Range(0, templates.Length)];
        }

        private string GenerateInfluencerPost()
        {
            var templates = new[]
            {
                "🎥 @Influencer just dropped their #Soulvan review! Go show some love: ",
                "🔥 Huge shoutout to @Influencer for the amazing gameplay video! #Soulvan community appreciates you!",
                "⚡ @Influencer is LIVE playing #Soulvan right now! Don't miss it: ",
                "🎮 @Influencer: 'This is the future of racing games' - Full review: "
            };

            return templates[UnityEngine.Random.Range(0, templates.Length)];
        }

        private ContentType GetNextContentType()
        {
            if (contentTypes == null || contentTypes.Length == 0)
            {
                contentTypes = new[] { ContentType.Screenshot, ContentType.Community, ContentType.Educational };
            }

            return contentTypes[UnityEngine.Random.Range(0, contentTypes.Length)];
        }

        private string[] GetRelevantHashtags(ContentType type)
        {
            return type switch
            {
                ContentType.Trailer => new[] { "#Soulvan", "#GameTrailer", "#Web3Gaming", "#RTX" },
                ContentType.Screenshot => new[] { "#Soulvan", "#GamingPhotography", "#RTX", "#DLSS" },
                ContentType.Gameplay => new[] { "#Soulvan", "#Gameplay", "#Web3Gaming", "#PlayToEarn" },
                ContentType.Community => new[] { "#Soulvan", "#GamingCommunity", "#Web3" },
                ContentType.Educational => new[] { "#Soulvan", "#Web3Gaming", "#Blockchain", "#Crypto" },
                ContentType.AMA => new[] { "#Soulvan", "#AMA", "#Web3Gaming" },
                ContentType.InfluencerReview => new[] { "#Soulvan", "#GamingReview", "#Sponsored" },
                _ => targetHashtags
            };
        }

        #endregion

        #region Community Engagement

        /// <summary>
        /// Engage with community posts, comments, and mentions.
        /// AI responds to player questions and feedback.
        /// </summary>
        private async Task EngageWithCommunity()
        {
            Debug.Log("[AIMarketingAgent] Engaging with community");

            foreach (var platform in platformAPIs.Keys)
            {
                try
                {
                    // Fetch mentions and comments
                    var mentions = await platformAPIs[platform].GetMentions("Soulvan");

                    foreach (var mention in mentions)
                    {
                        // AI generates appropriate response
                        var response = GenerateCommunityResponse(mention);

                        if (!string.IsNullOrEmpty(response))
                        {
                            await platformAPIs[platform].Reply(mention.id, response);
                            totalEngagements++;

                            Debug.Log($"[AIMarketingAgent] Replied to {mention.username} on {platform}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[AIMarketingAgent] Community engagement error on {platform}: {e.Message}");
                }
            }

            await Task.Delay(1000);
        }

        /// <summary>
        /// Generate AI response to community post.
        /// </summary>
        private string GenerateCommunityResponse(SocialMediaPost post)
        {
            // Sentiment analysis
            if (post.text.Contains("love") || post.text.Contains("amazing") || post.text.Contains("best"))
            {
                return "Thank you so much! We're thrilled you're enjoying Soulvan! 🎮🔥";
            }
            else if (post.text.Contains("bug") || post.text.Contains("issue") || post.text.Contains("problem"))
            {
                return "Thanks for reporting! Our team is looking into this. Please DM us with more details if you can!";
            }
            else if (post.text.Contains("when") || post.text.Contains("release") || post.text.Contains("date"))
            {
                return "Stay tuned! Big announcements coming soon. Join our Discord for the latest updates! 🚀";
            }
            else if (post.text.Contains("price") || post.text.Contains("cost") || post.text.Contains("SVN"))
            {
                return "Check out our tokenomics page: soulvan.com/tokenomics - $SVN is available on major DEXs! 💰";
            }

            return "Thanks for your support! 🙏 #Soulvan";
        }

        #endregion

        #region Influencer Marketing

        /// <summary>
        /// Automated influencer outreach and partnership management.
        /// </summary>
        private async Task InfluencerOutreach()
        {
            Debug.Log("[AIMarketingAgent] Running influencer outreach");

            // Find potential influencers
            var potentialInfluencers = await FindInfluencers();

            foreach (var influencer in potentialInfluencers)
            {
                if (influencerBudget >= influencer.rate)
                {
                    await SendInfluencerProposal(influencer);
                }
            }

            await Task.Delay(1000);
        }

        /// <summary>
        /// Find gaming/crypto influencers to partner with.
        /// </summary>
        private async Task<List<InfluencerProfile>> FindInfluencers()
        {
            await Task.Delay(500);

            // Stub: Would use influencer APIs to find creators
            return new List<InfluencerProfile>
            {
                new InfluencerProfile
                {
                    username = "GamingInfluencer1",
                    platform = "YouTube",
                    followers = 500000,
                    engagementRate = 0.08f,
                    niche = "Gaming",
                    rate = 2000f
                },
                new InfluencerProfile
                {
                    username = "CryptoGamer",
                    platform = "Twitter",
                    followers = 250000,
                    engagementRate = 0.12f,
                    niche = "Web3Gaming",
                    rate = 1500f
                }
            };
        }

        /// <summary>
        /// Send partnership proposal to influencer.
        /// </summary>
        private async Task SendInfluencerProposal(InfluencerProfile influencer)
        {
            Debug.Log($"[AIMarketingAgent] Sending proposal to {influencer.username} ({influencer.followers} followers)");

            await Task.Delay(500);

            // Stub: Would send actual DM/email via API
            string proposal = $@"
Hi {influencer.username},

We're reaching out from Soulvan, a next-gen Web3 racing game combining AAA graphics with blockchain technology.

We'd love to partner with you for a sponsored content series! Here's what we're offering:

- ${influencer.rate} per video
- Early access to the game
- Custom NFT car skin with your branding
- Revenue share from your referral code

Interested? Let's chat!

Best,
Soulvan Team
";

            activeInfluencers.Add(influencer);
            influencerBudget -= influencer.rate;

            Debug.Log($"[AIMarketingAgent] Proposal sent to {influencer.username}");
        }

        #endregion

        #region Revenue & Value Creation

        /// <summary>
        /// Handle new players acquired through marketing.
        /// Player purchases generate fees that flow to stability engine,
        /// which uses them to buy SVN and increase coin value.
        /// This creates a self-reinforcing cycle: Marketing → Players → Fees → SVN Value ↑
        /// </summary>
        private async Task OnPlayersAcquired(int playerCount)
        {
            newPlayers += playerCount;

            // Each player pays $59.99 on average (PC/console)
            float avgPurchasePrice = 59.99f;
            float totalRevenue = playerCount * avgPurchasePrice;
            totalRevenueGenerated += totalRevenue;

            // Platform takes 2.5% fee from each purchase
            float platformFees = totalRevenue * 0.025f;
            marketingFeesCollected += platformFees;

            Debug.Log($"[AIMarketingAgent] {playerCount} new players acquired!");
            Debug.Log($"[AIMarketingAgent] Revenue generated: ${totalRevenue:F2}");
            Debug.Log($"[AIMarketingAgent] Platform fees collected: ${platformFees:F2}");

            // Fees are sent to AI Stability Engine
            if (stabilityEngine != null)
            {
                // Convert USD fees to SVN (assuming $0.50 per SVN)
                float currentSVNPrice = 0.50f; // Would fetch from oracle in production
                float feesSVN = platformFees / currentSVNPrice;

                await stabilityEngine.AddFees(feesSVN);

                Debug.Log($"[AIMarketingAgent] {feesSVN:F2} SVN sent to Stability Engine");
                Debug.Log($"[AIMarketingAgent] AI will use these fees to buy more SVN, increasing coin value!");
            }

            // Calculate marketing ROI
            float roi = totalRevenueGenerated / Mathf.Max(1f, totalSpent);
            Debug.Log($"[AIMarketingAgent] Current Marketing ROI: {roi:F2}x (Revenue: ${totalRevenueGenerated:F2}, Spent: ${totalSpent:F2})");

            // Show value creation cycle
            Debug.Log($"[AIMarketingAgent] 💡 Value Creation Cycle:");
            Debug.Log($"  1. Marketing spent: ${totalSpent:F2}");
            Debug.Log($"  2. Players acquired: {newPlayers}");
            Debug.Log($"  3. Revenue generated: ${totalRevenueGenerated:F2}");
            Debug.Log($"  4. Fees to stability: ${marketingFeesCollected:F2}");
            Debug.Log($"  5. AI buys SVN → Price goes up ↑");
            Debug.Log($"  6. Higher SVN value → More marketing budget → Repeat!");

            await Task.Delay(100);
        }

        #endregion

        #region Campaign Monitoring

        /// <summary>
        /// Monitor active campaigns and adjust strategy.
        /// </summary>
        private async Task MonitorCampaigns()
        {
            Debug.Log("[AIMarketingAgent] Monitoring campaign performance");

            foreach (var campaign in campaignQueue)
            {
                if (campaign.status == CampaignStatus.Active)
                {
                    // Calculate campaign metrics
                    float roi = (newPlayers * 60f) / totalSpent; // $60 avg purchase
                    float engagementRate = (float)totalEngagements / totalPosts;

                    Debug.Log($"[Campaign: {campaign.name}] ROI: {roi:F2}x, Engagement: {engagementRate:F0}/post");

                    // AI optimization: Adjust budget allocation based on performance
                    if (roi < 1.0f)
                    {
                        Debug.LogWarning($"[AIMarketingAgent] Campaign {campaign.name} underperforming, reducing budget");
                        campaign.budget *= 0.8f;
                    }
                    else if (roi > 3.0f)
                    {
                        Debug.Log($"[AIMarketingAgent] Campaign {campaign.name} performing well, increasing budget");
                        campaign.budget *= 1.2f;
                    }
                }
            }

            await Task.Delay(1000);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get marketing stats dashboard.
        /// </summary>
        public MarketingStats GetStats()
        {
            return new MarketingStats
            {
                totalPosts = totalPosts,
                totalEngagements = totalEngagements,
                totalReach = totalReach,
                totalSpent = totalSpent,
                newPlayers = newPlayers,
                activeCampaigns = campaignQueue.Count,
                activeInfluencers = activeInfluencers.Count,
                averageROI = totalRevenueGenerated / Mathf.Max(1f, totalSpent),
                budgetRemaining = campaignBudgetUSD - totalSpent,
                totalRevenueGenerated = totalRevenueGenerated,
                marketingFeesCollected = marketingFeesCollected,
                svnValueIncrease = CalculateSVNValueIncrease()
            };
        }

        /// <summary>
        /// Calculate estimated SVN value increase from marketing fees.
        /// Marketing → Players → Fees → AI Buys SVN → Value ↑
        /// </summary>
        private float CalculateSVNValueIncrease()
        {
            // Estimate: For every $1000 in fees, SVN price increases by ~0.5%
            // This is because AI uses fees to buy SVN, reducing supply and increasing demand
            float estimatedIncrease = (marketingFeesCollected / 1000f) * 0.005f * 100f; // Percentage
            return estimatedIncrease;
        }

        /// <summary>
        /// Manually trigger a marketing campaign.
        /// </summary>
        public void LaunchCampaign(MarketingCampaign campaign)
        {
            campaignQueue.Enqueue(campaign);
            Debug.Log($"[AIMarketingAgent] Launched campaign: {campaign.name}");
        }

        #endregion
    }

    #region Data Structures

    [Serializable]
    public class MarketingCampaign
    {
        public string id;
        public string name;
        public string description;
        public float budget;
        public int duration; // days
        public string[] targetPlatforms;
        public ContentType[] contentTypes;
        public CampaignStatus status;
    }

    [Serializable]
    public class MarketingContent
    {
        public ContentType type;
        public string text;
        public string mediaUrl;
        public string[] platforms;
        public string[] hashtags;
        public DateTime timestamp;
    }

    [Serializable]
    public class InfluencerProfile
    {
        public string username;
        public string platform;
        public int followers;
        public float engagementRate;
        public string niche;
        public float rate; // USD per post/video
    }

    [Serializable]
    public class MarketingStats
    {
        public int totalPosts;
        public int totalEngagements;
        public int totalReach;
        public float totalSpent;
        public int newPlayers;
        public int activeCampaigns;
        public int activeInfluencers;
        public float averageROI;
        public float budgetRemaining;
        public float totalRevenueGenerated;
        public float marketingFeesCollected;
        public float svnValueIncrease; // Percentage increase from marketing fees
    }

    public enum ContentType
    {
        Trailer,
        Screenshot,
        Gameplay,
        Community,
        Educational,
        AMA,
        InfluencerReview
    }

    public enum CampaignStatus
    {
        Planned,
        Active,
        Paused,
        Completed
    }

    #endregion

    #region Social Media API Stubs

    public class SocialMediaAPI
    {
        public virtual async Task Post(MarketingContent content)
        {
            await Task.Delay(100);
        }

        public virtual async Task<List<SocialMediaPost>> GetMentions(string keyword)
        {
            await Task.Delay(100);
            return new List<SocialMediaPost>();
        }

        public virtual async Task Reply(string postId, string message)
        {
            await Task.Delay(100);
        }
    }

    public class TwitterAPI : SocialMediaAPI { }
    public class DiscordAPI : SocialMediaAPI { }
    public class RedditAPI : SocialMediaAPI { }
    public class YouTubeAPI : SocialMediaAPI { }
    public class TikTokAPI : SocialMediaAPI { }
    public class InstagramAPI : SocialMediaAPI { }

    public class SocialMediaPost
    {
        public string id;
        public string username;
        public string text;
        public DateTime timestamp;
    }

    #endregion
}
