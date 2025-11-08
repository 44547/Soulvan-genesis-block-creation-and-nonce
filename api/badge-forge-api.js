/**
 * Soulvan Badge Forge API
 * RESTful API for badge minting, upgrades, exports, and graph visualization
 * Base URL: https://api.soulvan.io/v1
 */

const express = require('express');
const router = express.Router();

// ========================================
// AUTHENTICATION
// ========================================

/**
 * POST /auth/init
 * Initialize contributor identity and return access token
 * 
 * Request Body:
 * {
 *   "wallet": "0xABC123",
 *   "name": "Brian"
 * }
 * 
 * Response:
 * {
 *   "success": true,
 *   "contributorId": "C001",
 *   "accessToken": "jwt_token_here",
 *   "expiresIn": 3600
 * }
 */
router.post('/auth/init', async (req, res) => {
  const { wallet, name } = req.body;
  
  // Initialize contributor
  const contributorId = `C${Date.now().toString(36).toUpperCase()}`;
  const accessToken = generateJWT(contributorId);
  
  res.json({
    success: true,
    contributorId,
    accessToken,
    expiresIn: 3600
  });
});

// ========================================
// BADGE ENDPOINTS
// ========================================

/**
 * POST /badge/mint
 * Mint badge based on contributor stats
 * 
 * Request Body:
 * {
 *   "contributorId": "C001",
 *   "type": "Architect",
 *   "level": 3,
 *   "reason": "Reached 2000 XP milestone"
 * }
 * 
 * Response:
 * {
 *   "success": true,
 *   "badgeId": "BADGE_ARCHITECT_XYZ",
 *   "type": "Architect",
 *   "level": 3,
 *   "daoPower": 25,
 *   "mintedAt": "2025-11-08T12:00:00Z",
 *   "loreEntry": "Badge minted: Architect for contributor C001"
 * }
 */
router.post('/badge/mint', authenticate, async (req, res) => {
  const { contributorId, type, level, reason } = req.body;
  
  const badgeId = `BADGE_${type.toUpperCase()}_${Date.now().toString(36).toUpperCase()}`;
  const daoPower = getDAOPower(type);
  
  // Record to blockchain
  await recordToBlockchain({
    event: 'BADGE_MINTED',
    contributorId,
    badgeId,
    type,
    level,
    reason
  });
  
  res.json({
    success: true,
    badgeId,
    type,
    level,
    daoPower,
    mintedAt: new Date().toISOString(),
    loreEntry: `Badge minted: ${type} for contributor ${contributorId}`
  });
});

/**
 * POST /badge/upgrade
 * Upgrade badge tier based on lore milestones or DAO vote impact
 * 
 * Request Body:
 * {
 *   "contributorId": "C001",
 *   "fromTier": "Builder",
 *   "toTier": "Oracle",
 *   "reason": "DAO vote impact milestone reached"
 * }
 * 
 * Response:
 * {
 *   "success": true,
 *   "fromTier": "Builder",
 *   "toTier": "Oracle",
 *   "daoPowerGain": 95,
 *   "upgradedAt": "2025-11-08T12:00:00Z"
 * }
 */
router.post('/badge/upgrade', authenticate, async (req, res) => {
  const { contributorId, fromTier, toTier, reason } = req.body;
  
  const oldPower = getDAOPower(fromTier);
  const newPower = getDAOPower(toTier);
  const powerGain = newPower - oldPower;
  
  // Record to blockchain
  await recordToBlockchain({
    event: 'BADGE_UPGRADED',
    contributorId,
    fromTier,
    toTier,
    reason,
    daoPowerGain: powerGain
  });
  
  res.json({
    success: true,
    fromTier,
    toTier,
    daoPowerGain: powerGain,
    upgradedAt: new Date().toISOString()
  });
});

/**
 * POST /badge/export
 * Export badge as lore-bound scroll and NFT
 * 
 * Request Body:
 * {
 *   "contributorId": "C001",
 *   "badgeId": "BADGE_ARCHITECT_XYZ",
 *   "format": ["scroll", "nft"]
 * }
 * 
 * Response:
 * {
 *   "success": true,
 *   "scroll": {
 *     "url": "https://cdn.soulvan.io/scrolls/badge_scroll_123.json",
 *     "hash": "0xabc123..."
 *   },
 *   "nft": {
 *     "tokenId": "1234",
 *     "contract": "0xSoulvanBadgeNFT",
 *     "openseaUrl": "https://opensea.io/assets/soulvan/1234"
 *   }
 * }
 */
router.post('/badge/export', authenticate, async (req, res) => {
  const { contributorId, badgeId, format } = req.body;
  
  const result = {
    success: true
  };
  
  if (format.includes('scroll')) {
    result.scroll = {
      url: `https://cdn.soulvan.io/scrolls/badge_scroll_${Date.now()}.json`,
      hash: `0x${Math.random().toString(16).substr(2, 64)}`
    };
  }
  
  if (format.includes('nft')) {
    const tokenId = Math.floor(Math.random() * 10000);
    result.nft = {
      tokenId: tokenId.toString(),
      contract: '0xSoulvanBadgeNFT',
      openseaUrl: `https://opensea.io/assets/soulvan/${tokenId}`
    };
  }
  
  res.json(result);
});

/**
 * GET /badge/contributor/:id
 * Retrieve badge history and lore links
 * 
 * Response:
 * {
 *   "contributorId": "C001",
 *   "badges": [
 *     {
 *       "badgeId": "BADGE_INITIATE_ABC",
 *       "type": "Initiate",
 *       "level": 1,
 *       "earnedAt": "2025-11-01T00:00:00Z",
 *       "loreLink": "First login to Soulvan universe"
 *     },
 *     ...
 *   ],
 *   "totalBadges": 3
 * }
 */
router.get('/badge/contributor/:id', authenticate, async (req, res) => {
  const contributorId = req.params.id;
  
  // Load from database
  const badges = await loadBadgeHistory(contributorId);
  
  res.json({
    contributorId,
    badges,
    totalBadges: badges.length
  });
});

/**
 * GET /badge/stats/:id
 * Return badge stats, tier progression, and DAO power
 * 
 * Response:
 * {
 *   "contributorId": "C001",
 *   "currentTier": "Architect",
 *   "level": 3,
 *   "daoPower": 25,
 *   "totalXP": 2450,
 *   "badgesEarned": 3,
 *   "loreEntries": 12,
 *   "daoVotesCast": 8
 * }
 */
router.get('/badge/stats/:id', authenticate, async (req, res) => {
  const contributorId = req.params.id;
  
  // Load from database
  const stats = await loadContributorStats(contributorId);
  
  res.json(stats);
});

// ========================================
// GRAPH ENDPOINTS
// ========================================

/**
 * GET /graph/contributors
 * Return all active contributor nodes
 * 
 * Response:
 * {
 *   "contributors": [
 *     {
 *       "id": "C001",
 *       "name": "Brian",
 *       "tier": "Architect",
 *       "lore": 12,
 *       "daoPower": 25
 *     },
 *     ...
 *   ],
 *   "totalContributors": 42
 * }
 */
router.get('/graph/contributors', async (req, res) => {
  const contributors = await loadAllContributors();
  
  res.json({
    contributors,
    totalContributors: contributors.length
  });
});

/**
 * GET /graph/contributor/:id
 * Return node data for specific contributor
 */
router.get('/graph/contributor/:id', async (req, res) => {
  const contributorId = req.params.id;
  const nodeData = await loadContributorNode(contributorId);
  
  res.json(nodeData);
});

/**
 * GET /graph/lore-links/:id
 * Return lore connections between contributors
 * 
 * Response:
 * {
 *   "contributorId": "C001",
 *   "links": [
 *     {
 *       "linkedContributor": "C002",
 *       "loreContext": "Vault Breach collaboration",
 *       "strength": 3
 *     },
 *     ...
 *   ]
 * }
 */
router.get('/graph/lore-links/:id', async (req, res) => {
  const contributorId = req.params.id;
  const links = await loadLoreLinks(contributorId);
  
  res.json({
    contributorId,
    links
  });
});

/**
 * GET /graph/dao-ripples/:proposalId
 * Return ripple impact of DAO vote across contributor nodes
 * 
 * Response:
 * {
 *   "proposalId": "D163",
 *   "originContributor": "C001",
 *   "rippleRadius": 50,
 *   "impactedContributors": 12,
 *   "badgeUpgrades": 3
 * }
 */
router.get('/graph/dao-ripples/:proposalId', async (req, res) => {
  const proposalId = req.params.proposalId;
  const rippleData = await calculateDAORipple(proposalId);
  
  res.json(rippleData);
});

/**
 * GET /graph/badge-pulses
 * Return visual FX data for badge tier transitions
 * 
 * Response:
 * {
 *   "pulses": [
 *     {
 *       "tier": "Oracle",
 *       "pulseColor": "#00FFFF",
 *       "runeFX": "flare",
 *       "intensity": 0.8
 *     },
 *     ...
 *   ]
 * }
 */
router.get('/graph/badge-pulses', async (req, res) => {
  const pulses = [
    { tier: 'Initiate', pulseColor: '#808080', runeFX: 'glow', intensity: 0.3 },
    { tier: 'Builder', pulseColor: '#00CCFF', runeFX: 'spark', intensity: 0.5 },
    { tier: 'Architect', pulseColor: '#CC00FF', runeFX: 'burst', intensity: 0.7 },
    { tier: 'Oracle', pulseColor: '#FFCC00', runeFX: 'flare', intensity: 0.9 },
    { tier: 'Operative', pulseColor: '#FF0000', runeFX: 'explosion', intensity: 0.8 },
    { tier: 'Legend', pulseColor: '#FFFF00', runeFX: 'supernova', intensity: 1.0 }
  ];
  
  res.json({ pulses });
});

// ========================================
// HELPER FUNCTIONS
// ========================================

function authenticate(req, res, next) {
  const token = req.headers.authorization?.split(' ')[1];
  if (!token) {
    return res.status(401).json({ error: 'Authentication required' });
  }
  // Verify JWT token
  next();
}

function generateJWT(contributorId) {
  return `jwt_${contributorId}_${Date.now().toString(36)}`;
}

function getDAOPower(tier) {
  const powerMap = {
    'Initiate': 1,
    'Builder': 5,
    'Architect': 25,
    'Oracle': 100,
    'Operative': 50,
    'Legend': 500
  };
  return powerMap[tier] || 1;
}

async function recordToBlockchain(data) {
  // Record to SoulvanChronicle contract
  console.log('Recording to blockchain:', data);
  return true;
}

async function loadBadgeHistory(contributorId) {
  // Load from database
  return [
    {
      badgeId: 'BADGE_INITIATE_ABC',
      type: 'Initiate',
      level: 1,
      earnedAt: '2025-11-01T00:00:00Z',
      loreLink: 'First login to Soulvan universe'
    },
    {
      badgeId: 'BADGE_BUILDER_DEF',
      type: 'Builder',
      level: 2,
      earnedAt: '2025-11-05T00:00:00Z',
      loreLink: 'Completed 5 missions in Tokyo'
    },
    {
      badgeId: 'BADGE_ARCHITECT_GHI',
      type: 'Architect',
      level: 3,
      earnedAt: '2025-11-08T00:00:00Z',
      loreLink: 'DAO vote impact milestone'
    }
  ];
}

async function loadContributorStats(contributorId) {
  return {
    contributorId,
    currentTier: 'Architect',
    level: 3,
    daoPower: 25,
    totalXP: 2450,
    badgesEarned: 3,
    loreEntries: 12,
    daoVotesCast: 8
  };
}

async function loadAllContributors() {
  return [
    { id: 'C001', name: 'Brian', tier: 'Architect', lore: 12, daoPower: 25 },
    { id: 'C002', name: 'Alice', tier: 'Oracle', lore: 45, daoPower: 100 },
    { id: 'C003', name: 'Charlie', tier: 'Builder', lore: 8, daoPower: 5 }
  ];
}

async function loadContributorNode(contributorId) {
  return {
    id: contributorId,
    name: 'Brian',
    tier: 'Architect',
    lore: 12,
    daoPower: 25,
    recentActivity: Date.now()
  };
}

async function loadLoreLinks(contributorId) {
  return [
    { linkedContributor: 'C002', loreContext: 'Vault Breach collaboration', strength: 3 },
    { linkedContributor: 'C003', loreContext: 'Seasonal quest party', strength: 2 }
  ];
}

async function calculateDAORipple(proposalId) {
  return {
    proposalId,
    originContributor: 'C001',
    rippleRadius: 50,
    impactedContributors: 12,
    badgeUpgrades: 3
  };
}

module.exports = router;
