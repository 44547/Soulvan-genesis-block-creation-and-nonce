# 🛡️ Soulvan Badge Forge - Complete Documentation

## 📋 Overview

The **Soulvan Badge Forge** is a comprehensive contributor badge management system featuring minting, upgrades, DAO integration, and interactive graph visualization. Contributors earn badges through XP milestones, DAO participation, and lore contributions.

---

## 🎯 Core Features

### **1. Badge Minting**
- 6 badge tiers: Initiate → Builder → Architect → Oracle → Operative → Legend
- Level-based progression (1-10 per tier)
- Automatic lore recording on blockchain
- Cinematic unlock animations with Soulvan voice: *"Your legend grows. The saga remembers."*

### **2. Badge Upgrades**
- DAO vote impact triggers automatic upgrades
- Lore milestone achievements unlock tier progression
- DAO power scaling: 1 → 5 → 25 → 100 → 50 → 500

### **3. Badge Exports**
- **Lore-bound scrolls**: JSON format with badge history
- **NFT minting**: ERC-721 tokens on blockchain
- OpenSea integration for trading

### **4. Badge Vault Graph**
- Interactive 3D visualization of contributor network
- Real-time lore links between contributors
- DAO ripple effects propagating across graph
- Badge tier pulse animations

---

## 🔧 CLI Usage

### **Installation**
```bash
npm install -g @soulvan/badge-cli
```

### **Commands**

#### **Mint Badge**
```bash
soulvan badge mint --type "Architect" --level 3 --export
```
**Output:**
```
🎖️  Minting Contributor Badge...
✅ Badge Type: Architect
✅ Badge Level: 3
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Badge ID: BADGE_ARCHITECT_XYZ123
DAO Power: +25
📜 Recording to Soulvan Chronicle...
🎙️  Soulvan: "Your legend grows. The saga remembers."
```

#### **Upgrade Badge**
```bash
soulvan badge upgrade --from "Builder" --to "Oracle" --reason "DAO vote impact"
```
**Output:**
```
⬆️  Upgrading Contributor Badge...
Current Tier: Builder
New Tier: Oracle
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Builder → Oracle
DAO Power: 5 → 100 (+95)
🎙️  Soulvan: "Your power grows. The vault recognizes your legend."
```

#### **Export Badge**
```bash
soulvan badge export --type "Architect" --scroll --nft
```
**Output:**
```
📤 Exporting Contributor Badge...
📜 Scroll exported: badge_scroll_1699459200.json
🔱 NFT minted: NFT_BADGE_XYZ
Token ID: #7342
View on OpenSea: https://opensea.io/assets/soulvan/7342
```

#### **View Badge History**
```bash
soulvan badge history --contributor "Brian"
```
**Output:**
```
📜 Badge History
Contributor: Brian
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[1] 2025-11-01
    Badge: Initiate (Level 1)
    Lore: First login to Soulvan universe

[2] 2025-11-05
    Badge: Builder (Level 2)
    Lore: Completed 5 missions in Tokyo

[3] 2025-11-08
    Badge: Architect (Level 3)
    Lore: DAO vote impact milestone

✅ Total Badges Earned: 3
```

#### **Badge Stats**
```bash
soulvan badge stats --contributor "Brian"
```
**Output:**
```
📊 Badge Statistics
Contributor: Brian
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Current Tier: Architect
Badge Level: 3
DAO Power: 25
Total XP: 2,450
Badges Earned: 3
Lore Entries: 12
DAO Votes Cast: 8
```

#### **DAO Trigger**
```bash
soulvan badge dao-trigger --proposal "D163" --vote "YES" --impact "Badge upgrade to Operative"
```
**Output:**
```
🗳️  DAO Vote Badge Trigger
Proposal: D163
Vote: YES
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🗳️  Casting DAO vote...
✅ Vote registered with power: 42
🎖️  Badge upgrade triggered!
🌟 Tier progression: Builder → Architect
⚡ DAO Power increased: +20
🎙️  Soulvan: "Your voice shapes the saga. The vault rewards your wisdom."
```

---

## 🌐 API Endpoints

### **Base URL**
```
https://api.soulvan.io/v1
```

### **Authentication**
```javascript
POST /auth/init
Body: {
  "wallet": "0xABC123",
  "name": "Brian"
}
Response: {
  "success": true,
  "contributorId": "C001",
  "accessToken": "jwt_token_here",
  "expiresIn": 3600
}
```

### **Badge Endpoints**

#### **Mint Badge**
```javascript
POST /badge/mint
Headers: { "Authorization": "Bearer <token>" }
Body: {
  "contributorId": "C001",
  "type": "Architect",
  "level": 3,
  "reason": "Reached 2000 XP milestone"
}
Response: {
  "success": true,
  "badgeId": "BADGE_ARCHITECT_XYZ",
  "type": "Architect",
  "level": 3,
  "daoPower": 25,
  "mintedAt": "2025-11-08T12:00:00Z"
}
```

#### **Upgrade Badge**
```javascript
POST /badge/upgrade
Body: {
  "contributorId": "C001",
  "fromTier": "Builder",
  "toTier": "Oracle",
  "reason": "DAO vote impact milestone reached"
}
Response: {
  "success": true,
  "fromTier": "Builder",
  "toTier": "Oracle",
  "daoPowerGain": 95,
  "upgradedAt": "2025-11-08T12:00:00Z"
}
```

#### **Export Badge**
```javascript
POST /badge/export
Body: {
  "contributorId": "C001",
  "badgeId": "BADGE_ARCHITECT_XYZ",
  "format": ["scroll", "nft"]
}
Response: {
  "success": true,
  "scroll": {
    "url": "https://cdn.soulvan.io/scrolls/badge_scroll_123.json",
    "hash": "0xabc123..."
  },
  "nft": {
    "tokenId": "1234",
    "contract": "0xSoulvanBadgeNFT",
    "openseaUrl": "https://opensea.io/assets/soulvan/1234"
  }
}
```

#### **Badge History**
```javascript
GET /badge/contributor/:id
Headers: { "Authorization": "Bearer <token>" }
Response: {
  "contributorId": "C001",
  "badges": [
    {
      "badgeId": "BADGE_INITIATE_ABC",
      "type": "Initiate",
      "level": 1,
      "earnedAt": "2025-11-01T00:00:00Z",
      "loreLink": "First login to Soulvan universe"
    }
  ],
  "totalBadges": 3
}
```

#### **Badge Stats**
```javascript
GET /badge/stats/:id
Response: {
  "contributorId": "C001",
  "currentTier": "Architect",
  "level": 3,
  "daoPower": 25,
  "totalXP": 2450,
  "badgesEarned": 3,
  "loreEntries": 12,
  "daoVotesCast": 8
}
```

### **Graph Endpoints**

#### **All Contributors**
```javascript
GET /graph/contributors
Response: {
  "contributors": [
    { "id": "C001", "name": "Brian", "tier": "Architect", "lore": 12, "daoPower": 25 },
    { "id": "C002", "name": "Alice", "tier": "Oracle", "lore": 45, "daoPower": 100 }
  ],
  "totalContributors": 42
}
```

#### **Lore Links**
```javascript
GET /graph/lore-links/:id
Response: {
  "contributorId": "C001",
  "links": [
    {
      "linkedContributor": "C002",
      "loreContext": "Vault Breach collaboration",
      "strength": 3
    }
  ]
}
```

#### **DAO Ripples**
```javascript
GET /graph/dao-ripples/:proposalId
Response: {
  "proposalId": "D163",
  "originContributor": "C001",
  "rippleRadius": 50,
  "impactedContributors": 12,
  "badgeUpgrades": 3
}
```

#### **Badge Pulses**
```javascript
GET /graph/badge-pulses
Response: {
  "pulses": [
    { "tier": "Oracle", "pulseColor": "#00FFFF", "runeFX": "flare", "intensity": 0.8 }
  ]
}
```

---

## 🎮 Unity Integration

### **Badge System Component**
```csharp
BadgeSystem badgeSystem = FindObjectOfType<BadgeSystem>();

// Mint badge
badgeSystem.MintBadge("C001", "Architect", "Reached 2000 XP");

// Upgrade badge
badgeSystem.UpgradeBadge("C001", "Builder", "Oracle", "DAO vote impact");

// Track vote impact
badgeSystem.TrackVoteImpact("C001", 42);

// Export badge
badgeSystem.ExportBadge("C001", "BADGE_ARCHITECT_XYZ");
```

### **Badge Vault Graph**
```csharp
BadgeVaultGraph graph = FindObjectOfType<BadgeVaultGraph>();

// Trigger DAO ripple
graph.TriggerDAORipple("D163", "C001", 42);

// Trigger tier pulse
graph.TriggerTierPulse("C001", "Oracle");

// Get node data
ContributorNodeData nodeData = graph.GetNodeData("C001");

// Export graph data
string jsonData = graph.ExportGraphData();
```

---

## 📊 Badge Tier System

| Tier | DAO Power | Required XP | Description |
|------|-----------|-------------|-------------|
| **Initiate** | 1 | 0 | First step in the saga |
| **Builder** | 5 | 500 | Contributor to the Soulvan world |
| **Architect** | 25 | 2,000 | Designer of saga branches |
| **Oracle** | 100 | 10,000 | Keeper of lore and prophecy |
| **Operative** | 50 | 5,000 | Master of missions and combat |
| **Legend** | 500 | 50,000 | Mythic contributor of the saga |

---

## 🎨 Visual Effects

### **Badge Tier Colors**
- **Initiate**: Gray (#808080)
- **Builder**: Cyan (#00CCFF)
- **Architect**: Purple (#CC00FF)
- **Oracle**: Gold (#FFCC00)
- **Operative**: Red (#FF0000)
- **Legend**: Yellow (#FFFF00)

### **Rune FX**
- **Glow**: Initiate unlock (30% intensity)
- **Spark**: Builder unlock (50% intensity)
- **Burst**: Architect unlock (70% intensity)
- **Flare**: Oracle unlock (90% intensity)
- **Explosion**: Operative unlock (80% intensity)
- **Supernova**: Legend unlock (100% intensity)

---

## 🔗 Integration Checklist

- [x] CLI tool with 6 commands
- [x] RESTful API with 14 endpoints
- [x] Unity BadgeSystem.cs (380 lines)
- [x] Unity BadgeVaultGraph.cs (420 lines)
- [x] Interactive graph visualization
- [x] DAO ripple effects
- [x] Badge tier pulse animations
- [x] Lore link connections
- [x] NFT minting integration
- [x] OpenSea export
- [x] Blockchain recording
- [x] Cinematic cutscenes

---

## 🚀 Quick Start

### **1. Install CLI**
```bash
npm install -g @soulvan/badge-cli
```

### **2. Initialize Contributor**
```bash
soulvan badge mint --type "Initiate" --level 1
```

### **3. Complete Missions & Earn XP**
```bash
# Through gameplay or API
curl -X POST https://api.soulvan.io/v1/badge/mint \
  -H "Authorization: Bearer <token>" \
  -d '{"contributorId":"C001","type":"Builder","level":2}'
```

### **4. Cast DAO Vote**
```bash
soulvan badge dao-trigger --proposal "D163" --vote "YES"
```

### **5. Upgrade Badge**
```bash
soulvan badge upgrade --from "Builder" --to "Architect" --reason "DAO impact"
```

### **6. Export Badge**
```bash
soulvan badge export --type "Architect" --scroll --nft
```

---

## 🎙️ Soulvan Voice Lines

- **Badge Mint**: *"Your legend grows. The saga remembers."*
- **Badge Upgrade**: *"Your power grows. The vault recognizes your legend."*
- **DAO Trigger**: *"Your voice shapes the saga. The vault rewards your wisdom."*
- **Export Success**: *"Your deeds are eternal. The chronicle is written."*

---

## 📞 Support

For questions or contributions, visit:
- **GitHub**: https://github.com/soulvan/badge-forge
- **Discord**: https://discord.gg/soulvan
- **API Docs**: https://api.soulvan.io/docs

**Built with Unity HDRP | Powered by Blockchain | Forged in the Vault**
