# Resource System Test Scene - Quick Start Guide

## 🚀 Getting Started (2 Minutes)

### Step 1: Wait for Unity to Compile

1. Open Unity Editor
2. Wait for Unity to finish compiling the new scripts (check bottom-right corner)
3. Once compilation is complete, the menu item will appear

### Step 2: Run the Automated Setup

1. In the top menu, click: **Santa → Testing → Setup Resource System Test Scene**
   - If you don't see the "Santa" menu, Unity is still compiling. Wait a moment and check again.
   - You can also check the Console for any compilation errors.
2. Wait for the "Setup Complete" dialog
3. Click "OK"

That's it! The scene is now fully configured and ready to test.

### Step 3: Test the Resource System

1. Press the **Play** button in Unity Editor
2. Wait for the status message: "ResourceManager initialized successfully!"
3. Click the test buttons to interact with the Resource System:

   - **Add Wood (+10)**: Adds 10 wood to inventory
   - **Add Iron (+10)**: Adds 10 iron ore to inventory
   - **Consume Wood (-5)**: Removes 5 wood (fails if insufficient)
   - **Consume Iron (-5)**: Removes 5 iron ore (fails if insufficient)
   - **Query Resources**: Displays detailed resource information
   - **Reset All**: Resets all resources to zero
   - **Set Capacity (100)**: Sets wood capacity limit to 100

4. Watch the **Resource Counts** panel (top-right) update in real-time
5. Check the **Status** text (top-left) for operation results
6. Open the **Console** window to see detailed logs

## 📋 What to Test

### Basic Operations
- ✅ Add resources and verify counts increase
- ✅ Consume resources and verify counts decrease
- ✅ Try to consume more than available (should fail gracefully)
- ✅ Query resources to see detailed information

### Capacity Management
- ✅ Set capacity limit for wood (100)
- ✅ Try to add more than 100 wood (should cap at 100)
- ✅ Verify capacity is displayed in resource counts

### Event System
- ✅ Verify UI updates automatically when resources change
- ✅ Check Console for event logs
- ✅ Confirm OnResourceChanged fires for each operation

### Reset Functionality
- ✅ Add various resources
- ✅ Click "Reset All"
- ✅ Verify all counts return to zero

## 🎯 Expected Behavior

### Successful Operations
- Status shows: "✓ Added 10 wood"
- Resource count increases
- UI updates immediately
- Console logs the operation

### Failed Operations
- Status shows: "✗ Failed to consume 5 wood (insufficient resources)"
- Resource count unchanged
- No error thrown (graceful failure)

### Capacity Limits
- Status shows: "Added only X wood (capacity limit reached)"
- Count stops at capacity limit
- Excess resources are not added

## 🔧 Troubleshooting

### Menu item "Santa → Testing" not visible
**Solution**: 
1. Wait for Unity to finish compiling (check bottom-right corner)
2. Check Console for compilation errors
3. Verify the file exists: `Assets/_Project/Scripts/Testing/Editor/ResourceSystemTestSceneSetup.cs`
4. Try restarting Unity Editor if the menu still doesn't appear

### "ResourceManager not found!" Error
**Solution**: Run the automated setup again (Santa → Testing → Setup Resource System Test Scene)

### Resources not displaying
**Solution**: Ensure ResourceData assets exist in `Assets/_Project/Resources/ResourceDefinitions/` with IDs:
- `"wood"`
- `"iron_ore"`

### Buttons not responding
**Solution**: 
1. Stop Play mode
2. Run the automated setup again
3. Press Play

## 📚 Additional Resources

- **Detailed Manual Setup**: See `README_ResourceSystemTest.md`
- **Implementation Details**: See `IMPLEMENTATION_SUMMARY.md`
- **ResourceManager API**: See `Assets/_Project/Scripts/Core/ResourceManager.cs`

## 🎮 Advanced Testing

Once basic testing is complete, try:

1. **Stress Testing**: Add large amounts of resources (1000+)
2. **Rapid Operations**: Click buttons quickly to test event handling
3. **Edge Cases**: Test with zero resources, negative amounts (should fail)
4. **Multiple Resources**: Add different resource types and verify isolation

## ✅ Success Criteria

Your test is successful if:
- ✅ ResourceManager initializes without errors
- ✅ All buttons work as expected
- ✅ Resource counts update in real-time
- ✅ Capacity limits are enforced
- ✅ Consume operations fail gracefully when insufficient
- ✅ Events fire correctly (check Console)
- ✅ Reset clears all resources

## 🚦 Next Steps

After successful testing:
1. ✅ Mark Task 15 as complete
2. ➡️ Proceed to Task 16: Write unit tests for ResourceManager
3. ➡️ Continue with Task 17: Integration testing and validation

---

**Need Help?** Check the Console window for detailed logs and error messages.

**Found a Bug?** Document the steps to reproduce and check the ResourceManager implementation.
