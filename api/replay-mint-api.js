/**
 * Soulvan Replay Mint API
 * RESTful API for bundling missions into saga replays, remixing graph-linked missions,
 * and exporting replays as scrolls or NFTs.
 * 
 * Endpoints:
 * - POST /auth/init - Initialize contributor identity
 * - POST /replay/bundle - Bundle missions into saga replay
 * - POST /replay/remix - Remix graph-linked missions
 * - POST /replay/export - Export replay as scroll/NFT/bundle
 * - POST /replay/dao-trigger - Trigger replay from DAO vote
 * - GET /replay/contributor/:id - Get contributor replay history
 * - GET /replay/stats/:id - Get replay statistics
 */

const express = require('express');
const jwt = require('jsonwebtoken');
const cors = require('cors');

const app = express();
const PORT = process.env.PORT || 3002;
const JWT_SECRET = process.env.JWT_SECRET || 'soulvan_replay_secret_key';

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

/**
 * Generate JWT token for contributor
 */
function generateJWT(contributorId) {
  return jwt.sign(
    { contributorId, timestamp: Date.now() },
    JWT_SECRET,
    { expiresIn: '1h' }
  );
}

/**
 * Record replay to blockchain (SoulvanChronicle)
 */
async function recordToBlockchain(data) {
  // Integration with SoulvanChronicle.sol
  console.log('[Blockchain] Recording:', data);
  return {
    txHash: `0x${Math.random().toString(16).substring(2, 66)}`,
    blockNumber: Math.floor(Math.random() * 1000000)
  };
}

/**
 * Load contributor replay history (mock data)
 */
function loadReplayHistory(contributorId) {
  return [
    {
      replayId: 'R001',
      narration: 'Brian\'s Rise',
      missions: ['Vault Breach', 'Oracle Echo'],
      createdAt: '2025-11-01T10:00:00Z',
      tier: 'Oracle',
      remixed: true,
      remixCount: 3
    },
    {
      replayId: 'R007',
      narration: 'Forge Divergence',
      missions: ['Chrono Forge', 'Legend Mint'],
      createdAt: '2025-11-05T14:30:00Z',
      tier: 'Legend',
      remixed: false,
      remixCount: 0
    }
  ];
}

/**
 * Load contributor statistics (mock data)
 */
function loadReplayStats(contributorId) {
  return {
    totalReplays: 12,
    totalRemixes: 7,
    loreLinks: 34,
    divergencePoints: 5,
    tier: 'Oracle'
  };
}

// ============================
// Authentication Endpoints
// ============================

/**
 * POST /auth/init
 * Initialize contributor identity and return access token
 */
app.post('/auth/init', (req, res) => {
  const { wallet, name } = req.body;
  
  if (!wallet) {
    return res.status(400).json({ error: 'Wallet address required' });
  }
  
  // Generate contributor ID from wallet
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
// Replay Bundling Endpoints
// ============================

/**
 * POST /replay/bundle
 * Bundle missions into saga replay episode
 */
app.post('/replay/bundle', authenticate, async (req, res) => {
  const { contributorId, missions, narration, mint } = req.body;
  
  if (!missions || missions.length === 0) {
    return res.status(400).json({ error: 'Missions array required' });
  }
  
  const replayId = `R${Math.floor(Math.random() * 1000).toString().padStart(3, '0')}`;
  const duration = `${missions.length * 3}m`;
  const mintedAt = mint ? new Date().toISOString() : null;
  
  // Record to blockchain
  await recordToBlockchain({
    type: 'replay_bundle',
    replayId,
    contributorId,
    missions,
    narration,
    timestamp: Date.now()
  });
  
  res.json({
    replayId,
    narration: narration || 'Saga Replay',
    missionCount: missions.length,
    duration,
    mintedAt,
    message: 'The saga converges. Your legend echoes.'
  });
});

/**
 * POST /replay/remix
 * Remix graph-linked missions into alternate saga branch
 */
app.post('/replay/remix', authenticate, async (req, res) => {
  const { contributorId, fragments, combine, mint } = req.body;
  
  if (!fragments || fragments.length === 0) {
    return res.status(400).json({ error: 'Fragments array required' });
  }
  
  const remixId = `RX${Math.floor(Math.random() * 1000).toString().padStart(3, '0')}`;
  const branches = combine ? 1 : fragments.length;
  const divergencePoint = `Mission ${Math.floor(Math.random() * 10) + 1}`;
  const mintedAt = mint ? new Date().toISOString() : null;
  
  // Record to blockchain
  await recordToBlockchain({
    type: 'saga_remix',
    remixId,
    contributorId,
    fragments,
    branches,
    timestamp: Date.now()
  });
  
  res.json({
    remixId,
    fragments: fragments.length,
    branches,
    divergencePoint,
    mintedAt,
    message: 'Your legend diverges. The vault remembers.'
  });
});

/**
 * POST /replay/export
 * Export replay as scroll, NFT, or bundle
 */
app.post('/replay/export', authenticate, async (req, res) => {
  const { replayId, formats } = req.body;
  
  if (!replayId) {
    return res.status(400).json({ error: 'Replay ID required' });
  }
  
  if (!formats || formats.length === 0) {
    return res.status(400).json({ error: 'Export formats required' });
  }
  
  const exports = [];
  
  formats.forEach(format => {
    if (format === 'scroll') {
      exports.push({
        format: 'scroll',
        url: `https://soulvan.io/scrolls/${replayId}.json`,
        hash: `0x${Math.random().toString(16).substring(2, 66)}`
      });
    } else if (format === 'nft') {
      const tokenId = Math.floor(Math.random() * 10000);
      exports.push({
        format: 'nft',
        tokenId,
        contract: '0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb',
        openseaUrl: `https://opensea.io/assets/0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb/${tokenId}`
      });
    } else if (format === 'bundle') {
      exports.push({
        format: 'bundle',
        url: `https://soulvan.io/bundles/${replayId}.zip`,
        size: `${Math.floor(Math.random() * 50) + 10}MB`
      });
    }
  });
  
  const exportedAt = new Date().toISOString();
  
  // Record to blockchain
  await recordToBlockchain({
    type: 'replay_export',
    replayId,
    formats,
    timestamp: Date.now()
  });
  
  res.json({
    replayId,
    exports,
    exportedAt,
    message: 'Your saga echoes beyond the vault.'
  });
});

/**
 * POST /replay/dao-trigger
 * Trigger replay minting from DAO vote impact
 */
app.post('/replay/dao-trigger', authenticate, async (req, res) => {
  const { proposalId, vote, impact, contributorId } = req.body;
  
  if (!proposalId || !vote) {
    return res.status(400).json({ error: 'Proposal ID and vote required' });
  }
  
  const replayId = `RD${Math.floor(Math.random() * 1000).toString().padStart(3, '0')}`;
  const rippleNodes = Math.floor(Math.random() * 20) + 5;
  const mintedAt = new Date().toISOString();
  
  // Record to blockchain
  await recordToBlockchain({
    type: 'dao_replay_trigger',
    replayId,
    proposalId,
    vote,
    contributorId,
    timestamp: Date.now()
  });
  
  res.json({
    replayId,
    proposalId,
    vote,
    rippleNodes,
    impact: impact || 'Replay bundle minted',
    mintedAt,
    message: 'The DAO echoes. Your vote ripples through the saga.'
  });
});

// ============================
// Replay History & Stats Endpoints
// ============================

/**
 * GET /replay/contributor/:id
 * Get contributor replay history
 */
app.get('/replay/contributor/:id', authenticate, (req, res) => {
  const { id } = req.params;
  
  const replays = loadReplayHistory(id);
  
  res.json({
    contributorId: id,
    replays,
    totalCount: replays.length
  });
});

/**
 * GET /replay/stats/:id
 * Get replay statistics
 */
app.get('/replay/stats/:id', authenticate, (req, res) => {
  const { id } = req.params;
  
  const stats = loadReplayStats(id);
  
  res.json({
    contributorId: id,
    ...stats
  });
});

// ============================
// Server Initialization
// ============================

app.listen(PORT, () => {
  console.log(`🎞️  Soulvan Replay Mint API running on port ${PORT}`);
  console.log(`📡 Ready to bundle, remix, and export saga replays`);
});

module.exports = app;
