using UnityEngine;

namespace Soulvan.Systems
{
    /// <summary>
    /// Client for interacting with DAO governance contracts.
    /// Reads seasonal arcs, mission proposals, and submits votes.
    /// </summary>
    public class DaoGovernanceClient : MonoBehaviour
    {
        [Header("Blockchain")]
        [SerializeField] private string governanceContractAddress;
        [SerializeField] private string seasonManagerAddress;

        [Header("Polling")]
        [SerializeField] private float pollInterval = 30f; // seconds
        
        private float lastPollTime;

        private void Update()
        {
            if (Time.time - lastPollTime > pollInterval)
            {
                PollGovernanceState();
                lastPollTime = Time.time;
            }
        }

        /// <summary>
        /// Poll blockchain for active seasonal arc.
        /// </summary>
        private async void PollGovernanceState()
        {
            // TODO: Query seasonManager.activeSeason()
            // TODO: Query seasonManager.activeMotifs()
            
            Debug.Log("[DaoGovernanceClient] Polling governance state...");
            await System.Threading.Tasks.Task.Delay(500); // Simulate RPC call
            
            // Example: Update local motif based on on-chain season
            var motifAPI = FindFirstObjectByType<MotifAPI>();
            if (motifAPI)
            {
                // Pseudo: int season = await seasonManager.activeSeason();
                // motifAPI.SetMotif((Motif)season, 0.7f);
            }
        }

        /// <summary>
        /// Submit governance proposal.
        /// </summary>
        public async void SubmitProposal(string description, string callData)
        {
            Debug.Log($"[DaoGovernanceClient] Submitting proposal: {description}");
            
            // TODO: Call governance.propose(target, callData, description)
            await System.Threading.Tasks.Task.Delay(1000);
            
            Infra.EventBus.EmitMotif("DAO_PROPOSAL_SUBMITTED");
        }

        /// <summary>
        /// Cast vote on active proposal.
        /// </summary>
        public async void CastVote(int proposalId, bool support)
        {
            Debug.Log($"[DaoGovernanceClient] Voting on proposal {proposalId}: {(support ? "FOR" : "AGAINST")}");
            
            // TODO: Call governance.castVote(proposalId, support)
            await System.Threading.Tasks.Task.Delay(500);
            
            Infra.EventBus.EmitMotif("DAO_VOTE_CAST");
        }

        /// <summary>
        /// Get active proposals from blockchain.
        /// </summary>
        public async System.Threading.Tasks.Task<ProposalData[]> GetActiveProposals()
        {
            // TODO: Query governance.proposalCount() and fetch details
            await System.Threading.Tasks.Task.Delay(1000);
            return new ProposalData[0]; // Stub
        }
    }

    [System.Serializable]
    public class ProposalData
    {
        public int id;
        public string description;
        public int forVotes;
        public int againstVotes;
        public bool executed;
    }
}
