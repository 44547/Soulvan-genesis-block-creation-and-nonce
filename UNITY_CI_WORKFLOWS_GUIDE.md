# Unity CI/CD Workflows Guide

## Overview

This repository includes comprehensive CI/CD workflows for Unity projects using GitHub Actions and the [GameCI](https://game.ci/) tooling suite. These workflows provide automated testing, building, and package management for the Soulvan Unity HDRP project.

## Available Workflows

### 1. Unity Test Runner (`unity-test.yml`)

**Purpose**: Automatically runs Unity tests in both EditMode and PlayMode.

**Triggers**:
- Push to `main` or `develop` branches (when Unity files change)
- Pull requests to `main` or `develop` branches
- Manual workflow dispatch

**Features**:
- Runs EditMode tests (editor-only tests)
- Runs PlayMode tests (runtime tests)
- Caches Unity Library for faster builds
- Uploads test results as artifacts
- Publishes test results as PR comments

**Usage**:
```bash
# Tests run automatically on push/PR
# Or trigger manually from Actions tab
```

### 2. Unity Build (`unity-build.yml`)

**Purpose**: Builds the Unity project for multiple platforms.

**Triggers**:
- Push to `main` branch
- Version tags (e.g., `v1.0.0`)
- Pull requests to `main` branch
- Manual workflow dispatch (with platform selection)

**Platforms Supported**:
- Windows (StandaloneWindows64)
- Linux (StandaloneLinux64)
- macOS (StandaloneOSX - requires macOS runner)

**Features**:
- Multi-platform builds with matrix strategy
- Library caching for faster builds
- Automatic versioning with semantic versioning
- Build artifacts uploaded for each platform
- Automatic GitHub release creation for tags
- Archives (ZIP for Windows, TAR.GZ for Linux/macOS)

**Usage**:
```bash
# Build automatically on push to main
# Or create a release tag:
git tag v1.0.0
git push origin v1.0.0

# Or trigger manually from Actions tab
```

### 3. Unity Activation (`unity-activation.yml`)

**Purpose**: Generate Unity license activation file for CI/CD.

**Triggers**:
- Manual workflow dispatch only

**Features**:
- Generates activation file (.alf)
- Provides step-by-step instructions
- Downloads as artifact

**Usage**:
1. Go to Actions tab → Unity Activation → Run workflow
2. Download the `.alf` file from artifacts
3. Go to https://license.unity3d.com/manual
4. Upload the `.alf` file
5. Download the returned `.ulf` license file
6. Add the `.ulf` contents as `UNITY_LICENSE` secret in repository settings

### 4. Unity Package Validation (`unity-package-validation.yml`)

**Purpose**: Validates Unity package structure and creates distribution archives.

**Triggers**:
- Push to `main` or `develop` branches (when package files change)
- Pull requests to `main` or `develop` branches
- Manual workflow dispatch

**Features**:
- Validates `package.json` syntax
- Checks for required package files
- Runs Unity package tests
- Creates `.tgz` package archives
- Uploads package artifacts

**Usage**:
```bash
# Validation runs automatically on package changes
# Or trigger manually from Actions tab
```

### 5. NeonVault CI Package (`ci-neonvault-package.yml`)

**Purpose**: Comprehensive CI pipeline for NeonVault package with JSON importing, testing, and release.

**Triggers**:
- Push to `main` branch (when Unity files change)
- Manual workflow dispatch

**Features**:
- Sets environment variables from secrets
- Runs Unity EditorPrefs configuration
- Executes JSON importers (strongly typed and CSV variants)
- Runs EditMode tests
- Creates UPM package archive
- Publishes to GitHub Releases (on tags or manual dispatch)

**Usage**:
```bash
# Runs automatically on push to main
# Or trigger manually from Actions tab for release
```

## Setup Instructions

### Prerequisites

1. **Unity Version**: The workflows are configured for Unity 2021.3.31f1 (LTS)
2. **GitHub Repository**: Your Unity project must be in the repository root or adjust paths accordingly
3. **Git LFS**: Enabled for large assets (automatically handled by workflows)

### Required Secrets

Configure these secrets in your repository (Settings → Secrets and variables → Actions):

#### Option 1: Personal Unity License (Recommended for individuals)

```
UNITY_EMAIL=your-unity-email@example.com
UNITY_PASSWORD=your-unity-password
UNITY_LICENSE=<contents of .ulf file - see Unity Activation workflow>
```

#### Option 2: Unity Pro/Plus License (For organizations)

```
UNITY_LICENSE=<contents of .ulf file>
```

#### Soulvan-Specific Secrets (Optional)

```
SOULVAN_API_BASE_URL=https://api.dev.soulvan
SOULVAN_REPLAY_ENDPOINT=https://api.dev.soulvan/replay/log
SOULVAN_DAO_ENDPOINT=https://api.dev.soulvan/dao/impact
SOULVAN_DEFAULT_CONTRIBUTOR=C_CI
```

### First-Time Setup

1. **Generate Unity License**:
   ```bash
   # Run Unity Activation workflow
   Actions → Unity Activation → Run workflow
   
   # Download .alf file from artifacts
   # Upload to https://license.unity3d.com/manual
   # Add returned .ulf contents to UNITY_LICENSE secret
   ```

2. **Verify Secrets**:
   - Ensure all required secrets are set in repository settings
   - Test with a manual workflow run

3. **Test Workflows**:
   ```bash
   # Trigger test workflow manually
   Actions → Unity Test Runner → Run workflow
   
   # Check results and fix any issues
   ```

4. **Configure Build Settings**:
   - Verify Unity version in workflows matches your project
   - Adjust platform matrix if needed
   - Update paths if your Unity project is not in root

## Workflow Configuration

### Unity Version

Update the Unity version in all workflows:

```yaml
env:
  UNITY_VERSION: '2021.3.31f1'  # Change this to match your project
```

### Custom Build Platforms

Modify the matrix in `unity-build.yml`:

```yaml
strategy:
  matrix:
    targetPlatform:
      - StandaloneWindows64
      - StandaloneLinux64
      - StandaloneOSX  # Requires macOS runner
      - WebGL          # Add WebGL support
      - Android        # Add Android support
```

### Custom Test Modes

Modify the matrix in `unity-test.yml`:

```yaml
strategy:
  matrix:
    testMode:
      - EditMode
      - PlayMode
```

## Caching Strategy

All workflows use GitHub Actions cache to speed up builds:

```yaml
- name: Cache Unity Library
  uses: actions/cache@v3
  with:
    path: Library
    key: Library-${{ matrix.testMode }}-${{ hashFiles('Assets/**', 'Packages/**') }}
```

**Benefits**:
- Faster build times (5-10x speedup after first run)
- Reduced CI minutes usage
- Automatic cache invalidation on project changes

## Troubleshooting

### Unity License Issues

**Problem**: `Missing Unity license`

**Solution**:
```bash
# Re-run Unity Activation workflow
# Ensure UNITY_LICENSE secret contains full .ulf file contents
# Verify no extra whitespace or formatting issues
```

### Build Failures

**Problem**: `Unity build failed with exit code 1`

**Solution**:
```bash
# Check workflow logs for detailed error messages
# Common issues:
# - Missing references in scenes
# - Compilation errors in scripts
# - Missing packages or dependencies
# - Incorrect Unity version

# Test locally first:
Unity -batchmode -quit -projectPath . -buildTarget StandaloneWindows64
```

### Test Failures

**Problem**: Tests failing in CI but passing locally

**Solution**:
```bash
# Common causes:
# - Platform-specific code
# - Missing test dependencies
# - Race conditions in PlayMode tests
# - Environment variable differences

# Debug by downloading test results artifact
# Review test logs in workflow output
```

### Disk Space Issues

**Problem**: `No space left on device`

**Solution**:
```yaml
# Add disk cleanup step (already in unity-build.yml):
- name: Free disk space
  run: |
    sudo rm -rf /usr/share/dotnet
    sudo rm -rf /opt/ghc
    sudo rm -rf /usr/local/share/boost
```

### Cache Issues

**Problem**: Builds not using cache or cache corruption

**Solution**:
```bash
# Clear cache manually:
# Go to Actions → Caches → Delete old caches
# Or update cache key in workflow

# For corrupted cache:
# Add restore-keys for fallback caching
restore-keys: |
  Library-${{ matrix.testMode }}-
  Library-
```

## Best Practices

### 1. Incremental Changes
- Test workflows with small changes first
- Use manual triggers during initial setup
- Monitor CI minutes usage

### 2. Secret Management
- Never commit Unity license files
- Use repository secrets for API keys
- Rotate secrets periodically
- Use environment-specific secrets

### 3. Build Optimization
- Enable library caching
- Use specific trigger paths to avoid unnecessary builds
- Consider build matrices carefully (each platform = separate job)

### 4. Testing Strategy
- Run EditMode tests on every push
- Run PlayMode tests on PR only (they're slower)
- Use test coverage reports
- Fix flaky tests promptly

### 5. Release Management
- Use semantic versioning tags
- Include changelog in releases
- Test builds before tagging
- Archive older releases

## Performance Metrics

Typical workflow execution times (after cache):

| Workflow | Duration | CI Minutes |
|----------|----------|------------|
| Unity Test (EditMode) | 3-5 min | 3-5 min |
| Unity Test (PlayMode) | 8-12 min | 8-12 min |
| Unity Build (Single Platform) | 10-15 min | 10-15 min |
| Unity Build (All Platforms) | 30-45 min | 90-135 min |
| Package Validation | 5-8 min | 5-8 min |

**Cost Optimization**:
- Use cache to reduce build times by 5-10x
- Limit builds to necessary platforms
- Use path filters to avoid unnecessary runs
- Consider self-hosted runners for heavy workloads

## Additional Resources

- [GameCI Documentation](https://game.ci/docs)
- [Unity Manual - Command Line Arguments](https://docs.unity3d.com/Manual/CommandLineArguments.html)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Unity Cloud Build](https://unity.com/products/cloud-build)

## Support

For issues specific to:
- **GameCI Actions**: [GameCI Discord](https://game.ci/discord)
- **Unity Editor**: [Unity Forums](https://forum.unity.com/)
- **Soulvan Project**: Create an issue in this repository

## License

These workflows are part of the Soulvan project and are provided as-is under the MIT License.
