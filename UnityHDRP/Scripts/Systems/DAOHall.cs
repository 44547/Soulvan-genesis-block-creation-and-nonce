using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soulvan.Systems
{
    /// <summary>
    /// DAO Hall system with contributor voting podiums.
    /// Features real-time voting, voice overlays, vote archives, and cinematic cutscenes.
    /// </summary>
    public class DAOHall : MonoBehaviour
    {
        [Header("DAO Configuration")]
        public string daoName = "Soulvan DAO";
        public int totalSeats = 100;
        public List<DAOProposal> activeProposals = new List<DAOProposal>();

        [Header("Voting Podiums")]
        public Transform[] contributorPodiums;
        public GameObject podiumRuneLights;
        public Dictionary<string, VotePodium> activePodiums = new Dictionary<string, VotePodium>();

        [Header("Vote HUD")]
        public GameObject voteHUDPanel;
        public TextMeshProUGUI proposalTitleText;
        public TextMeshProUGUI proposalDescriptionText;
        public TextMeshProUGUI voteCountText;
        public Image votingProgressBar;

        [Header("Real-Time Feed")]
        public Transform voteFeedContainer;
        public GameObject voteFeedEntryPrefab;
        public ScrollRect voteFeedScroll;

        [Header("Vote Archive")]
        public Transform voteArchiveContainer;
        public GameObject archiveEntryPrefab;
        public TextMeshProUGUI archiveStatsText;

        [Header("FX")]
        public ParticleSystem voteCastFX;
        public ParticleSystem proposalPassFX;
        public AudioClip voteSound;
        public AudioClip proposalPassSound;
        public AudioClip voiceOverlayClip;

        [Header("Cinematic")]
        public DAOHallCutsceneTrigger cutsceneTrigger;

        private Dictionary<string, List<Vote>> voteHistory = new Dictionary<string, List<Vote>>();

        private void Start()
        {
            InitializeDAOHall();
            LoadActiveProposals();
            SetupVotingPodiums();
        }

        /// <summary>
        /// Initialize DAO Hall.
        /// </summary>
        private void InitializeDAOHall()
        {
            Debug.Log($"[DAOHall] Initializing {daoName}...");

            // Setup HUD
            if (voteHUDPanel != null)
            {
                voteHUDPanel.SetActive(false);
            }

            // Record to lore
            SoulvanLore.Record($"{daoName} hall initialized with {totalSeats} contributor seats");
        }

        /// <summary>
        /// Load active proposals.
        /// </summary>
        private void LoadActiveProposals()
        {
            // Example proposals - in production, load from SoulvanGovernance contract
            activeProposals.Add(new DAOProposal
            {
                proposalId = "D163",
                title = "Contributor Expansion Program",
                description = "Increase contributor rewards by 25% and add 3 new badge tiers",
                voteYes = 42,
                voteNo = 18,
                votingEnds = "2025-11-15",
                requiredQuorum = 60,
                status = ProposalStatus.Active
            });

            activeProposals.Add(new DAOProposal
            {
                proposalId = "D164",
                title = "Multiverse Bridge Funding",
                description = "Allocate 50,000 SVN for cross-realm portal development",
                voteYes = 35,
                voteNo = 25,
                votingEnds = "2025-11-20",
                requiredQuorum = 60,
                status = ProposalStatus.Active
            });

            Debug.Log($"[DAOHall] Loaded {activeProposals.Count} active proposals");
        }

        /// <summary>
        /// Setup voting podiums.
        /// </summary>
        private void SetupVotingPodiums()
        {
            for (int i = 0; i < contributorPodiums.Length; i++)
            {
                Transform podium = contributorPodiums[i];
                
                // Add rune lights
                if (podiumRuneLights != null)
                {
                    Instantiate(podiumRuneLights, podium.position + Vector3.up * 0.5f, Quaternion.identity, podium);
                }
            }

            Debug.Log($"[DAOHall] Setup {contributorPodiums.Length} voting podiums");
        }

        /// <summary>
        /// Cast contributor vote.
        /// </summary>
        public void CastVote(string contributorId, string proposalId, VoteChoice choice, string reason)
        {
            DAOProposal proposal = activeProposals.Find(p => p.proposalId == proposalId);
            if (proposal == null)
            {
                Debug.Log($"[DAOHall] ❌ Proposal not found: {proposalId}");
                return;
            }

            // Get contributor voting power from ArchitectKit
            int votingPower = GetContributorVotingPower(contributorId);

            Debug.Log($"[DAOHall] 🗳️ Vote cast by {contributorId}: {choice} on {proposalId} (Power: {votingPower})");

            // Record vote
            Vote vote = new Vote
            {
                contributorId = contributorId,
                proposalId = proposalId,
                choice = choice,
                votingPower = votingPower,
                reason = reason,
                timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            if (!voteHistory.ContainsKey(contributorId))
            {
                voteHistory[contributorId] = new List<Vote>();
            }
            voteHistory[contributorId].Add(vote);

            // Update proposal counts
            if (choice == VoteChoice.Yes)
            {
                proposal.voteYes += votingPower;
            }
            else if (choice == VoteChoice.No)
            {
                proposal.voteNo += votingPower;
            }

            // Play vote FX
            if (voteCastFX != null)
            {
                voteCastFX.Play();
            }

            if (voteSound != null)
            {
                AudioSource.PlayClipAtPoint(voteSound, transform.position);
            }

            // Play voice overlay
            PlayVoiceOverlay(contributorId, reason);

            // Update vote feed
            AddVoteFeedEntry(vote);

            // Check if proposal passes
            CheckProposalStatus(proposal);

            // Record to lore
            SoulvanLore.Record($"Contributor {contributorId} voted {choice} on proposal {proposalId}: {reason}");

            // Trigger cinematic for dramatic votes
            if (votingPower >= 50)
            {
                TriggerVoteCutscene();
            }
        }

        /// <summary>
        /// Get contributor voting power.
        /// </summary>
        private int GetContributorVotingPower(string contributorId)
        {
            ArchitectKit kit = FindObjectOfType<ArchitectKit>();
            if (kit != null && kit.contributorId == contributorId)
            {
                return kit.GetStats().daoPower;
            }

            return 1; // Default power
        }

        /// <summary>
        /// Play voice overlay for vote.
        /// </summary>
        private void PlayVoiceOverlay(string contributorId, string reason)
        {
            if (voiceOverlayClip != null)
            {
                AudioSource.PlayClipAtPoint(voiceOverlayClip, transform.position);
            }

            Debug.Log($"[DAOHall] 🎙️ Voice overlay: {contributorId} says '{reason}'");
        }

        /// <summary>
        /// Add entry to vote feed.
        /// </summary>
        private void AddVoteFeedEntry(Vote vote)
        {
            if (voteFeedContainer == null || voteFeedEntryPrefab == null) return;

            GameObject entryObj = Instantiate(voteFeedEntryPrefab, voteFeedContainer);
            VoteFeedEntry entry = entryObj.GetComponent<VoteFeedEntry>();
            if (entry != null)
            {
                entry.SetData(vote);
            }

            // Auto-scroll to bottom
            if (voteFeedScroll != null)
            {
                Canvas.ForceUpdateCanvases();
                voteFeedScroll.verticalNormalizedPosition = 0f;
            }
        }

        /// <summary>
        /// Check if proposal passes or fails.
        /// </summary>
        private void CheckProposalStatus(DAOProposal proposal)
        {
            int totalVotes = proposal.voteYes + proposal.voteNo;

            if (totalVotes >= proposal.requiredQuorum)
            {
                if (proposal.voteYes > proposal.voteNo)
                {
                    proposal.status = ProposalStatus.Passed;
                    OnProposalPassed(proposal);
                }
                else
                {
                    proposal.status = ProposalStatus.Failed;
                    OnProposalFailed(proposal);
                }
            }
        }

        /// <summary>
        /// Handle proposal pass.
        /// </summary>
        private void OnProposalPassed(DAOProposal proposal)
        {
            Debug.Log($"[DAOHall] ✅ Proposal PASSED: {proposal.proposalId} - {proposal.title}");

            // Play pass FX
            if (proposalPassFX != null)
            {
                proposalPassFX.Play();
            }

            if (proposalPassSound != null)
            {
                AudioSource.PlayClipAtPoint(proposalPassSound, transform.position);
            }

            // Record to lore
            SoulvanLore.Record($"DAO Proposal {proposal.proposalId} PASSED: {proposal.title}");

            // Export as lore-bound decision
            ExportProposalDecision(proposal);

            // Trigger cinematic
            TriggerVoteCutscene();
        }

        /// <summary>
        /// Handle proposal fail.
        /// </summary>
        private void OnProposalFailed(DAOProposal proposal)
        {
            Debug.Log($"[DAOHall] ❌ Proposal FAILED: {proposal.proposalId} - {proposal.title}");

            // Record to lore
            SoulvanLore.Record($"DAO Proposal {proposal.proposalId} FAILED: {proposal.title}");
        }

        /// <summary>
        /// Export proposal decision as scroll/NFT.
        /// </summary>
        private void ExportProposalDecision(DAOProposal proposal)
        {
            Debug.Log($"[DAOHall] 📜 Exporting proposal decision: {proposal.proposalId}");

            // In production, mint NFT with proposal data
            // SoulvanLore.ExportMissionLore(proposal.proposalId, 0, 0);
        }

        /// <summary>
        /// Trigger vote cutscene.
        /// </summary>
        private void TriggerVoteCutscene()
        {
            if (cutsceneTrigger != null)
            {
                cutsceneTrigger.TriggerCutscene();
            }
        }

        /// <summary>
        /// Get vote history for contributor.
        /// </summary>
        public List<Vote> GetVoteHistory(string contributorId)
        {
            if (voteHistory.ContainsKey(contributorId))
            {
                return voteHistory[contributorId];
            }

            return new List<Vote>();
        }

        /// <summary>
        /// Load vote archive.
        /// </summary>
        public void LoadVoteArchive()
        {
            if (voteArchiveContainer == null || archiveEntryPrefab == null) return;

            // Clear existing entries
            foreach (Transform child in voteArchiveContainer)
            {
                Destroy(child.gameObject);
            }

            // Load all votes from all contributors
            int totalVotes = 0;
            foreach (var kvp in voteHistory)
            {
                foreach (var vote in kvp.Value)
                {
                    GameObject entryObj = Instantiate(archiveEntryPrefab, voteArchiveContainer);
                    VoteArchiveEntry entry = entryObj.GetComponent<VoteArchiveEntry>();
                    if (entry != null)
                    {
                        entry.SetData(vote);
                    }

                    totalVotes++;
                }
            }

            // Update stats
            if (archiveStatsText != null)
            {
                archiveStatsText.text = $"Total Votes: {totalVotes} | Active Proposals: {activeProposals.Count}";
            }

            Debug.Log($"[DAOHall] Archive loaded with {totalVotes} votes");
        }
    }

    /// <summary>
    /// DAO proposal data.
    /// </summary>
    [System.Serializable]
    public class DAOProposal
    {
        public string proposalId;
        public string title;
        public string description;
        public int voteYes;
        public int voteNo;
        public string votingEnds;
        public int requiredQuorum;
        public ProposalStatus status;
    }

    /// <summary>
    /// Vote data.
    /// </summary>
    [System.Serializable]
    public class Vote
    {
        public string contributorId;
        public string proposalId;
        public VoteChoice choice;
        public int votingPower;
        public string reason;
        public string timestamp;
    }

    /// <summary>
    /// Vote choice enum.
    /// </summary>
    public enum VoteChoice
    {
        Yes,
        No,
        Abstain
    }

    /// <summary>
    /// Proposal status enum.
    /// </summary>
    public enum ProposalStatus
    {
        Active,
        Passed,
        Failed,
        Expired
    }

    /// <summary>
    /// Vote podium data.
    /// </summary>
    public class VotePodium
    {
        public Transform podiumTransform;
        public GameObject runeLights;
        public string contributorId;
        public bool isActive;
    }
}
