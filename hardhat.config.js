require('dotenv').config();
require('@nomicfoundation/hardhat-toolbox');

/**
 * @type import('hardhat/config').HardhatUserConfig
 */
module.exports = {
  solidity: {
    version: '0.8.21',
    settings: {
      optimizer: { enabled: true, runs: 200 }
    }
  },
  networks: {
    hardhat: {},
    // Add real networks via env vars
    // sepolia: { url: process.env.SEPOLIA_RPC, accounts: [process.env.DEPLOYER_KEY].filter(Boolean) }
  },
  etherscan: {
    apiKey: process.env.ETHERSCAN_KEY || ''
  }
};
