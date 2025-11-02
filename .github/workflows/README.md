# GitHub Actions CI/CD for Unity

This directory contains GitHub Actions workflows for automated testing and building of Santa's Workshop Automation.

## Workflows

### `unity-ci.yml` - Main CI/CD Pipeline

Runs on:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop`
- Manual trigger via workflow_dispatch

#### Jobs

1. **Test** (runs on all triggers)
   - EditMode tests (editor tests)
   - PlayMode tests (runtime tests)
   - Uploads test results as artifacts

2. **Build Windows** (runs only on push to `main`)
   - Builds Windows 64-bit executable
   - Uploads build as artifact

3. **Build Linux** (runs only on push to `main`)
   - Builds Linux 64-bit executable
   - Uploads build as artifact

4. **Code Quality** (runs on all triggers)
   - Runs .NET code analysis
   - Checks code formatting

## Setup Instructions

### 1. Add Unity License Secret

You need to add your Unity license file as a GitHub secret:

1. Go to your repository on GitHub
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Name: `UNITY_LICENSE`
5. Value: Paste the entire contents of `C:\ProgramData\Unity\Unity_lic.ulf`
6. Click **Add secret**

### 2. Enable GitHub Actions

1. Go to **Settings** → **Actions** → **General**
2. Under "Actions permissions", select **Allow all actions and reusable workflows**
3. Click **Save**

### 3. Enable Git LFS (if using large assets)

```powershell
git lfs install
git lfs track "*.png"
git lfs track "*.jpg"
git lfs track "*.fbx"
git lfs track "*.wav"
git lfs track "*.ogg"
```

## Viewing Results

### Test Results
- Go to **Actions** tab in your repository
- Click on a workflow run
- View test results in the job summary
- Download detailed test results from artifacts

### Build Artifacts
- After a successful build on `main` branch
- Go to **Actions** → Select workflow run
- Download build artifacts (Build-Windows, Build-Linux)

## Caching

The workflow caches the Unity `Library` folder to speed up subsequent builds:
- First run: ~15-30 minutes
- Cached runs: ~5-10 minutes

Cache is invalidated when:
- Assets, Packages, or ProjectSettings change
- Unity version changes
- 7 days of inactivity

## Troubleshooting

### License Activation Fails
- Verify `UNITY_LICENSE` secret is set correctly
- Check Unity version matches `UNITY_VERSION` in workflow
- Ensure license is valid and not expired

### Tests Fail
- Check test results in artifacts
- Run tests locally: **Window** → **General** → **Test Runner**
- Verify all test assemblies are properly configured

### Build Fails
- Check build logs in Actions tab
- Verify all scenes are added to Build Settings
- Check for platform-specific compilation errors

### Cache Issues
- Manually clear cache: **Actions** → **Caches** → Delete cache
- Cache will rebuild on next run

## Manual Workflow Trigger

You can manually trigger the workflow:
1. Go to **Actions** tab
2. Select **Unity CI/CD** workflow
3. Click **Run workflow**
4. Select branch and click **Run workflow**

## Performance Tips

1. **Use cache effectively**: Don't modify Assets/Packages unnecessarily
2. **Run tests locally first**: Catch issues before pushing
3. **Use draft PRs**: Prevent unnecessary CI runs during development
4. **Limit build targets**: Only build for platforms you need

## Unity Test Framework

### Creating Tests

**EditMode Test** (Assets/Tests/Editor/):
```csharp
using NUnit.Framework;
using UnityEngine;

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
```

**PlayMode Test** (Assets/Tests/Runtime/):
```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

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
    }
}
```

## Cost Considerations

GitHub Actions provides:
- **Free tier**: 2,000 minutes/month for private repos
- **Public repos**: Unlimited minutes

Unity builds typically use:
- Test job: ~5-10 minutes
- Build job: ~10-20 minutes per platform

Monitor usage: **Settings** → **Billing** → **Actions**

## Additional Resources

- [Unity Test Framework Documentation](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)
- [GameCI Documentation](https://game.ci/docs)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
