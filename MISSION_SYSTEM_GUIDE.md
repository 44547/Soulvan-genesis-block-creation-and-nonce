# Soulvan Mission System & Progression Guide

## Overview
Complete integration guide for Soulvan's mission-driven progression system. Players advance through 5 tiers (Street Racer → Mythic Legend) by completing driving missions, stealth infiltrations, boss battles, and DAO rituals.

---

## 5-Tier Progression System

### Tier 1: Street Racer
**Description:** New driver on Soulvan's neon streets  
**Requirements:** None (starting tier)  
**Unlocks:**
- Basic hypercars (Porsche 992.2, Aston Martin Valhalla)
- Driving missions
- Garage access
- Storm motif

**Avatar Evolution:** Basic street racer avatar with default skin

---

### Tier 2: Mission Runner
**Description:** Trusted courier for underground operations  
**Requirements:**
- 10 missions completed
- 100 SVN tokens

**Unlocks:**
- Stealth missions
- Biometric infiltration
- On-foot gameplay
- Calm motif

**Avatar Evolution:** Mission runner outfit with glowing accents (Storm motif VFX)

---

### Tier 3: Arc Champion
**Description:** Victor of Soulvan's narrative arcs  
**Requirements:**
- 25 missions completed
- 3 bosses defeated
- 500 SVN tokens

**Unlocks:**
- Boss battles
- Elite hypercars (McLaren W1, Pagani Utopia)
- Cosmic motif access

**Avatar Evolution:** Arc Champion armor with cosmic aura effects

---

### Tier 4: DAO Hero
**Description:** Governance leader shaping Soulvan's future  
**Requirements:**
- 50 missions completed
- 5 bosses defeated
- 10 DAO votes participated
- 2,000 SVN tokens

**Unlocks:**
- DAO rituals
- Governance voting
- Proposal creation
- Oracle motif

**Avatar Evolution:** DAO Hero regalia with oracle runes and aura

---

### Tier 5: Mythic Legend
**Description:** Transcended hero of Soulvan lore  
**Requirements:**
- 100 missions completed
- 10 bosses defeated
- 50 DAO votes participated
- 10,000 SVN tokens

**Unlocks:**
- All content unlocked
- Mythic hypercars (Bugatti Bolide, Rimac Nevera)
- Exclusive DAO proposals
- All motifs simultaneously
- Legendary NFTs

**Avatar Evolution:** Mythic Legend form with transcendent VFX (multi-motif aura: Storm lightning + Cosmic aurora + Oracle runes)

---

## Mission Types

### 1. Driving Missions
**Gameplay:** High-speed races, rival pursuits, time trials  
**Controller:** `DriverController` (in-car physics-based movement)  
**Key Mechanics:**
- Waypoint navigation with radius detection (5m)
- Rival defeat system (collision + damage)
- Time limit enforcement (60-180 seconds)
- Nitro boost mechanics

**Example Mission Flow:**
1. Player enters hypercar (Bugatti Bolide)
2. Mission start: "Deliver package to downtown waypoint"
3. VehiclePhysics applies torque curves (1,600 HP)
4. SensoryImmersion activates Storm haptics (pulsing triggers)
5. Waypoint reached → Mission complete → SVN reward

---

### 2. Stealth Missions
**Gameplay:** Infiltration, hacking, cargo delivery  
**Controller:** `OnFootController` (crouch, stealth mode, interaction)  
**Key Mechanics:**
- Detection system (3-strike limit before mission failure)
- Hacking terminals (3-second coroutine)
- Cargo delivery to waypoints
- Stealth mode (Calm motif overlay, 0.5x speed multiplier)

**Example Mission Flow:**
1. Player exits vehicle (press F to enter on-foot mode)
2. Mission start: "Hack 3 terminals without detection"
3. OnFootController.EnterStealthMode() → Calm motif applied
4. Player crouches (C key), sneaks past guards
5. Reaches terminal → Interaction prompt → 3s hacking coroutine
6. All terminals hacked → Mission complete → NFT reward

---

### 3. Boss Battles
**Gameplay:** Multi-phase pursuit/duel with rival bosses  
**Controller:** `DriverController` + `OnFootController` (transitions between phases)  
**Key Mechanics:**
- Boss health phases (100% → 75% → 50% → 25% → 0%)
- Cinematic phase transitions (camera shake, slow-motion)
- Arena boundaries (boss escapes if player leaves zone)
- On-foot duel phase (final 25% health)

**Example Mission Flow:**
1. Boss pursuit begins (DriverController in McLaren W1)
2. Player damages boss health to 75% → Phase 1 complete
3. Cinematic: Boss stops, challenges player to on-foot duel
4. OnFootController activated → Attack (LMB), Dodge (Space)
5. Boss defeated (0% health) → Victory cinematic → Legendary NFT reward

---

### 4. DAO Rituals
**Gameplay:** Governance voting with cinematic Oracle motif overlay  
**Controller:** UI-based (no player movement, focus on wallet interaction)  
**Key Mechanics:**
- Voting UI with proposal details (title, description, choices)
- Wallet signature confirmation
- Oracle motif overlay (runes pulsing, ethereal VFX)
- Real blockchain transaction to SoulvanGovernance contract

**Example Mission Flow:**
1. DAO ritual begins → Oracle motif overlays screen (1.0 intensity)
2. Voting UI displays: "Proposal: Increase Arc 2 mission rewards?"
3. Player chooses: Yes (1) / No (0)
4. WalletController.VoteOnProposal() → Sign transaction
5. Vote recorded on-chain → SoulvanChronicle logs entry
6. Mission complete → DAO participation count incremented

---

## Integration Steps

### Step 1: Scene Setup
```csharp
// Hierarchy:
// - GameManager (MissionManager, ProgressionSystem)
// - Player (DriverController, OnFootController, VehiclePhysics, SensoryImmersion)
// - Hypercar (Rigidbody, WheelCollider x4, VehicleProfile ScriptableObject)
// - Camera (Follow player, smooth lag)
// - UI (MissionHUD, ProgressionHUD, WalletHUD)
// - EventBus (singleton, persistent across scenes)

// Assign references in Inspector:
// - MissionManager.walletController → WalletController
// - MissionManager.rewardService → RewardService
// - ProgressionSystem.walletController → WalletController
// - DriverController.vehiclePhysics → VehiclePhysics component
// - SensoryImmersion.vehiclePhysics → VehiclePhysics component
```

### Step 2: Vehicle Profile Configuration
Create VehicleProfile ScriptableObjects for each hypercar:

**Bugatti Bolide Profile:**
```csharp
// Create → Soulvan → Vehicle Profile
// Name: BugattiBolideProfile

torqueCurve: AnimationCurve with keyframes:
  (0 RPM, 0 Nm)
  (2000 RPM, 1200 Nm)
  (6000 RPM, 1600 Nm)
  (8000 RPM, 1400 Nm)

maxRPM: 8500
gearRatios: [3.5, 2.2, 1.5, 1.1, 0.9, 0.75]
finalDriveRatio: 3.42
drivetrainType: RWD

downforceCoefficient: 2.5
dragCoefficient: 0.25
activeAeroEnabled: true

mass: 1240 kg
centerOfMassOffset: (0, -0.3, 0)
```

**Repeat for all 8 hypercars** (McLaren W1, Ferrari SF90 XX, Pagani Utopia, Rimac Nevera, Aston Martin Valhalla, Porsche 992.2, Longbow Speedster)

### Step 3: Mission Database Setup
```csharp
// In MissionManager, add missions:

// Tier 1 missions (Street Racer):
AddMission(new MissionData
{
    id = "mission_001",
    title = "Neon Streets Delivery",
    description = "Deliver package to downtown waypoint",
    type = MissionType.Driving,
    requiredTier = 1,
    rewards = new MissionRewards { svnReward = 50f, nftReward = null },
    targetPosition = new Vector3(100, 0, 200)
});

// Tier 2 missions (Mission Runner):
AddMission(new MissionData
{
    id = "mission_010",
    title = "Server Room Infiltration",
    description = "Hack 3 terminals without detection",
    type = MissionType.Stealth,
    requiredTier = 2,
    rewards = new MissionRewards { svnReward = 100f, nftReward = "ipfs://stealth_badge_001" },
    hackTargetCount = 3
});

// Tier 3 missions (Arc Champion):
AddMission(new MissionData
{
    id = "boss_001",
    title = "Rival Pursuit: Shadow Racer",
    description = "Defeat the Shadow Racer in pursuit",
    type = MissionType.Boss,
    requiredTier = 3,
    rewards = new MissionRewards { svnReward = 500f, nftReward = "ipfs://boss_victory_001" },
    bossName = "Shadow Racer",
    bossHealth = 1000f
});

// Tier 4 missions (DAO Hero):
AddMission(new MissionData
{
    id = "dao_001",
    title = "Governance Vote: Arc 2 Rewards",
    description = "Vote on increasing Arc 2 mission rewards",
    type = MissionType.Dao,
    requiredTier = 4,
    rewards = new MissionRewards { svnReward = 200f, nftReward = null },
    proposalId = "prop_001"
});
```

### Step 4: Input System Configuration
Create Input Actions asset:

**Action Map: Player**
- Throttle (Value, Gamepad Right Trigger + W key)
- Brake (Value, Gamepad Left Trigger + S key)
- Steer (Value, Gamepad Left Stick X + A/D keys)
- Nitro (Button, Gamepad A button + Left Shift key)
- Jump (Button, Gamepad X button + Space key)
- Crouch (Button, Gamepad B button + C key)
- Interact (Button, Gamepad Y button + F key)

Assign to DriverController and OnFootController via PlayerInput component.

### Step 5: Sensory Immersion Setup
```csharp
// Assign audio sources in SensoryImmersion:
// - engineAudioSource → AudioSource on Hypercar (engine sound loop)
// - ambientAudioSource → AudioSource on Camera (ambient music)

// Create Audio Mixer:
// - Snapshots: StormSnapshot, CalmSnapshot, CosmicSnapshot, OracleSnapshot
// - Apply snapshot when motif changes

// Create UI Canvas for HUD motifs:
// - Storm: Lightning flash image (flicker animation)
// - Calm: Fog overlay (subtle glow)
// - Cosmic: Aurora particles (animated gradient)
// - Oracle: Rune glyphs (pulsing alpha)
```

### Step 6: Wallet Integration
```csharp
// Connect WalletController to blockchain:
// 1. Deploy smart contracts to testnet (Hardhat: npx hardhat run scripts/deploy.js --network sepolia)
// 2. Copy contract addresses to WalletController
// 3. Fund test wallet with testnet ETH (faucet)
// 4. Initialize wallet in game:

void Start()
{
    var walletController = FindObjectOfType<WalletController>();
    walletController.Initialize("https://sepolia.infura.io/v3/YOUR_KEY", "11155111");
    walletController.ConnectWallet(); // Prompts passphrase
}
```

### Step 7: Testing Flow
```csharp
// Test Tier 1 → Tier 2 progression:
// 1. Start game → Player at Tier 1 (Street Racer)
// 2. Complete 10 driving missions → Earn 500 SVN
// 3. ProgressionSystem checks requirements → Tier 2 unlocked
// 4. Cinematic plays (Calm motif overlay, avatar evolution)
// 5. Stealth missions now available in MissionManager

// Test Boss Battle:
// 1. Player reaches Tier 3 (Arc Champion)
// 2. Start boss mission (mission_boss_001)
// 3. BossBattle controller spawns boss vehicle
// 4. Player damages boss to 25% health
// 5. Phase transition → On-foot duel begins
// 6. OnFootController activated → Attack boss with melee
// 7. Boss defeated → Victory cinematic → Legendary NFT minted

// Test DAO Ritual:
// 1. Player reaches Tier 4 (DAO Hero)
// 2. Start DAO ritual mission (dao_001)
// 3. Oracle motif overlays screen (runes pulsing)
// 4. Voting UI displays proposal
// 5. Player votes → WalletController signs transaction
// 6. Vote recorded on SoulvanGovernance contract
// 7. Mission complete → DAO participation count incremented
```

---

## Performance Optimization

### Physics Optimization
```csharp
// VehiclePhysics.cs uses FixedUpdate for physics calculations
// Target 60 FPS with RTX 5090 + DLSS 4.0 Frame Generation = 240 FPS

// Optimize wheel collider calculations:
fixedDeltaTime = 0.02f; // 50 Hz physics update rate
maxAngularVelocity = 20f; // Prevent excessive rotation
```

### Mission System Optimization
```csharp
// MissionManager caches active mission only (no inactive mission updates)
// Use object pooling for waypoint markers, boss projectiles

// ProgressionSystem saves to PlayerPrefs + blockchain:
// - PlayerPrefs: Immediate local save (0ms)
// - Blockchain: Async save (2-5s transaction time, non-blocking)
```

### Sensory Immersion Optimization
```csharp
// SensoryImmersion updates at 60 Hz:
// - Force feedback: Every frame (critical for steering feel)
// - Haptics: 20 Hz (enough for vibration perception)
// - Adaptive audio: 30 Hz (pitch/volume changes)
// - HUD motifs: 60 Hz (smooth VFX)

// Use culling for off-screen HUD elements
```

---

## Troubleshooting

### Issue: Tier not unlocking after requirements met
**Solution:** Check ProgressionSystem.CheckTierUnlock() logs. Ensure all 4 requirements met (missions, bosses, DAO votes, SVN balance). Verify PlayerPrefs values.

### Issue: Boss not taking damage
**Solution:** Ensure OnFootController.attackPoint is assigned in Inspector. Check enemyLayers mask includes boss layer. Verify BossBattle.ApplyDamage() is called.

### Issue: Haptics not working
**Solution:** Confirm Gamepad connected (Input System). Check SensoryImmersion.gamepad is not null. Verify Input System package installed (2.0.0+).

### Issue: Wallet transaction failing
**Solution:** Check WalletController logs for error messages. Verify sufficient testnet ETH for gas. Confirm contract addresses correct. Test RPC endpoint connectivity.

---

## Content Creation Guide

### Creating New Missions
```csharp
// 1. Define mission data:
var newMission = new MissionData
{
    id = "mission_custom_001",
    title = "Your Mission Title",
    description = "Your mission description",
    type = MissionType.Driving, // or Stealth, Boss, Dao
    requiredTier = 2, // Minimum tier to unlock
    rewards = new MissionRewards { svnReward = 150f, nftReward = "ipfs://your_nft_uri" }
};

// 2. Add mission-specific parameters:
// For Driving: targetPosition, rivalCount, timeLimit
// For Stealth: hackTargetCount, detectionLimit
// For Boss: bossName, bossHealth
// For DAO: proposalId

// 3. Register in MissionManager:
missionManager.AddMission(newMission);

// 4. Create mission scene assets (waypoints, terminals, boss vehicle)
```

### Creating Custom Bosses
```csharp
// 1. Create boss vehicle prefab (use hypercar base)
// 2. Add BossBattle component
// 3. Configure boss health, attack patterns
// 4. Design arena boundaries (colliders)
// 5. Add cinematic cameras for phase transitions
// 6. Create boss AI script (pursuit logic, attack patterns)
```

### Creating Mission Storyboards
```markdown
## Mission Storyboard Template

**Mission ID:** mission_arc2_001  
**Title:** [Mission Title]  
**Description:** [Brief description]  
**Type:** Driving / Stealth / Boss / DAO  
**Required Tier:** [1-5]  
**Narrative Arc:** [Arc 1, Arc 2, etc.]

**Story Context:**
[2-3 sentences setting up mission narrative]

**Gameplay Flow:**
1. [Mission start event]
2. [Player action]
3. [Game response]
4. [Success condition]
5. [Reward grant]

**Cinematic Moments:**
- [Camera angle, VFX, audio cues]

**Rewards:**
- SVN: [Amount]
- NFT: [URI or null]
```

---

## API Reference

### ProgressionSystem
```csharp
void AddMissionCompletion(string missionId);
void AddBossDefeat(string bossName);
void AddDaoVoteParticipation();
void UpdateSVNBalance(float newBalance);
TierData GetCurrentTierData();
float GetProgressToNextTier();
bool IsFeatureUnlocked(string feature);
bool IsMotifUnlocked(Motif motif);
```

### MissionManager
```csharp
void StartMission(string missionId);
void CompleteMission(string missionId, bool success);
List<MissionData> GetAvailableMissions();
MissionData GetMissionById(string missionId);
```

### DriverController
```csharp
void SetTarget(Vector3 targetPosition); // AI-assisted driving
```

### OnFootController
```csharp
void EnterStealthMode();
void ExitStealthMode();
void PerformAttack();
void PerformDodge();
```

### VehiclePhysics
```csharp
void SetThrottle(float throttle); // 0-1
void SetBrake(float brake); // 0-1
void SetSteering(float steering); // -1 to 1
void SetNitro(bool active);
float GetCurrentSpeed(); // km/h
float GetCurrentRPM();
```

### SensoryImmersion
```csharp
void UpdateForceFeedback(float speed, float wheelSlip);
void UpdateHaptics(Motif currentMotif);
void UpdateAdaptiveAudio(float rpm, float speed);
void UpdateHUDMotifs(Motif currentMotif);
```

---

## Next Steps

1. **Content Creation:**
   - Design 100+ missions across all types
   - Create mission storyboards for narrative arcs
   - Build boss arenas and cinematic sequences

2. **VFX Assets:**
   - Storm lightning effects (HDRP VFX Graph)
   - Calm fog overlays (volumetric fog)
   - Cosmic aurora particles (Niagara in Unreal)
   - Oracle rune glyphs (UI animation)

3. **Audio Assets:**
   - Record/license engine sounds for 8 hypercars
   - Compose adaptive music for each motif
   - Create ambient soundscapes (city, highway, desert)

4. **Testnet Deployment:**
   - Deploy contracts to Sepolia testnet
   - Test wallet integration end-to-end
   - Verify DAO voting on-chain

5. **Playtesting:**
   - Balance mission difficulty curves
   - Tune vehicle physics for realism vs fun
   - Optimize sensory feedback intensity

---

**Soulvan Mission System & Progression: Complete** ✅
