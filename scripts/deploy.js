const { ethers } = require("hardhat");

async function main() {
  const [deployer] = await ethers.getSigners();
  console.log("Deployer:", deployer.address);

  // Deploy SoulvanCoin
  const Coin = await ethers.getContractFactory("SoulvanCoin");
  const coin = await Coin.deploy();
  await coin.waitForDeployment();
  console.log("SoulvanCoin:", coin.target);

  // Deploy Car Skin NFT
  const Skin = await ethers.getContractFactory("SoulvanCarSkin");
  const skin = await Skin.deploy();
  await skin.waitForDeployment();
  console.log("SoulvanCarSkin:", skin.target);

  // Deploy Chronicle
  const Chronicle = await ethers.getContractFactory("SoulvanChronicle");
  const chronicle = await Chronicle.deploy();
  await chronicle.waitForDeployment();
  console.log("SoulvanChronicle:", chronicle.target);

  // Deploy Governance (short periods for local dev)
  const Governance = await ethers.getContractFactory("SoulvanGovernance");
  const votingDelay = 1; // 1 block
  const votingPeriod = 50; // ~ short local
  const threshold = ethers.utils.parseEther("1");
  const gov = await Governance.deploy(coin.target, votingDelay, votingPeriod, threshold);
  await gov.waitForDeployment();
  console.log("SoulvanGovernance:", gov.target);

  // Deploy SeasonManager
  const SeasonManager = await ethers.getContractFactory("SoulvanSeasonManager");
  const seasonManager = await SeasonManager.deploy(chronicle.target);
  await seasonManager.waitForDeployment();
  console.log("SoulvanSeasonManager:", seasonManager.target);

  // Deploy MissionRegistry
  const MissionRegistry = await ethers.getContractFactory("SoulvanMissionRegistry");
  const missionRegistry = await MissionRegistry.deploy(coin.target, chronicle.target);
  await missionRegistry.waitForDeployment();
  console.log("SoulvanMissionRegistry:", missionRegistry.target);

  // Grant proposer role if needed (already deployer) & connect coin minter/logger roles
  const MINTER_ROLE = await coin.MINTER_ROLE();
  await (await coin.grantRole(MINTER_ROLE, deployer.address)).wait();

  const LOGGER_ROLE = await chronicle.LOGGER_ROLE();
  await (await chronicle.grantRole(LOGGER_ROLE, deployer.address)).wait();
  await (await chronicle.grantRole(LOGGER_ROLE, seasonManager.target)).wait();
  await (await chronicle.grantRole(LOGGER_ROLE, missionRegistry.target)).wait();

  // Grant MissionRegistry ability to mint SVN rewards
  await (await coin.grantRole(MINTER_ROLE, missionRegistry.target)).wait();

  // Example initial mint & log
  await (await coin.mint(deployer.address, ethers.utils.parseEther("1000"))).wait();
  console.log("Minted 1000 SVN to deployer");

  // Simple Chronicle entry (type 0 race) with dummy hash
  const dummyHash = ethers.hexlify(ethers.randomBytes(32));
  await (await chronicle.log(0, deployer.address, dummyHash)).wait();
  console.log("Chronicle entry logged.");
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
