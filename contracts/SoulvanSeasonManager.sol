// SPDX-License-Identifier: MIT
pragma solidity ^0.8.21;

import "@openzeppelin/contracts/access/AccessControl.sol";

interface ISoulvanChronicle {
    function log(uint8 entryType, address actor, bytes32 contentHash) external returns (uint256 id);
}

/// @title SoulvanSeasonManager
/// @notice Controls seasonal arcs (Storm -> Calm -> Cosmic) and active motif packs for visual/audio/haptic overlays.
contract SoulvanSeasonManager is AccessControl {
    bytes32 public constant SEASON_ADMIN_ROLE = keccak256("SEASON_ADMIN_ROLE");

    enum Season { Storm, Calm, Cosmic }

    struct MotifPack {
        // Off-chain identifiers; clients resolve to shaders/audio/haptic profiles
        string visual;  // e.g., "storm_v1", "calm_sunrise", "cosmic_aurora"
        string audio;   // e.g., FMOD/Wwise bank id
        string haptic;  // e.g., pattern id
    }

    Season public activeSeason;
    MotifPack public activeMotifs;
    ISoulvanChronicle public chronicle; // optional

    event SeasonChanged(Season season);
    event MotifsUpdated(string visual, string audio, string haptic);

    constructor(address _chronicle) {
        _setupRole(DEFAULT_ADMIN_ROLE, msg.sender);
        _setupRole(SEASON_ADMIN_ROLE, msg.sender);
        activeSeason = Season.Storm;
        if (_chronicle != address(0)) chronicle = ISoulvanChronicle(_chronicle);
    }

    function setSeason(Season season, bytes32 contentHash) external onlyRole(SEASON_ADMIN_ROLE) {
        activeSeason = season;
        emit SeasonChanged(season);
        if (address(chronicle) != address(0)) {
            // entryType 6 reserved for Season events
            chronicle.log(6, msg.sender, contentHash);
        }
    }

    function setMotifs(MotifPack calldata pack, bytes32 contentHash) external onlyRole(SEASON_ADMIN_ROLE) {
        activeMotifs = pack;
        emit MotifsUpdated(pack.visual, pack.audio, pack.haptic);
        if (address(chronicle) != address(0)) {
            // entryType 7 reserved for Motif updates
            chronicle.log(7, msg.sender, contentHash);
        }
    }
}
