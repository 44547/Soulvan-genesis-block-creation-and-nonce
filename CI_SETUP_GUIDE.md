# Soulvan NeonVault — CI Setup Guide

## Overview

This guide covers configuring CI secrets, running JSON importers, and packaging Unity UPM artifacts with GitHub Actions.

---

## CI Secrets Configuration

### Required Secrets (GitHub Settings → Secrets and Variables → Actions)

Add these repository secrets:

1. **SOULVAN_API_BASE_URL**
   - Base API URL (e.g., `https://api.dev.soulvan`)
   - Used by ServerRpcClient, ReplaySeedLogger, DAOMissionNotifier

2. **SOULVAN_REPLAY_ENDPOINT**
   - Replay logging endpoint (e.g., `https://api.dev.soulvan/replay/log`)
   - Use short-lived tokens or server-side relay

3. **SOULVAN_DAO_ENDPOINT**
   - DAO impact endpoint (e.g., `https://api.dev.soulvan/dao/impact`)
   - For governance integration

4. **SOULVAN_DEFAULT_CONTRIBUTOR**
   - Default contributor ID for CI runs (e.g., `C_CI`)
   - Used for replay attribution

5. **UNITY_LICENSE** (optional, for game-ci/unity-builder)
   - Unity license file content (base64 encoded)
   - Required for Unity CLI operations in CI

### Security Notes

- **Never commit tokens to source control**
- Use repository or organization-level secret stores
- Prefer server-side signing for deterministic seeds
- Client should only send unsigned digest; server returns signedSeed for provenance

---

## Local Development Setup

For local Unity Editor work, use EditorPrefs keys (avoids committing secrets):

```csharp
// Set in Unity Editor Console or custom EditorWindow
EditorPrefs.SetString("Soulvan:ApiBaseUrl", "https://api.dev.soulvan");
EditorPrefs.SetString("Soulvan:ReplayEndpoint", "https://api.dev.soulvan/replay/log");
EditorPrefs.SetString("Soulvan:DaoEndpoint", "https://api.dev.soulvan/dao/impact");
EditorPrefs.SetString("Soulvan:DefaultContributor", "C_DEV");
```

Or use OS environment variables in Unity scripts:

```csharp
var apiBase = Environment.GetEnvironmentVariable("SOULVAN_API_BASE_URL") ?? "https://api.dev.soulvan";
```

---

## Running JSON Importers

### 1. Seed JSON Preparation

Place JSON files in:
```
Assets/Soulvan/ScriptableObjects/seed-json/
```

Each JSON must include a `"type"` field:
```json
{
  "type": "SO_MissionModule",
  "moduleId": "approach:magrail",
  "displayName": "MagRail Infiltration",
  "weight": 0.6,
  "entryTags": ["approach"],
  "exitTags": ["perimeter"]
}
```

### 2. Unity Editor Manual Import

**Option A: MiniJSON Importer** (requires MiniJSON)
- Window → Soulvan → Import Seed JSON
- Select folder (default: `Assets/Soulvan/ScriptableObjects/seed-json`)
- Click "Import JSON -> ScriptableObjects"

**Option B: Strongly Typed Importer** (uses JsonUtility)
- Window → Soulvan → Import Strongly Typed JSON
- Select folder
- Click "Import JSON -> ScriptableObjects"

**Option C: CSV Logging Importer** (generates import-log.csv)
- Window → Soulvan → Import Typed JSON with CSV Log
- Set CSV output path (default: `Assets/Soulvan/Docs/import-log.csv`)
- Click "Import and Write CSV"

### 3. Validation

After import:
- Window → Soulvan → Validate Soulvan Assets
- Check Console for any warnings
- Fix missing fields or malformed assets

### 4. CI Import (via executeMethod)

```bash
# Set EditorPrefs from environment variables
Unity -batchmode -quit -projectPath . \
  -executeMethod Soulvan.CI.CIImportWrappers.SetEditorPrefsFromEnv \
  -logFile ci_setprefs.log

# Run importer
Unity -batchmode -quit -projectPath . \
  -executeMethod Soulvan.CI.CIImportWrappers.RunStrongJsonImport \
  -logFile ci_importer.log

# Run tests
Unity -batchmode -quit -projectPath . \
  -runTests -testPlatform EditMode \
  -testResults TestResults/editmode-results.xml \
  -logFile ci_tests.log
```

---

## Packaging UPM .tgz

### Local Packaging

**Linux/macOS/WSL/Git Bash:**
```bash
mkdir -p dist
tar -czf dist/com.soulvan.neonvault-1.0.0.tgz Packages/com.soulvan.neonvault
```

**Windows PowerShell:**
```powershell
New-Item -ItemType Directory -Force -Path dist
Compress-Archive -Path "Packages\com.soulvan.neonvault" -DestinationPath "dist\com.soulvan.neonvault-1.0.0.zip"
# Rename .zip to .tgz if required by registry
```

### CI Packaging

GitHub Actions workflow `.github/workflows/package-upm.yml` automatically:
1. Creates `dist/com.soulvan.neonvault-1.0.0.tgz`
2. Uploads as build artifact
3. Attaches to GitHub Release (on workflow_dispatch or tag push)

---

## GitHub Actions Workflows

### Workflow 1: `package-upm.yml`
- **Trigger**: Push to `main` (paths: `Packages/`, `Assets/Soulvan/`)
- **Actions**:
  - Checkout repo
  - Create .tgz archive
  - Upload artifact
  - Create GitHub Release (on manual dispatch or tag)

### Workflow 2: `ci-neonvault-package.yml`
- **Trigger**: Push to `main`, workflow_dispatch
- **Actions**:
  - Set EditorPrefs from secrets
  - Run JSON importer
  - Run EditMode tests
  - Create UPM package
  - Publish to GitHub Releases

**Note**: Unity CLI steps are commented out by default. Uncomment after adding Unity installation step (e.g., `game-ci/unity-builder` action).

---

## Post-Import Checklist

1. **Verify Assets Created**
   - Check `Assets/Soulvan/ScriptableObjects/MissionModules/`
   - Check `Assets/Soulvan/ScriptableObjects/Datacores/`
   - Check `Assets/Soulvan/ScriptableObjects/Heat/`
   - Check `Assets/Soulvan/ScriptableObjects/Vehicles/`

2. **Run Validator**
   - Window → Soulvan → Validate Soulvan Assets
   - Fix any issues shown in Console

3. **Scene Wiring** (if using NeonVaultHeist.unity)
   - Open scene
   - Assign ScriptableObject references in MissionController inspector
   - Set API endpoints in ServerRpcClient
   - Verify event subscriptions (OnMissionCompleted → ReplaySeedLogger, DAOMissionNotifier)

4. **Test in Play Mode**
   - Press Play in Unity Editor
   - Use debug console (`~` key) to trigger mission events
   - Verify replay logging and DAO impact calls

---

## Troubleshooting

### Importer Fails with JSON Parsing Errors
- Validate JSON structure at https://jsonlint.com
- Ensure DTO field names match JSON exactly
- Check for missing `"type"` field

### CSV Log is Empty
- Confirm files are in exact folder with `.json` extension
- Check CSV output path is writable
- Review importer log for exceptions

### Validator Reports Missing Fields
- Open failing asset in inspector
- Manually populate missing fields
- Re-import JSON after fixing source file

### CI Fails with Unity License Error
- Add `UNITY_LICENSE` secret (base64 encoded)
- Use `game-ci/unity-builder` action for Unity installation
- Check Unity version matches project

### Auto-Wire Doesn't Find Objects
- Ensure active scene is `NeonVaultHeist`
- Check GameObject and component names match conventions
- Manually verify inspector references after auto-wire

---

## Credits

Inspired by GTA series mission systems (Rockstar Games).

---

## Contact

For CI pipeline help or secret management questions, consult your DevOps lead or CI provider documentation.
