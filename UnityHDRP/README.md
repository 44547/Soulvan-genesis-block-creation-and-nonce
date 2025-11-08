# Soulvan Unity HDRP + Unreal Engine 5 AI Implementation

## Overview

Complete AI system for Soulvan racing + mission game implementing:
- **Perception**: Raycasting vision, threat evaluation, oracle markers
- **Decision**: Hybrid state machine + utility AI for race/stealth/boss behaviors
- **Action**: Physics-based driving, GTA-style missions, combat
- **Systems**: Motif overlays, blockchain rewards, DAO governance, performance scaling

---

## Directory Structure

```
UnityHDRP/Scripts/
├── AI/
│   ├── Perception/
│   │   ├── VisionSensor.cs          # Raycasting LOS checks
│   │   └── ThreatEvaluator.cs       # Proximity + speed + damage scoring
│   ├── Decision/
│   │   ├── AgentBlackboard.cs       # Memory/state store
│   │   └── UtilityScorer.cs         # Goal selection (Race/Stealth/Flee/Boss/Recover)
│   ├── Actions/
│   │   ├── DrivingController.cs     # Physics driving (drift, nitro, brake)
│   │   ├── MissionActions.cs        # Cargo pickup/drop, hacking
│   │   └── CombatActions.cs         # Ramming, weapons, evasion
│   └── AgentBrain.cs                # Main controller integrating all systems
├── Systems/
│   ├── MotifAPI.cs                  # Visual/audio/haptic overlays
│   ├── RewardService.cs             # Blockchain NFT/SVN minting
│   ├── DaoGovernanceClient.cs       # On-chain season/proposal queries
│   └── PerformanceScaler.cs         # Adaptive quality (FPS-based)
└── Infra/
    └── EventBus.cs                  # Decoupled event system

UnrealEngine5/Source/Soulvan/AI/
├── SoulvanMotifComponent.h/.cpp     # Niagara + Audio motif system
└── SoulvanThreatService.h/.cpp      # BT Service for threat evaluation
```

---

## Unity HDRP Setup

### 1. Prerequisites
- Unity 2022.3 LTS or later
- HDRP package installed
- Input System package (new)
- Cinemachine (optional for camera)

### 2. Scene Setup

```csharp
// Create agent GameObject
GameObject agent = new GameObject("SoulvanAgent");
agent.AddComponent<Rigidbody>();
agent.AddComponent<VisionSensor>();
agent.AddComponent<ThreatEvaluator>();
agent.AddComponent<DrivingController>();
agent.AddComponent<MissionActions>();
agent.AddComponent<CombatActions>();
agent.AddComponent<AgentBrain>();

// Create global services
GameObject services = new GameObject("SoulvanServices");
services.AddComponent<MotifAPI>();
services.AddComponent<RewardService>();
services.AddComponent<DaoGovernanceClient>();
services.AddComponent<PerformanceScaler>();
```

### 3. Waypoint Setup

Create empty GameObjects as waypoints and assign to `AgentBrain.racePath[]` in Inspector:

```
Hierarchy:
- RacePath
  - Waypoint_01
  - Waypoint_02
  - ...
  - Waypoint_N
```

### 4. Particle Systems

Assign particle systems to `MotifAPI`:
- **StormRain**: Rain/lightning particles
- **CalmFog**: Fog + soft lighting
- **CosmicAurora**: Aurora ribbons + galaxy particles
- **OracleRunes**: Glowing rune symbols

### 5. Audio

Assign audio clips for each motif to `MotifAPI.musicBus`:
- Storm: Heavy bass, electronic
- Calm: Soft strings, ambient
- Cosmic: Synth swells, orchestral
- Oracle: Deep drums, chants

---

## Unreal Engine 5 Setup

### 1. Prerequisites
- Unreal Engine 5.3+
- Chaos Vehicles plugin enabled
- Niagara plugin enabled
- Enhanced Input plugin

### 2. Blueprint Setup

Create `BP_SoulvanAgent` based on `AWheeledVehiclePawn`:

1. Add `SoulvanMotifComponent`
2. Assign Niagara systems (Storm/Calm/Cosmic/Oracle)
3. Add Audio Component for music bus
4. Create AI Controller: `BP_SoulvanAIController`

### 3. Behavior Tree

Create `BT_SoulvanAgent`:

```
Root
├── Selector
│   ├── Flee (ThreatLevel > 0.65)
│   │   └── Task: MoveTo (opposite of ThreatPos)
│   ├── BossDuel (BossEngaged == true)
│   │   └── Task: AggressiveFollow + Ram
│   ├── StealthDeliver (HasCargo == true)
│   │   └── Task: SlowMoveTo + LOS checks
│   ├── Recover (Damage > 0.6 OR Fuel < 0.2)
│   │   └── Task: MoveTo Pitstop
│   └── Race (Default)
│       └── Task: DriveToWaypoint
└── Services
    ├── S_ThreatUpdate (updates ThreatLevel every 0.5s)
    └── S_MotifUpdate (pushes motif to Niagara)
```

### 4. Blackboard Keys

```
- Rival (Object)
- LastThreatPos (Vector)
- ThreatLevel (Float)
- SpeedKmh (Float)
- DamagePct (Float)
- MotifIntensity (Float)
- HasCargo (Bool)
- BossEngaged (Bool)
- MissionActive (Bool)
```

---

## Agent Behaviors

### Racing
- **Goal**: Complete laps on race path
- **Motif**: Storm (intensity scales with threat)
- **Aggression**: 0.6 base, 1.0 under threat
- **Rewards**: 50 SVN per lap completion

### Stealth Delivery
- **Goal**: Transport cargo without detection
- **Motif**: Calm (intensity 0.7)
- **Aggression**: 0.35 (slow, careful)
- **Detection**: LOS checks against "Drone" tagged objects
- **Rewards**: Seasonal badge "calm_delivery"

### Flee
- **Goal**: Escape from threat
- **Motif**: Storm (max intensity)
- **Aggression**: 1.0 (full speed)
- **Logic**: Drive away from `lastSeenThreatPos`

### Boss Duel
- **Goal**: Defeat rival boss
- **Motif**: Cosmic (intensity scales with threat)
- **Aggression**: 0.9
- **Combat**: Ram attacks, aggressive pursuit
- **Rewards**: 200 SVN + boss-specific NFT

### Recover
- **Goal**: Repair damage and refuel
- **Motif**: Oracle (0.5 intensity)
- **Logic**: Brake, navigate to pitstop, simulate repair
- **Trigger**: Damage > 0.6 OR Fuel < 0.2

---

## Motif System

### Visual Overlays
- **Storm**: Lightning flashes, rain streaks, neon glow
- **Calm**: Fog layers, soft bloom, warm lens flares
- **Cosmic**: Aurora ribbons, galaxy particles, lens flares
- **Oracle**: Rune particles, golden glows, sacred geometry

### Audio
- **Pitch**: Scales 0.95 → 1.08 with intensity
- **Volume**: Scales 0.6 → 1.0 with intensity
- **Crossfade**: Smooth transition between motif tracks

### Haptic (Future)
- Storm: Sharp pulses
- Calm: Gentle waves
- Cosmic: Rhythmic surges
- Oracle: Deep resonance

---

## Performance Scaling

`PerformanceScaler` monitors FPS and adjusts:
- Particle emission rates
- Shadow quality/distance
- Motif intensity multiplier
- Max particles per system

**Targets**:
- Min FPS: 30
- Target FPS: 60
- Max FPS: 144

**Quality Bounds**:
- Min quality: 0.3 (low-end hardware)
- Max quality: 1.0 (high-end hardware)

---

## Blockchain Integration

### Reward Flow
1. Agent completes mission/race
2. `RewardService.MintSVNReward()` called
3. Event emitted via `EventBus.EmitSVNMinted()`
4. Backend service listens and submits blockchain tx
5. `MissionRegistry.completeMission()` mints SVN to player wallet

### DAO Governance
1. `DaoGovernanceClient` polls `SeasonManager.activeSeason()` every 30s
2. Season changes trigger `MotifAPI.SetMotif()` update
3. Players vote via `DaoGovernanceClient.CastVote()`
4. Proposals logged to `SoulvanChronicle`

---

## Testing Checklist

### Unity Tests
```bash
# 1. Spawn 10 agents with mixed goals
for i in 0..9:
    agent = Instantiate(agentPrefab)
    agent.bb.missionActive = Random.value > 0.5
    agent.bb.bossEngaged = Random.value > 0.7
```

- [ ] Validate LOS in stealth zones (drones block paths)
- [ ] Stress test storm motif at max intensity (check FPS)
- [ ] Boss duel: tune overtake distance, drift curve
- [ ] Cutscenes: animate motif transitions via Timeline
- [ ] Rewards: confirm badge grant fires once per mission

### Unreal Tests
```cpp
// Spawn test agents
for (int32 i = 0; i < 10; ++i)
{
    AAIController* AI = World->SpawnActor<AAIController>();
    AI->RunBehaviorTree(BT_SoulvanAgent);
}
```

- [ ] BT Service: S_ThreatUpdate correctly calculates threat
- [ ] Task: T_StealthDeliver checks EQS for drones
- [ ] Niagara: Storm/Calm/Cosmic systems scale with intensity
- [ ] Audio: Crossfade works between motif tracks
- [ ] Boss battle: Ram attacks apply damage correctly

### Performance Profiling
- [ ] Deep profiling with 10+ agents (Unity: Profiler, Unreal: Insights)
- [ ] GPU profiling (NVIDIA NSight, RenderDoc)
- [ ] Adaptive scaling maintains 60 FPS on mid-tier hardware
- [ ] Motif particle counts scale down under load

---

## Integration with Blockchain

### Unity → Blockchain
```csharp
// After mission completion
RewardService.CompleteMissionOnChain(missionId, playerWallet, resultHash);

// Pseudo web3 call:
// await missionRegistry.completeMission(missionId, playerWallet, resultHash);
```

### Blockchain → Unity
```csharp
// Poll seasonal arc
async void PollSeason()
{
    int season = await seasonManager.activeSeason();
    MotifAPI.SetMotif((Motif)season, 0.7f);
}
```

### Event Flow
```
Unity AgentBrain
    ↓
EventBus.EmitMissionCompleted()
    ↓
RewardService.MintSVNReward()
    ↓
Backend Service (Node.js + ethers.js)
    ↓
MissionRegistry.completeMission() tx
    ↓
SoulvanChronicle.log() immutable record
```

---

## Tuning Parameters

### Driving
- `maxSpeedKmh`: 240 (high-speed racing)
- `accelerationForce`: 55 (responsive)
- `turnRateDegPerSec`: 2.4 (drift-friendly)
- `driftMultiplier`: 100 (cinematic slides)

### Threat Evaluation
- `rivalWeight`: 0.45 (proximity to rival)
- `policeWeight`: 0.35 (chase intensity)
- `speedWeight`: 0.15 (risk from speed)
- `damageWeight`: 0.05 (vehicle condition)

### Utility Scoring
- Race: 0.7 (mission active, no cargo)
- Stealth: 0.8 (has cargo)
- Flee: 0.9 (threat > 0.65)
- Boss: 0.85 (boss engaged)
- Recover: 0.75 (damage > 0.6 OR fuel < 0.2)

---

## Advanced Features

### VFX Graph Integration (Unity)
```csharp
if (stormVFX)
{
    stormVFX.SetFloat("Intensity", intensity);
    stormVFX.SetFloat("EmissionRate", Mathf.Lerp(50f, 500f, intensity));
}
```

### Niagara Integration (Unreal)
```cpp
StormRain->SetFloatParameter(FName("EmissionRate"), FMath::Lerp(10.f, 200.f, Intensity));
```

### Post-Processing
- Storm: High contrast, desaturation
- Calm: Soft bloom, warm temperature
- Cosmic: Lens flares, chromatic aberration
- Oracle: Golden tint, vignette

---

## Deployment

### Unity Build
```bash
# Build for Windows (High-End)
Unity -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWindows64

# Build for Consoles (Optimized)
Unity -batchmode -quit -projectPath . -executeMethod BuildScript.BuildPS5
```

### Unreal Packaging
```bash
# Windows (Shipping configuration)
RunUAT BuildCookRun -project=Soulvan.uproject -platform=Win64 -configuration=Shipping

# PS5 (Development)
RunUAT BuildCookRun -project=Soulvan.uproject -platform=PS5 -configuration=Development
```

---

## Known Issues & Roadmap

### Known Issues
- [ ] Stealth LOS checks expensive with many drones (optimize with spatial hashing)
- [ ] Boss ram attacks can launch player too far (tune impulse force)
- [ ] Motif crossfade has brief audio pop (add fade curves)

### Roadmap
- [ ] Multiplayer racing (synchronize waypoints + motifs)
- [ ] VR/AR chronicle replay viewer
- [ ] Cinematic cutscene NFT minting
- [ ] Real-time subgraph integration for Chronicle events
- [ ] Advanced haptic patterns (DualSense, Xbox Elite)

---

## Support & Resources

- **Unity HDRP Docs**: https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@latest
- **Unreal Niagara**: https://docs.unrealengine.com/5.3/niagara-overview
- **Blockchain Integration**: See `/contracts` directory for deployed smart contracts
- **ARCHITECTURE.md**: Full Soulvan vision document

---

## License
MIT
