# Unity CI/CD Quick Reference

## 🚀 Quick Commands

### Run Tests Locally
```bash
# EditMode tests
Unity -batchmode -quit -projectPath . -runTests -testPlatform EditMode

# PlayMode tests  
Unity -batchmode -quit -projectPath . -runTests -testPlatform PlayMode
```

### Build Locally
```bash
# Windows
Unity -batchmode -quit -projectPath . -buildTarget StandaloneWindows64 -buildPath builds/windows

# Linux
Unity -batchmode -quit -projectPath . -buildTarget StandaloneLinux64 -buildPath builds/linux

# macOS
Unity -batchmode -quit -projectPath . -buildTarget StandaloneOSX -buildPath builds/macos
```

### Create Package
```bash
cd Packages/com.soulvan.neonvault
tar -czf ../../com.soulvan.neonvault-1.0.0.tgz .
```

## 🔧 Required Secrets

Add these in **Settings → Secrets and variables → Actions**:

| Secret | Description | Required |
|--------|-------------|----------|
| `UNITY_LICENSE` | Contents of .ulf license file | ✅ Yes |
| `UNITY_EMAIL` | Unity account email | ✅ Yes |
| `UNITY_PASSWORD` | Unity account password | ✅ Yes |
| `SOULVAN_API_BASE_URL` | API base URL | ⚠️ Optional |
| `SOULVAN_REPLAY_ENDPOINT` | Replay endpoint | ⚠️ Optional |
| `SOULVAN_DAO_ENDPOINT` | DAO endpoint | ⚠️ Optional |
| `SOULVAN_DEFAULT_CONTRIBUTOR` | Default contributor ID | ⚠️ Optional |

## 📋 Workflows Overview

| Workflow | Trigger | Duration (cached) | Purpose |
|----------|---------|-------------------|---------|
| Unity Test | Push/PR | 3-12 min | Run tests |
| Unity Build | Push to main | 10-15 min | Build platforms |
| Package Validation | Push/PR | 5-8 min | Validate packages |
| Unity Activation | Manual | 1-2 min | Get license |
| NeonVault CI | Push to main | 10-20 min | Full pipeline |

## 🎯 Common Tasks

### Get Unity License for CI
```bash
1. Actions → Unity Activation → Run workflow
2. Download .alf file
3. Go to https://license.unity3d.com/manual
4. Upload .alf, download .ulf
5. Copy .ulf contents to UNITY_LICENSE secret
```

### Trigger Manual Build
```bash
1. Actions → Unity Build → Run workflow
2. Select platform (Windows/Linux/macOS)
3. Wait for completion
4. Download from Artifacts
```

### Create Release
```bash
git tag v1.0.0
git push origin v1.0.0
# Workflows automatically create GitHub Release
```

### Debug Failed Workflow
```bash
1. Click failed workflow in Actions
2. Click failed job
3. Expand failed step
4. Review logs
5. Download artifacts if available
```

## 📊 Workflow Permissions

All workflows use explicit permissions:

```yaml
permissions:
  contents: read   # Read repo
  actions: read    # Read artifacts
  checks: write    # Write test results (test workflows only)
```

## 🔍 Troubleshooting

| Problem | Solution |
|---------|----------|
| License error | Re-run Unity Activation workflow |
| Build fails | Check Unity version matches (2021.3.31f1) |
| Tests fail | Download test results artifact |
| Out of disk | Workflows include cleanup steps |
| Cache issues | Clear cache: Actions → Caches |

## 📖 Documentation

- **[UNITY_CI_WORKFLOWS_GUIDE.md](UNITY_CI_WORKFLOWS_GUIDE.md)** - Complete guide
- **[UNITY_TOOLING_GUIDE.md](UNITY_TOOLING_GUIDE.md)** - Development tools
- **[UNITY_CI_IMPLEMENTATION_SUMMARY.md](UNITY_CI_IMPLEMENTATION_SUMMARY.md)** - What was implemented
- **[CI_SETUP_GUIDE.md](CI_SETUP_GUIDE.md)** - Secrets configuration

## 💡 Best Practices

✅ **DO**:
- Cache Library folder for faster builds
- Use path filters to avoid unnecessary runs
- Test locally before pushing
- Keep Unity version consistent
- Add descriptive commit messages

❌ **DON'T**:
- Commit Unity license files
- Skip test failures
- Build all platforms on every commit
- Ignore security warnings
- Clear cache unnecessarily

## 🔗 Useful Links

- [GameCI Docs](https://game.ci/docs)
- [Unity Command Line](https://docs.unity3d.com/Manual/CommandLineArguments.html)
- [GitHub Actions](https://docs.github.com/en/actions)

## 📞 Support

- **Issues**: GitHub Issues with `[Unity]` or `[CI]` tag
- **GameCI**: [Discord](https://game.ci/discord)
- **Unity**: [Forums](https://forum.unity.com/)

---

**Last Updated**: November 2025  
**Unity Version**: 2021.3.31f1  
**GameCI Version**: v4
