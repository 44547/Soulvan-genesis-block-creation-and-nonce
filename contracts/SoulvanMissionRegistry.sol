// SPDX-License-Identifier: MIT
pragma solidity ^0.8.21;

import "@openzeppelin/contracts/access/AccessControl.sol";

interface ISoulvanCoin {
    function MINTER_ROLE() external view returns (bytes32);
    function grantRole(bytes32 role, address account) external;
    function mint(address to, uint256 amount) external;
}

interface ISoulvanChronicle2 {
    function log(uint8 entryType, address actor, bytes32 contentHash) external returns (uint256 id);
}

/// @title SoulvanMissionRegistry
/// @notice GTA-style mission catalog with completion settlement: SVN rewards + Chronicle logging.
contract SoulvanMissionRegistry is AccessControl {
    bytes32 public constant ADMIN_ROLE = keccak256("ADMIN_ROLE");
    bytes32 public constant EXECUTOR_ROLE = keccak256("EXECUTOR_ROLE");

    struct Mission {
        // Minimal on-chain summary; richer payload referenced via contentHash
        uint256 rewardSVN; // 18 decimals amount
        bool active;
        // enum-style tags
        uint8 missionType;     // 0 deliver, 1 heist, 2 boss, 3 race-assist, etc.
        uint8 environmentType; // 0 city, 1 industrial, 2 cosmic, 3 mountain, etc.
        bytes32 contentHash;   // IPFS/Arweave hash with mission script & cinematic data
    }

    ISoulvanCoin public immutable coin;
    ISoulvanChronicle2 public immutable chronicle;

    uint256 public nextMissionId;
    mapping(uint256 => Mission) public missions;
    mapping(uint256 => mapping(address => bool)) public completed; // missionId => player => done

    event MissionCreated(uint256 indexed id, uint8 missionType, uint8 environmentType, uint256 rewardSVN, bytes32 contentHash);
    event MissionStatus(uint256 indexed id, bool active);
    event MissionCompleted(uint256 indexed id, address indexed player, uint256 rewardSVN);

    constructor(ISoulvanCoin _coin, ISoulvanChronicle2 _chronicle) {
        coin = _coin;
        chronicle = _chronicle;
        _setupRole(DEFAULT_ADMIN_ROLE, msg.sender);
        _setupRole(ADMIN_ROLE, msg.sender);
        _setupRole(EXECUTOR_ROLE, msg.sender);
    }

    function createMission(uint8 missionType, uint8 environmentType, uint256 rewardSVN, bytes32 contentHash)
        external
        onlyRole(ADMIN_ROLE)
        returns (uint256 id)
    {
        id = nextMissionId++;
        missions[id] = Mission({
            rewardSVN: rewardSVN,
            active: true,
            missionType: missionType,
            environmentType: environmentType,
            contentHash: contentHash
        });
        emit MissionCreated(id, missionType, environmentType, rewardSVN, contentHash);
        // Chronicle entryType 1 = Mission created
        if (address(chronicle) != address(0)) chronicle.log(1, msg.sender, contentHash);
    }

    function setActive(uint256 id, bool isActive) external onlyRole(ADMIN_ROLE) {
        missions[id].active = isActive;
        emit MissionStatus(id, isActive);
    }

    /// @notice Mark mission completed, mint rewards, and log to chronicle.
    function completeMission(uint256 id, address player, bytes32 resultHash) external onlyRole(EXECUTOR_ROLE) {
        Mission memory m = missions[id];
        require(m.active, "Mission inactive");
        require(!completed[id][player], "Already completed");
        completed[id][player] = true;

        // Mint SVN reward to player; registry must be granted MINTER_ROLE on coin
        coin.mint(player, m.rewardSVN);

        // Chronicle entryType 3 = Reward, or use 1 for mission result; we log as 1 with result
        if (address(chronicle) != address(0)) chronicle.log(1, player, resultHash);

        emit MissionCompleted(id, player, m.rewardSVN);
    }
}
