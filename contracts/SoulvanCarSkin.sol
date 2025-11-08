// SPDX-License-Identifier: MIT
pragma solidity ^0.8.21;

import "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";

/// @title SoulvanCarSkin NFT
/// @notice ERC721 tokens representing car skins / cosmetic upgrades. Metadata served from off-chain (IPFS/Arweave).
contract SoulvanCarSkin is ERC721URIStorage, AccessControl {
    bytes32 public constant MINTER_ROLE = keccak256("MINTER_ROLE");
    bytes32 public constant METADATA_ROLE = keccak256("METADATA_ROLE");

    uint256 private _nextId = 1;

    event SkinMinted(uint256 indexed tokenId, address indexed to, string uri);
    event SkinUpgraded(uint256 indexed tokenId, string newUri);

    constructor() ERC721("SoulvanCarSkin", "SVNSKIN") {
        _setupRole(DEFAULT_ADMIN_ROLE, msg.sender);
        _setupRole(MINTER_ROLE, msg.sender);
        _setupRole(METADATA_ROLE, msg.sender);
    }

    function mint(address to, string calldata metadataURI) external onlyRole(MINTER_ROLE) returns (uint256 tokenId) {
        tokenId = _nextId++;
        _safeMint(to, tokenId);
        _setTokenURI(tokenId, metadataURI);
        emit SkinMinted(tokenId, to, metadataURI);
    }

    /// @notice Update metadata (e.g., upgrade visual skin); off-chain host must reflect new attributes.
    function upgradeMetadata(uint256 tokenId, string calldata newURI) external onlyRole(METADATA_ROLE) {
        require(_exists(tokenId), "Skin: DNE");
        _setTokenURI(tokenId, newURI);
        emit SkinUpgraded(tokenId, newURI);
    }

    // Required override to resolve AccessControl + ERC721 multiple inheritance of supportsInterface
    function supportsInterface(bytes4 interfaceId) public view override(AccessControl, ERC721URIStorage) returns (bool) {
        return super.supportsInterface(interfaceId);
    }
}
