# Troubleshooting: "Santa" Menu Not Appearing

If you can't see the **Santa → Testing → Setup Resource System Test Scene** menu item in Unity, follow these steps:

## Step 1: Check Unity Compilation Status

1. Look at the **bottom-right corner** of Unity Editor
2. You should see a progress bar or spinning icon if Unity is compiling
3. Wait for compilation to complete (usually 10-30 seconds)
4. Once complete, check the menu again

## Step 2: Check for Compilation Errors

1. Open the **Console** window (Window → General → Console)
2. Look for any red error messages
3. Common errors:

### Error: "The type or namespace name 'TMPro' could not be found"
**Solution**: Import TextMeshPro
1. Window → TextMeshPro → Import TMP Essential Resources
2. Click "Import"
3. Wait for Unity to recompile

### Error: "The type or namespace name 'ResourceManager' could not be found"
**Solution**: Ensure ResourceManager.cs exists
1. Check that `Assets/_Project/Scripts/Core/ResourceManager.cs` exists
2. If missing, you need to implement Task 1-14 first

### Error: "The type or namespace name 'ResourceSystemTester' could not be found"
**Solution**: Check the tester script exists
1. Verify `Assets/_Project/Scripts/Testing/ResourceSystemTester.cs` exists
2. Check for syntax errors in the file

## Step 3: Verify File Structure

Ensure these files exist:

```
Assets/
└── _Project/
    └── Scripts/
        └── Testing/
            ├── ResourceSystemTester.cs
            └── Editor/
                └── ResourceSystemTestSceneSetup.cs
```

**Important**: The `Editor` folder MUST be named exactly "Editor" (case-sensitive) for Unity to recognize it as an editor script folder.

## Step 4: Check File Contents

1. Open `ResourceSystemTestSceneSetup.cs` in your code editor
2. Verify it starts with `#if UNITY_EDITOR`
3. Verify it has the `[MenuItem("Santa/Testing/Setup Resource System Test Scene")]` attribute
4. Verify the namespace is `SantasWorkshop.Testing.Editor`

## Step 5: Force Recompilation

If the menu still doesn't appear:

1. **Method 1**: Reimport the script
   - Right-click `ResourceSystemTestSceneSetup.cs` in Project window
   - Select "Reimport"
   - Wait for compilation

2. **Method 2**: Refresh Assets
   - Edit → Preferences → External Tools
   - Click "Regenerate project files"
   - Or press Ctrl+R (Windows) / Cmd+R (Mac)

3. **Method 3**: Restart Unity
   - Save your work
   - Close Unity Editor
   - Reopen the project
   - Wait for compilation

## Step 6: Check Unity Version

The script requires:
- Unity 6.0 or later
- TextMeshPro package installed
- UI package installed

Verify your Unity version:
1. Help → About Unity
2. Should show Unity 6.0.x or later

## Step 7: Manual Verification

Test if the script is being recognized:

1. Open `ResourceSystemTestSceneSetup.cs` in Unity
2. In the Inspector, you should see "Script" with the file name
3. If you see "The associated script cannot be loaded", there's a compilation error

## Alternative: Use Manual Setup

If you can't get the menu to appear, you can set up the scene manually:

1. Follow the instructions in `MANUAL_SETUP_ALTERNATIVE.md`
2. This takes 5-10 minutes but doesn't require the menu

## Common Causes and Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| Menu not appearing | Unity still compiling | Wait for compilation to finish |
| Menu not appearing | Compilation errors | Check Console, fix errors |
| Menu not appearing | Wrong folder structure | Ensure script is in `Editor/` folder |
| Menu not appearing | Script syntax error | Check script for errors |
| Menu not appearing | Unity cache issue | Restart Unity Editor |
| Menu not appearing | Missing dependencies | Import TextMeshPro, check ResourceManager exists |

## Debug: Check if MenuItem is Registered

1. Open Unity's **Console** window
2. Type this in the search bar: `MenuItem`
3. If you see errors related to MenuItem, there's an issue with the script

## Still Not Working?

If none of the above works:

1. **Use Manual Setup**: Follow `MANUAL_SETUP_ALTERNATIVE.md`
2. **Check Prerequisites**: Ensure Tasks 1-14 are complete
3. **Verify Dependencies**: 
   - ResourceManager.cs exists and compiles
   - ResourceSystemTester.cs exists and compiles
   - TextMeshPro is imported
4. **Check Unity Console**: Look for any warnings or errors

## Expected Behavior

When working correctly:
1. Unity compiles the script (10-30 seconds)
2. Menu appears: **Santa → Testing → Setup Resource System Test Scene**
3. Clicking the menu opens the test scene and sets it up automatically
4. A dialog appears: "Setup Complete"

## Quick Test

To verify the script is working:

1. Open `ResourceSystemTestSceneSetup.cs`
2. Add this line at the top of the `SetupTestScene()` method:
   ```csharp
   Debug.Log("Menu item clicked!");
   ```
3. Save the file
4. Wait for Unity to recompile
5. Click the menu item (if it appears)
6. Check Console for "Menu item clicked!" message

If you see the message, the script is working but may have an error later in execution.

---

**Need More Help?**
- Check `MANUAL_SETUP_ALTERNATIVE.md` for manual setup instructions
- Check `README_ResourceSystemTest.md` for detailed documentation
- Check Unity Console for specific error messages
