# Soulvan NeonVault Unity Package

## Overview

The `com.soulvan.neonvault` package contains all the core systems, prefabs, and ScriptableObjects for the Neon Vault Heist mission framework.

## Package Structure

```
Packages/com.soulvan.neonvault/
├── package.json          # Package manifest
├── README.md            # This file
├── Editor/              # Editor scripts and tools
├── Runtime/             # Runtime scripts
│   ├── Scripts/         # Core gameplay scripts
│   ├── Prefabs/         # Reusable prefabs
│   └── ScriptableObjects/  # Data assets
└── Tests/               # Package tests
    ├── Editor/          # EditMode tests
    └── Runtime/         # PlayMode tests
```

## Installation

### Via Git URL

Add this to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.soulvan.neonvault": "https://github.com/44547/Soulvan-genesis-block-creation-and-nonce.git?path=/Packages/com.soulvan.neonvault"
  }
}
```

### Via Package Manager

1. Open Unity Package Manager (Window → Package Manager)
2. Click the + button → Add package from git URL
3. Paste the git URL above

### Via Local Package

1. Download the package from releases
2. Extract to your project's `Packages/` folder
3. Unity will automatically detect and import it

## Requirements

- Unity 2020.3 or higher
- Unity HDRP (High Definition Render Pipeline)
- TextMeshPro
- NavMesh Components (optional, for AI navigation)

## Features

### Core Systems

- **Mission Controller**: State machine for heist missions
- **AI Director**: Adaptive difficulty scaling
- **Role Manager**: 4-player role system with unique abilities
- **Replay System**: Deterministic replay logging
- **DAO Integration**: Governance overlay and impact tracking

### Prefabs

- Mission Controllers
- AI Systems
- Vehicle Controllers
- Hacking Mini-game
- Camera Systems

### ScriptableObjects

- Mission Modules
- Datacore Tiers
- Heat Modifiers
- Vehicle Blueprints

## Quick Start

1. Import the package into your Unity project
2. Open the example scene: `Assets/Soulvan/Scenes/NeonVaultHeist.unity`
3. Configure API endpoints in `ServerRpcClient` GameObject
4. Press Play to test the mission system

## Documentation

- [Complete System Guide](../../COMPLETE_SYSTEM_GUIDE.md)
- [Neon Vault Heist Guide](../../NEON_VAULT_HEIST_GUIDE.md)
- [Mission System Guide](../../MISSION_SYSTEM_GUIDE.md)
- [Unity CI/CD Workflows Guide](../../UNITY_CI_WORKFLOWS_GUIDE.md)

## API Configuration

Set these EditorPrefs or environment variables:

```csharp
EditorPrefs.SetString("Soulvan:ApiBaseUrl", "https://api.dev.soulvan");
EditorPrefs.SetString("Soulvan:ReplayEndpoint", "https://api.dev.soulvan/replay/log");
EditorPrefs.SetString("Soulvan:DaoEndpoint", "https://api.dev.soulvan/dao/impact");
```

Or use environment variables:
```bash
export SOULVAN_API_BASE_URL=https://api.dev.soulvan
export SOULVAN_REPLAY_ENDPOINT=https://api.dev.soulvan/replay/log
export SOULVAN_DAO_ENDPOINT=https://api.dev.soulvan/dao/impact
```

## Testing

Run tests via Unity Test Runner:

```bash
# Via Unity Editor
Window → General → Test Runner

# Via command line
Unity -batchmode -quit -projectPath . -runTests -testPlatform EditMode

# Via CI/CD
# Tests run automatically via GitHub Actions workflows
```

## Support

- [GitHub Issues](https://github.com/44547/Soulvan-genesis-block-creation-and-nonce/issues)
- [Documentation](../../README.md)

## License

MIT License - See [LICENSE](../../LICENSE) for details

## Version History

### 1.0.0 (Current)
- Initial release
- Complete heist mission system
- DAO integration
- Replay system
- Multi-platform support
