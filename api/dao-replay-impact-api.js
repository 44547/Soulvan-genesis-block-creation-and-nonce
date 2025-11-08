/**
 * Soulvan DAO Replay Impact API
 * RESTful API for DAO governance visualization and replay impact tracking
 * 
 * Endpoints:
 * - POST /auth/init - Initialize contributor identity
 * - GET /dao/proposals - Get all active and historical DAO proposals
 * - GET /dao/vote-ripples/:proposalId - Get vote ripple data
 * - GET /dao/tier-upgrades/:id - Get contributors who leveled up from DAO participation
 * - GET /dao/governance-trails/:id - Get contributor influence paths across proposals
 * - POST /dao/impact - Record DAO impact from mission completion
 * - POST /dao/vote - Record vote event with ripple propagation
 */

const express = require('express');
const jwt = require('jsonwebtoken');
const cors = require('cors');

const app = express();
const PORT = process.env.PORT || 3004;
const JWT_SECRET = process.env.JWT_SECRET || 'soulvan_dao_impact_secret';

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
function loadProposals() {
  return [
    {
      id: 'D163',
      title: 'Remix Vault Expansion',
      description: 'Expand remix vault capacity and add new lore fragments',
      status: 'Active',
      votes: { yes: 128, no: 12, abstain: 8 },
      createdAt: '2025-11-01T10:00:00Z',
      endAt: '2025-11-15T10:00:00Z'
    },
    {
      id: 'D164',
      title: 'Neon City Mission Pack',
      description: 'Add new heist missions in neon city district',
      status: 'Passed',
      votes: { yes: 145, no: 23, abstain: 12 },
      createdAt: '2025-10-20T14:00:00Z',
      endAt: '2025-11-05T14:00:00Z'
    },
    {
      id: 'D165',
      title: 'Seasonal Badge Multiplier',
      description: 'Increase XP multiplier for seasonal contributors',
      status: 'Active',
      votes: { yes: 89, no: 34, abstain: 15 },
      createdAt: '2025-11-05T09:00:00Z',
      endAt: '2025-11-20T09:00:00Z'
    }
  ];
}

function loadVoteRipples(proposalId) {
  return {
    proposalId,
    rippleNodes: ['C001', 'C007', 'C014', 'C023', 'C042'],
    intensity: 'high',
    rippleRadius: 60,
    voteBreakdown: {
      yes: ['C001', 'C007', 'C014'],
      no: ['C042'],
      abstain: ['C023']
    }
  };
}

function loadTierUpgrades(proposalId) {
  return [
    {
      contributorId: 'C001',
      from: 'Oracle',
      to: 'Legend',
      triggeringVote: proposalId,
      upgradedAt: '2025-11-08T11:00:00Z',
      daoPowerGain: 400
    },
    {
      contributorId: 'C007',
      from: 'Architect',
      to: 'Oracle',
      triggeringVote: proposalId,
      upgradedAt: '2025-11-08T11:05:00Z',
      daoPowerGain: 75
    }
  ];
}

function loadGovernanceTrails(contributorId) {
  return {
    contributorId,
    participatedProposals: ['D163', 'D164', 'D165'],
    influenceScore: 8.7,
    totalVotes: 24,
    votingPower: 100,
    tier: 'Oracle',
    trail: {
      positions: [
        { x: 10, y: 5, z: -15 },
        { x: 25, y: 8, z: -30 },
        { x: 40, y: 12, z: -45 }
      ],
      connections: [
        { from: 'D163', to: 'D164', influence: 3.2 },
        { from: 'D164', to: 'D165', influence: 5.5 }
      ]
    }
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
// DAO Governance Endpoints
// ============================

/**
 * GET /dao/proposals
 * Get all active and historical DAO proposals
 */
app.get('/dao/proposals', authenticate, (req, res) => {
  const proposals = loadProposals();
  
  res.json({
    proposals,
    totalCount: proposals.length,
    activeCount: proposals.filter(p => p.status === 'Active').length,
    message: 'The DAO awaits your vote.'
  });
});

/**
 * GET /dao/vote-ripples/:proposalId
 * Get vote ripple data showing impact across contributors
 */
app.get('/dao/vote-ripples/:proposalId', authenticate, (req, res) => {
  const { proposalId } = req.params;
  
  const rippleData = loadVoteRipples(proposalId);
  
  res.json({
    ...rippleData,
    message: 'Your vote ripples through the vault.'
  });
});

/**
 * GET /dao/tier-upgrades/:id
 * Get contributors who leveled up from DAO participation
 * Can pass proposalId or contributorId
 */
app.get('/dao/tier-upgrades/:id', authenticate, (req, res) => {
  const { id } = req.params;
  
  const upgrades = loadTierUpgrades(id);
  
  res.json({
    id,
    upgrades,
    totalUpgrades: upgrades.length,
    message: 'You rise. The vault remembers.'
  });
});

/**
 * GET /dao/governance-trails/:id
 * Get contributor influence paths across proposals
 */
app.get('/dao/governance-trails/:id', authenticate, (req, res) => {
  const { id } = req.params;
  
  const trailData = loadGovernanceTrails(id);
  
  res.json({
    ...trailData,
    message: 'Your influence shapes the saga.'
  });
});

/**
 * POST /dao/impact
 * Record DAO impact from mission completion or event
 */
app.post('/dao/impact', authenticate, async (req, res) => {
  const { proposalId, contributorId, replayId, heatDelta, influenceScore, tierPulse } = req.body;
  
  if (!contributorId) {
    return res.status(400).json({ error: 'Contributor ID required' });
  }
  
  const impactId = `I${Math.floor(Math.random() * 1000).toString().padStart(3, '0')}`;
  const rippleNodes = ['C001', 'C007', 'C014', 'C023'];
  const badgeUpgrades = tierPulse ? 1 : 0;
  
  // Record to blockchain
  await recordToBlockchain({
    type: 'dao_impact',
    impactId,
    proposalId: proposalId || null,
    contributorId,
    replayId: replayId || null,
    heatDelta,
    influenceScore,
    tierPulse,
    timestamp: Date.now()
  });
  
  res.json({
    impactId,
    proposalId: proposalId || 'N/A',
    contributorId,
    rippleNodes,
    badgeUpgrades,
    heatDelta: heatDelta || 0,
    influenceScore: influenceScore || 0,
    recordedAt: new Date().toISOString(),
    message: 'The DAO echoes. Your impact reverberates.'
  });
});

/**
 * POST /dao/vote
 * Record vote event with ripple propagation
 */
app.post('/dao/vote', authenticate, async (req, res) => {
  const { proposalId, contributorId, vote, votePower } = req.body;
  
  if (!proposalId || !contributorId || !vote) {
    return res.status(400).json({ error: 'Proposal ID, contributor ID, and vote required' });
  }
  
  const voteId = `V${Math.floor(Math.random() * 10000).toString().padStart(4, '0')}`;
  const rippleRadius = votePower * 2 || 50;
  const affectedContributors = Math.floor(Math.random() * 15) + 5;
  
  // Calculate potential tier upgrade
  const upgradeChance = Math.random();
  const tierUpgrade = upgradeChance > 0.7 ? {
    from: 'Oracle',
    to: 'Legend',
    daoPowerGain: 400
  } : null;
  
  // Record to blockchain
  await recordToBlockchain({
    type: 'dao_vote',
    voteId,
    proposalId,
    contributorId,
    vote,
    votePower: votePower || 10,
    timestamp: Date.now()
  });
  
  res.json({
    voteId,
    proposalId,
    contributorId,
    vote,
    votePower: votePower || 10,
    rippleRadius,
    affectedContributors,
    tierUpgrade,
    votedAt: new Date().toISOString(),
    message: vote === 'YES' 
      ? 'Your voice echoes in golden waves.' 
      : vote === 'NO' 
        ? 'Your dissent ripples crimson.' 
        : 'Your contemplation flows blue.'
  });
});

/**
 * POST /dao/create-proposal
 * Create new DAO proposal (requires proposer role)
 */
app.post('/dao/create-proposal', authenticate, async (req, res) => {
  const { title, description, contributorId, votingPeriodDays } = req.body;
  
  if (!title || !description) {
    return res.status(400).json({ error: 'Title and description required' });
  }
  
  const proposalId = `D${Math.floor(Math.random() * 1000) + 100}`;
  const endDate = new Date();
  endDate.setDate(endDate.getDate() + (votingPeriodDays || 14));
  
  // Record to blockchain
  await recordToBlockchain({
    type: 'proposal_created',
    proposalId,
    title,
    description,
    author: contributorId,
    timestamp: Date.now()
  });
  
  res.json({
    proposalId,
    title,
    description,
    status: 'Active',
    votes: { yes: 0, no: 0, abstain: 0 },
    createdAt: new Date().toISOString(),
    endAt: endDate.toISOString(),
    message: 'Your proposal enters the vault. Let the saga decide.'
  });
});

/**
 * GET /dao/contributor-stats/:id
 * Get comprehensive DAO statistics for a contributor
 */
app.get('/dao/contributor-stats/:id', authenticate, (req, res) => {
  const { id } = req.params;
  
  const stats = {
    contributorId: id,
    tier: 'Oracle',
    daoPower: 100,
    totalVotes: 24,
    proposalsCreated: 3,
    proposalsPassed: 2,
    influenceScore: 8.7,
    tierUpgrades: 2,
    recentActivity: [
      { proposalId: 'D165', vote: 'YES', timestamp: '2025-11-08T10:00:00Z' },
      { proposalId: 'D164', vote: 'YES', timestamp: '2025-11-05T15:30:00Z' },
      { proposalId: 'D163', vote: 'NO', timestamp: '2025-11-02T09:15:00Z' }
    ]
  };
  
  res.json({
    ...stats,
    message: 'Your governance legacy grows with each vote.'
  });
});

// ============================
// Server Initialization
// ============================

app.listen(PORT, () => {
  console.log(`🗳️  Soulvan DAO Replay Impact API running on port ${PORT}`);
  console.log(`📡 Ready to track proposals, ripples, and governance trails`);
});

module.exports = app;
