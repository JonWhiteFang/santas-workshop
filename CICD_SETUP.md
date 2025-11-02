# CI/CD Setup Guide for Santa's Workshop Automation

This guide will help you set up automated testing and building with GitHub Actions.

## ✅ What's Been Created

1. **GitHub Actions Workflow** (`.github/workflows/unity-ci.yml`)
   - Automated Unity tests (EditMode & PlayMode)
   - Windows and Linux builds
   - Code quality checks

2. **Test Framework** (`Assets/Tests/`)
   - EditMode test assembly and examples
   - PlayMode test assembly and examples
   - Ready to run in Unity Test Runner

## 🚀 Setup Steps

### Step 1: Add Unity License to GitHub Secrets

1. **Copy your Unity license file content**:
   - File location: `C:\ProgramData\Unity\Unity_lic.ulf`
   - Open in text editor and copy entire contents

2. **Add to GitHub**:
   - Go to your repository on GitHub
   - Navigate to **Settings** → **Secrets and variables** → **Actions**
   - Click **New repository secret**
   - Name: `UNITY_LICENSE`
   - Value: Paste the entire XML content from Unity_lic.ulf
   - Click **Add secret**

### Step 2: Enable GitHub Actions

1. Go to **Settings** → **Actions** → **General**
2. Under "Actions permissions", select **Allow all actions and reusable workflows**
3. Click **Save**

### Step 3: Push to GitHub

```powershell
# Add the new files
git add .github/ Assets/Tests/ CICD_SETUP.md

# Commit
git commit -m "Add CI/CD pipeline with Unity tests"

# Push to trigger the workflow
git push origin main
```

### Step 4: Monitor First Run

1. Go to **Actions** tab in your GitHub repository
2. You should see "Unity CI/CD" workflow running
3. Click on it to view progress
4. First run will take ~15-30 minutes (downloads Unity, builds cache)
5. Subsequent runs will be much faster (~5-10 minutes)

## 🧪 Running Tests Locally

### In Unity Editor

1. Open Unity
2. Go to **Window** → **General** → **Test Runner**
3. Click **EditMode** tab → **Run All**
4. Click **PlayMode** tab → **Run All**

### Via Command Line

```powershell
# Run EditMode tests
& "C:\Program Files\Unity\Hub\Editor\6000.2.10f1\Editor\Unity.exe" `
  -runTests -batchmode -projectPath . `
  -testPlatform EditMode `
  -testResults TestResults-EditMode.xml

# Run PlayMode tests
& "C:\Program Files\Unity\Hub\Editor\6000.2.10f1\Editor\Unity.exe" `
  -runTests -batchmode -projectPath . `
  -testPlatform PlayMode `
  -testResults TestResults-PlayMode.xml
```

## 📊 What Gets Tested

### Current Tests (Examples)

**EditMode Tests** (run in editor, no Play mode):
- Basic assertions and logic
- GameObject creation
- Vector math
- String manipulation
- Collection operations

**PlayMode Tests** (run in Play mode with full Unity runtime):
- Time-based operations
- GameObject instantiation with components
- Transform movement over time
- MonoBehaviour lifecycle
- Coroutines

### Adding Your Own Tests

**EditMode Test Template**:
```csharp
using NUnit.Framework;

namespace SantasWorkshop.Tests.Editor
{
    public class ResourceManagerTests
    {
        [Test]
        public void ResourceManager_AddResource_IncreasesAmount()
        {
            // Arrange
            var manager = new ResourceManager();
            
            // Act
            manager.AddResource(ResourceType.IronOre, 10);
            
            // Assert
            Assert.AreEqual(10, manager.GetResourceAmount(ResourceType.IronOre));
        }
    }
}
```

**PlayMode Test Template**:
```csharp
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections;

namespace SantasWorkshop.Tests.Runtime
{
    public class MachineTests
    {
        [UnityTest]
        public IEnumerator Machine_Process_ProducesOutput()
        {
            // Arrange
            var machine = new GameObject().AddComponent<Smelter>();
            
            // Act
            machine.StartProcessing();
            yield return new WaitForSeconds(2f);
            
            // Assert
            Assert.IsTrue(machine.HasOutput);
            
            // Cleanup
            Object.Destroy(machine.gameObject);
        }
    }
}
```

## 🔧 Workflow Triggers

The CI/CD pipeline runs on:

1. **Push to `main` or `develop`**
   - Runs all tests
   - Builds Windows and Linux versions (main only)

2. **Pull Request to `main` or `develop`**
   - Runs all tests
   - No builds (saves time)

3. **Manual Trigger**
   - Go to Actions → Unity CI/CD → Run workflow
   - Select branch and run

## 📦 Build Artifacts

After successful builds on `main` branch:

1. Go to **Actions** tab
2. Click on completed workflow run
3. Scroll to **Artifacts** section
4. Download:
   - **Build-Windows**: Windows 64-bit executable
   - **Build-Linux**: Linux 64-bit executable
   - **Test Results**: Detailed test reports

## ⚡ Performance & Caching

**First Run**: ~15-30 minutes
- Downloads Unity
- Builds Library folder
- Runs tests
- Creates builds

**Cached Runs**: ~5-10 minutes
- Uses cached Library folder
- Only rebuilds changed assets

**Cache Invalidation**:
- Assets, Packages, or ProjectSettings change
- Unity version changes
- 7 days of inactivity

## 🐛 Troubleshooting

### License Activation Fails
```
Error: Failed to activate Unity license
```
**Solution**: 
- Verify `UNITY_LICENSE` secret is set correctly
- Copy entire XML content including `<?xml version...>` header
- Check license hasn't expired

### Tests Fail Locally But Pass in CI
**Solution**:
- Check for platform-specific code
- Verify all dependencies are committed
- Check for hardcoded paths

### Build Fails
```
Error: Build failed with exit code 1
```
**Solution**:
- Check all scenes are in Build Settings
- Verify no compilation errors
- Check platform-specific settings

### Cache Issues
**Solution**:
- Go to Actions → Caches
- Delete old caches
- Re-run workflow

## 💰 GitHub Actions Usage

**Free Tier** (Private Repos):
- 2,000 minutes/month
- ~20-40 full CI runs per month

**Public Repos**:
- Unlimited minutes

**Typical Usage**:
- Test job: ~5-10 minutes
- Build job: ~10-20 minutes per platform

**Monitor Usage**:
- Settings → Billing → Actions

## 📚 Next Steps

1. **Add Real Tests**: Replace example tests with actual game logic tests
2. **Add More Platforms**: Add macOS builds if needed
3. **Add Code Coverage**: Track test coverage percentage
4. **Add Deployment**: Auto-deploy to Steam/itch.io on release
5. **Add Notifications**: Slack/Discord notifications on build status

## 🔗 Useful Links

- [Unity Test Framework Docs](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)
- [GameCI Documentation](https://game.ci/docs)
- [GitHub Actions Docs](https://docs.github.com/en/actions)
- [NUnit Assertions](https://docs.nunit.org/articles/nunit/writing-tests/assertions/assertion-models/constraint.html)

## ✨ Status Badge

Add this to your README.md to show build status:

```markdown
![Unity CI/CD](https://github.com/YOUR_USERNAME/YOUR_REPO/workflows/Unity%20CI/CD/badge.svg)
```

Replace `YOUR_USERNAME` and `YOUR_REPO` with your actual GitHub username and repository name.
