# Soulvan Developer Quick Reference

**⚡ Fast access to APIs, configurations, and common tasks**

---

## 🎮 Core APIs

### VehiclePhysics
```csharp
// Set player input
vehiclePhysics.SetThrottle(0.8f);     // 0-1
vehiclePhysics.SetBrake(0.5f);        // 0-1
vehiclePhysics.SetSteering(-0.3f);    // -1 to 1
vehiclePhysics.SetNitro(true);        // bool

// Query vehicle state
float speed = vehiclePhysics.GetCurrentSpeed();    // km/h
float rpm = vehiclePhysics.GetCurrentRPM();        // 0-8000+
int gear = vehiclePhysics.currentGear;             // 1-6
```

### MissionManager
```csharp
// Start/complete missions
missionManager.StartMission("mission_001");
missionManager.CompleteMission("mission_001", success: true);

// Query missions
List<MissionData> available = missionManager.GetAvailableMissions();
MissionData data = missionManager.GetMissionById("boss_001");
```

### ProgressionSystem
```csharp
// Track player progress
progressionSystem.AddMissionCompletion("mission_001");
progressionSystem.AddBossDefeat("Shadow Racer");
progressionSystem.AddDaoVoteParticipation();
progressionSystem.UpdateSVNBalance(1500f);

// Query progression
TierData tier = progressionSystem.GetCurrentTierData();
float progress = progressionSystem.GetProgressToNextTier(); // 0-1
bool unlocked = progressionSystem.IsFeatureUnlocked("Boss battles");
```

### WalletController
```csharp
// Initialize wallet
walletController.Initialize("https://sepolia.infura.io/v3/KEY", "11155111");
await walletController.ConnectWallet();

// Blockchain operations
await walletController.ClaimReward("reward_001", "ipfs://metadata");
await walletController.VoteOnProposal("prop_001", choice: 1);

// Query state
bool connected = walletController.IsConnected;
string address = walletController.walletAddress;
```

### SensoryImmersion
```csharp
// Update feedback systems (called automatically in Update)
sensoryImmersion.UpdateForceFeedback(speed, wheelSlip);
sensoryImmersion.UpdateHaptics(Motif.Storm);
sensoryImmersion.UpdateAdaptiveAudio(rpm, speed);
sensoryImmersion.UpdateHUDMotifs(Motif.Cosmic);
```

### EventBus
```csharp
// Subscribe to events
EventBus.OnMissionComplete += (missionId, success) => { /* ... */ };
EventBus.OnTierUnlocked += (tier, tierName) => { /* ... */ };
EventBus.OnMotifChanged += (newMotif) => { /* ... */ };

// Emit events
EventBus.EmitMissionStart("mission_001");
EventBus.EmitTierUnlocked(3, "Arc Champion");
EventBus.EmitMotifChanged(Motif.Oracle);
```

---

## 🏎️ Vehicle Profile Configuration

**Create ScriptableObject:** Create → Soulvan → Vehicle Profile

```csharp
// Bugatti Bolide Example
torqueCurve: AnimationCurve
  (0, 0)
  (2000, 1200)
  (6000, 1600)
  (8000, 1400)

maxRPM: 8500
gearRatios: [3.5, 2.2, 1.5, 1.1, 0.9, 0.75]
finalDriveRatio: 3.42
drivetrainType: RWD

downforceCoefficient: 2.5
dragCoefficient: 0.25
activeAeroEnabled: true

mass: 1240
centerOfMassOffset: (0, -0.3, 0)
```

**Assign to VehiclePhysics component** in Inspector.

---

## 🎯 Mission Creation Template

```csharp
// Add to MissionManager.Awake() or custom mission loader

// Driving Mission
missionManager.AddMission(new MissionData
{
    id = "mission_custom_001",
    title = "Your Mission Title",
    description = "Brief description",
    type = MissionType.Driving,
    requiredTier = 1,
    rewards = new MissionRewards 
    { 
        svnReward = 100f, 
        nftReward = null 
    },
    targetPosition = new Vector3(100, 0, 200),
    timeLimit = 120f
});

// Stealth Mission
missionManager.AddMission(new MissionData
{
    id = "stealth_001",
    title = "Infiltration Mission",
    description = "Hack terminals undetected",
    type = MissionType.Stealth,
    requiredTier = 2,
    rewards = new MissionRewards 
    { 
        svnReward = 150f, 
        nftReward = "ipfs://stealth_badge" 
    },
    hackTargetCount = 3,
    detectionLimit = 3
});

// Boss Battle
missionManager.AddMission(new MissionData
{
    id = "boss_001",
    title = "Rival Duel",
    description = "Defeat the rival boss",
    type = MissionType.Boss,
    requiredTier = 3,
    rewards = new MissionRewards 
    { 
        svnReward = 500f, 
        nftReward = "ipfs://boss_victory" 
    },
    bossName = "Shadow Racer",
    bossHealth = 1000f
});

// DAO Ritual
missionManager.AddMission(new MissionData
{
    id = "dao_001",
    title = "Governance Vote",
    description = "Vote on DAO proposal",
    type = MissionType.Dao,
    requiredTier = 4,
    rewards = new MissionRewards 
    { 
        svnReward = 200f, 
        nftReward = null 
    },
    proposalId = "prop_001"
});
```

---

## 🎨 Motif System

**4 Motifs:**
```csharp
Motif.Storm   // Electric blue/purple, lightning VFX, 0.9 haptics
Motif.Calm    // Seafoam/mint, fog overlay, 0.3 haptics
Motif.Cosmic  // Pink/gold/purple, aurora, 1.0 haptics
Motif.Oracle  // Violet/cyan, rune glyphs, 0.7 haptics
```

**Usage:**
```csharp
// Set motif globally
MotifAPI motifAPI = FindObjectOfType<MotifAPI>();
motifAPI.SetMotif(Motif.Storm, intensity: 0.8f);

// Query current motif
Motif current = motifAPI.currentMotif;

// Check if unlocked for player
bool unlocked = progressionSystem.IsMotifUnlocked(Motif.Cosmic);
```

---

## 🔧 Input System Setup

**Action Map: Player**

| Action | Type | Gamepad | Keyboard |
|--------|------|---------|----------|
| Throttle | Value | Right Trigger | W |
| Brake | Value | Left Trigger | S |
| Steer | Value | Left Stick X | A/D |
| Nitro | Button | A | Left Shift |
| Jump | Button | X | Space |
| Crouch | Button | B | C |
| Interact | Button | Y | F |

**Assign in Inspector:**
- PlayerInput component on Player GameObject
- Action Asset: InputActions (create via Create → Input Actions)

---

## 📊 5-Tier Progression Quick Table

| Tier | Name | Missions | Bosses | Votes | SVN | Unlocks |
|------|------|----------|--------|-------|-----|---------|
| 1 | Street Racer | 0 | 0 | 0 | 0 | Basic cars, driving, Storm |
| 2 | Mission Runner | 10 | 0 | 0 | 100 | Stealth, on-foot, Calm |
| 3 | Arc Champion | 25 | 3 | 0 | 500 | Bosses, elite cars, Cosmic |
| 4 | DAO Hero | 50 | 5 | 10 | 2K | DAO rituals, Oracle |
| 5 | Mythic Legend | 100 | 10 | 50 | 10K | All content, mythic cars |

---

## 🏁 Scene Setup Checklist

**GameManager:**
- [ ] MissionManager component
- [ ] ProgressionSystem component
- [ ] WalletController component
- [ ] RewardService component
- [ ] MotifAPI component
- [ ] EventBus (singleton, DontDestroyOnLoad)

**Player:**
- [ ] DriverController component
- [ ] OnFootController component
- [ ] PlayerInput component (Input Actions assigned)
- [ ] CharacterController component

**Hypercar:**
- [ ] Rigidbody (mass 1200-2000 kg)
- [ ] VehiclePhysics component
- [ ] SensoryImmersion component
- [ ] 4 WheelCollider children (FL, FR, RL, RR)
- [ ] VehicleProfile ScriptableObject assigned
- [ ] AudioSource (engine sound loop)

**Camera:**
- [ ] Transform reference in DriverController/OnFootController
- [ ] AudioSource (ambient music)

**UI:**
- [ ] Canvas with MissionHUD
- [ ] ProgressionHUD (tier display, progress bar)
- [ ] WalletHUD (balance, address)
- [ ] HUD Motif Overlays (lightning/fog/aurora/runes)

---

## 🔐 Wallet Integration Steps

**1. Deploy Contracts (Testnet)**
```bash
cd /workspaces/Soulvan-genesis-block-creation-and-nonce
npx hardhat run scripts/deploy.js --network sepolia
```

**2. Copy Contract Addresses**
```csharp
// In WalletController.cs
private const string COIN_ADDRESS = "0x...";      // SoulvanCoin
private const string NFT_ADDRESS = "0x...";       // SoulvanCarSkin
private const string CHRONICLE_ADDRESS = "0x..."; // SoulvanChronicle
private const string GOVERNANCE_ADDRESS = "0x..."; // SoulvanGovernance
```

**3. Fund Test Wallet**
- Get testnet ETH: https://sepoliafaucet.com/
- Send 0.1 ETH to test wallet address

**4. Initialize in Game**
```csharp
void Start()
{
    var wallet = FindObjectOfType<WalletController>();
    wallet.Initialize("https://sepolia.infura.io/v3/YOUR_KEY", "11155111");
    wallet.ConnectWallet(); // Prompts passphrase input
}
```

---

## 🎯 Common Tasks

### Start a Mission
```csharp
MissionManager manager = FindObjectOfType<MissionManager>();
manager.StartMission("mission_001");
```

### Complete a Mission
```csharp
manager.CompleteMission("mission_001", success: true);
// Automatically awards rewards, updates progression
```

### Unlock Next Tier
```csharp
// Automatic when requirements met
// Manually trigger for testing:
ProgressionSystem progression = FindObjectOfType<ProgressionSystem>();
progression.AddMissionCompletion("test_mission"); // Repeat until unlock
```

### Spawn a Hypercar
```csharp
HypercarGarage garage = FindObjectOfType<HypercarGarage>();
GameObject bugatti = garage.SpawnHypercar("bugatti_bolide", new Vector3(0, 1, 0));
```

### Switch Between In-Car/On-Foot
```csharp
// Enable DriverController, disable OnFootController for in-car
driverController.enabled = true;
onFootController.enabled = false;

// Enable OnFootController, disable DriverController for on-foot
driverController.enabled = false;
onFootController.enabled = true;
```

### Change Motif
```csharp
MotifAPI motifAPI = FindObjectOfType<MotifAPI>();
motifAPI.SetMotif(Motif.Cosmic, intensity: 1.0f);
// Automatically updates VFX, audio, haptics, HUD
```

### Claim Reward
```csharp
WalletController wallet = FindObjectOfType<WalletController>();
await wallet.ClaimReward("reward_001", "ipfs://metadata_uri");
// Mints NFT, updates balance, triggers cinematic
```

### Vote on Proposal
```csharp
await wallet.VoteOnProposal("prop_001", choice: 1); // 0=No, 1=Yes
// Signs transaction, records on-chain, triggers Oracle motif
```

---

## 🐛 Debugging

### Enable Debug Logs
```csharp
// Add to top of script
#define DEBUG_MODE

#if DEBUG_MODE
Debug.Log("[YourScript] Debug message");
#endif
```

### Visualize Physics
```csharp
// VehiclePhysics draws velocity vectors in Scene view
// Enable Gizmos in Scene view to see:
// - Green arrow: Forward velocity
// - Red arrow: Lateral velocity (slip)
// - Yellow spheres: Wheel positions
```

### Check Event Bus
```csharp
// Subscribe to all events for debugging
EventBus.OnMissionStart += (id) => Debug.Log($"Mission started: {id}");
EventBus.OnMissionComplete += (id, s) => Debug.Log($"Mission complete: {id}, success: {s}");
EventBus.OnTierUnlocked += (t, n) => Debug.Log($"Tier unlocked: {t} - {n}");
```

### Inspect Mission State
```csharp
// Check active mission
MissionManager manager = FindObjectOfType<MissionManager>();
MissionData active = manager.GetMissionById(manager.activeMissionId);
Debug.Log($"Active: {active.title}, Type: {active.type}, Tier: {active.requiredTier}");
```

### Verify Wallet Connection
```csharp
WalletController wallet = FindObjectOfType<WalletController>();
Debug.Log($"Connected: {wallet.IsConnected}, Address: {wallet.walletAddress}");
```

---

## 📦 Package Dependencies

**Unity Packages (Package Manager):**
- Input System (2.0.0+)
- HDRP (12.0.0+ for Unity 2022.3 LTS)
- Visual Effect Graph (12.0.0+)
- Post Processing (3.2.0+)
- TextMeshPro (3.0.0+)

**Hardhat Packages (npm):**
```bash
npm install --save-dev hardhat @nomiclabs/hardhat-ethers ethers
npm install @openzeppelin/contracts
```

---

## 🚀 Performance Tips

**Physics Optimization:**
```csharp
// In Project Settings → Time
Fixed Timestep: 0.02 (50 Hz)
Maximum Allowed Timestep: 0.1

// In Rigidbody
Max Angular Velocity: 20
Interpolation: Interpolate (for smooth camera follow)
```

**Mission System:**
```csharp
// Only update active mission (no inactive mission polling)
// Use object pooling for waypoint markers
```

**VFX:**
```csharp
// Scale particle counts with PerformanceScaler
// Cull off-screen particles
// Use simple billboard particles for distant effects
```

**Audio:**
```csharp
// Use Audio Mixer snapshots for instant motif transitions
// Crossfade ambient tracks (0.5s duration)
// Limit simultaneous audio sources (max 32)
```

---

## 🎬 RTX 5090 + DLSS 4.0

**Enable RTX Features:**
```csharp
RTXAutoScaler rtx = FindObjectOfType<RTXAutoScaler>();
rtx.EnableRTX(true);
rtx.SetRayTracingQuality(RTXQuality.Full); // Full/Hybrid/Off
```

**Configure DLSS:**
```csharp
DLSSController dlss = FindObjectOfType<DLSSController>();
dlss.EnableDLSS(true);
dlss.SetDLSSMode(DLSSMode.Balanced); // Quality/Balanced/Performance
dlss.EnableFrameGeneration(true); // 2-4x FPS boost
```

**Target Performance:**
- 4K native @ 60 FPS (Full RT)
- 4K DLSS Balanced @ 120 FPS (Full RT + Frame Gen)
- 4K DLSS Performance @ 240 FPS (Full RT + Frame Gen)

---

## 📞 Support

**Documentation Files:**
- `FEATURE_MANIFEST.md` - Complete feature list
- `MISSION_SYSTEM_GUIDE.md` - Mission/progression details
- `UNIFIED_BUILD_GUIDE.md` - Build instructions
- `ARCHITECTURE.md` - Vision document
- `FINAL_REPORT.md` - Implementation report

**Scripts Location:** `UnityHDRP/Scripts/`  
**Contracts Location:** `contracts/`  
**Tests:** `test/Soulvan.test.js` (run: `npx hardhat test`)

---

**Quick Ref v1.0 | Soulvan Developer Toolkit** 🏎️⚡
