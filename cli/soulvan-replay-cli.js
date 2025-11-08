#!/usr/bin/env node

/**
 * Soulvan Replay Mint CLI
 * Command-line interface for bundling missions into saga replays,
 * remixing graph-linked missions, and exporting replays as scrolls or NFTs.
 * 
 * Commands:
 * - bundle: Combine missions into saga replay episode
 * - remix: Remix graph-linked missions into alternate saga branch
 * - export: Export replay as scroll, NFT, or bundle
 * - dao-trigger: Trigger replay minting from DAO vote impact
 * - history: View contributor replay history
 * - stats: Show replay stats and lore links
 */

const { Command } = require('commander');
const chalk = require('chalk');
const axios = require('axios');
require('dotenv').config();

const program = new Command();
const API_BASE = process.env.SOULVAN_API_URL || 'http://localhost:3002';

// ASCII Art Banner
const banner = `
${chalk.cyan('╔═══════════════════════════════════════════════════════╗')}
${chalk.cyan('║')}   ${chalk.yellow('SOULVAN REPLAY FORGE')}                           ${chalk.cyan('║')}
${chalk.cyan('║')}   ${chalk.gray('Remix the saga. Diverge the legend.')}            ${chalk.cyan('║')}
${chalk.cyan('╚═══════════════════════════════════════════════════════╝')}
`;

// Helper: Sleep for cinematic effect
const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

// Helper: Get tier color
function getTierColor(tier) {
  const colors = {
    'Initiate': chalk.blue,
    'Builder': chalk.green,
    'Architect': chalk.magenta,
    'Oracle': chalk.cyan,
    'Operative': chalk.yellow,
    'Legend': chalk.yellowBright
  };
  return colors[tier] || chalk.white;
}

program
  .name('soulvan-replay')
  .description('Soulvan Replay Mint CLI - Bundle, remix, and export saga replays')
  .version('1.0.0');

// ============================
// COMMAND: bundle
// ============================
program
  .command('bundle')
  .description('Bundle missions into a saga replay episode')
  .option('--missions <missions>', 'Mission names (comma-separated)', 'Vault Breach, Oracle Echo')
  .option('--narration <narration>', 'Replay narration title', 'Saga Replay')
  .option('--mint', 'Mint replay as NFT immediately')
  .option('--contributor <id>', 'Contributor ID', 'C001')
  .action(async (options) => {
    console.log(banner);
    console.log(chalk.cyan('🎞️  Bundling Missions into Saga Replay...\n'));
    
    await sleep(800);
    
    const missions = options.missions.split(',').map(m => m.trim());
    
    console.log(chalk.gray('Missions:'));
    missions.forEach(mission => {
      console.log(chalk.white(`  • ${mission}`));
    });
    console.log();
    
    await sleep(1000);
    
    try {
      const response = await axios.post(`${API_BASE}/replay/bundle`, {
        contributorId: options.contributor,
        missions: missions,
        narration: options.narration,
        mint: options.mint || false
      });
      
      const { replayId, missionCount, duration, mintedAt } = response.data;
      
      console.log(chalk.green('✅ Replay Bundled Successfully!\n'));
      console.log(chalk.white(`Replay ID: ${chalk.yellowBright(replayId)}`));
      console.log(chalk.white(`Missions: ${chalk.cyan(missionCount)} episodes`));
      console.log(chalk.white(`Duration: ${chalk.cyan(duration)}`));
      
      if (options.mint) {
        console.log(chalk.white(`Minted: ${chalk.green(mintedAt)}`));
        console.log(chalk.yellow('🎖️  NFT Minted!'));
      }
      
      await sleep(500);
      console.log(chalk.gray('\n"The saga converges. Your legend echoes."'));
      
    } catch (error) {
      console.error(chalk.red(`\n❌ Bundle failed: ${error.message}`));
    }
  });

// ============================
// COMMAND: remix
// ============================
program
  .command('remix')
  .description('Remix graph-linked missions into alternate saga branch')
  .option('--fragments <fragments>', 'Lore fragment IDs (comma-separated)', 'F001, F007')
  .option('--combine', 'Combine fragments into single remix')
  .option('--mint', 'Mint remix as NFT')
  .option('--contributor <id>', 'Contributor ID', 'C001')
  .action(async (options) => {
    console.log(banner);
    console.log(chalk.magenta('🔀 Remixing Saga Branch...\n'));
    
    await sleep(800);
    
    const fragments = options.fragments.split(',').map(f => f.trim());
    
    console.log(chalk.gray('Lore Fragments:'));
    fragments.forEach(fragment => {
      console.log(chalk.white(`  🧩 ${fragment}`));
    });
    console.log();
    
    await sleep(1200);
    
    try {
      const response = await axios.post(`${API_BASE}/replay/remix`, {
        contributorId: options.contributor,
        fragments: fragments,
        combine: options.combine || false,
        mint: options.mint || false
      });
      
      const { remixId, branches, divergencePoint, mintedAt } = response.data;
      
      console.log(chalk.green('✅ Remix Created Successfully!\n'));
      console.log(chalk.white(`Remix ID: ${chalk.magenta(remixId)}`));
      console.log(chalk.white(`Branches: ${chalk.cyan(branches)} alternate paths`));
      console.log(chalk.white(`Divergence: ${chalk.yellow(divergencePoint)}`));
      
      if (options.mint) {
        console.log(chalk.white(`Minted: ${chalk.green(mintedAt)}`));
        console.log(chalk.yellow('🎖️  Remix NFT Forged!'));
      }
      
      await sleep(500);
      console.log(chalk.gray('\n"Your legend diverges. The vault remembers."'));
      
    } catch (error) {
      console.error(chalk.red(`\n❌ Remix failed: ${error.message}`));
    }
  });

// ============================
// COMMAND: export
// ============================
program
  .command('export')
  .description('Export replay as scroll, NFT, or bundle')
  .option('--replay <id>', 'Replay ID', 'R001')
  .option('--scroll', 'Export as lore scroll')
  .option('--nft', 'Export as NFT')
  .option('--bundle', 'Export as bundled archive')
  .action(async (options) => {
    console.log(banner);
    console.log(chalk.yellow('📜 Exporting Replay...\n'));
    
    await sleep(800);
    
    const formats = [];
    if (options.scroll) formats.push('scroll');
    if (options.nft) formats.push('nft');
    if (options.bundle) formats.push('bundle');
    
    if (formats.length === 0) {
      console.log(chalk.red('❌ No export format specified. Use --scroll, --nft, or --bundle.'));
      return;
    }
    
    console.log(chalk.gray('Export Formats:'));
    formats.forEach(format => {
      console.log(chalk.white(`  📦 ${format}`));
    });
    console.log();
    
    await sleep(1000);
    
    try {
      const response = await axios.post(`${API_BASE}/replay/export`, {
        replayId: options.replay,
        formats: formats
      });
      
      const { exports, exportedAt } = response.data;
      
      console.log(chalk.green('✅ Replay Exported Successfully!\n'));
      
      exports.forEach(exp => {
        if (exp.format === 'scroll') {
          console.log(chalk.white(`📜 Scroll: ${chalk.cyan(exp.url)}`));
          console.log(chalk.gray(`   Hash: ${exp.hash}`));
        } else if (exp.format === 'nft') {
          console.log(chalk.white(`🎖️  NFT: Token #${chalk.yellow(exp.tokenId)}`));
          console.log(chalk.gray(`   Contract: ${exp.contract}`));
          console.log(chalk.cyan(`   OpenSea: ${exp.openseaUrl}`));
        } else if (exp.format === 'bundle') {
          console.log(chalk.white(`📦 Bundle: ${chalk.magenta(exp.url)}`));
          console.log(chalk.gray(`   Size: ${exp.size}`));
        }
      });
      
      console.log(chalk.white(`\nExported: ${chalk.green(exportedAt)}`));
      
      await sleep(500);
      console.log(chalk.gray('\n"Your saga echoes beyond the vault."'));
      
    } catch (error) {
      console.error(chalk.red(`\n❌ Export failed: ${error.message}`));
    }
  });

// ============================
// COMMAND: dao-trigger
// ============================
program
  .command('dao-trigger')
  .description('Trigger replay minting from DAO vote impact')
  .option('--proposal <id>', 'Proposal ID', 'D163')
  .option('--vote <vote>', 'Vote (YES/NO)', 'YES')
  .option('--impact <impact>', 'Impact description', 'Replay bundle minted')
  .option('--contributor <id>', 'Contributor ID', 'C001')
  .action(async (options) => {
    console.log(banner);
    console.log(chalk.cyan('⚡ Triggering DAO Replay Mint...\n'));
    
    await sleep(800);
    
    console.log(chalk.white(`Proposal: ${chalk.yellow(options.proposal)}`));
    console.log(chalk.white(`Vote: ${options.vote === 'YES' ? chalk.green('YES') : chalk.red('NO')}`));
    console.log(chalk.white(`Impact: ${chalk.gray(options.impact)}`));
    console.log();
    
    await sleep(1200);
    
    try {
      const response = await axios.post(`${API_BASE}/replay/dao-trigger`, {
        proposalId: options.proposal,
        vote: options.vote,
        impact: options.impact,
        contributorId: options.contributor
      });
      
      const { replayId, rippleNodes, mintedAt } = response.data;
      
      console.log(chalk.green('✅ DAO Replay Triggered!\n'));
      console.log(chalk.white(`Replay ID: ${chalk.cyan(replayId)}`));
      console.log(chalk.white(`Ripple Nodes: ${chalk.yellow(rippleNodes)} contributors affected`));
      console.log(chalk.white(`Minted: ${chalk.green(mintedAt)}`));
      
      await sleep(500);
      console.log(chalk.gray('\n"The DAO echoes. Your vote ripples through the saga."'));
      
    } catch (error) {
      console.error(chalk.red(`\n❌ DAO trigger failed: ${error.message}`));
    }
  });

// ============================
// COMMAND: history
// ============================
program
  .command('history')
  .description('View contributor replay history')
  .option('--contributor <id>', 'Contributor ID', 'C001')
  .action(async (options) => {
    console.log(banner);
    console.log(chalk.cyan('📜 Replay History\n'));
    
    await sleep(800);
    
    try {
      const response = await axios.get(`${API_BASE}/replay/contributor/${options.contributor}`);
      
      const { replays } = response.data;
      
      if (replays.length === 0) {
        console.log(chalk.gray('No replay history found.'));
        return;
      }
      
      console.log(chalk.white(`Contributor: ${chalk.yellow(options.contributor)}\n`));
      
      replays.forEach((replay, index) => {
        const color = getTierColor(replay.tier);
        console.log(color(`${index + 1}. ${replay.narration}`));
        console.log(chalk.gray(`   ID: ${replay.replayId}`));
        console.log(chalk.gray(`   Missions: ${replay.missions.join(', ')}`));
        console.log(chalk.gray(`   Created: ${replay.createdAt}`));
        
        if (replay.remixed) {
          console.log(chalk.magenta(`   🔀 Remixed: ${replay.remixCount} branches`));
        }
        
        console.log();
      });
      
    } catch (error) {
      console.error(chalk.red(`\n❌ History retrieval failed: ${error.message}`));
    }
  });

// ============================
// COMMAND: stats
// ============================
program
  .command('stats')
  .description('Show replay stats and lore links')
  .option('--contributor <id>', 'Contributor ID', 'C001')
  .action(async (options) => {
    console.log(banner);
    console.log(chalk.cyan('📊 Replay Statistics\n'));
    
    await sleep(800);
    
    try {
      const response = await axios.get(`${API_BASE}/replay/stats/${options.contributor}`);
      
      const { totalReplays, totalRemixes, loreLinks, divergencePoints, tier } = response.data;
      
      const tierColor = getTierColor(tier);
      
      console.log(tierColor(`Contributor: ${options.contributor}`));
      console.log(tierColor(`Tier: ${tier}\n`));
      
      console.log(chalk.white(`Total Replays: ${chalk.cyan(totalReplays)}`));
      console.log(chalk.white(`Total Remixes: ${chalk.magenta(totalRemixes)}`));
      console.log(chalk.white(`Lore Links: ${chalk.yellow(loreLinks)}`));
      console.log(chalk.white(`Divergence Points: ${chalk.red(divergencePoints)}`));
      
      await sleep(500);
      console.log(chalk.gray('\n"Your saga branches across the vault."'));
      
    } catch (error) {
      console.error(chalk.red(`\n❌ Stats retrieval failed: ${error.message}`));
    }
  });

program.parse(process.argv);

// Show help if no command provided
if (!process.argv.slice(2).length) {
  console.log(banner);
  program.outputHelp();
}
