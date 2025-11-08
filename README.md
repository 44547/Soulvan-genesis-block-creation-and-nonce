# Soulvan - Complete Ecosystem

**Neon-Noir Universe** combining blockchain governance, GTA-style heist missions, and procedural saga remixing.

This repository contains:
- **Blockchain Layer**: Solidity contracts for rewards, NFTs, governance, and chronicle logging
- **Unity HDRP Systems**: Complete heist mission framework with DAO overlay visualization
- **Node.js APIs**: Replay minting, DAO governance, and RemixForge graph services
- **CLI Tools**: Contributor badge forging and replay bundle management

---

## System Overview

### Blockchain Contracts

- `SoulvanCoin.sol` — ERC20Votes token (SVN) for rewards & governance voting power
- `SoulvanCarSkin.sol` — ERC721 cosmetic NFTs (car skins, visual upgrades)
- `SoulvanChronicle.sol` — Immutable append-only log for races, missions, governance events
- `SoulvanGovernance.sol` — Proposal + voting system with snapshot voting
- `SoulvanSeasonManager.sol` — Seasonal arcs (Storm/Calm/Cosmic) and motif pack management
- `SoulvanMissionRegistry.sol` — Mission catalog with SVN rewards + Chronicle integration

### Unity HDRP Systems

**Neon Vault Heist Mission System**:
- `MissionController.cs` — Core orchestration with state machine and heat tracking
- `AIDirector_Heist.cs` — Adaptive difficulty scaling based on tension
- `RoleManager.cs` — 4-player roles (Driver/Infiltrator/Systems/Support) with unique abilities
- `HackingMiniGame.cs` — Pattern-based vault breach puzzles
- `CarryAndTradeoff.cs` — Datacore carrying mechanic with movement penalties
- `VehicleController_Hovercar.cs` — Arcade hovercar physics with boost mechanics
- `PursuitAI.cs` — Enemy chase AI with ramming and shooting
- `CameraCinematicController.cs` — Spline-based cinematic camera system
- `FXManager.cs` — Centralized visual effects with object pooling
- `NetworkSync_Authoritative.cs` — Server-authoritative sync with anti-cheat

**DAO Governance Overlay**:
- `DAOOverlayFXManager.cs` — Visual overlay for proposals, vote ripples, tier bursts, governance trails
- `DAOImpactGraphSDK.cs` — Integration layer connecting DAO overlay with RemixForge graph
- `RemixForgeGraph.cs` — 3D contributor graph with circular layout and real-time API sync

**Replay & Seed System**:
- `ReplaySeedLogger.cs` — Async replay logging with retry logic and local persistence
- `DAOMissionNotifier.cs` — DAO impact calculation with tier evaluation
- `DeterministicSeedUtil.cs` — Deterministic seed generation with server signing

### Node.js APIs

**Replay Mint API** (port 3002):
- POST `/replay/bundle` — Bundle replay actions into replayId
- POST `/replay/remix` — Create saga remix from existing replay
- POST `/replay/log` — Log deterministic replay seed with server signature

**DAO Replay Impact API** (port 3004):
- POST `/dao/impact` — Record mission impact (heat → influence conversion)
- POST `/dao/vote` — Cast vote with ripple propagation
- GET `/dao/proposals` — Get active governance proposals
- GET `/dao/vote-ripples/:proposalId` — Get vote ripple visualization data
- GET `/dao/contributor-stats/:id` — Get contributor tier and DAO power

**Remix Forge Graph API** (port 3003):
- GET `/remix-forge/lineage/:id` — Get replay lineage tree
- GET `/remix-forge/echo/:id` — Get echo trail for saga variants
- POST `/remix-forge/scroll` — Create contributor scroll with badges
- POST `/remix-forge/dao-ripple` — Trigger DAO vote ripple on graph

### CLI Tools

**Badge Forge CLI** (`cli/soulvan-badge-cli.js`):
- `forge <contributorId>` — Generate SVG badge with tier and DAO power
- `list` — List all contributor badges
- `export <contributorId>` — Export badge as PNG/JSON

**Replay Package CLI** (`cli/soulvan-replay-cli.js`):
- `bundle <files...>` — Bundle replay actions into replayId
- `remix <replayId> <variant>` — Create saga remix variant
- `export <replayId>` — Export replay data as JSON
- `dao-trigger <replayId>` — Trigger DAO impact from replay
- `history <contributorId>` — Show contributor replay history

---

## Quick Start

### Blockchain Setup


**Tech Stack**: Hardhat + OpenZeppelin Contracts (Solidity ^0.8.21)

```bash
# Install dependencies
npm install

# Compile contracts
npx hardhat compile

# Run tests
npx hardhat test

# Deploy to local network
npx hardhat node
npx hardhat run scripts/deploy.js --network localhost
```

**Environment Variables**:
```
SEPOLIA_RPC=https://...
DEPLOYER_KEY=0x...
ETHERSCAN_KEY=...
```

### Unity HDRP Setup

**Requirements**:
- Unity 2021.3+ with HDRP 12+
- TextMeshPro package
- NavMesh Components

**Import Project**:
1. Open Unity Hub
2. Add project from `/UnityHDRP/`
3. Wait for package import and compilation
4. Open scene: `Assets/Soulvan/Scenes/NeonVaultHeist.unity`

**Import ScriptableObjects**:
1. Window → Soulvan → Import ScriptableObjects from JSON
2. Source: `Assets/Scripts/ScriptableObjects/seed-json`
3. Target: `Assets/ScriptableObjects`
4. Click "Import All"
5. Verify assets created in:
   - `Assets/ScriptableObjects/MissionModules/`
   - `Assets/ScriptableObjects/Datacores/`
   - `Assets/ScriptableObjects/Heat/`
   - `Assets/ScriptableObjects/Vehicles/`

**Configure APIs**:
1. Select `ServerRpcClient` GameObject in scene
2. Set Base URL: `https://api.soulvan` (or local dev server)
3. Auth token set automatically via `InitAuth()` at runtime

### Unity CI/CD Setup

**Automated Workflows** powered by GitHub Actions and GameCI:

**Quick Start**:
```bash
# 1. Generate Unity license for CI
Actions → Unity Activation → Run workflow
# Download .alf file, get license from https://license.unity3d.com/manual

# 2. Add secrets to repository
Settings → Secrets and variables → Actions
# Add: UNITY_LICENSE, UNITY_EMAIL, UNITY_PASSWORD

# 3. Workflows run automatically on push/PR
# Or trigger manually from Actions tab
```

**Available Workflows**:
- **Unity Test Runner**: Automated EditMode and PlayMode tests
- **Unity Build**: Multi-platform builds (Windows, Linux, macOS)
- **Unity Package Validation**: Package structure and integrity checks
- **NeonVault CI**: Complete pipeline with JSON import, tests, and release

**Features**:
- ✅ Automatic testing on every push
- ✅ Multi-platform builds with artifact uploads
- ✅ Package validation and distribution
- ✅ GitHub Release creation for version tags
- ✅ Library caching for 5-10x faster builds
- ✅ Test result reporting in PRs

**Documentation**: See [UNITY_CI_WORKFLOWS_GUIDE.md](UNITY_CI_WORKFLOWS_GUIDE.md) for complete setup instructions.

### Node.js API Setup

**Install Dependencies**:
```bash
# Replay Mint API
cd api
npm install
node replay-mint-api.js # Port 3002

# DAO Replay Impact API
node dao-replay-impact-api.js # Port 3004

# Remix Forge Graph API
node remix-forge-graph-api.js # Port 3003
```

**Environment Variables**:
```
JWT_SECRET=your_jwt_secret_here
PORT=3002
DB_CONNECTION=your_db_connection_string
```

### CLI Tools Setup

**Install**:
```bash
# Badge Forge CLI
cd cli
npm install
npm link

# Usage
soulvan-badge forge C001
soulvan-badge list

# Replay Package CLI
cd cli
npm install
npm link

# Usage
soulvan-replay bundle replay_1.json replay_2.json
soulvan-replay remix R1234 storm-variant
```

---

## Documentation

### Complete Guides

- **[NEON_VAULT_HEIST_GUIDE.md](NEON_VAULT_HEIST_GUIDE.md)** — Complete Neon Vault Heist system documentation
  - Architecture & mission flow
  - Component reference for all 20+ scripts
  - ScriptableObjects guide
  - Setup & integration guide
  - Troubleshooting

- **[COMPLETE_SYSTEM_GUIDE.md](COMPLETE_SYSTEM_GUIDE.md)** — Full ecosystem guide
  - System architecture
  - Blockchain integration
  - Unity implementation
  - API endpoints
  - CLI tool usage

- **[MISSION_SYSTEM_GUIDE.md](MISSION_SYSTEM_GUIDE.md)** — Mission system deep dive
  - State machine architecture
  - Heat system mechanics
  - Role bonuses & abilities
  - Datacore tier selection

- **[REMIX_FORGE_GUIDE.md](REMIX_FORGE_GUIDE.md)** — RemixForge saga system
  - Remix seed generation
  - Saga variant creation
  - Echo trail visualization
  - Contributor scrolls

- **[BADGE_FORGE_GUIDE.md](BADGE_FORGE_GUIDE.md)** — Badge forging system
  - SVG badge generation
  - Tier progression
  - DAO power visualization

- **[UNITY_TOOLING_GUIDE.md](UNITY_TOOLING_GUIDE.md)** — Unity development tools
  - Editor setup and configuration
  - ScriptableObject importers
  - Testing framework
  - Command-line tools
  - Performance optimization

- **[UNITY_CI_WORKFLOWS_GUIDE.md](UNITY_CI_WORKFLOWS_GUIDE.md)** — CI/CD automation
  - GitHub Actions workflows
  - Unity Test Runner integration
  - Multi-platform builds
  - Package validation
  - License activation guide

### Quick References

- **[DEVELOPER_QUICKREF.md](DEVELOPER_QUICKREF.md)** — Cheat sheet for developers
- **[FEATURE_MANIFEST.md](FEATURE_MANIFEST.md)** — Complete feature list
- **[ARCHITECTURE.md](ARCHITECTURE.md)** — System architecture diagrams
- **[CI_SETUP_GUIDE.md](CI_SETUP_GUIDE.md)** — CI secrets and configuration

---

## Key Features

### 🎮 Neon Vault Heist Missions

**GTA-Style Co-op Heists** with procedural generation:
- **7-Beat Structure**: Infiltration → Breach → Extraction → Chase
- **4-Player Roles**: Driver (1.2x credits), Infiltrator (1.1x), Systems (1.15x), Support (1.0x)
- **Adaptive AI Director**: Tension-based difficulty scaling from heat, time, alerts
- **Datacore Tiers**: Standard (1000c), Rare (2500c), Mythic (5000c) based on performance
- **Deterministic Replays**: Server-signed seeds for RemixForge saga variants

**Gameplay Systems**:
- **Pattern Hacking**: Memory-based vault breach minigame
- **Hover Vehicles**: Arcade physics with boost mechanics (2x speed, 3s duration)
- **Pursuit AI**: Dynamic chase enemies with ramming (500 force) and shooting
- **Cinematic Cutscenes**: Spline-based camera paths with FOV control

### 🏛️ DAO Governance Overlay

**Real-time DAO Visualization** integrated with Unity graph:
- **Proposal Nodes**: Pulse with vote activity (Gold=YES, Red=NO, Blue=ABSTAIN)
- **Vote Ripples**: Expanding waves across affected contributor nodes
- **Tier Upgrades**: Cinematic bursts with voice line "You rise. The vault remembers."
- **Governance Trails**: LineRenderer paths showing contributor → proposal influence

**Heat-to-Influence Conversion**:
```
50 heat → 5.0 DAO influence
100 heat → 10.0 DAO influence (tier upgrade threshold)
```

**Tier Progression**: Initiate → Builder → Architect → Oracle → Legend

### 🔄 RemixForge Saga System

**Procedural Saga Remixing** from replay seeds:
- **Remix Variants**: Storm-tinted, Calm-filtered, Cosmic-warped visual/audio mods
- **Echo Trails**: Lineage trees showing original → remix → sub-remix
- **Contributor Scrolls**: SVG badges with tier, DAO power, badges earned
- **Provenance**: Server-signed HMAC for replay authenticity

### 🔐 Anti-Cheat & Security

**Server-Authoritative Networking**:
- **State Sync**: 0.5s intervals with position/heat validation
- **Heat Validation**: Max 50 heat change/second (exceeded = correction)
- **Position Validation**: Max 5m deviation (exceeded = teleport correction)
- **Event Validation**: Critical events verified by server before acceptance
- **Replay Signatures**: HMAC-SHA256 with server secret (client cannot forge)

---

## Architecture

### Mission Flow Diagram

```
[Player] → [MissionController]
              ↓
        [Procedural Composer]
              ↓ (select modules)
        [Module Activation]
              ↓ (calculate heat)
        [AIDirector_Heist]
              ↓ (spawn scaling)
        [SpawnManager]
              ↓ (enemy waves)
        [PursuitAI]
              ↓ (chase/combat)
        [Mission Complete]
              ↓
        [Choose DatacoreTier]
              ↓ (performance score)
        [ReplaySeedLogger]
              ↓ (queue + retry)
        [POST /replay/log]
              ↓ (server signs)
        [DAOMissionNotifier]
              ↓ (calculate influence)
        [POST /dao/impact]
              ↓ (update DAO power)
        [DAOOverlayFXManager]
              ↓ (visualize)
        [RemixForgeGraph]
```

### Data Layer

**ScriptableObjects** for data-driven design:
- **SO_MissionModule**: 6 procedural modules with entry/exit tags
- **SO_DatacoreTier**: 3 reward tiers with performance requirements
- **SO_HeatModifiers**: 15 action modifiers (-2 to +50 heat)
- **SO_VehicleBlueprint**: 3 vehicle variants with upgrade slots

**JSON Seed Data** for editor import:
- `mission-modules.json`: Approach, breach, extraction modules
- `datacore-tiers.json`: Standard, rare, mythic tiers
- `heat-modifiers.json`: All heat-affecting actions
- `vehicle-blueprints.json`: Prototype Hover-X, Enforcer, Phantom

---

## API Endpoints

### Replay Mint API (Port 3002)

```
POST /replay/bundle
  Body: { actions: [...] }
  Response: { replayId, bundleHash }

POST /replay/remix
  Body: { parentReplayId, variant }
  Response: { remixId, remixSeed, echoTrail }

POST /replay/log
  Body: { missionId, timestamp, modules, contributorId, heat }
  Response: { replayId, signedSeed, remixSeed }

GET /replay/:replayId
  Response: { replayId, actions, lineage, metadata }
```

### DAO Replay Impact API (Port 3004)

```
POST /dao/impact
  Body: { proposalId, contributorId, replayId, heatDelta, influenceScore }
  Response: { impactId, rippleNodes[], tierUpgrade }

POST /dao/vote
  Body: { proposalId, contributorId, vote, votePower }
  Response: { voteId, rippleNodes[], intensity }

GET /dao/proposals
  Response: [ { id, title, votes {yes, no, abstain}, status } ]

GET /dao/vote-ripples/:proposalId
  Response: { rippleNodes[], intensity, rippleRadius }

GET /dao/contributor-stats/:id
  Response: { contributorId, tier, daoPower, badges[], influence }
```

### Remix Forge Graph API (Port 3003)

```
GET /remix-forge/lineage/:replayId
  Response: { lineage: [...], depth, variants }

GET /remix-forge/echo/:replayId
  Response: { echoTrail: [...], influenceScore }

POST /remix-forge/scroll
  Body: { contributorId, badges, tier }
  Response: { scrollSvg, metadata }

POST /remix-forge/dao-ripple
  Body: { proposalId, contributorId, intensity }
  Response: { affectedNodes[], visualData }
```

---

## Testing

### Blockchain Tests

```bash
# Run full test suite
npx hardhat test

# Test specific contract
npx hardhat test test/Soulvan.test.js --grep "SoulvanCoin"

# Coverage report
npx hardhat coverage
```

### Unity Play Mode Tests

**Manual Testing**:
1. Open `NeonVaultHeist.unity` scene
2. Press Play in Unity Editor
3. Use debug overlay: Press `~` to open console
4. Commands:
   - `mission.start()` — Start mission
   - `mission.heat(50)` — Add 50 heat
   - `mission.complete()` — Force complete
   - `dao.proposal("D163")` — Create test proposal
   - `dao.vote("D163", "YES")` — Cast vote

**Debug Overlay Tools**:
- Heat slider: Manually adjust heat 0-100
- Tension slider: Override AIDirector tension 0-1
- Spawn button: Force enemy spawn wave
- Role selector: Change player role on-the-fly

### API Integration Tests

```bash
# Install test dependencies
npm install --save-dev mocha chai supertest

# Run API tests
cd api
npm test

# Test specific endpoint
npm test -- --grep "POST /replay/log"
```

---

## Deployment

### Blockchain Deployment

**Testnet (Sepolia)**:
```bash
npx hardhat run scripts/deploy.js --network sepolia
```

**Mainnet**:
```bash
npx hardhat run scripts/deploy.js --network mainnet
```

**Verify Contracts**:
```bash
npx hardhat verify --network sepolia DEPLOYED_ADDRESS "Constructor Arg1" "Arg2"
```

### Unity Build

**Windows Standalone**:
1. File → Build Settings
2. Platform: Windows (x86_64)
3. Scenes: Add `NeonVaultHeist.unity`
4. Build and Run

**Configuration**:
- API URLs: Set in ServerRpcClient prefab before build
- Graphics: High (HDRP required)
- Scripting Backend: IL2CPP (for performance)

### API Deployment

**Docker Deployment**:
```bash
# Build images
docker-compose build

# Start services
docker-compose up -d

# Check logs
docker-compose logs -f replay-mint-api
```

**Manual Deployment**:
```bash
# Install PM2
npm install -g pm2

# Start APIs
pm2 start api/replay-mint-api.js --name replay-mint
pm2 start api/dao-replay-impact-api.js --name dao-impact
pm2 start api/remix-forge-graph-api.js --name remix-forge

# Monitor
pm2 monit
```

---

## Contributing

**Development Workflow**:
1. Fork repository
2. Create feature branch: `git checkout -b feature/neon-chase`
3. Commit changes: `git commit -m "Add neon chase AI"`
4. Push to branch: `git push origin feature/neon-chase`
5. Open Pull Request

**Code Standards**:
- **Solidity**: Follow OpenZeppelin style guide
- **C# (Unity)**: Follow Microsoft C# conventions
- **JavaScript**: ESLint with Airbnb style guide

**Testing Requirements**:
- All new contracts must have >80% test coverage
- Unity scripts must include XML documentation
- API endpoints must have integration tests

---

## License

MIT License - See [LICENSE](LICENSE) for details

---

## Support

**Documentation**: All guides in root directory
**Issues**: [GitHub Issues](https://github.com/44547/Soulvan/issues)
**Discord**: [Soulvan Community](#) (invite link)

---

**© 2024 Soulvan Project** — Neon-Noir Universe with DAO Governance & Procedural Saga Remixing

Deploy to localhost:

```bash
npx hardhat run scripts/deploy.js --network localhost
```

## Chronicle Entries

`SoulvanChronicle` stores minimal on-chain data plus a content hash to a richer JSON payload (IPFS/Arweave). Entry fields:
- `timestamp` (block time)
- `actor` (player or system)
- `entryType` (0 Race, 1 Mission, 2 Governance, 3 Reward, 6 Season, 7 Motif)
- `contentHash` (bytes32)

## Seasonal Arcs & Motifs

`SoulvanSeasonManager` rotates between Storm (0), Calm (1), and Cosmic (2) arcs. Each season can update active motif packs:
- `visual` — shader/VFX identifiers (e.g., `"storm_v1"`, `"calm_sunrise"`, `"cosmic_aurora"`)
- `audio` — FMOD/Wwise bank IDs
- `haptic` — vibration pattern IDs

Game clients read `activeSeason` and `activeMotifs` to overlay cinematic visual/audio/haptic effects.

## Mission Flow (GTA-style)

1. **Admin** creates mission via `MissionRegistry.createMission(missionType, environmentType, rewardSVN, contentHash)`.
   - `missionType`: 0 deliver, 1 heist, 2 boss, 3 race-assist, etc.
   - `environmentType`: 0 city, 1 industrial, 2 cosmic, 3 mountain, etc.
2. **Executor** (game server or authorized module) calls `completeMission(id, player, resultHash)` after player completes objectives.
3. Registry mints SVN reward to player and logs result to Chronicle (entryType 1).
4. Mission marked complete; player cannot repeat.

Example off-chain flow:
- Player enters heist mission in neon city.
- On success, game server signs completion proof, submits `completeMission` tx.
- Player receives SVN + cinematic replay NFT minted separately.

## Governance Flow (Simplified)
1. Account with vote power and `PROPOSER_ROLE` calls `propose(target, callData, description)`.
2. After `votingDelay` blocks, voting opens until `endBlock`.
3. Weight = `getPastVotes(voter, startBlock)`.
4. If `forVotes > againstVotes`, anyone can `execute(id)` to perform the encoded call on the target.

## Badge Forge System

**Soulvan Badge Forge** provides comprehensive contributor badge management with minting, upgrades, and interactive graph visualization.

### Features
- **6 Badge Tiers**: Initiate → Builder → Architect → Oracle → Operative → Legend
- **DAO Integration**: Badge upgrades triggered by vote impact
- **Interactive Graph**: 3D visualization of contributor network with lore links
- **NFT Exports**: Mint badges as ERC-721 tokens
- **CLI Tool**: Command-line interface for badge operations
- **RESTful API**: 14 endpoints for badge and graph management

### Quick Start
```bash
# Install CLI
npm install -g @soulvan/badge-cli

# Mint a badge
soulvan badge mint --type "Architect" --level 3 --export

# Upgrade badge
soulvan badge upgrade --from "Builder" --to "Oracle" --reason "DAO vote impact"

# Export as NFT
soulvan badge export --type "Architect" --scroll --nft

# View history
soulvan badge history --contributor "Brian"
```

### Documentation
- **Badge Forge Guide**: See [BADGE_FORGE_GUIDE.md](BADGE_FORGE_GUIDE.md)
- **Complete System Guide**: See [COMPLETE_SYSTEM_GUIDE.md](COMPLETE_SYSTEM_GUIDE.md)
- **API Reference**: [api/badge-forge-api.js](api/badge-forge-api.js)
- **Unity Integration**: [UnityHDRP/Scripts/Systems/BadgeSystem.cs](UnityHDRP/Scripts/Systems/BadgeSystem.cs)

## Remix Forge System

**Soulvan Remix Forge** enables contributors to bundle missions into saga replays, remix graph-linked missions into alternate storylines, and visualize contributor networks with interactive 3D graphs.

### Features
- **Replay Bundling**: Combine missions into cinematic saga episodes
- **Saga Remixing**: Create alternate storyline branches from lore fragments
- **3D Graph Visualization**: Interactive contributor network with echo trails
- **Scroll Orbits**: Floating lore scrolls around remix nodes
- **DAO Ripples**: Wave propagation from vote events across the graph
- **Cutscene System**: 5 cinematic types (Divergence, Echo, Scroll, Lineage, DAO Impact)
- **Multi-Platform**: Unity HDRP, Unreal Engine 5, and Web Dashboard support

### Quick Start
```bash
# Install Replay CLI
npm install -g @soulvan/replay-cli

# Bundle missions into replay
soulvan replay bundle --missions "Vault Breach, Oracle Echo" --narration "Brian's Rise" --mint

# Remix saga branch
soulvan replay remix --fragments "F001, F007" --combine --mint

# Export replay
soulvan replay export --replay R163 --scroll --nft --bundle

# DAO trigger
soulvan replay dao-trigger --proposal D163 --vote YES --impact "Replay bundle minted"

# View history
soulvan replay history --contributor C001
```

### Documentation
- **Remix Forge Guide**: See [REMIX_FORGE_GUIDE.md](REMIX_FORGE_GUIDE.md)
- **Replay API**: [api/replay-mint-api.js](api/replay-mint-api.js)
- **Graph API**: [api/remix-forge-graph-api.js](api/remix-forge-graph-api.js)
- **Unity Integration**: [UnityHDRP/Scripts/Systems/RemixForgeGraph.cs](UnityHDRP/Scripts/Systems/RemixForgeGraph.cs)
- **Unreal Integration**: [UnrealEngine5/Source/Soulvan/RemixForgeBlueprints.cpp](UnrealEngine5/Source/Soulvan/RemixForgeBlueprints.cpp)

### Unity Components
- **RemixForgeGraph.cs**: 3D graph with lineage paths, echo trails, scroll orbits
- **RemixForgeGraphComponents.cs**: RemixNode, EchoTrail, ScrollOrbit classes
- **RemixForgeCutsceneKit.cs**: 5 cutscene types with camera paths

### Unreal Blueprints
- **BP_RemixLineage**: Remix ancestry path renderer
- **BP_RemixEcho**: Saga echo trail animator
- **BP_ScrollConstellation**: Lore scroll orbit display
- **BP_RemixDivergence**: Cinematic divergence reveal
- **BP_DAORippleEvent**: DAO vote ripple propagation

### Voice Lines
- *"The saga converges. Your legend echoes."* (Bundle)
- *"Your legend diverges. The vault remembers."* (Remix)
- *"Your saga echoes beyond the vault."* (Export)
- *"The DAO echoes. Your vote ripples through the saga."* (DAO Trigger)
- *"Your remix echoes through the vault."* (Echo Ripple)

## Roadmap
- Multi-target proposals/timelock
- Subgraph indexer for Chronicle (event indexing for races/missions/seasons)
- Emissions/staking for SVN
- In-game role bridges to control minting/logging
- VR/AR chronicle replay sessions
- Cinematic cutscene NFT minting
- Boss battle expansions & multiplayer racing
- DAO-authored seasonal quest scripts
- **Badge Forge Web Dashboard** (In Progress)
- **Multiverse Bridge** portal system
- **Chrono Forge** timeline editor
- **Mythic Replay Engine** with voice narration

## Security
Prototype only; unaudited. Use small balances until audited. Add timelock and multi-sig for production roles.

## License
MIT