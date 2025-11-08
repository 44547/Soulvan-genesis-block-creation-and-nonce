// SPDX-License-Identifier: MIT
pragma solidity ^0.8.21;

import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/token/ERC20/extensions/ERC20Votes.sol";

/// @title SoulvanGovernance
/// @notice Minimal snapshot-based governance using SoulvanCoin voting power. Not upgradeable; off-chain infrastructure can extend.
contract SoulvanGovernance is AccessControl {
    bytes32 public constant PROPOSER_ROLE = keccak256("PROPOSER_ROLE");

    struct Proposal {
        address proposer;
        uint64 startBlock;
        uint64 endBlock;
        string description; // Off-chain description / IPFS pointer allowed
        bytes callData;     // Encoded function calls (single target for simplicity)
        address target;
        uint256 forVotes;
        uint256 againstVotes;
        bool executed;
        bool canceled;
    }

    ERC20Votes public immutable token;
    uint64 public votingDelay;      // blocks after proposal creation
    uint64 public votingPeriod;     // blocks voting remains open
    uint256 public proposalThreshold; // minimum voting power to propose

    Proposal[] public proposals;
    mapping(uint256 => mapping(address => bool)) public hasVoted;

    event ProposalCreated(uint256 id, address proposer, string description);
    event VoteCast(uint256 id, address voter, bool support, uint256 weight);
    event ProposalExecuted(uint256 id);
    event ProposalCanceled(uint256 id);

    constructor(ERC20Votes _token, uint64 _votingDelay, uint64 _votingPeriod, uint256 _proposalThreshold) {
        token = _token;
        votingDelay = _votingDelay;
        votingPeriod = _votingPeriod;
        proposalThreshold = _proposalThreshold;
        _setupRole(DEFAULT_ADMIN_ROLE, msg.sender);
        _setupRole(PROPOSER_ROLE, msg.sender);
    }

    function propose(address target, bytes calldata callData, string calldata description) external returns (uint256 id) {
        require(hasRole(PROPOSER_ROLE, msg.sender), "Not proposer");
        require(token.getVotes(msg.sender) >= proposalThreshold, "Threshold");
        uint64 start = uint64(block.number + votingDelay);
        uint64 end = start + votingPeriod;
        id = proposals.length;
        proposals.push(Proposal({
            proposer: msg.sender,
            startBlock: start,
            endBlock: end,
            description: description,
            callData: callData,
            target: target,
            forVotes: 0,
            againstVotes: 0,
            executed: false,
            canceled: false
        }));
        emit ProposalCreated(id, msg.sender, description);
    }

    function castVote(uint256 id, bool support) external {
        Proposal storage p = proposals[id];
        require(block.number >= p.startBlock && block.number <= p.endBlock, "Voting closed");
        require(!hasVoted[id][msg.sender], "Voted");
        uint256 weight = token.getPastVotes(msg.sender, p.startBlock);
        require(weight > 0, "No votes");
        hasVoted[id][msg.sender] = true;
        if (support) p.forVotes += weight; else p.againstVotes += weight;
        emit VoteCast(id, msg.sender, support, weight);
    }

    function state(uint256 id) public view returns (uint8) {
        Proposal storage p = proposals[id];
        if (p.canceled) return 4; // Canceled
        if (block.number < p.startBlock) return 0; // Pending
        if (block.number <= p.endBlock) return 1; // Active
        if (p.executed) return 3; // Executed
        return p.forVotes > p.againstVotes ? 2 : 5; // Succeeded or Defeated
    }

    function execute(uint256 id) external {
        Proposal storage p = proposals[id];
        require(state(id) == 2, "Not successful");
        p.executed = true;
        if (p.target != address(0) && p.callData.length > 0) {
            (bool ok, bytes memory data) = p.target.call(p.callData);
            require(ok, string(data));
        }
        emit ProposalExecuted(id);
    }

    function cancel(uint256 id) external {
        Proposal storage p = proposals[id];
        require(hasRole(DEFAULT_ADMIN_ROLE, msg.sender) || msg.sender == p.proposer, "No auth");
        require(!p.executed, "Executed");
        p.canceled = true;
        emit ProposalCanceled(id);
    }

    function proposalCount() external view returns (uint256) { return proposals.length; }
}
