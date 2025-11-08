/**
 * Soulvan Remix Forge Graph API
 * RESTful API for remix lineage, echo mapping, scroll orbits, and DAO ripples
 * 
 * Endpoints:
 * - POST /auth/init - Initialize contributor identity
 * - GET /remix/lineage/:id - Get remix ancestry and saga divergence paths
 * - GET /remix/echo/:id - Get ripple effects of remix events
 * - GET /remix/scrolls/:id - Get lore scrolls orbiting remix nodes
 * - GET /remix/dao-ripples/:proposalId - Get DAO-triggered remix ripples
 * - GET /remix/graph/contributors - Get all contributor nodes
 */

const express = require('express');
const jwt = require('jsonwebtoken');
const cors = require('cors');

const app = express();
const PORT = process.env.PORT || 3003;
const JWT_SECRET = process.env.JWT_SECRET || 'soulvan_remix_forge_secret';

app.use(express.json());
app.use(cors());

// ============================
// Middleware: Authentication
// ============================
function authenticate(req, res, next) {
  const authHeader = req.headers.authorization;
  
  if (!authHeader) {
    return res.status(401).json({ error: 'No authorization header' });
  }
  
  const token = authHeader.split(' ')[1];
  
  try {
    const decoded = jwt.verify(token, JWT_SECRET);
    req.contributor = decoded;
    next();
  } catch (error) {
    return res.status(403).json({ error: 'Invalid or expired token' });
  }
}

// ============================
// Helper Functions
// ============================

function generateJWT(contributorId) {
  return jwt.sign(
    { contributorId, timestamp: Date.now() },
    JWT_SECRET,
    { expiresIn: '1h' }
  );
}

async function recordToBlockchain(data) {
  console.log('[Blockchain] Recording:', data);
  return {
    txHash: `0x${Math.random().toString(16).substring(2, 66)}`,
    blockNumber: Math.floor(Math.random() * 1000000)
  };
}

// Mock data generators
function loadContributorNodes() {
  return [
    { id: 'C001', name: 'Brian', tier: 'Oracle', remixCount: 12, loreLinks: 34 },
    { id: 'C007', name: 'Sage', tier: 'Legend', remixCount: 47, loreLinks: 89 },
    { id: 'C014', name: 'Cipher', tier: 'Architect', remixCount: 8, loreLinks: 21 },
    { id: 'C023', name: 'Echo', tier: 'Builder', remixCount: 3, loreLinks: 7 },
    { id: 'C042', name: 'Nexus', tier: 'Operative', remixCount: 19, loreLinks: 45 }
  ];
}

function loadRemixLineage(contributorId) {
  const lineages = {
    'C001': {
      lineage: ['Vault Breach', 'Oracle Echo', 'Forge Divergence', 'Legend Mint'],
      branches: 3,
      tier: 'Oracle'
    },
    'C007': {
      lineage: ['Genesis Block', 'Chronicle Birth', 'Saga Evolution', 'Mythic Ascension', 'Legend Forge'],
      branches: 7,
      tier: 'Legend'
    }
  };
  
  return lineages[contributorId] || {
    lineage: ['First Remix'],
    branches: 1,
    tier: 'Initiate'
  };
}

function loadEchoMap(remixId) {
  return {
    remixId,
    echoNodes: ['C001', 'C007', 'C014', 'C023'],
    intensity: 'high',
    rippleRadius: 45
  };
}

function loadScrollOrbits(contributorId) {
  return {
    scrolls: [
      { title: 'Echo of the Oracle', format: 'scroll', url: 'https://soulvan.io/scrolls/oracle-echo.json' },
      { title: 'Vault Divergence', format: 'nft', url: 'https://opensea.io/assets/...' },
      { title: 'Saga Remix Bundle', format: 'bundle', url: 'https://soulvan.io/bundles/remix-001.zip' }
    ]
  };
}

function loadDAORipples(proposalId) {
  return {
    proposalId,
    rippleRadius: 60,
    impactedContributors: ['C001', 'C007', 'C014', 'C023', 'C042'],
    badgeUpgrades: 3,
    remixesMinted: 5,
    rippleStrength: 'high'
  };
}

// ============================
// Authentication Endpoints
// ============================

app.post('/auth/init', (req, res) => {
  const { wallet, name } = req.body;
  
  if (!wallet) {
    return res.status(400).json({ error: 'Wallet address required' });
  }
  
  const contributorId = `C${wallet.substring(2, 6).toUpperCase()}`;
  const accessToken = generateJWT(contributorId);
  
  res.json({
    contributorId,
    name: name || 'Anonymous Contributor',
    accessToken,
    expiresIn: 3600
  });
});

// ============================
// Remix Graph Endpoints
// ============================

/**
 * GET /remix/graph/contributors
 * Get all contributor nodes in the remix graph
 */
app.get('/remix/graph/contributors', authenticate, (req, res) => {
  const contributors = loadContributorNodes();
  
  res.json({
    contributors,
    totalCount: contributors.length,
    message: 'Your saga branches across the vault.'
  });
});

/**
 * GET /remix/lineage/:id
 * Get remix ancestry and saga divergence paths for a contributor
 */
app.get('/remix/lineage/:id', authenticate, (req, res) => {
  const { id } = req.params;
  
  const lineageData = loadRemixLineage(id);
  
  res.json({
    id,
    ...lineageData,
    message: 'The lineage reveals your saga evolution.'
  });
});

/**
 * GET /remix/echo/:id
 * Get ripple effects of remix events across lore-linked contributors
 */
app.get('/remix/echo/:id', authenticate, (req, res) => {
  const { id } = req.params;
  
  const echoData = loadEchoMap(id);
  
  res.json({
    ...echoData,
    message: 'Your remix echoes through the vault.'
  });
});

/**
 * GET /remix/scrolls/:id
 * Get lore scrolls orbiting remix nodes
 */
app.get('/remix/scrolls/:id', authenticate, (req, res) => {
  const { id } = req.params;
  
  const scrollData = loadScrollOrbits(id);
  
  res.json({
    contributorId: id,
    ...scrollData,
    message: 'Your scrolls orbit the remix forge.'
  });
});

/**
 * GET /remix/dao-ripples/:proposalId
 * Get DAO-triggered remix ripple data
 */
app.get('/remix/dao-ripples/:proposalId', authenticate, (req, res) => {
  const { proposalId } = req.params;
  
  const rippleData = loadDAORipples(proposalId);
  
  res.json({
    ...rippleData,
    message: 'The DAO ripples through the remix graph.'
  });
});

/**
 * POST /remix/trigger-divergence
 * Trigger remix divergence event
 */
app.post('/remix/trigger-divergence', authenticate, async (req, res) => {
  const { contributorId, remixId, divergencePoint } = req.body;
  
  if (!contributorId || !remixId) {
    return res.status(400).json({ error: 'Contributor ID and Remix ID required' });
  }
  
  await recordToBlockchain({
    type: 'remix_divergence',
    contributorId,
    remixId,
    divergencePoint,
    timestamp: Date.now()
  });
  
  res.json({
    contributorId,
    remixId,
    divergencePoint: divergencePoint || 'Mission 5',
    triggeredAt: new Date().toISOString(),
    message: 'Your legend diverges. The vault remembers.'
  });
});

/**
 * POST /remix/mint-scroll
 * Mint lore scroll from remix path
 */
app.post('/remix/mint-scroll', authenticate, async (req, res) => {
  const { contributorId, remixId, format } = req.body;
  
  if (!contributorId || !remixId) {
    return res.status(400).json({ error: 'Contributor ID and Remix ID required' });
  }
  
  const scrollId = `SCR${Math.floor(Math.random() * 1000).toString().padStart(3, '0')}`;
  
  await recordToBlockchain({
    type: 'scroll_mint',
    scrollId,
    contributorId,
    remixId,
    format: format || 'scroll',
    timestamp: Date.now()
  });
  
  const mintData = {
    scrollId,
    format: format || 'scroll'
  };
  
  if (format === 'nft') {
    const tokenId = Math.floor(Math.random() * 10000);
    mintData.tokenId = tokenId;
    mintData.contract = '0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb';
    mintData.openseaUrl = `https://opensea.io/assets/0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb/${tokenId}`;
  } else {
    mintData.url = `https://soulvan.io/scrolls/${scrollId}.json`;
    mintData.hash = `0x${Math.random().toString(16).substring(2, 66)}`;
  }
  
  res.json({
    ...mintData,
    mintedAt: new Date().toISOString(),
    message: 'Your scroll echoes beyond the vault.'
  });
});

// ============================
// Server Initialization
// ============================

app.listen(PORT, () => {
  console.log(`🔀 Soulvan Remix Forge Graph API running on port ${PORT}`);
  console.log(`📡 Ready to map lineage, echo ripples, and orbit scrolls`);
});

module.exports = app;
