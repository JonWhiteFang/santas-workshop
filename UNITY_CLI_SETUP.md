# Unity CLI CI/CD Setup

This workflow uses Unity's official CLI directly, bypassing GameCI. This works with Unity 6!

## How It Works

1. **Downloads Unity Editor** directly from Unity's servers
2. **Caches the installation** for faster subsequent runs
3. **Activates license** using your Unity account
4. **Runs tests** using Unity's built-in test runner
5. **Builds the project** using Unity's build pipeline

## Required GitHub Secrets

Add these secrets at: https://github.com/JonWhiteFang/santas-workshop/settings/secrets/actions

### For Unity Personal (Free)

1. **UNITY_EMAIL**
   - Your Unity account email
   - Example: `jono2411@outlook.com`

2. **UNITY_PASSWORD**
   - Your Unity account password
   - If you have 2FA: Generate an app password at https://id.unity.com/en/account/edit

### For Unity Plus/Pro (Optional)

3. **UNITY_SERIAL** (only if you have Plus/Pro)
   - Your Unity serial key
   - Found in Unity Hub → Preferences → Licenses
   - Format: `XX-XXXX-XXXX-XXXX-XXXX-XXXX`

## Unity Version Configuration

The workflow is currently set to Unity 6000.0.23f1. To change it:

1. Open `.github/workflows/unity-cli.yml`
2. Update these environment variables:
   ```yaml
   env:
     UNITY_VERSION: 6000.0.23f1
     UNITY_VERSION_CHANGESET: 0c8aa4a77886
   ```

### Finding Your Unity Version

To find your exact Unity version and changeset:

1. Open Unity Hub
2. Go to Installs
3. Click the gear icon next to your Unity 6 installation
4. The version shows as: `6000.0.23f1 (0c8aa4a77886)`
   - Version: `6000.0.23f1`
   - Changeset: `0c8aa4a77886`

Or check in Unity Editor:
- **Help** → **About Unity**
- Version format: `6000.0.23f1 (0c8aa4a77886)`

## Workflow Features

### Test Job
- ✅ Runs EditMode tests (editor tests)
- ✅ Runs PlayMode tests (runtime tests)
- ✅ Generates XML test results
- ✅ Uploads test results as artifacts
- ✅ Caches Unity installation (~10GB)
- ✅ Caches Library folder for faster builds

### Build Job
- ✅ Builds Windows 64-bit executable
- ✅ Only runs on push to `main` branch
- ✅ Uploads build as artifact
- ✅ Uses cached Unity installation

## Performance

**First Run**: ~20-30 minutes
- Downloads Unity (~10GB)
- Builds Library folder
- Runs tests

**Cached Runs**: ~5-10 minutes
- Uses cached Unity installation
- Uses cached Library folder
- Only rebuilds changed assets

## Advantages Over GameCI

1. ✅ **Works with Unity 6** immediately
2. ✅ **Official Unity CLI** - no third-party dependencies
3. ✅ **Full control** over Unity installation
4. ✅ **Same commands** you use locally
5. ✅ **No Docker** complexity

## Disadvantages

1. ⚠️ **Slower first run** (downloads Unity)
2. ⚠️ **More complex** workflow file
3. ⚠️ **Manual version management** (no auto-updates)
4. ⚠️ **Larger cache** (~10GB Unity + Library)

## Troubleshooting

### "Unity executable not found"
- Check Unity version and changeset are correct
- Verify download URL is valid
- Check cache was restored properly

### "License activation failed"
- Verify UNITY_EMAIL and UNITY_PASSWORD are correct
- If using 2FA, use app password not regular password
- Check Unity account is active

### "Tests failed to run"
- Check test assembly definitions exist
- Verify tests are in correct folders (Assets/Tests/)
- Check Unity Test Framework package is installed

### "Build failed"
- Verify all scenes are added to Build Settings
- Check for compilation errors in scripts
- Ensure all required packages are installed

## Monitoring

### View Logs
1. Go to: https://github.com/JonWhiteFang/santas-workshop/actions
2. Click on workflow run
3. Click on job (Test or Build)
4. Expand steps to see detailed logs

### Download Artifacts
1. Go to completed workflow run
2. Scroll to "Artifacts" section
3. Download:
   - **Test Results**: XML test reports
   - **Build-Windows**: Compiled executable

## Cost Considerations

GitHub Actions free tier:
- **Public repos**: Unlimited minutes
- **Private repos**: 2,000 minutes/month

This workflow uses:
- First run: ~30 minutes
- Cached runs: ~10 minutes

**Estimate**: ~20-40 runs per month on free tier (private repo)

## Optimization Tips

1. **Use cache effectively**
   - Don't modify Assets/Packages unnecessarily
   - Cache persists for 7 days

2. **Run tests locally first**
   - Catch issues before pushing
   - Save CI minutes

3. **Use draft PRs**
   - Prevent CI runs during development
   - Mark PR as ready when done

4. **Limit build triggers**
   - Builds only run on `main` branch
   - Tests run on all branches

## Alternative: Use Unity Cloud Build

If this is too complex, consider Unity Cloud Build:
- Official Unity CI/CD service
- Integrated with Unity Dashboard
- Automatic Unity version management
- Free tier available

https://unity.com/products/cloud-build

## Next Steps

1. ✅ Add GitHub secrets (UNITY_EMAIL, UNITY_PASSWORD)
2. ✅ Update Unity version in workflow (if needed)
3. ✅ Push to trigger workflow
4. ✅ Monitor first run (will take ~30 minutes)
5. ✅ Subsequent runs will be much faster!
