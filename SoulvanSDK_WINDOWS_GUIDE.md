# SoulvanSDK Windows Integration Guide

**Version:** 1.0.0  
**Target Platforms:** Unity 2021.3+, Unreal Engine 5.2+, Windows 10/11  
**Date:** November 2025

---

## Overview

SoulvanSDK provides cinematic wallet, mission, and lore systems for the Soulvan gaming universe. Supports Unity, Unreal Engine, and CLI tools on Windows with full RTX 5090 + DLSS 4.0 integration.

---

## Features

- **Wallet-bound identity avatars** with deterministic generation
- **5-tier progression system** (Street Racer → Mythic Legend)
- **4 mission types** (Driving, Stealth, Boss, DAO Rituals)
- **DAO voting** with Oracle motif overlays
- **Lore Chronicle** for on-chain saga logging
- **Replay NFTs** and badge minting
- **Bridge Service** for multi-chain support
- **CLI tools** for wallet ops, lore exports, badge minting

---

## Installation

### 📦 1. Windows Installer

**File:** `SoulvanSDK_Setup_v1.0.0.exe`  
**Download:** https://soulvan.io/downloads/sdk

**Installs to:**
- Unity SDK → `C:\Program Files\SoulvanSDK\Unity\`
- Unreal Plugin → `C:\Program Files\SoulvanSDK\Unreal\`
- CLI Tools → `C:\Program Files\SoulvanSDK\CLI\`
- Environment Variable: `SOULVAN_SDK_PATH` set to `C:\Program Files\SoulvanSDK\`

**Installation Steps:**
1. Run `SoulvanSDK_Setup_v1.0.0.exe` as Administrator
2. Accept license agreement
3. Choose installation directory (default recommended)
4. Select components: Unity, Unreal, CLI (all by default)
5. Click "Install"
6. Restart Visual Studio/Unity/Unreal after installation

---

## Unity Integration

### ✅ Requirements
- Unity 2021.3 LTS or newer
- TextMeshPro (3.0.6+)
- Input System (1.5.1+)
- Windows 10/11 with .NET 6+

### 📁 Unity SDK Structure

```
C:\Program Files\SoulvanSDK\Unity\
└── Assets\SoulvanSDK\
    ├── Wallet\
    │   ├── ISoulvanWallet.cs
    │   ├── WalletController.cs
    │   ├── AvatarRenderer.cs
    │   └── WalletUI.prefab
    ├── Missions\
    │   ├── MissionManager.cs
    │   ├── DrivingMission.cs
    │   ├── StealthMission.cs
    │   ├── BossBattle.cs
    │   └── DaoRitual.cs
    ├── Lore\
    │   ├── LoreChronicle.cs
    │   ├── BadgeMintService.cs
    │   └── LoreExporter.cs
    ├── Bridge\
    │   ├── EventBus.cs
    │   ├── BridgeService.cs
    │   ├── Reconciler.cs
    │   └── LoreExporter.cs
    ├── Systems\
    │   ├── ProgressionSystem.cs
    │   ├── ProgressionTracker.cs
    │   ├── MotifAPI.cs
    │   ├── UpdateManager.cs
    │   ├── RTXAutoScaler.cs
    │   └── DLSSController.cs
    ├── UI\
    │   ├── MissionHUD.cs
    │   ├── RitualPanel.prefab
    │   └── DashboardCanvas.prefab
    ├── Resources\
    │   └── AvatarMaterials\
    │       ├── Storm.mat
    │       ├── Calm.mat
    │       ├── Cosmic.mat
    │       ├── Oracle.mat
    │       └── Mythic.mat
    └── Samples\
        ├── SampleMissionScene.unity
        ├── SampleWalletScene.unity
        └── SampleDashboard.unity
```

### 🔧 Unity Setup Steps

1. **Import SoulvanSDK Package**
   ```
   Unity → Assets → Import Package → Custom Package
   Select: C:\Program Files\SoulvanSDK\Unity\SoulvanSDK.unitypackage
   ```

2. **Configure Project Settings**
   ```
   Edit → Project Settings → Player → Other Settings
   - Scripting Backend: IL2CPP
   - Api Compatibility Level: .NET Standard 2.1
   - Target Architectures: x86_64
   ```

3. **Install Dependencies**
   ```
   Window → Package Manager
   - TextMeshPro (3.0.6+)
   - Input System (1.5.1+)
   - Visual Effect Graph (12.0.0+)
   - HDRP (12.0.0+ for Unity 2022.3)
   ```

4. **Add SoulvanSDK to Scene**
   - Drag `Prefabs/SoulvanSDKManager.prefab` into your scene
   - Assign WalletController, MissionManager, ProgressionSystem references
   - Configure RPC URL and Chain ID in Inspector

5. **Configure Input System**
   - Create Input Actions asset or use `Samples/SoulvanInputActions.inputactions`
   - Assign to PlayerInput component on player GameObject

### 🧪 Unity Sample Scenes

**SampleMissionScene.unity** - Complete mission system demo
- Driving mission with waypoints
- Stealth mission with hacking terminals
- Boss battle with health phases
- DAO ritual with voting UI

**SampleWalletScene.unity** - Wallet operations demo
- Wallet unlock/lock flows
- Avatar rendering with tier updates
- Badge minting visualization
- Lore Chronicle logging

**SampleDashboard.unity** - Contributor dashboard demo
- Active mission tracking
- Lore log visualization
- Wallet stats display
- Dev tools panel

---

## Unreal Engine Integration

### ✅ Requirements
- Unreal Engine 5.2+
- Visual Studio 2022
- Windows SDK 10.0.22621.0+
- .NET 6+ SDK

### 📁 Unreal Plugin Structure

```
C:\Program Files\SoulvanSDK\Unreal\
└── Plugins\SoulvanSDK\
    ├── Source\
    │   ├── SoulvanWallet\
    │   │   ├── USoulvanWalletSubsystem.h/.cpp
    │   │   └── WalletWidget.uasset
    │   ├── SoulvanMissions\
    │   │   ├── DrivingMissionComponent.h/.cpp
    │   │   ├── StealthMissionComponent.h/.cpp
    │   │   ├── BossBattleComponent.h/.cpp
    │   │   └── DaoRitualComponent.h/.cpp
    │   ├── SoulvanLore\
    │   │   ├── LoreChronicle.h/.cpp
    │   │   ├── BadgeMintService.h/.cpp
    │   │   └── LoreExporter.h/.cpp
    │   ├── SoulvanBridge\
    │   │   ├── EventBus.h/.cpp
    │   │   ├── BridgeService.h/.cpp
    │   │   └── Reconciler.h/.cpp
    ├── Resources\
    │   └── AvatarMaterials\
    │       ├── M_Storm.uasset
    │       ├── M_Calm.uasset
    │       ├── M_Cosmic.uasset
    │       ├── M_Oracle.uasset
    │       └── M_Mythic.uasset
    ├── Content\
    │   ├── Samples\
    │   │   ├── L_MissionDemo.umap
    │   │   ├── L_WalletDemo.umap
    │   │   └── L_DashboardDemo.umap
    └── SoulvanSDK.uplugin
```

### 🔧 Unreal Setup Steps

1. **Copy Plugin to Project**
   ```
   Copy C:\Program Files\SoulvanSDK\Unreal\Plugins\SoulvanSDK\
   To <YourProject>\Plugins\SoulvanSDK\
   ```

2. **Enable Plugin**
   ```
   Unreal Editor → Edit → Plugins → Search "Soulvan"
   Check "Enabled" for SoulvanSDK
   Restart Editor
   ```

3. **Regenerate Visual Studio Files**
   ```
   Right-click <YourProject>.uproject
   → Generate Visual Studio project files
   ```

4. **Build Plugin**
   ```
   Open <YourProject>.sln in Visual Studio 2022
   Build → Build Solution (Ctrl+Shift+B)
   ```

5. **Add Wallet Subsystem to Level Blueprint**
   ```blueprint
   BeginPlay → InitializeSoulvanWallet
   - RPC URL: https://sepolia.infura.io/v3/YOUR_KEY
   - Chain ID: 11155111
   ```

### 🧪 Unreal Sample Maps

**L_MissionDemo.umap** - Mission system showcase
- Driving mission with Chaos Vehicles physics
- Stealth mission with detection system
- Boss battle with Niagara VFX
- DAO ritual with UMG voting UI

**L_WalletDemo.umap** - Wallet operations
- Wallet unlock widget
- Avatar material updates for tiers
- Badge minting notifications
- Lore Chronicle integration

**L_DashboardDemo.umap** - Contributor dashboard
- Active mission HUD
- Lore log scroll view
- Wallet stats widget
- Dev tools menu

---

## CLI Tools

### 📁 Location
```
C:\Program Files\SoulvanSDK\CLI\SoulvanCLI.exe
```

### 🔧 Commands

**Wallet Operations:**
```bash
# Create new wallet
SoulvanCLI.exe wallet create --name "MyWallet"

# Export wallet (backup seed phrase)
SoulvanCLI.exe wallet export --address 0xABC123

# Get wallet balance
SoulvanCLI.exe wallet balance --address 0xABC123
```

**Badge Minting:**
```bash
# Mint tier badge
SoulvanCLI.exe badge mint --type "tier_3_badge" --address 0xABC123

# Mint boss trophy
SoulvanCLI.exe badge mint --type "boss_flame_samurai" --address 0xABC123
```

**Lore Operations:**
```bash
# Export saga chapter
SoulvanCLI.exe lore export --chapter "Arc Ascension" --address 0xABC123

# View player lore log
SoulvanCLI.exe lore view --address 0xABC123
```

**DAO Operations:**
```bash
# Cast vote
SoulvanCLI.exe vote cast --proposal "DAO-Season-5" --choice 2 --address 0xABC123

# Create proposal (requires PROPOSER_ROLE)
SoulvanCLI.exe vote propose --title "Increase Mission Rewards" --description "..." --address 0xABC123
```

**Replay Operations:**
```bash
# Mint replay NFT
SoulvanCLI.exe replay mint --mission "storm_pursuit" --address 0xABC123

# Export all replays for player
SoulvanCLI.exe replay export-all --address 0xABC123
```

### 📂 Output Paths
```
Wallets → C:\Users\<User>\SoulvanWallets\
Logs → C:\Users\<User>\SoulvanLogs\
Replays → C:\Users\<User>\SoulvanReplays\
Lore → C:\Users\<User>\SoulvanLore\
```

---

## Security & Storage

### 🔐 Wallet Security
- **Encryption:** Windows DPAPI (Data Protection API)
- **Storage:** `C:\Users\<User>\SoulvanWallets\<address>.json`
- **Biometrics:** Optional unlock via Windows Hello
- **Passphrase:** Required for wallet creation/unlock

### 🔒 Secure Storage Best Practices
1. Enable Windows Hello for quick unlock
2. Backup seed phrases to secure location (not cloud)
3. Use strong passphrases (16+ characters)
4. Enable Windows Defender for malware protection
5. Verify contract addresses before transactions

### 🛡️ Transaction Verification
- **Reconciler:** Verifies all mints, votes, lore logs
- **Max Retries:** 3 attempts with 5s delay
- **Gas Estimation:** Automatic with 20% buffer
- **Nonce Management:** Automatic via Web3 provider

---

## Developer Experience

### 📊 Logging

**Unity Console Logs:**
```csharp
[WalletController] Wallet connected: 0xABC123
[MissionManager] Mission started: mission_001
[ProgressionTracker] Advancing to Tier 3
[BadgeMintService] Badge minted: boss_flame_samurai_trophy
[LoreChronicle] Mission logged: storm_pursuit
```

**Windows Event Viewer:**
```
Application → SoulvanSDK
- Wallet operations (create, unlock, lock)
- Badge minting (success/failure)
- Transaction verification (tx hash, status)
- Lore exports (replay NFTs, saga chapters)
```

### 🧪 Testing

**Unity Play Mode:**
1. Enter Play Mode
2. Use SoulvanSDKManager Inspector to trigger events
3. Monitor Console for logs
4. Check WalletHUD for balance updates

**Unreal PIE (Play In Editor):**
1. Press Play (Alt+P)
2. Use Blueprint nodes to trigger wallet ops
3. Monitor Output Log for events
4. Check UMG widgets for visual feedback

**CLI Testing:**
```bash
# Test wallet creation
SoulvanCLI.exe wallet create --name "TestWallet"

# Test badge minting (testnet)
SoulvanCLI.exe badge mint --type "test_badge" --address <address> --network sepolia
```

### 🔧 Debugging

**Unity Debugging:**
- Attach Visual Studio debugger to Unity process
- Set breakpoints in SoulvanSDK scripts
- Use `Debug.Log()` for event tracking

**Unreal Debugging:**
- Launch from Visual Studio (F5)
- Set breakpoints in C++ source
- Use `UE_LOG(LogSoulvan, Log, TEXT("..."))` for logging

**CLI Debugging:**
- Run with `--verbose` flag for detailed logs
- Check `C:\Users\<User>\SoulvanLogs\` for error traces

---

## CI/CD Integration

### GitHub Actions
```yaml
# .github/workflows/soulvan-ci.yml
name: SoulvanSDK CI

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '6.0.x'
      
      - name: Build CLI
        run: dotnet build CLI/SoulvanCLI.csproj
      
      - name: Run Tests
        run: dotnet test CLI/SoulvanCLI.csproj
      
      - name: Build Unity Package
        run: |
          "C:\Program Files\Unity\Hub\Editor\2022.3.0f1\Editor\Unity.exe" -quit -batchmode -projectPath Unity/ -executeMethod PackageExporter.Export
```

### Azure DevOps
```yaml
# azure-pipelines.yml
trigger:
  - main

pool:
  vmImage: 'windows-latest'

steps:
  - task: DotNetCoreCLI@2
    inputs:
      command: 'build'
      projects: 'CLI/SoulvanCLI.csproj'
  
  - task: DotNetCoreCLI@2
    inputs:
      command: 'test'
      projects: 'CLI/SoulvanCLI.csproj'
```

---

## API Reference

See [SoulvanSDK_API_Reference.md](API_Reference.md) for complete API documentation.

---

## Sample Projects

Included sample projects demonstrate:
- Wallet unlock/lock flows
- Mission completion with badge minting
- Tier progression with avatar updates
- DAO voting with Oracle motif overlays
- Replay NFT minting
- Lore Chronicle logging
- Contributor dashboard UI

---

## Support

**Documentation:** https://docs.soulvan.io/sdk  
**Community:** https://discord.gg/soulvan  
**Issues:** https://github.com/soulvan/sdk/issues  
**Email:** support@soulvan.io

---

**SoulvanSDK v1.0.0 | Windows Integration Complete** 🏎️⚡🌌
