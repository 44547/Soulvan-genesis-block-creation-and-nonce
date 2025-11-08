# Neon Vault Heist System Guide

Complete documentation for the Soulvan Neon Vault Heist mission system with DAO integration, procedural generation, and replay recording.

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Mission Flow](#mission-flow)
4. [Component Reference](#component-reference)
5. [ScriptableObjects](#scriptableobjects)
6. [Setup Guide](#setup-guide)
7. [API Integration](#api-integration)
8. [Replay & RemixForge](#replay--remixforge)
9. [DAO Integration](#dao-integration)
10. [Troubleshooting](#troubleshooting)

---

## Overview

The Neon Vault Heist is a GTA-style co-op heist mission system featuring:

- **Dynamic difficulty scaling** via AIDirector based on tension (heat + time + alerts)
- **4-player roles**: Driver, Infiltrator, Systems, Support with unique abilities
- **Procedural mission composition** from modular ScriptableObjects
- **Deterministic replay seeds** for RemixForge saga variants
- **DAO governance integration** with heat-to-influence conversion and tier upgrades
- **Server-authoritative networking** with anti-cheat validation

### Key Features

- **Hover vehicle physics** with boost mechanics and pursuit evasion
- **Pattern-based hacking minigame** with role bonuses
- **Datacore carry-and-trade system** with movement penalties
- **Cinematic cutscenes** via spline-based camera paths
- **Visual FX management** with object pooling
- **Real-time DAO overlay** showing proposals, vote ripples, tier bursts

---

## Architecture

### Core Systems

```
MissionController (orchestration)
├── AIDirector_Heist (adaptive difficulty)
├── RoleManager (player role assignments)
├── SpawnManager (enemy spawning)
├── ReplaySeedLogger (async replay logging)
└── DAOMissionNotifier (DAO impact tracking)

Gameplay Systems
├── HackingMiniGame (pattern puzzles)
├── CarryAndTradeoff (datacore mechanics)
├── VehicleController_Hovercar (arcade physics)
├── PursuitAI (chase enemies)
├── CameraCinematicController (cutscenes)
├── FXManager (visual effects)
└── NetworkSync_Authoritative (anti-cheat)

DAO Integration
├── DAOOverlayFXManager (visual overlay)
├── DAOImpactGraphSDK (graph integration)
└── RemixForgeGraph (contributor visualization)

Data Layer
├── SO_MissionModule (procedural modules)
├── SO_DatacoreTier (reward tiers)
├── SO_HeatModifiers (heat actions)
└── SO_VehicleBlueprint (vehicle specs)
```

### Data Flow

```
Mission Start
    ↓
MissionController generates deterministic seed
    ↓
Procedural Composer selects modules (approach → breach → extraction)
    ↓
Players activate modules → Heat accumulates
    ↓
AIDirector adjusts tension → Spawns scale
    ↓
Mission Complete → Choose DatacoreTier based on performance
    ↓
ReplaySeedLogger queues seed → Server signs → Returns replayId
    ↓
DAOMissionNotifier calculates influence → Checks tier upgrade
    ↓
DAO API triggers ripples → DAOOverlayFX visualizes
    ↓
RemixForgeGraph pulses contributor nodes
```

---

## Mission Flow

### 7-Beat Structure

**Beat 1: Infiltration** (approach module)
- Entry methods: Magrail (low heat), Street Parade (high heat)
- Role: Infiltrator bonus for stealth

**Beat 2: Perimeter Bypass** (perimeter module)
- Security cameras, patrol paths
- Role: Systems hack speed bonus

**Beat 3: Vault Breach** (breach module)
- Pattern routing puzzle or service hatch
- Failure → alarm triggered (20 heat)

**Beat 4: Datacore Retrieval** (interior module)
- Pick up datacore (5 heat)
- Movement penalty (0.7x speed)

**Beat 5: Extraction Start** (extraction module)
- Get to vehicle
- Role: Driver boost bonus

**Beat 6: Chase Sequence** (pursuit)
- AIDirector spawns enemies based on tension
- PursuitAI rams and shoots
- Heat spikes on vehicle damage

**Beat 7: Post-Mission** (complete)
- ChooseDatacoreTier: performance score = (timeScore * 0.6 + heatScore * 0.4)
- Thresholds: 0.9 = Mythic, 0.7 = Rare, else Standard
- FinalizeAndLog → Replay + DAO impact

---

## Component Reference

### MissionController

**Purpose**: Core mission orchestration with state machine and heat tracking.

**Key Methods**:
- `StartMission()`: Initialize mission state, launch RunMissionLoop
- `RegisterModuleActivation(moduleId)`: Add module to activated list, calculate heat
- `ChooseDatacoreTier()`: Calculate performance score, select tier
- `GenerateDeterministicSeed()`: Hash missionId + startTime + modules + heat
- `FinalizeAndLog()`: Create ReplayLogDto, queue for logging
- `AssignRole(playerId, role)`: Store role assignment
- `EndMissionSuccess() / EndMissionFail()`: Set state, emit events

**State Machine**: Waiting → InProgress → (Paused) → Completed/Failed

**Events**: OnMissionStarted, OnMissionUpdated (0.5s), OnMissionCompleted, OnMissionFailed

### AIDirector_Heist

**Purpose**: Adaptive difficulty scaling based on real-time tension calculation.

**Tension Formula**:
```
tension = (heatNorm * 0.6) + (timeFactor * 0.2) + (alertFactor * 0.2)
```

**Spawn Scaling**:
- `spawnInterval = baseSpawnInterval * tensionToSpawnRate.Evaluate(tension)`
- `spawnCount = baseEnemyCount * (1 + tension * 2)` (clamped to maxEnemiesPerWave)
- `aggression = tensionToAggression.Evaluate(tension)`

**Audio Integration**: Sets AudioMixer parameters `ChaseIntensity` and `TensionPadVolume`

**Methods**:
- `UpdateTension()`: Recalculate tension from heat/time/alerts
- `AddImmediateAlert(alertAmount)`: Spike tension for triggered alarms
- `GetAggressionMultiplier()`: Returns current aggression for external AI

### RoleManager

**Purpose**: Manage 4-player role assignments with bonuses.

**Roles**:
- **Driver**: 1.2x credits, Vehicle Boost ability
- **Infiltrator**: 1.1x credits, Stealth Cloak ability
- **Systems**: 1.15x credits, Hack Speed ability (1.5x faster hacking)
- **Support**: 1.0x credits, Team Shield ability

**Methods**:
- `AssignRole(playerId, role)`: Assign role (prevents duplicates)
- `GetPlayerRole(playerId)`: Query player's role
- `IsRoleAvailable(role)`: Check if role is free
- `CalculateCreditReward(playerId, baseCredits)`: Apply role multiplier
- `ActivateRoleAbility(playerId)`: Trigger role-specific power

### HackingMiniGame

**Purpose**: Pattern-matching puzzle for vault breaches.

**Configuration**:
- `gridSize`: 16 nodes (4x4 grid)
- `patternLength`: 5 nodes to match
- `timeLimit`: 30s (adjusted by Systems role: /1.5)
- `failureHeatPenalty`: 15 heat on failure
- `successHeatReduction`: 5 heat on success

**Flow**:
1. `StartHacking(playerId)`: Generate random pattern, show briefly
2. Player clicks nodes to replicate pattern
3. `CheckPattern()`: Compare player input to target
4. Success → Grant access, reduce heat
5. Failure → Trigger alarm, add heat penalty

**Events**: `OnHackingComplete(success, timeBonus)`

### CarryAndTradeoff

**Purpose**: Datacore carrying mechanic with movement penalties and player trading.

**Penalties While Carrying**:
- Movement speed: 0.7x
- Sprint: Disabled (configurable)
- Weapons: Disabled (configurable)

**Trading**:
- Press `E` near teammate → Start 2s tradeoff timer
- Players must stay within 3m range
- Success → Datacore transferred, no heat change
- Failure → If players separate, trade cancelled

**Drop Mechanics**:
- `dropOnDamage`: true → Datacore drops when player damaged (10 heat)
- `Drop()`: Manual drop (1m in front of player)

**Events**: `OnPickup`, `OnDrop`, `OnTradeoffComplete`

### VehicleController_Hovercar

**Purpose**: Arcade-style hovercar physics for getaway sequences.

**Hover System**:
- 4-point hover rays (front-left, front-right, rear-left, rear-right)
- Spring force: `compressionRatio * hoverForce + dampingForce`
- Target height: 2m above ground

**Movement**:
- Acceleration: 80 units/s
- Max speed: 420 units/s (840 during boost)
- Turn speed: 90°/s
- Brake force: 150 units/s

**Boost**:
- Duration: 3s (3.9s for Driver role with 1.3x bonus)
- Cooldown: 10s
- Speed multiplier: 2x

**Damage**:
- Max health: 100
- Destroyed at 0 health → Spawn explosion, mission fail
- Damage flash effect on hit

**Methods**:
- `SetDriver(playerId, role)`: Apply role bonuses
- `TakeDamage(damage)`: Apply damage, check for death
- `GetSpeed()`: Current forward speed
- `IsBoostReady()`: Check boost availability

### PursuitAI

**Purpose**: Enemy AI for chase sequences with NavMesh pathfinding.

**Chase States**:
- **Pursuing**: Follow target with prediction
- **Ramming**: Close range (≤5m) collision attack
- **Shooting**: Medium range (5-30m) projectile attacks
- **Retreating**: Break off if distance >100m

**Aggression Scaling**:
- Base aggression: 1.0
- Tension multiplier: from AIDirector (0.3-1.5x)
- Speed: 15 * (baseAggression + tensionAggression)
- Damage: weaponDamage * (baseAggression + tensionAggression)

**Ramming**:
- Force: 500 * aggression
- Damage: 20 * aggression
- Cooldown: 5s

**Shooting**:
- Fire rate: 2 shots/second
- Weapon damage: 10 per shot * aggression
- Range: 30m

### CameraCinematicController

**Purpose**: Spline-based cinematic camera for mission cutscenes.

**Spline Generation**:
- Linear interpolation: Simple Lerp between nodes
- Catmull-Rom interpolation: Smooth curves (requires 4+ nodes)
- Segments: 10 interpolation points per node

**Playback**:
- Duration: Configurable (default 5s)
- Speed curve: AnimationCurve for variable speed
- Look target: Optional Transform or look ahead along path
- FOV override: Optional field-of-view change

**Camera Effects**:
- Shake: Perlin noise-based intensity
- Blend in/out: Smooth transition from/to gameplay camera

**Methods**:
- `PlayCinematic()`: Start cinematic playback
- `StopCinematic()`: Force stop and return to gameplay camera

### FXManager

**Purpose**: Centralized visual effects management with object pooling.

**FX Types**:
- RunePulse: DAO rune flare effects
- Ripple: Vote ripple waves
- TierBurst: Tier upgrade particle bursts
- Explosion: Vehicle/enemy explosions
- MuzzleFlash: Weapon fire effects
- ImpactSparks: Collision/hit effects

**Pooling**:
- Initial pool size: 10 per FX type
- Auto-expand on demand
- Return to pool after duration

**Methods**:
- `SpawnFX(fxType, position, rotation, duration)`: Spawn FX at world position
- `SpawnRunePulse(position, color, scale)`: Convenience for rune pulses
- `SpawnRipple(position, color, radius)`: Convenience for ripples
- `SpawnTierBurst(position, color, intensity)`: Convenience for tier bursts
- `ReturnFXToPool(fx, fxType)`: Manual return to pool
- `ClearAllFX()`: Despawn all active effects

### NetworkSync_Authoritative

**Purpose**: Server-authoritative state sync with anti-cheat validation.

**Sync Interval**: 0.5s (configurable)

**Validation**:
- Heat deviation check: Flags heat changes >50/sec
- Position deviation check: Flags position changes >5m
- Event validation: Critical events verified by server

**Corrections**:
- Server can force heat value
- Server can correct player positions
- Server can force mission state (e.g., fail on cheat detection)

**Methods**:
- `SyncWithServer()`: Send client state, receive server state
- `ValidateEvent(eventType, eventData, callback)`: Request server validation for critical events

**Anti-Cheat**:
- `maxHeatChangePerSecond`: 50 (exceeded = correction applied)
- `maxClientDeviation`: 5m (exceeded = teleport correction)
- Server stores authoritative replay seed signature

---

## ScriptableObjects

### SO_MissionModule

**Purpose**: Define modular mission segments for procedural generation.

**Fields**:
- `moduleId`: Unique ID (e.g., "approach:magrail")
- `displayName`: UI name (e.g., "Magrail Silent Entry")
- `description`: Flavor text
- `weight`: Selection probability (0-1, higher = more likely)
- `entryTags`: Required tags to activate (e.g., ["approach"])
- `exitTags`: Tags provided on completion (e.g., ["perimeter"])
- `heatModifier`: Heat delta (-1 to +20)
- `heatMultiplier`: Heat scaling (0.5-2.0)
- `creditBonus`: Bonus credits for completion
- `experienceReward`: XP reward
- `notes`: Developer notes

**Example**:
```json
{
  "moduleId": "approach:magrail",
  "displayName": "Magrail Silent Entry",
  "weight": 0.6,
  "entryTags": ["approach"],
  "exitTags": ["perimeter"],
  "heatModifier": -1.0,
  "creditBonus": 500
}
```

**Methods**:
- `CanActivate(currentTags)`: Check if module can be used
- `GetHeatDelta(baseHeat)`: Calculate total heat change

### SO_DatacoreTier

**Purpose**: Define datacore reward tiers based on performance.

**Fields**:
- `tierId`: Unique ID (e.g., "datacore:mythic")
- `displayName`: UI name (e.g., "Mythic Datacore")
- `rarityScore`: Rarity (1-100, lower = more rare)
- `dropChance`: Drop probability (0.01-1.0)
- `uiColor`: Hex color (e.g., "#FFD700")
- `glowIntensity`: Particle intensity (1-5)
- `creditReward`: Credits awarded (1000-5000)
- `experienceReward`: XP awarded (50-300)
- `daoInfluenceBonus`: DAO power gain (2-10)
- `minPerformanceScore`: Required score (0-1)
- `requiredRole`: Role requirement (empty = any)

**Tier Progression**:
- **Standard**: rarityScore=10, minPerformanceScore=0.0, 1000 credits
- **Rare**: rarityScore=3, minPerformanceScore=0.7, 2500 credits
- **Mythic**: rarityScore=1, minPerformanceScore=0.9, 5000 credits

**Methods**:
- `QualifiesFor(performanceScore, playerRole)`: Check eligibility
- `GetTotalRewardValue()`: Calculate combined reward value

### SO_HeatModifiers

**Purpose**: Define heat changes for player actions.

**Modifier List**:
- `alarmTriggered`: +20 heat
- `datacorePickup`: +5 heat
- `useHoloDecoy`: -2 heat
- `magrailEntry`: -1 heat
- `streetParadeEntry`: +8 heat
- `breach:pattern:success`: +10 heat
- `breach:pattern:failed`: +25 heat
- `combat:kill`: +20 heat
- `combat:stun`: +10 heat
- `stealth:takedown`: +5 heat
- `vehicle:damaged`: +15 heat
- `vehicle:destroyed`: +50 heat
- `datacore:dropped`: +10 heat
- `datacore:tradeoff`: 0 heat

**Methods**:
- `GetHeatDelta(action)`: Get heat change for action
- `HasAction(action)`: Check if action exists
- `GetDangerousActions()`: Get all actions with positive heat
- `GetCoolingActions()`: Get all actions with negative heat

### SO_VehicleBlueprint

**Purpose**: Define vehicle specifications and upgrade slots.

**Fields**:
- `blueprintId`: Unique ID (e.g., "hovercar:prototypeA")
- `displayName`: UI name (e.g., "Prototype Hover-X")
- `description`: Vehicle description
- `maxSpeed`: Max speed (320-420 units/s)
- `acceleration`: Accel rate (60-80)
- `handling`: Handling rating (0.7-0.9)
- `brakeForce`: Braking power (140-180)
- `boostMultiplier`: Boost speed (1.5-2.5x)
- `boostDuration`: Boost time (2-4s)
- `boostCooldown`: Boost cooldown (10-15s)
- `signatureBase`: Detection radius (5-15m)
- `signatureBoost`: Signature while boosting (12-25m)
- `signatureDamaged`: Signature when damaged (15-20m)
- `maxHealth`: HP (75-150)
- `armorRating`: Damage reduction (0-0.5)
- `upgradeSlots`: Available upgrades (["engine", "thruster", "stealth"])

**Vehicle Variants**:
- **Prototype Hover-X**: Speed=420, Handling=0.85, Health=100, Armor=0
- **Enforcer Heavy Cruiser**: Speed=320, Handling=0.7, Health=150, Armor=0.3
- **Phantom Stealth Runner**: Speed=380, Handling=0.9, Health=75, Signature=5

**Methods**:
- `GetEffectiveSignature(isBoosting, isDamaged)`: Calculate detection radius
- `ApplyArmorReduction(incomingDamage)`: Calculate damage after armor

---

## Setup Guide

### 1. Scene Setup

**Required GameObjects**:
```
NeonVaultHeist_Root
├── MissionController (GameObject)
│   ├── MissionController (script)
│   ├── AIDirector_Heist (script)
│   ├── RoleManager (script)
│   ├── ReplaySeedLogger (script)
│   └── DAOMissionNotifier (script)
├── NetworkSync_Authoritative (GameObject)
│   └── NetworkSync_Authoritative (script)
├── ServerRpcClient (GameObject)
│   └── ServerRpcClient (script)
├── SpawnManager (GameObject)
│   └── SpawnManager (script)
├── FXManager (GameObject)
│   └── FXManager (script)
├── DAOImpactGraphSDK (GameObject)
│   ├── DAOImpactGraphSDK (script)
│   ├── DAOOverlayFXManager (reference)
│   └── RemixForgeGraph (reference)
└── Environment (GameObject group)
    ├── NeonCity_BlockA (Prefab)
    ├── Skybridge_Array (Prefab)
    ├── Vault_Exterior_Module (Prefab)
    ├── Vault_Interior_Core (Prefab)
    ├── VerticalFreeway_Group (Prefab)
    └── OrbitalCourierDock (Prefab)
```

### 2. Import ScriptableObjects

**Using Unity Editor Tool**:
1. Window → Soulvan → Import ScriptableObjects from JSON
2. Set source folder: `Assets/Scripts/ScriptableObjects/seed-json`
3. Set target folder: `Assets/ScriptableObjects`
4. Click "Import All"
5. Assets created at:
   - `Assets/ScriptableObjects/MissionModules/`
   - `Assets/ScriptableObjects/Datacores/`
   - `Assets/ScriptableObjects/Heat/`
   - `Assets/ScriptableObjects/Vehicles/`

### 3. Configure MissionController

**Inspector Setup**:
- Mission ID: `"neon_vault_001"`
- Mission Time Limit: `1200` (20 minutes)
- Max Players: `4`
- Mission Modules: Assign SO_MissionModule assets
- Datacore Tiers: Assign SO_DatacoreTier assets
- Heat Modifiers: Assign SO_HeatModifiers asset

### 4. Configure AIDirector_Heist

**Tuning**:
- Base Spawn Interval: `5` seconds
- Base Enemy Count: `3`
- Tension Weights: Heat=0.6, Time=0.2, Alert=0.2
- Max Heat For Tension: `100`
- Max Time For Tension: `300` seconds
- Max Enemies Per Wave: `12`
- Tension Curves: Adjust in inspector for desired pacing

### 5. Configure ServerRpcClient

**API Setup**:
- Base URL: `https://api.soulvan` (or your dev server)
- Auth Token: Set programmatically or via PlayerPrefs
- Max Retries: `3`
- Request Timeout: `30` seconds

### 6. Configure SpawnManager

**Spawn Setup**:
- Enemy Prefab: Assign pursuit drone/vehicle prefab
- Spawn Points: Assign Transform array for spawn locations
- Min Spawn Distance From Player: `30` meters
- Max Active Enemies: `20`
- Use Pooling: `true`
- Initial Pool Size: `10`

### 7. Configure DAOImpactGraphSDK

**Integration**:
- DAO Overlay: Assign DAOOverlayFXManager component
- Remix Graph: Assign RemixForgeGraph component
- Auto Sync Interval: `5` seconds
- Enable Proposal Sync: `true`
- Enable Ripple Propagation: `true`
- Graph Ripple Multiplier: `2.0`

---

## API Integration

### Endpoints Used

**Replay Mint API** (port 3002):
- POST `/replay/log`: Log replay seed with deterministic hash
  - Request: `{missionId, timestamp, modules, contributorId, heat, performance, datacoreTier}`
  - Response: `{replayId, signedSeed, remixSeed}`

**DAO Replay Impact API** (port 3004):
- POST `/dao/impact`: Record mission impact on DAO governance
  - Request: `{proposalId, contributorId, replayId, heatDelta, influenceScore, tierPulse}`
  - Response: `{impactId, rippleNodes, badgeUpgrades}`
- GET `/dao/proposals`: Get active governance proposals
- GET `/dao/vote-ripples/:proposalId`: Get vote ripple visualization data
- GET `/dao/contributor-stats/:id`: Get contributor tier and DAO power

**Mission Sync API** (custom endpoint):
- POST `/mission/sync`: Server-authoritative state sync
  - Request: `{missionId, elapsed, heat, missionState, playerStates[]}`
  - Response: `{serverState, corrections[]}`

### Authentication

All API calls use JWT bearer tokens:
```csharp
ServerRpcClient.Instance.SetAuthToken(jwtToken);
```

Initial auth:
```csharp
yield return ServerRpcClient.Instance.InitAuth(walletAddress, (success, response) => {
    if (success) {
        // Token automatically set
        Debug.Log("Auth complete");
    }
});
```

---

## Replay & RemixForge

### Deterministic Seed Generation

**Seed Composition**:
```
seedInput = missionId|timestamp|modules|contributorId
digest = SHA256(seedInput)
signedSeed = digest:serverSignature (HMAC-SHA256 with server secret)
```

**Example**:
```csharp
string[] modules = {"approach:magrail", "breach:pattern", "extraction:freeway"};
long timestamp = DeterministicSeedUtil.ComputeCurrentTimestamp();
string seedInput = DeterministicSeedUtil.ComposeSeedInput(
    "neon_vault_001",
    timestamp,
    modules,
    "C001"
);
string digest = DeterministicSeedUtil.ComputeLocalDigest(seedInput);
// Server signs digest and returns signedSeed
```

### Replay Logging Flow

```
Mission Complete
    ↓
MissionController.FinalizeAndLog()
    ↓
Create ReplayLogDto:
  - missionId
  - digest (local)
  - timestamp
  - modules[]
  - contributorId
  - heat
  - performanceScore
  - datacoreTier
    ↓
ReplaySeedLogger.QueueAndSend(dto)
    ↓
LocalWrite(dto) to Application.persistentDataPath/ReplayLogs/
    ↓
SendWithRetry(dto) → POST /replay/log
    ↓
Server validates, signs seed, returns:
  - replayId (R[1000-9999])
  - signedSeed (digest:signature)
  - remixSeed (seed for RemixForge variants)
    ↓
ReplaySeedLogger persists signedSeed + replayId
    ↓
RemixForge can use remixSeed for saga variants
```

### Retry Logic

**Exponential Backoff**:
```
waitTime = Pow(retryBackoffMultiplier, attempts - 1)
```

**Default**: 3 attempts with multiplier=2
- Attempt 1: 0s delay
- Attempt 2: 2^0 = 1s delay
- Attempt 3: 2^1 = 2s delay

**Failure Handling**:
- After max retries → `PersistForManualSync()`
- Saves to `/ReplayLogs/Failed/FAILED_{missionId}_{seed}.json`
- On game restart → `LoadPersistedLogs()` auto-loads failed logs
- Call `RetryFailedLogs()` to re-queue for another attempt

---

## DAO Integration

### Heat-to-Influence Conversion

**Formula**:
```
influenceScore = heatDelta * heatToInfluenceMultiplier
Default multiplier: 0.1
Clamped: 0-10

Example: 50 heat → 5.0 influence
```

### Tier Upgrade Evaluation

**Threshold**: 50 heat minimum

**Tier Progression**:
```
Initiate (C001)
    ↓ 50 heat
Builder (C001)
    ↓ 50 heat
Architect (C001)
    ↓ 50 heat
Oracle (C001)
    ↓ 50 heat
Legend (C001)
```

**Evaluation Flow**:
```csharp
// In DAOMissionNotifier
if (heatDelta >= tierUpgradeThreshold) { // 50
    string currentTier = PlayerPrefs.GetString($"Tier_{contributorId}", "Initiate");
    string nextTier = GetNextTier(currentTier);
    
    TierPulseData pulse = new TierPulseData {
        from = currentTier,
        to = nextTier,
        daoPowerGain = 10.0f
    };
    
    // Include in DAO impact payload
    payload.tierPulse = pulse;
}
```

### Visual FX Triggers

**Vote Ripple**:
```csharp
DAOImpactGraphSDK.Instance.PropagateVoteRipple(
    proposalId: "D163",
    contributorId: "C001",
    vote: "YES",
    votePower: 100.0f
);
```

**Tier Upgrade Burst**:
```csharp
DAOImpactGraphSDK.Instance.VisualizeTierUpgrade(
    contributorId: "C001",
    fromTier: "Builder",
    toTier: "Architect"
);
```

**Governance Trail**:
```csharp
List<string> proposalIds = new List<string> { "D163", "D164", "D165" };
DAOImpactGraphSDK.Instance.CreateGovernanceTrail("C001", proposalIds);
```

### Voice Lines

**Tier Upgrade**: "You rise. The vault remembers."
- Played by DAOOverlayFXManager.TriggerTierUpgradeBurst()
- Audio clip: `tierUpgradeAudio`

---

## Troubleshooting

### Mission Not Starting

**Symptoms**: MissionController state stays in Waiting

**Checks**:
1. Ensure `StartMission()` is called
2. Check `missionTimeLimit` > 0
3. Verify mission modules assigned in inspector
4. Check console for initialization errors

**Fix**:
```csharp
MissionController mc = FindObjectOfType<MissionController>();
mc.StartMission();
```

### Heat Not Accumulating

**Symptoms**: Heat stays at 0 despite actions

**Checks**:
1. Verify `SO_HeatModifiers` assigned to MissionController
2. Check `RegisterModuleActivation()` is being called
3. Inspect `SO_HeatModifiers` asset for correct action IDs
4. Enable debug logs in MissionController

**Fix**:
```csharp
mc.RegisterModuleActivation("datacorePickup"); // Should add 5 heat
Debug.Log($"Current heat: {mc.state.heat}");
```

### Replay Logging Failing

**Symptoms**: ReplaySeedLogger shows failed count >0

**Checks**:
1. Verify ServerRpcClient is initialized with auth token
2. Check server URL is correct
3. Check network connectivity
4. Inspect failed log files in `Application.persistentDataPath/ReplayLogs/Failed/`

**Manual Retry**:
```csharp
ReplaySeedLogger.Instance.RetryFailedLogs();
```

**Check Failed Logs**:
```csharp
int failedCount = ReplaySeedLogger.Instance.GetFailedCount();
Debug.Log($"Failed logs: {failedCount}");
```

### DAO Overlay Not Showing

**Symptoms**: No proposal nodes or vote ripples visible

**Checks**:
1. Verify DAOOverlayFXManager has RemixForgeGraph reference
2. Check DAOImpactGraphSDK is initialized
3. Verify DAO API auth token is set
4. Check camera layer masks include DAO overlay layer

**Force Sync**:
```csharp
DAOImpactGraphSDK.Instance.SyncProposals(); // Manual sync
int proposalCount = DAOImpactGraphSDK.Instance.GetActiveProposalCount();
Debug.Log($"Active proposals: {proposalCount}");
```

### Vehicle Physics Issues

**Symptoms**: Hovercar falls through ground or bounces erratically

**Checks**:
1. Ensure ground has correct layer (included in `groundLayer` mask)
2. Check `hoverHeight` (default 2m)
3. Verify `hoverForce` is sufficient (default 100)
4. Check Rigidbody is not kinematic
5. Ensure NavMesh is baked for pursuit AI

**Tuning**:
```csharp
VehicleController_Hovercar vehicle = GetComponent<VehicleController_Hovercar>();
vehicle.hoverHeight = 2f;
vehicle.hoverForce = 100f;
vehicle.hoverDamping = 5f;
```

### AI Not Spawning

**Symptoms**: No enemies spawned during chase

**Checks**:
1. Verify SpawnManager has enemy prefab assigned
2. Check spawn points array is populated
3. Ensure NavMesh is baked in scene
4. Verify AIDirector_Heist is calling SpawnManager.RequestSpawns()
5. Check maxActiveEnemies not exceeded

**Force Spawn**:
```csharp
SpawnManager sm = FindObjectOfType<SpawnManager>();
sm.RequestSpawns(5, 1.0f); // Spawn 5 enemies at aggression 1.0
```

### Network Sync Corrections

**Symptoms**: Server frequently correcting client state

**Checks**:
1. Reduce `syncInterval` for more frequent sync (default 0.5s)
2. Check `maxClientDeviation` (default 5m) - reduce if needed
3. Verify `maxHeatChangePerSecond` (default 50) is reasonable
4. Enable client-side prediction if disabled

**Adjust Tolerance**:
```csharp
NetworkSync_Authoritative netSync = GetComponent<NetworkSync_Authoritative>();
netSync.maxClientDeviation = 3f; // Stricter
netSync.maxHeatChangePerSecond = 30f; // Stricter
```

---

## Additional Resources

- **MISSION_SYSTEM_GUIDE.md**: Core mission system documentation
- **REMIX_FORGE_GUIDE.md**: RemixForge and saga system
- **COMPLETE_SYSTEM_GUIDE.md**: Full Soulvan ecosystem guide
- **README.md**: Quick start and overview

**API Documentation**:
- Replay Mint API: Port 3002 endpoints
- DAO Replay Impact API: Port 3004 endpoints
- Remix Forge Graph API: Port 3003 endpoints

**Unity Packages**:
- HDRP required for visual rendering
- NavMesh Components for AI pathfinding
- TextMeshPro for UI text

---

## Version History

**v1.0.0** - Initial Neon Vault Heist system
- Core mission orchestration with MissionController
- AIDirector adaptive difficulty scaling
- 4-player role system with unique abilities
- Deterministic replay seed generation
- DAO integration with heat-to-influence conversion
- Procedural mission composition from ScriptableObjects
- Server-authoritative networking with anti-cheat

---

**© 2024 Soulvan Project - Neon Vault Heist System**
