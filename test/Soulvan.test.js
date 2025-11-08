const { expect } = require("chai");
const { ethers } = require("hardhat");

describe("Soulvan Suite", function () {
  let coin, skin, chronicle, gov, seasonManager, missionRegistry, deployer, user;

  beforeEach(async () => {
    [deployer, user] = await ethers.getSigners();

    const Coin = await ethers.getContractFactory("SoulvanCoin");
  coin = await Coin.deploy();
  await coin.waitForDeployment();
  coinAddress = await coin.getAddress();

    const Skin = await ethers.getContractFactory("SoulvanCarSkin");
  skin = await Skin.deploy();
  await skin.waitForDeployment();

    const Chronicle = await ethers.getContractFactory("SoulvanChronicle");
  chronicle = await Chronicle.deploy();
  await chronicle.waitForDeployment();

    const Governance = await ethers.getContractFactory("SoulvanGovernance");
    gov = await Governance.deploy(await coin.getAddress(), 1, 20, ethers.parseEther("1"));
    await gov.waitForDeployment();
    const govAddress = await gov.getAddress();

    const SeasonManager = await ethers.getContractFactory("SoulvanSeasonManager");
    seasonManager = await SeasonManager.deploy(await chronicle.getAddress());
    await seasonManager.waitForDeployment();

    const MissionRegistry = await ethers.getContractFactory("SoulvanMissionRegistry");
    missionRegistry = await MissionRegistry.deploy(await coin.getAddress(), await chronicle.getAddress());
    await missionRegistry.waitForDeployment();

    // Mint initial votes to deployer
  const MINTER_ROLE = await coin.MINTER_ROLE();
    await coin.grantRole(MINTER_ROLE, deployer.address);
    await coin.mint(deployer.address, ethers.parseEther("10"));
    // allow governor to execute mint via proposal
    await coin.grantRole(MINTER_ROLE, govAddress);
    // allow mission registry to mint rewards
    await coin.grantRole(MINTER_ROLE, await missionRegistry.getAddress());
    // allow season manager and mission registry to log to chronicle
    const LOGGER_ROLE = await chronicle.LOGGER_ROLE();
    await chronicle.grantRole(LOGGER_ROLE, await seasonManager.getAddress());
    await chronicle.grantRole(LOGGER_ROLE, await missionRegistry.getAddress());
  });  it("mints SVN", async () => {
  expect(await coin.balanceOf(deployer.address)).to.equal(ethers.parseEther("10"));
  });

  it("mints car skin NFT", async () => {
    await skin.mint(user.address, "ipfs://skin1");
    // Sequential IDs start at 1
    expect(await skin.ownerOf(1)).to.equal(user.address);
  });

  it("logs chronicle entry", async () => {
  const dummyHash = ethers.hexlify(ethers.randomBytes(32));
    const tx = await chronicle.log(0, user.address, dummyHash);
    await tx.wait();
    expect(await chronicle.entryCount()).to.equal(1);
  });

  it("creates and votes on proposal (success)", async () => {
    // delegate voting power to self to meet threshold
    await coin.delegate(deployer.address);
    // create proposal
  const callData = coin.interface.encodeFunctionData("mint", [user.address, ethers.parseEther("1")]);
  await gov.propose(coinAddress, callData, "Mint 1 SVN to user");
  const id = Number((await gov.proposalCount()) - 1n);

    // advance blocks
    for (let i = 0; i < 2; i++) await ethers.provider.send("evm_mine", []);

    await gov.castVote(id, true);

    // fast-forward voting period
    for (let i = 0; i < 25; i++) await ethers.provider.send("evm_mine", []);

    await gov.execute(id);

    expect(await coin.balanceOf(user.address)).to.equal(ethers.parseEther("1"));
  });

  it("activates seasonal arc and motifs", async () => {
    const dummyHash = ethers.hexlify(ethers.randomBytes(32));
    await seasonManager.setSeason(1, dummyHash); // Calm
    expect(await seasonManager.activeSeason()).to.equal(1);

    await seasonManager.setMotifs({ visual: "calm_sunrise", audio: "calm_bank", haptic: "calm_pattern" }, dummyHash);
    const motifs = await seasonManager.activeMotifs();
    expect(motifs.visual).to.equal("calm_sunrise");
  });

  it("creates and completes GTA-style mission", async () => {
    const missionHash = ethers.hexlify(ethers.randomBytes(32));
    const rewardAmount = ethers.parseEther("5");
    await missionRegistry.createMission(1, 0, rewardAmount, missionHash); // heist, city
    const mission = await missionRegistry.missions(0);
    expect(mission.active).to.be.true;
    expect(mission.rewardSVN).to.equal(rewardAmount);

    const resultHash = ethers.hexlify(ethers.randomBytes(32));
    await missionRegistry.completeMission(0, user.address, resultHash);
    expect(await coin.balanceOf(user.address)).to.equal(rewardAmount);
    expect(await missionRegistry.completed(0, user.address)).to.be.true;
  });
});