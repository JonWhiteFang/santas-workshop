# Unity License Setup for GitHub Actions

## The Problem

Your Unity Personal license is **machine-locked** and won't work on GitHub Actions runners. Unity Personal licenses are tied to specific hardware (see `MachineBindings` in your license file).

## Solution: Use Unity Email/Password Activation

For CI/CD with Unity Personal, you need to activate Unity on the runner using your Unity account credentials.

### Step 1: Add GitHub Secrets

Go to: https://github.com/JonWhiteFang/santas-workshop/settings/secrets/actions

Add these **3 secrets**:

1. **UNITY_EMAIL**
   - Your Unity account email
   - Example: `jono2411@outlook.com`

2. **UNITY_PASSWORD**
   - Your Unity account password
   - ⚠️ Use your actual Unity Hub password

3. **UNITY_LICENSE** (optional, but recommended)
   - Copy your license file content (the one you showed me)
   - This helps speed up activation
   - Even though it's machine-locked, it provides license info

### Step 2: Enable 2FA Bypass (Important!)

If you have 2FA enabled on your Unity account:

1. Go to: https://id.unity.com/en/account/edit
2. Navigate to **Security** → **Two-Factor Authentication**
3. Generate an **App Password** for CI/CD
4. Use this app password as `UNITY_PASSWORD` secret (not your regular password)

### Step 3: Verify Secrets Are Added

After adding all secrets, you should see:
- `UNITY_EMAIL` (hidden as ***)
- `UNITY_PASSWORD` (hidden as ***)
- `UNITY_LICENSE` (hidden as ***)

### Step 4: Re-run Workflow

1. Go to: https://github.com/JonWhiteFang/santas-workshop/actions
2. Click on the latest failed run
3. Click **"Re-run all jobs"**

## How It Works

The workflow will:
1. Use your email/password to sign in to Unity
2. Request a temporary license for the CI runner
3. Run tests with that license
4. Return the license when done

This is the official way to use Unity Personal in CI/CD.

## Alternative: Unity Pro/Plus License

If you have Unity Pro or Plus:
- Those licenses are **not** machine-locked
- You can use the license file directly
- No email/password needed
- More reliable for CI/CD

## Troubleshooting

### "Invalid credentials"
- Double-check email and password
- If using 2FA, make sure you're using an app password
- Try logging into Unity Hub manually to verify credentials

### "License activation failed"
- Unity Personal has activation limits (2 machines at a time)
- Make sure you're not using all available activations
- Deactivate Unity on unused machines

### "Too many activation requests"
- Unity limits activation requests per day
- Wait a few hours and try again
- Consider using Unity Pro for CI/CD

## Current Workflow Status

✅ Workflow is configured to use email/password activation
✅ Falls back to license file if provided
✅ Automatically returns license after tests

Just add the secrets and re-run!
