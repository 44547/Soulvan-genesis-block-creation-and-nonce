// SPDX-License-Identifier: MIT
pragma solidity ^0.8.21;

import "@openzeppelin/contracts/access/AccessControl.sol";

/// @title SoulvanChronicle
/// @notice Immutable log of race & mission outcomes. Stores minimal data + content hash reference for rich off-chain payload.
contract SoulvanChronicle is AccessControl {
    bytes32 public constant LOGGER_ROLE = keccak256("LOGGER_ROLE");

    // Entry type constants for clarity
    uint8 public constant TYPE_RACE = 0;
    uint8 public constant TYPE_MISSION = 1;
    uint8 public constant TYPE_GOVERNANCE = 2;
    uint8 public constant TYPE_REWARD = 3;
    uint8 public constant TYPE_SEASON = 6;
    uint8 public constant TYPE_MOTIF = 7;

    struct Entry {
        uint64 timestamp;      // block timestamp
        address actor;         // player or system module
        uint8 entryType;       // 0 = Race, 1 = Mission, 2 = Governance, 3 = Reward
        bytes32 contentHash;   // IPFS/Arweave/other hash of detailed JSON
    }

    Entry[] public entries;

    event ChronicleEntry(uint256 indexed id, uint8 entryType, address indexed actor, bytes32 contentHash);

    constructor() {
        _setupRole(DEFAULT_ADMIN_ROLE, msg.sender);
        _setupRole(LOGGER_ROLE, msg.sender);
    }

    function log(uint8 entryType, address actor, bytes32 contentHash) external onlyRole(LOGGER_ROLE) returns (uint256 id) {
        id = entries.length;
        entries.push(Entry({
            timestamp: uint64(block.timestamp),
            actor: actor,
            entryType: entryType,
            contentHash: contentHash
        }));
        emit ChronicleEntry(id, entryType, actor, contentHash);
    }

    function entryCount() external view returns (uint256) { return entries.length; }
}
