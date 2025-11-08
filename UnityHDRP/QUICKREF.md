# Soulvan AI Quick Reference

## Component Cheat Sheet

### Unity Components

| Component | Purpose | Key Methods |
|-----------|---------|-------------|
| `AgentBrain` | Main AI controller | `RaceBehaviour()`, `StealthBehaviour()`, `FleeBehaviour()`, `BossBehaviour()`, `RecoverBehaviour()` |
| `VisionSensor` | LOS raycasting | `SeeTarget(Transform)`, `LineOfSight(Vector3)`, `FindVisibleTargets(string tag)` |
| `ThreatEvaluator` | Threat scoring | `Evaluate(rival, policePos, speed, damage)` |
| `DrivingController` | Physics driving | `DriveTowards(waypoint, aggression)`, `Brake(intensity)`, `ApplyNitro(force)` |
| `MissionActions` | GTA missions | `PickupCargo()`, `DropCargo()`, `HackGate(callback)` |
| `CombatActions` | Boss battles | `RamTarget()`, `FireWeapon()`, `Evade(threatDir)` |
| `MotifAPI` | Visual/audio overlays | `SetMotif(motif, intensity)`, `TransitionToNextSeason()` |
| `RewardService` | Blockchain rewards | `GrantSeasonalBadge()`, `MintSVNReward()`, `MintCarSkin()` |
| `PerformanceScaler` | Adaptive quality | `GetQualityScalar()`, `SetQuality()`, `EnableAutoScale()` |
| `EventBus` | Global events | `EmitMotif()`, `EmitMissionCompleted()`, `EmitBossDefeated()` |

### Unreal Components

| Component | Purpose | Key Functions |
|-----------|---------|---------------|
| `USoulvanMotifComponent` | Niagara + Audio | `SetMotif(EMotif, float)`, `GetCurrentMotif()` |
| `UBTService_ThreatUpdate` | BT threat service | `TickNode()` (auto-called by BT) |

---

## Goal Selection Logic

```
Utility Scores (normalized via softmax):
- Race:   0.7 if missionActive && !hasCargo, else 0.4
- Stealth: 0.8 if hasCargo, else 0.2
- Flee:   0.9 if threatLevel > 0.65, else 0.1
- Boss:   0.85 if bossEngaged, else 0.1
- Recover: 0.75 if (damage > 0.6 OR fuel < 0.2), else 0.15

Highest score wins.
```

---

## Threat Calculation

```
Threat = 0.45 * (1 / rivalDist) +
         0.35 * (1 / policeDist) +
         0.15 * (speed / maxSpeed) +
         0.05 * damagePct

Clamped to [0, 1]
```

---

## Motif Intensity Mapping

```
Intensity = 0.4 + threatLevel * 0.6
Scaled by PerformanceScaler.GetQualityScalar()

Visual FX:
- Storm:  100% rate
- Calm:   50% rate
- Cosmic: 80% rate
- Oracle: 60% rate

Audio:
- Pitch: Lerp(0.95, 1.08, intensity)
- Volume: Lerp(0.6, 1.0, intensity)
```

---

## Event Flow Examples

### Mission Completion
```
1. Agent reaches delivery waypoint with cargo
2. MissionActions.DropCargo(bb)
3. EventBus.EmitMissionCompleted("stealth_delivery")
4. RewardService.GrantSeasonalBadge("calm_delivery", wallet)
5. Backend submits blockchain tx
6. MissionRegistry.completeMission() mints SVN
7. Chronicle.log() records immutable entry
```

### Boss Defeat
```
1. CombatActions.RamTarget(boss)
2. Boss HealthComponent.TakeDamage(25)
3. Health reaches 0
4. EventBus.EmitBossDefeated(bossId)
5. RewardService.MintSVNReward(wallet, 200)
6. Chronicle.log() with TYPE_REWARD
```

### Season Change
```
1. DAO votes pass on-chain
2. SeasonManager.setSeason(1) [Calm]
3. Chronicle.log() with TYPE_SEASON
4. DaoGovernanceClient polls every 30s
5. Detects season change
6. MotifAPI.SetMotif(Motif.Calm, 0.7)
7. EventBus.EmitSeasonChanged(1)
```

---

## Inspector Setup (Unity)

### AgentBrain
```
- Race Path: Array of Transform waypoints
- Show Debug Gizmos: true (for development)
```

### MotifAPI
```
- Storm Rain: ParticleSystem ref
- Calm Fog: ParticleSystem ref
- Cosmic Aurora: ParticleSystem ref
- Oracle Runes: ParticleSystem ref
- Music Bus: AudioSource ref
- Storm/Calm/Cosmic/Oracle Music: AudioClip refs
- Post Process Volume: Volume ref (optional)
```

### DrivingController
```
- Max Speed Kmh: 240
- Acceleration Force: 55
- Turn Rate Deg Per Sec: 2.4
- Brake Force: 120
- Drift Curve: AnimationCurve (0→0, 1→1 EaseInOut)
- Drift Multiplier: 100
- Downforce Multiplier: 2
```

### VisionSensor
```
- Max Distance: 80
- Obstacle Mask: LayerMask (obstacles, buildings)
- Target Mask: LayerMask (rivals, cargo, drones)
- FOV Angle: 120
```

---

## Blackboard State

```csharp
bb.targetWaypoint     // Next objective location
bb.rival              // Transform of rival car
bb.cargo              // Transform of cargo object
bb.lastSeenThreatPos  // Last known police/threat position
bb.threatLevel        // 0..1 normalized threat score
bb.fuelPct            // 0..1 fuel remaining
bb.damagePct          // 0..1 damage taken
bb.hasCargo           // Bool: carrying cargo
bb.isInStealthZone    // Bool: in stealth detection area
bb.missionActive      // Bool: mission in progress
bb.bossEngaged        // Bool: boss battle active
bb.motifIntensity     // 0..1 visual/audio intensity
```

---

## Performance Targets

| Hardware Tier | Target FPS | Quality Scalar | Particle Max | Shadow Distance |
|---------------|------------|----------------|--------------|-----------------|
| Low-end       | 30         | 0.3            | 100          | 50m             |
| Mid-tier      | 60         | 0.7            | 500          | 100m            |
| High-end      | 144        | 1.0            | 1000         | 150m            |

---

## Debug Commands (Unity Console)

```csharp
// Force motif
MotifAPI.SetMotif(Motif.Storm, 1.0f);

// Spawn threat
bb.lastSeenThreatPos = Camera.main.transform.position;
bb.threatLevel = 0.9f;

// Complete mission instantly
EventBus.EmitMissionCompleted("test_mission");

// Disable auto-scaling
PerformanceScaler.SetQuality(1.0f);

// Trigger season change
EventBus.EmitSeasonChanged(1); // Calm
```

---

## Common Issues & Fixes

### Agent stuck at waypoint
- Check `racePath` array is populated
- Verify waypoint distance threshold (8f default)
- Ensure Rigidbody isn't kinematic

### No visual motifs
- Check MotifAPI references in Inspector
- Verify particle systems are not paused
- Check Post Process Volume weight

### Low FPS with motifs
- Enable PerformanceScaler auto-scaling
- Reduce particle max counts
- Lower shadow distance in Quality Settings

### Stealth detection not working
- Tag drones as "Drone"
- Check VisionSensor FOV angle and masks
- Verify LineOfSight() raycasts

### Blockchain rewards not minting
- Check RewardService contract addresses
- Verify RPC URL connectivity
- Enable debug logs in RewardService

---

## Next Steps

1. **Test AI behaviors** in isolated scenes
2. **Profile performance** with 10+ agents
3. **Integrate blockchain** backend service
4. **Create race tracks** with waypoint paths
5. **Design mission scripts** with objectives
6. **Tune driving physics** curves
7. **Add cinematic cutscenes** via Timeline
8. **Deploy to testnet** for live blockchain testing

---

## Resources

- Full implementation: `/UnityHDRP/Scripts/`
- Smart contracts: `/contracts/`
- Architecture doc: `/ARCHITECTURE.md`
- Unity HDRP manual: https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@latest
- Unreal Niagara: https://docs.unrealengine.com/5.3/niagara-overview
