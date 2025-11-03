# How to Add UNITY_LICENSE Secret to GitHub

## Quick Steps

1. **Copy your Unity license file**:
   ```powershell
   Get-Content "C:\ProgramData\Unity\Unity_lic.ulf" | Set-Clipboard
   ```
   This copies the entire license file to your clipboard.

2. **Go to GitHub**:
   - Open: https://github.com/JonWhiteFang/santas-workshop/settings/secrets/actions
   - Or navigate: Your Repo → Settings → Secrets and variables → Actions

3. **Add the secret**:
   - Click **"New repository secret"**
   - Name: `UNITY_LICENSE`
   - Value: Paste from clipboard (Ctrl+V)
   - Click **"Add secret"**

## Verify It's Added

After adding the secret, you should see:
- `UNITY_LICENSE` listed under "Repository secrets"
- The value will be hidden (shows as `***`)

## Re-run the Workflow

1. Go to: https://github.com/JonWhiteFang/santas-workshop/actions
2. Click on the latest failed workflow run
3. Click **"Re-run all jobs"** button (top right)

## What the License Looks Like

Your license file should start with:
```xml
<?xml version="1.0" encoding="UTF-8"?><root><TimeStamp Value="...
```

Make sure you copy the **entire file** including the XML header.

## Troubleshooting

### "License activation failed"
- Make sure you copied the entire file (including `<?xml...`)
- Check that your Unity license hasn't expired
- Verify you're using Unity Personal (not Pro/Enterprise without proper license)

### "Secret not found"
- Make sure the secret name is exactly `UNITY_LICENSE` (case-sensitive)
- Verify you added it to the correct repository

### Still failing?
- Check the Actions logs for specific error messages
- The license file should be ~2-3KB in size
- Make sure there are no extra spaces or characters

## Alternative: Manual Copy

If the PowerShell command doesn't work:

1. Open `C:\ProgramData\Unity\Unity_lic.ulf` in Notepad
2. Press Ctrl+A (select all)
3. Press Ctrl+C (copy)
4. Paste into GitHub secret value field
