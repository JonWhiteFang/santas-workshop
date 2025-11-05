# Quick Start: Machine Framework Test Scene

**5-Minute Setup Guide**

## Step 1: Open the Scene

1. In Unity Project window, navigate to:
   ```
   Assets/_Project/Scenes/TestScenes/
   ```
2. Double-click `MachineFrameworkTest.unity`

## Step 2: Verify Scene Setup

Check the Hierarchy window contains:
- ✅ Main Camera
- ✅ Directional Light
- ✅ ResourceManager
- ✅ GridManager
- ✅ TestExtractor_1
- ✅ TestProcessor_1

## Step 3: Enter Play Mode

1. Click the Play button (or press `Ctrl+P`)
2. Watch the Console for initialization messages:
   ```
   ResourceManager initialized with X resources.
   GridManager initialized: 20x20 grid with cell size 1
   ```

## Step 4: Inspect a Machine

1. Select `TestExtractor_1` in Hierarchy
2. In Inspector, observe:
   - Machine ID (auto-generated GUID)
   - Current State (Idle/WaitingForInput)
   - Processing Progress (0.0)
   - Power Status (true)

## Step 5: Watch State Transitions

1. Keep Console visible
2. Watch for state transition messages:
   ```
   Machine {id} transitioned from Idle to WaitingForInput
   ```

## What's Next?

### To Test Recipe Processing:

1. Create a Recipe ScriptableObject:
   - Right-click in Project → Create → Santa → Recipe
   - Configure inputs/outputs
   - Assign to TestProcessor_1

2. Add input resources:
   - Use ResourceManager.AddResource() in code
   - Or create a test UI button

3. Watch the machine process:
   - State: Idle → Processing → Idle
   - Progress: 0.0 → 1.0
   - Outputs produced

### To Test Power Management:

1. Select a machine
2. In Inspector, find the script component
3. Call `SetPowered(false)` via context menu or code
4. Watch state change to NoPower
5. Call `SetPowered(true)` to restore

### To Test Grid Integration:

1. Enable grid visualization (should be on by default)
2. Observe grid lines in Scene view
3. Note machine positions align with grid
4. Try moving machines to see grid updates

## Troubleshooting

**No initialization messages?**
- Check ResourceManager and GridManager are in scene
- Verify they have the correct scripts attached

**Machines not visible?**
- Check Scene view camera position
- Machines are at Y=0, camera should be above

**No state transitions?**
- Machines need MachineData assigned
- Machines need recipes to process
- Check Console for error messages

## Full Documentation

For complete testing instructions, see:
`README_MachineFrameworkTest.md` in this folder

---

**Ready to test!** 🎄
