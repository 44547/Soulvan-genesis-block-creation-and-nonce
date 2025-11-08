# Unity Tooling and Development Guide

## Overview

This guide covers Unity-specific development tools, editor extensions, and automation scripts for the Soulvan project.

## Unity Editor Version

**Recommended**: Unity 2021.3.31f1 (LTS)

Download from: [Unity Download Archive](https://unity.com/releases/editor/archive)

## Editor Setup

### 1. Project Import

```bash
# Clone the repository
git clone https://github.com/44547/Soulvan-genesis-block-creation-and-nonce.git
cd Soulvan-genesis-block-creation-and-nonce

# Open Unity Hub and add project
# Select Unity 2021.3.31f1 or compatible version
```

### 2. Required Packages

The project uses Unity Package Manager (UPM). Required packages are listed in `Packages/manifest.json`:

- **Unity HDRP**: High Definition Render Pipeline
- **TextMeshPro**: Advanced text rendering
- **Visual Scripting**: (Optional) For visual programming
- **Test Framework**: For unit and integration testing

### 3. HDRP Configuration

1. Open Edit → Project Settings → Graphics
2. Verify HDRP Asset is assigned
3. Configure quality levels in Project Settings → Quality

## Unity Editor Tools

### ScriptableObject Importers

#### JSON to ScriptableObject Importer

Import JSON seed data into Unity ScriptableObjects:

**Location**: `Window → Soulvan → Import Seed JSON`

**Features**:
- Batch import from JSON files
- Strong type validation
- CSV logging for audit trail
- Automatic asset creation

**Usage**:
```bash
# Via Unity Editor
Window → Soulvan → Import Seed JSON

# Via Command Line (CI/CD)
Unity -batchmode -quit -projectPath . \
  -executeMethod Soulvan.CI.CIImportWrappers.RunStrongJsonImport \
  -logFile import.log
```

**Supported Types**:
- `SO_MissionModule`: Mission configuration data
- `SO_DatacoreTier`: Reward tier definitions
- `SO_HeatModifiers`: Heat system configuration
- `SO_VehicleBlueprint`: Vehicle specifications

#### CSV Import Logging

Generate import audit logs:

**Location**: `Window → Soulvan → Import Typed JSON with CSV Log`

**Output**: `Assets/Soulvan/Docs/import-log.csv`

**Fields**:
- Import timestamp
- File name
- Asset type
- Success/failure status
- Error messages

### Asset Validation Tool

Validate ScriptableObject integrity:

**Location**: `Window → Soulvan → Validate Soulvan Assets`

**Checks**:
- Required fields populated
- Valid reference links
- Data consistency
- Missing dependencies

**Usage**:
```csharp
// Via editor menu
Window → Soulvan → Validate Soulvan Assets

// Via command line
Unity -batchmode -quit -projectPath . \
  -executeMethod Soulvan.Editor.ValidateSoulvanAssets.Run \
  -logFile validation.log
```

### Scene Auto-Wire Tool

Automatically wire GameObject references:

**Location**: `Window → Soulvan → Auto-Wire Scene`

**Features**:
- Finds and links components by convention
- Validates scene setup
- Reports missing references
- Backs up scene before modifications

**Conventions**:
- GameObject names match field names (PascalCase → camelCase)
- Required tags: `MissionController`, `ServerRpcClient`, etc.
- Required layers: `Player`, `Enemy`, `Vehicle`

## Command Line Tools

### Batch Mode Operations

Unity supports headless command-line execution for automation:

#### Build Project

```bash
Unity -batchmode -quit -projectPath . \
  -buildTarget StandaloneWindows64 \
  -buildPath ./builds/windows \
  -logFile build.log
```

#### Run Tests

```bash
# EditMode tests
Unity -batchmode -quit -projectPath . \
  -runTests -testPlatform EditMode \
  -testResults TestResults/editmode.xml \
  -logFile test-edit.log

# PlayMode tests
Unity -batchmode -quit -projectPath . \
  -runTests -testPlatform PlayMode \
  -testResults TestResults/playmode.xml \
  -logFile test-play.log
```

#### Execute Custom Methods

```bash
Unity -batchmode -quit -projectPath . \
  -executeMethod Namespace.ClassName.MethodName \
  -logFile custom.log
```

### Build Scripts

#### Windows Build

```bash
./scripts/build-windows.sh
```

#### Linux Build

```bash
./scripts/build-linux.sh
```

#### macOS Build

```bash
./scripts/build-macos.sh
```

## Testing Framework

### Test Structure

```
Tests/
├── Editor/              # EditMode tests (editor-only)
│   ├── MissionTests.cs
│   ├── AITests.cs
│   └── ImporterTests.cs
└── Runtime/             # PlayMode tests (runtime)
    ├── GameplayTests.cs
    ├── NetworkTests.cs
    └── IntegrationTests.cs
```

### Writing Tests

#### EditMode Test Example

```csharp
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Soulvan.Tests.Editor
{
    public class MissionModuleTests
    {
        [Test]
        public void MissionModule_ValidateData_Success()
        {
            // Arrange
            var module = ScriptableObject.CreateInstance<SO_MissionModule>();
            module.moduleId = "test-module";
            module.displayName = "Test Module";
            
            // Act
            bool isValid = module.Validate();
            
            // Assert
            Assert.IsTrue(isValid);
        }
    }
}
```

#### PlayMode Test Example

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Soulvan.Tests.Runtime
{
    public class MissionControllerTests
    {
        [UnityTest]
        public IEnumerator MissionController_StartMission_StateChanges()
        {
            // Arrange
            var go = new GameObject();
            var controller = go.AddComponent<MissionController>();
            
            // Act
            controller.StartMission();
            yield return new WaitForSeconds(0.5f);
            
            // Assert
            Assert.AreEqual(MissionState.Active, controller.State);
            
            // Cleanup
            Object.Destroy(go);
        }
    }
}
```

### Running Tests

#### Via Unity Editor

1. Open Window → General → Test Runner
2. Select EditMode or PlayMode tab
3. Click "Run All" or select specific tests
4. View results in Test Runner window

#### Via Command Line

```bash
# Run all tests
./scripts/run-tests.sh

# Run specific test category
Unity -batchmode -quit -projectPath . \
  -runTests -testPlatform EditMode \
  -testFilter "Soulvan.Tests.Editor.MissionModuleTests" \
  -testResults TestResults/results.xml
```

#### Via CI/CD

Tests run automatically via GitHub Actions workflows on every push and PR.

## Debugging Tools

### Unity Console Commands

Press `~` in play mode to open debug console:

#### Mission Commands

```bash
# Start mission
mission.start()

# Add heat
mission.heat(50)

# Complete mission
mission.complete()

# Set state
mission.state(MissionState.Chase)
```

#### DAO Commands

```bash
# Create proposal
dao.proposal("D163")

# Cast vote
dao.vote("D163", "YES")

# Show stats
dao.stats("C001")
```

#### Debug Overlays

```bash
# Toggle heat visualization
debug.heat(true)

# Toggle AI paths
debug.ai_paths(true)

# Toggle performance stats
debug.perf(true)
```

### Debug Inspector

Enable advanced inspector features:

1. Select any GameObject
2. Click the context menu (⋮) in Inspector
3. Enable "Debug Mode"
4. View private fields and properties

### Profiler

Analyze performance:

1. Open Window → Analysis → Profiler
2. Press Play in editor
3. Monitor CPU, GPU, Memory, Rendering
4. Use Timeline View for detailed analysis

**Key Metrics**:
- Frame time: < 16.67ms (60 FPS)
- Draw calls: < 1000
- SetPass calls: < 100
- Memory: Monitor heap allocations

## Build Configuration

### Build Settings

Configure via File → Build Settings:

#### Platform-Specific Settings

**Windows**:
- Architecture: x86_64
- Scripting Backend: IL2CPP (recommended)
- API Level: .NET 4.x

**Linux**:
- Architecture: x86_64
- Scripting Backend: Mono (faster builds) or IL2CPP (better performance)

**macOS**:
- Architecture: Universal (Intel + Apple Silicon)
- Scripting Backend: IL2CPP

**WebGL**:
- Compression: Brotli
- Memory size: 2GB minimum
- Exception handling: Explicitly Thrown Exceptions Only

### Player Settings

Configure via Edit → Project Settings → Player:

#### Company & Product

```
Company Name: Soulvan
Product Name: Soulvan NeonVault
Version: 1.0.0
```

#### Resolution & Presentation

```
Fullscreen Mode: Exclusive Fullscreen
Default Screen Width: 1920
Default Screen Height: 1080
Run In Background: true (for networking)
```

#### Graphics

```
Color Space: Linear
Graphics API: DirectX 12 (Windows), Vulkan (Linux), Metal (macOS)
HDRP Asset: Assigned
```

## Package Management

### Creating UPM Packages

1. **Update package.json**:
```json
{
  "name": "com.soulvan.neonvault",
  "version": "1.0.0",
  "displayName": "Soulvan NeonVault",
  "description": "Neon Vault Heist mission framework",
  "unity": "2020.3",
  "dependencies": {}
}
```

2. **Create package archive**:
```bash
cd Packages/com.soulvan.neonvault
tar -czf ../../com.soulvan.neonvault-1.0.0.tgz .
```

3. **Test installation**:
```bash
# Add to another project
{
  "dependencies": {
    "com.soulvan.neonvault": "file:../../com.soulvan.neonvault-1.0.0.tgz"
  }
}
```

### Package Distribution

Packages are automatically built and published via GitHub Actions:

- **Workflow**: `.github/workflows/package-upm.yml`
- **Trigger**: Push to main, tags, or manual dispatch
- **Output**: `.tgz` archive attached to GitHub Release

## Performance Optimization

### Asset Optimization

- **Textures**: Use appropriate compression (BC7 for Windows, ASTC for mobile)
- **Models**: Optimize mesh count, combine materials
- **Audio**: Use streaming for long clips, compressed for effects

### Code Optimization

- **Use Object Pooling**: For frequently spawned objects
- **Cache Components**: Avoid `GetComponent` in Update
- **Avoid String Allocation**: Use StringBuilder or constants
- **Profile Regularly**: Use Unity Profiler to identify bottlenecks

### Build Size Optimization

- **Strip Engine Code**: Enable in Player Settings
- **Managed Code Stripping**: High (IL2CPP builds)
- **Compress Assets**: Enable asset bundle compression
- **Remove Debug Symbols**: In release builds

## Troubleshooting

### Common Issues

#### Missing References

**Problem**: "NullReferenceException" or missing component references

**Solution**:
```bash
# Run auto-wire tool
Window → Soulvan → Auto-Wire Scene

# Or manually assign in Inspector
```

#### Build Errors

**Problem**: Build fails with compilation errors

**Solution**:
```bash
# Clear Library folder
rm -rf Library/

# Reimport all assets
Assets → Reimport All

# Check for platform-specific code issues
```

#### Performance Issues

**Problem**: Low FPS or stuttering

**Solution**:
```bash
# Open Profiler
Window → Analysis → Profiler

# Identify bottlenecks
# Optimize physics, rendering, or scripting as needed

# Adjust quality settings
Edit → Project Settings → Quality
```

## Additional Resources

- [Unity Manual](https://docs.unity3d.com/Manual/)
- [Unity Scripting API](https://docs.unity3d.com/ScriptReference/)
- [Unity Forums](https://forum.unity.com/)
- [GameCI Documentation](https://game.ci/docs)

## Support

For Unity-specific issues:
- Check [Unity Forums](https://forum.unity.com/)
- Review [Unity Answers](https://answers.unity.com/)
- Create issue in this repository with `[Unity]` tag

## License

This documentation is part of the Soulvan project and is provided under the MIT License.
