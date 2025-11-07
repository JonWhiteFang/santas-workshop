# Phase 1 Verification Checklist

**Project**: Santa's Workshop Automation  
**Checkpoint**: Foundation Complete  
**Date Started**: _____________  
**Date Completed**: _____________  
**Verified By**: _____________

---

## Prerequisites

- [x] Unity 6.0 (6000.2.10f1) is open with SantasWorkshopAutomation project loaded
- [x] All Phase 1 specs (1.1-1.5) are marked as complete
- [x] No compilation errors visible in Console window

---

## Step 1: Verify Project Builds Without Errors

### 1.1 Check for Compilation Errors
- [x] Open Unity Console (`Window → General → Console` or `Ctrl+Shift+C`)
- [x] Click "Clear" button to start fresh
- [x] Click "Collapse" to group similar messages
- [x] Verify error counter shows **0 errors** (red icon)
- [x] **Result**: No compilation errors ✅

**If errors found**:
- [x] Read each error message
- [x] Double-click error to jump to problematic script
- [x] Fix error and recompile
- [x] Repeat until 0 errors

### 1.2 Run All Tests
- [ ] Open Test Runner (`Window → General → Test Runner` or `Ctrl+Alt+T`)
- [ ] Select "EditMode" tab
- [ ] Click "Run All" button
- [ ] Wait for all tests to complete
- [ ] **Result**: All tests pass (green checkmarks) ✅

**Expected Test Results**:
- [x] Resource Management System: 28/28 tests passing ✅
- [x] Grid & Placement System: All tests passing ✅
- [x] Machine Framework: 19/19 tests passing ✅
- [x] Time & Simulation Manager: 47/47 tests passing ✅

**Latest Test Run** (November 7, 2025):
- Total Tests: 47
- Passed: 47 (100%)
- Failed: 0
- Test Duration: ~0.04 seconds
- Test Results: `C:/Users/jono2/AppData/LocalLow/DefaultCompany/SantasWorkshopAutomation/TestResults.xml`

**If tests fail**:
- [ ] Click on failed test to see details
- [ ] Read assertion message
- [ ] Fix underlying issue
- [ ] Re-run tests

### 1.3 Perform a Test Build
- [ ] Go to `File → Build Settings`
- [ ] Verify "PC, Mac & Linux Standalone" is selected
- [ ] Verify "Target Platform" is "Windows"
- [ ] Click "Build" (not "Build And Run")
- [ ] Choose temporary folder: `Builds/Checkpoint1/`
- [ ] Wait for build to complete (2-5 minutes)
- [ ] **Result**: Build completes without errors ✅

**If build fails**:
- [ ] Check Console for build errors
- [ ] Fix errors (common: missing scenes, asset references)
- [ ] Rebuild

---

## Step 2: Verify Grid System Displays Correctly

### 2.1 Create Test Scene
- [ ] Create new scene (`File → New Scene`)
- [ ] Save scene (`File → Save As...`)
- [ ] Location: `Assets/_Project/Scenes/TestScenes/`
- [ ] Name: `Checkpoint1_FoundationTest.unity`
- [ ] Click "Save"

### 2.2 Add Grid Manager to Scene
- [ ] Create empty GameObject (`Right-click in Hierarchy → Create Empty`)
- [ ] Rename to "GridManager"
- [ ] Add GridManager component (`Add Component → Scripts → Core → Grid Manager`)
- [ ] Verify Inspector settings:
  - [ ] Grid Width: 100
  - [ ] Grid Height: 100
  - [ ] Cell Size: 1.0

### 2.3 Visualize the Grid
- [ ] Add GridVisualizer component to GridManager GameObject
- [ ] In Scene view, verify green grid lines are visible
- [ ] **Result**: Grid lines visible in Scene view ✅

**If grid not visible**:
- [ ] Check "Show Grid" is enabled in Inspector
- [ ] Verify Gizmos are enabled in Scene view (button in top-right)
- [ ] Check grid color alpha is not 0

### 2.4 Test Grid Coordinate Conversion
- [ ] Run grid coordinate tests in Test Runner
- [ ] Verify `WorldToGrid_ConvertsCorrectly` test passes
- [ ] Verify `GridToWorld_ConvertsCorrectly` test passes
- [ ] **Result**: Both coordinate conversion tests pass ✅

---

## Step 3: Verify Placement System with Validation

### 3.1 Add Placement Controller to Scene
- [ ] Create empty GameObject (`Right-click → Create Empty`)
- [ ] Rename to "PlacementController"
- [ ] Add PlacementController component
- [ ] Assign GridManager reference in Inspector
- [ ] Drag GridManager GameObject to Grid Manager field

### 3.2 Create Test Placeable Object
- [ ] Create cube (`Right-click in Hierarchy → 3D Object → Cube`)
- [ ] Rename to "TestBuilding"
- [ ] Position at (0, 0, 0)
- [ ] Scale to (0.9, 0.5, 0.9)
- [ ] Add TestPlaceable component (create script if needed)
- [ ] Create prefab: Drag to `Assets/_Project/Prefabs/`
- [ ] Name prefab "TestBuilding.prefab"
- [ ] Delete TestBuilding from Hierarchy

### 3.3 Test Placement Validation
- [ ] Enter Play Mode (`Ctrl+P`)
- [ ] Test placement at position (5, 0, 5)
- [ ] Verify first placement succeeds (green color, "Can place: true")
- [ ] Test placement at same location again
- [ ] Verify second placement fails (red color, "Can place: false")
- [ ] Check Console for appropriate messages
- [ ] **Result**: Placement validation works correctly ✅

### 3.4 Test Ghost Preview (if implemented)
- [ ] Move mouse over grid
- [ ] Verify ghost preview follows mouse
- [ ] Verify color changes based on validity (green = valid, red = invalid)
- [ ] **Result**: Ghost preview works correctly ✅ / ⏳ Not yet implemented

---

## Step 4: Verify Resources Can Be Created and Stored

### 4.1 Add Resource Manager to Scene
- [ ] Create empty GameObject (`Right-click → Create Empty`)
- [ ] Rename to "ResourceManager"
- [ ] Add ResourceManager component

### 4.2 Test Resource Operations
- [ ] Enter Play Mode
- [ ] Verify can add 100 Iron Ore
- [ ] Check amount is 100
- [ ] Verify can remove 50 Iron Ore
- [ ] Check amount is now 50
- [ ] Try to remove 100 Iron Ore (should fail)
- [ ] Verify amount is still 50
- [ ] Check Console for test messages
- [ ] **Result**: All resource operations work correctly ✅

**Expected Console Output**:
- [ ] "Added 100 Iron Ore"
- [ ] "Current Iron Ore: 100 (Expected: 100)"
- [ ] "Removed 50 Iron Ore: True"
- [ ] "Current Iron Ore: 50 (Expected: 50)"
- [ ] "Tried to remove 100 Iron Ore (only 50 available): False"
- [ ] "Current Iron Ore: 50 (Expected: 50)"
- [ ] "All Resource Tests Passed!"

### 4.3 Test Resource Events (if implemented)
- [ ] Subscribe to resource change events
- [ ] Add/remove resources
- [ ] Verify events fire with correct parameters
- [ ] Check Console for event messages
- [ ] **Result**: Resource events work correctly ✅ / ⏳ Not yet implemented

---

## Step 5: Verify Time Controls Work

### 5.1 Add Time Manager to Scene
- [ ] Create empty GameObject (`Right-click → Create Empty`)
- [ ] Rename to "TimeManager"
- [ ] Add TimeManager component

### 5.2 Create Time Control UI
- [ ] Create Canvas (`Right-click in Hierarchy → UI → Canvas`)
- [ ] Rename to "TimeControlUI"
- [ ] Set Canvas Scaler to "Scale With Screen Size"
- [ ] Add Panel (rename to "TimeControlPanel")
- [ ] Position panel at bottom-right of screen
- [ ] Add 4 buttons: Pause, 1x, 2x, 5x
- [ ] Add 2 text labels: Day display, Speed display
- [ ] Add TimeControlUI component to Canvas
- [ ] Assign all button and text references in Inspector

### 5.3 Test Time Controls
- [ ] Enter Play Mode
- [ ] Click "Pause" button
  - [ ] Verify time stops
  - [ ] Verify speed shows "0x"
- [ ] Click "1x" button
  - [ ] Verify time resumes at normal speed
  - [ ] Verify speed shows "1x"
- [ ] Click "2x" button
  - [ ] Verify time speeds up
  - [ ] Verify speed shows "2x"
  - [ ] Verify day counter advances faster
- [ ] Click "5x" button
  - [ ] Verify time speeds up more
  - [ ] Verify speed shows "5x"
- [ ] Watch day counter advance over time
- [ ] Verify no errors in Console
- [ ] **Result**: All time controls work correctly ✅

---

## Step 6: Verify Basic Machine Framework Can Be Instantiated

### 6.1 Create Test Machine
- [ ] Create cube (`Right-click in Hierarchy → 3D Object → Cube`)
- [ ] Rename to "TestMachine"
- [ ] Add TestMachine component (create script if needed)
- [ ] Position at (0, 0, 0)

### 6.2 Test Machine Instantiation
- [ ] Enter Play Mode
- [ ] Check Console for lifecycle messages:
  - [ ] "TestMachine: Awake called"
  - [ ] "TestMachine: InitializeStates called"
  - [ ] "TestMachine: Start called"
  - [ ] "TestMachine: Idle state"
  - [ ] "TestMachine: Processing state"
- [ ] Verify all lifecycle methods called in correct order
- [ ] Verify state transitions work
- [ ] Verify no errors or exceptions
- [ ] **Result**: Machine framework instantiates correctly ✅

### 6.3 Test Machine with Ports (if implemented)
- [ ] Verify input ports are created
- [ ] Verify output ports are created
- [ ] Check Console for port count messages
- [ ] **Result**: Machine ports work correctly ✅ / ⏳ Not yet implemented

---

## Step 7: Create Complete Test Scene

### 7.1 Scene Setup
- [ ] Open `Checkpoint1_FoundationTest.unity` scene
- [ ] Verify scene contains:
  - [ ] GridManager with GridVisualizer
  - [ ] PlacementController
  - [ ] ResourceManager
  - [ ] TimeManager
  - [ ] TimeControlUI Canvas
  - [ ] Main Camera (positioned to see grid)
  - [ ] Directional Light

### 7.2 Add Test Objects
- [ ] Place 3-5 TestBuilding prefabs on grid at different positions
- [ ] Place 2-3 TestMachine objects on grid
- [ ] Arrange camera to see all objects

### 7.3 Final Verification Checklist

**Enter Play Mode and verify**:

#### Grid System
- [ ] Grid lines visible in Scene view
- [ ] Grid coordinates convert correctly
- [ ] No errors related to grid

#### Placement System
- [ ] Can place test buildings on grid
- [ ] Placement validation works (can't place on occupied cells)
- [ ] Ghost preview shows correct position (if implemented)
- [ ] Placed objects snap to grid correctly

#### Resource System
- [ ] Can add resources
- [ ] Can remove resources
- [ ] Resource amounts tracked correctly
- [ ] Can't remove more than available
- [ ] Events fire when resources change (if implemented)

#### Time System
- [ ] Time controls UI visible and functional
- [ ] Pause button stops time
- [ ] Speed buttons change time scale
- [ ] Day counter advances
- [ ] Speed display updates correctly

#### Machine Framework
- [ ] Test machines instantiate without errors
- [ ] Machine lifecycle methods called correctly
- [ ] State machine transitions work
- [ ] Ports created correctly (if implemented)

#### General
- [ ] No errors in Console
- [ ] No warnings (or only expected warnings)
- [ ] Scene runs smoothly (30+ FPS)
- [ ] Can play for 2-3 minutes without issues

---

## Step 8: Document Results

### 8.1 Create Checkpoint Report
- [ ] Create file: `Documentation/Checkpoints/Checkpoint1_Report.md`
- [ ] Fill in test results summary table
- [ ] Document detailed test results for each system
- [ ] List any issues found
- [ ] Note performance metrics (FPS, memory, load time)
- [ ] Add recommendations for improvements
- [ ] Write conclusion and approval status

### 8.2 Take Screenshots
- [ ] Screenshot: Test scene in Scene view (showing grid)
- [ ] Screenshot: Test scene in Game view (showing UI)
- [ ] Screenshot: Test Runner results (all tests passing)
- [ ] Screenshot: Console (showing no errors)
- [ ] Screenshot: Inspector views of key components
- [ ] Save all screenshots to: `Documentation/Checkpoints/Checkpoint1_Screenshots/`

### 8.3 Save Test Scene
- [ ] Save test scene (`File → Save`)
- [ ] Verify scene location: `Assets/_Project/Scenes/TestScenes/Checkpoint1_FoundationTest.unity`
- [ ] Commit to version control:
  ```powershell
  git add Assets/_Project/Scenes/TestScenes/Checkpoint1_FoundationTest.unity
  git add Documentation/Checkpoints/Checkpoint1_Report.md
  git add Documentation/Checkpoints/Checkpoint1_Screenshots/
  git commit -m "checkpoint: Complete Checkpoint #1 - Foundation verification"
  git push
  ```

---

## Step 9: Final Decision

### 9.1 Review Final Checklist
- [ ] All Phase 1 specs (1.1-1.5) are complete
- [ ] Project builds without errors
- [ ] All tests pass
- [ ] Grid system works correctly
- [ ] Placement system works correctly
- [ ] Resource system works correctly
- [ ] Time system works correctly
- [ ] Machine framework works correctly
- [ ] Test scene created and saved
- [ ] Checkpoint report written
- [ ] Screenshots taken
- [ ] No critical or high-severity bugs
- [ ] Performance is acceptable (30+ FPS)

### 9.2 Make Decision

**If ALL items above are checked**:
- [ ] ✅ **APPROVED FOR PHASE 2**
- [ ] Update SPEC_ORDER.md to mark Checkpoint #1 as complete
- [ ] Update tech.md with completion date
- [ ] Update README.md if needed
- [ ] Begin Phase 2 specs (2.1-2.4)

**If ANY items are NOT checked**:
- [ ] ❌ **NOT READY FOR PHASE 2**
- [ ] Document which items failed in checkpoint report
- [ ] Create tasks to fix issues
- [ ] Schedule re-run of checkpoint when fixes are complete

---

## Troubleshooting Reference

### Grid not visible in Scene view
- [ ] Ensure GridVisualizer component is attached
- [ ] Check "Show Grid" is enabled in Inspector
- [ ] Verify Gizmos are enabled in Scene view
- [ ] Check grid color alpha is not 0

### Placement validation always fails
- [ ] Verify GridManager is properly initialized
- [ ] Check grid bounds (width/height)
- [ ] Ensure cell size is correct (1.0)
- [ ] Debug log the grid position being checked

### Resources not adding/removing
- [ ] Verify ResourceManager singleton is initialized
- [ ] Check ResourceType enum matches resource definitions
- [ ] Ensure capacity is not 0
- [ ] Debug log resource operations

### Time controls not working
- [ ] Verify TimeManager singleton is initialized
- [ ] Check Time.timeScale is being set correctly
- [ ] Ensure buttons are hooked up to correct methods
- [ ] Debug log time scale changes

### Machines not instantiating
- [ ] Check MachineBase class is not abstract (or use concrete subclass)
- [ ] Verify all required components are present
- [ ] Check for errors in Awake/Start methods
- [ ] Ensure prefab references are assigned

---

## Completion Summary

**Total Items**: 150+  
**Items Completed**: _____  
**Completion Percentage**: _____%

**Estimated Time**: 2-4 hours

**Status**: 
- [ ] ✅ COMPLETE - Ready for Phase 2
- [ ] ⏳ IN PROGRESS
- [ ] ❌ BLOCKED - Issues need resolution

**Notes**:
_____________________________________________________________________________
_____________________________________________________________________________
_____________________________________________________________________________
_____________________________________________________________________________

---

**Verified By**: _____________  
**Date**: _____________  
**Signature**: _____________

---

**Next Steps**: Once approved, begin Phase 2 development starting with Spec 2.1 (Conveyor Belt System)

🎄 **Good luck with your verification!** 🎄
