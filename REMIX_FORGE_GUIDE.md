# Soulvan Remix Forge System Guide

**Remix the saga. Diverge the legend.**

The Soulvan Remix Forge is a comprehensive system for bundling missions into saga replays, remixing graph-linked missions into alternate storylines, and visualizing contributor networks with interactive 3D graphs.

---

## 📑 Table of Contents

1. [Overview](#overview)
2. [CLI Usage](#cli-usage)
3. [Replay Mint API](#replay-mint-api)
4. [Remix Forge Graph API](#remix-forge-graph-api)
5. [Unity Integration](#unity-integration)
6. [Unreal Engine Integration](#unreal-engine-integration)
7. [Visual Effects Reference](#visual-effects-reference)
8. [Voice Lines](#voice-lines)
9. [Quick Start](#quick-start)

---

## Overview

The Remix Forge ecosystem consists of:

### **Replay Mint System**
- **CLI Tool**: Bundle missions, remix sagas, export replays
- **API**: RESTful endpoints for replay operations
- **Blockchain**: All replays recorded to SoulvanChronicle

### **Remix Forge Graph**
- **3D Visualization**: Interactive contributor network
- **Echo Trails**: Animated connections showing remix lineage
- **Scroll Orbits**: Floating lore scrolls around nodes
- **DAO Ripples**: Wave propagation from vote events

### **Cutscene System**
- **Divergence Reveal**: Glowing rune trails for alternate paths
- **Echo Ripple**: Expanding waves across contributors
- **Scroll Constellation**: Orbiting scroll cinematics
- **Lineage Path**: Camera sweep through ancestry
- **DAO Impact**: Ripple visualization

---

## CLI Usage

### Installation

```bash
npm install -g @soulvan/replay-cli
```

### Authentication

Set your API endpoint in `.env`:
```
SOULVAN_API_URL=http://localhost:3002
```

### Commands

#### 1. Bundle Missions

Bundle missions into a saga replay episode:

```bash
soulvan replay bundle --missions "Vault Breach, Oracle Echo" --narration "Brian's Rise" --mint
```

**Output:**
```
╔═══════════════════════════════════════════════════════╗
║   SOULVAN REPLAY FORGE                                ║
║   Remix the saga. Diverge the legend.                 ║
╚═══════════════════════════════════════════════════════╝

🎞️  Bundling Missions into Saga Replay...

Missions:
  • Vault Breach
  • Oracle Echo

✅ Replay Bundled Successfully!

Replay ID: R163
Missions: 2 episodes
Duration: 6m
Minted: 2025-11-08T10:30:00Z
🎖️  NFT Minted!

"The saga converges. Your legend echoes."
```

#### 2. Remix Saga Branch

Remix graph-linked missions into alternate storyline:

```bash
soulvan replay remix --fragments "F001, F007" --combine --mint
```

**Output:**
```
🔀 Remixing Saga Branch...

Lore Fragments:
  🧩 F001
  🧩 F007

✅ Remix Created Successfully!

Remix ID: RX042
Branches: 1 alternate paths
Divergence: Mission 7
Minted: 2025-11-08T10:35:00Z
🎖️  Remix NFT Forged!

"Your legend diverges. The vault remembers."
```

#### 3. Export Replay

Export replay as scroll, NFT, or bundle:

```bash
soulvan replay export --replay R163 --scroll --nft --bundle
```

**Output:**
```
📜 Exporting Replay...

Export Formats:
  📦 scroll
  📦 nft
  📦 bundle

✅ Replay Exported Successfully!

📜 Scroll: https://soulvan.io/scrolls/R163.json
   Hash: 0xa7f3...

🎖️  NFT: Token #4567
   Contract: 0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb
   OpenSea: https://opensea.io/assets/...

📦 Bundle: https://soulvan.io/bundles/R163.zip
   Size: 32MB

Exported: 2025-11-08T10:40:00Z

"Your saga echoes beyond the vault."
```

#### 4. DAO Trigger

Trigger replay minting from DAO vote impact:

```bash
soulvan replay dao-trigger --proposal D163 --vote YES --impact "Replay bundle minted"
```

**Output:**
```
⚡ Triggering DAO Replay Mint...

Proposal: D163
Vote: YES
Impact: Replay bundle minted

✅ DAO Replay Triggered!

Replay ID: RD089
Ripple Nodes: 17 contributors affected
Minted: 2025-11-08T10:45:00Z

"The DAO echoes. Your vote ripples through the saga."
```

#### 5. View History

View contributor replay history:

```bash
soulvan replay history --contributor C001
```

**Output:**
```
📜 Replay History

Contributor: C001

1. Brian's Rise
   ID: R163
   Missions: Vault Breach, Oracle Echo
   Created: 2025-11-01T10:00:00Z
   🔀 Remixed: 3 branches

2. Forge Divergence
   ID: R007
   Missions: Chrono Forge, Legend Mint
   Created: 2025-11-05T14:30:00Z
```

#### 6. View Statistics

Show replay stats and lore links:

```bash
soulvan replay stats --contributor C001
```

**Output:**
```
📊 Replay Statistics

Contributor: C001
Tier: Oracle

Total Replays: 12
Total Remixes: 7
Lore Links: 34
Divergence Points: 5

"Your saga branches across the vault."
```

---

## Replay Mint API

Base URL: `http://localhost:3002`

### Authentication

**POST** `/auth/init`

Initialize contributor identity and return access token.

**Request:**
```json
{
  "wallet": "0xABC123",
  "name": "Brian"
}
```

**Response:**
```json
{
  "contributorId": "C001",
  "name": "Brian",
  "accessToken": "jwt_token_here",
  "expiresIn": 3600
}
```

### Replay Endpoints

#### Bundle Missions

**POST** `/replay/bundle`

**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "contributorId": "C001",
  "missions": ["Vault Breach", "Oracle Echo"],
  "narration": "Brian's Rise",
  "mint": true
}
```

**Response:**
```json
{
  "replayId": "R163",
  "narration": "Brian's Rise",
  "missionCount": 2,
  "duration": "6m",
  "mintedAt": "2025-11-08T10:30:00Z",
  "message": "The saga converges. Your legend echoes."
}
```

#### Remix Saga

**POST** `/replay/remix`

**Request:**
```json
{
  "contributorId": "C001",
  "fragments": ["F001", "F007"],
  "combine": true,
  "mint": true
}
```

**Response:**
```json
{
  "remixId": "RX042",
  "fragments": 2,
  "branches": 1,
  "divergencePoint": "Mission 7",
  "mintedAt": "2025-11-08T10:35:00Z",
  "message": "Your legend diverges. The vault remembers."
}
```

#### Export Replay

**POST** `/replay/export`

**Request:**
```json
{
  "replayId": "R163",
  "formats": ["scroll", "nft", "bundle"]
}
```

**Response:**
```json
{
  "replayId": "R163",
  "exports": [
    {
      "format": "scroll",
      "url": "https://soulvan.io/scrolls/R163.json",
      "hash": "0xa7f3..."
    },
    {
      "format": "nft",
      "tokenId": 4567,
      "contract": "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
      "openseaUrl": "https://opensea.io/assets/..."
    }
  ],
  "exportedAt": "2025-11-08T10:40:00Z"
}
```

#### DAO Trigger

**POST** `/replay/dao-trigger`

**Request:**
```json
{
  "proposalId": "D163",
  "vote": "YES",
  "impact": "Replay bundle minted",
  "contributorId": "C001"
}
```

**Response:**
```json
{
  "replayId": "RD089",
  "proposalId": "D163",
  "vote": "YES",
  "rippleNodes": 17,
  "mintedAt": "2025-11-08T10:45:00Z"
}
```

#### Get History

**GET** `/replay/contributor/:id`

**Response:**
```json
{
  "contributorId": "C001",
  "replays": [
    {
      "replayId": "R163",
      "narration": "Brian's Rise",
      "missions": ["Vault Breach", "Oracle Echo"],
      "createdAt": "2025-11-01T10:00:00Z",
      "tier": "Oracle",
      "remixed": true,
      "remixCount": 3
    }
  ]
}
```

#### Get Statistics

**GET** `/replay/stats/:id`

**Response:**
```json
{
  "contributorId": "C001",
  "totalReplays": 12,
  "totalRemixes": 7,
  "loreLinks": 34,
  "divergencePoints": 5,
  "tier": "Oracle"
}
```

---

## Remix Forge Graph API

Base URL: `http://localhost:3003`

### Graph Endpoints

#### Get Contributors

**GET** `/remix/graph/contributors`

Returns all contributor nodes in the remix graph.

**Response:**
```json
{
  "contributors": [
    {
      "id": "C001",
      "name": "Brian",
      "tier": "Oracle",
      "remixCount": 12,
      "loreLinks": 34
    }
  ],
  "totalCount": 5
}
```

#### Get Lineage

**GET** `/remix/lineage/:id`

Returns remix ancestry and saga divergence paths.

**Response:**
```json
{
  "id": "C001",
  "lineage": ["Vault Breach", "Oracle Echo", "Forge Divergence"],
  "branches": 3,
  "tier": "Oracle"
}
```

#### Get Echo Map

**GET** `/remix/echo/:id`

Returns ripple effects of remix events.

**Response:**
```json
{
  "remixId": "RX042",
  "echoNodes": ["C001", "C007", "C014"],
  "intensity": "high",
  "rippleRadius": 45
}
```

#### Get Scroll Orbits

**GET** `/remix/scrolls/:id`

Returns lore scrolls orbiting remix nodes.

**Response:**
```json
{
  "contributorId": "C001",
  "scrolls": [
    {
      "title": "Echo of the Oracle",
      "format": "scroll",
      "url": "https://soulvan.io/scrolls/oracle-echo.json"
    }
  ]
}
```

#### Get DAO Ripples

**GET** `/remix/dao-ripples/:proposalId`

Returns DAO-triggered remix ripple data.

**Response:**
```json
{
  "proposalId": "D163",
  "rippleRadius": 60,
  "impactedContributors": ["C001", "C007", "C014"],
  "badgeUpgrades": 3,
  "remixesMinted": 5
}
```

---

## Unity Integration

### Setup

1. Add `RemixForgeGraph.cs` to your scene
2. Configure prefabs: `remixNodePrefab`, `echoTrailPrefab`, `scrollOrbitPrefab`
3. Set API base URL in inspector
4. Add `RemixForgeCutsceneKit.cs` for cinematic cutscenes

### Usage Example

```csharp
// Get RemixForgeGraph component
RemixForgeGraph graph = GetComponent<RemixForgeGraph>();

// Trigger DAO ripple
graph.TriggerDAORipple("D163", "C001", 100);

// Trigger remix burst
graph.TriggerRemixBurst("RX042", "C001");

// Focus camera on node
graph.FocusOnNode("C001");

// Highlight lineage path
List<string> ancestors = new List<string> { "C001", "C007", "C014" };
graph.TriggerLineageGlow("C001", ancestors);
```

### Cutscene Triggers

```csharp
RemixForgeCutsceneKit cutscenes = GetComponent<RemixForgeCutsceneKit>();

// Trigger divergence reveal
cutscenes.TriggerDivergenceReveal("C001", "RX042");

// Trigger echo ripple
List<string> echoNodes = new List<string> { "C007", "C014", "C023" };
cutscenes.TriggerEchoRipple("C001", echoNodes);

// Trigger scroll constellation
cutscenes.TriggerScrollConstellation("C001");

// Trigger lineage path
cutscenes.TriggerLineagePath("C001", ancestors);

// Trigger DAO impact
cutscenes.TriggerDAOImpact("D163", "C001", 100);
```

---

## Unreal Engine Integration

### Blueprint Actors

1. **BP_RemixLineage**: Renders remix ancestry paths
2. **BP_RemixEcho**: Animates saga echo trails
3. **BP_ScrollConstellation**: Displays lore scroll orbits
4. **BP_RemixDivergence**: Cinematic divergence reveal
5. **BP_DAORippleEvent**: DAO vote ripple propagation

### Usage Example

```cpp
// Create BP_RemixLineage actor
ARemixLineageActor* LineageActor = GetWorld()->SpawnActor<ARemixLineageActor>();
LineageActor->ContributorId = "C001";
LineageActor->AncestorIds = {"C007", "C014"};
LineageActor->RenderLineagePath(AncestorPositions);

// Trigger echo ripple
ARemixEchoActor* EchoActor = GetWorld()->SpawnActor<ARemixEchoActor>();
EchoActor->SourceNodeId = "C001";
EchoActor->TriggerEchoRipple(SourcePosition);

// Spawn scroll constellation
AScrollConstellationActor* ScrollActor = GetWorld()->SpawnActor<AScrollConstellationActor>();
ScrollActor->CenterPosition = FVector(0, 0, 0);
ScrollActor->SpawnScrollOrbits();
```

---

## Visual Effects Reference

### Badge Tier Colors

| Tier       | Color Code | RGB          |
|------------|------------|--------------|
| Initiate   | Blue       | (128,128,255)|
| Builder    | Green      | (128,255,128)|
| Architect  | Magenta    | (255,128,255)|
| Oracle     | Cyan       | (128,255,255)|
| Operative  | Yellow     | (255,255,128)|
| Legend     | Gold       | (255,255,0)  |

### Echo Intensity Colors

| Intensity | Color Code | Opacity |
|-----------|------------|---------|
| Low       | Blue       | 30%     |
| Medium    | Cyan       | 60%     |
| High      | Gold       | 90%     |

### Scroll Format Colors

| Format | Color Code | Effect    |
|--------|------------|-----------|
| Scroll | Parchment  | Soft glow |
| NFT    | Magenta    | Pulse     |
| Bundle | Green      | Spin      |

### Rune FX Intensity

| Effect         | Intensity | Description                  |
|----------------|-----------|------------------------------|
| Glow           | 30%       | Soft ambient light           |
| Pulse          | 60%       | Rhythmic brightness changes  |
| Flare          | 85%       | Bright burst with trails     |
| Supernova      | 100%      | Explosive full-screen effect |

---

## Voice Lines

### Replay Mint
- **Bundle**: *"The saga converges. Your legend echoes."*
- **Remix**: *"Your legend diverges. The vault remembers."*
- **Export**: *"Your saga echoes beyond the vault."*
- **DAO Trigger**: *"The DAO echoes. Your vote ripples through the saga."*

### Remix Forge Graph
- **Divergence**: *"Your legend diverges. The vault remembers."*
- **Echo**: *"Your remix echoes through the vault."*
- **Scroll**: *"Your scrolls orbit the forge."*
- **Lineage**: *"The lineage reveals your saga."*
- **DAO Ripple**: *"The DAO ripples through the remix graph."*

---

## Quick Start

### 1. Install CLI
```bash
npm install -g @soulvan/replay-cli
```

### 2. Start API Servers
```bash
# Terminal 1: Replay Mint API
node api/replay-mint-api.js

# Terminal 2: Remix Forge Graph API
node api/remix-forge-graph-api.js
```

### 3. Bundle Your First Replay
```bash
soulvan replay bundle --missions "Mission 1, Mission 2" --narration "My Saga" --mint
```

### 4. Export as NFT
```bash
soulvan replay export --replay R001 --nft
```

### 5. Integrate Unity Graph
```csharp
// Add to your Unity scene
RemixForgeGraph graph = gameObject.AddComponent<RemixForgeGraph>();
graph.apiBaseUrl = "http://localhost:3003";
```

### 6. Trigger Cutscene
```csharp
RemixForgeCutsceneKit cutscenes = gameObject.AddComponent<RemixForgeCutsceneKit>();
cutscenes.TriggerDivergenceReveal("C001", "RX042");
```

---

## Architecture

```
Soulvan Remix Forge Ecosystem
│
├── CLI (soulvan-replay-cli.js)
│   ├── Commands: bundle, remix, export, dao-trigger, history, stats
│   └── Animated terminal output with chalk colors
│
├── Replay Mint API (replay-mint-api.js)
│   ├── Authentication: JWT with 1-hour expiration
│   ├── Endpoints: 7 routes for replay operations
│   └── Blockchain integration: SoulvanChronicle recording
│
├── Remix Forge Graph API (remix-forge-graph-api.js)
│   ├── Endpoints: 8 routes for graph visualization
│   ├── Lineage mapping, echo trails, scroll orbits
│   └── DAO ripple calculations
│
├── Unity HDRP
│   ├── RemixForgeGraph.cs (3D graph visualization)
│   ├── RemixForgeGraphComponents.cs (Node, Trail, Scroll classes)
│   └── RemixForgeCutsceneKit.cs (5 cutscene types)
│
└── Unreal Engine 5
    ├── RemixForgeBlueprints.cpp (5 Blueprint actors)
    └── RemixForgeWidget (UI dashboard)
```

---

## Support

- **Documentation**: [REMIX_FORGE_GUIDE.md](./REMIX_FORGE_GUIDE.md)
- **CLI Reference**: `soulvan replay --help`
- **API Docs**: See [Replay Mint API](#replay-mint-api) and [Remix Forge Graph API](#remix-forge-graph-api)
- **Unity Scripts**: [RemixForgeGraph.cs](./UnityHDRP/Scripts/Systems/RemixForgeGraph.cs)
- **Unreal Blueprints**: [RemixForgeBlueprints.cpp](./UnrealEngine5/Source/Soulvan/RemixForgeBlueprints.cpp)

---

**"Your legend branches across the vault. The saga remembers."**
