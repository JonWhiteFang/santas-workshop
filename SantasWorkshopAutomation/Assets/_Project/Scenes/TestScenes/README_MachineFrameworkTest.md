# Machine Framework Test Scene

**Scene**: `MachineFrameworkTest.unity`  
**Purpose**: Manual testing environment for the Machine Framework system  
**Created**: November 5, 2025

## Overview

This test scene provides a complete environment for manually testing the Machine Framework implementation. It includes all necessary managers and test machine instances configured for immediate testing.

## Scene Contents

### Core Managers

1. **ResourceManager**
   - Singleton instance managing all game resources
   - Automatically initializes resource database from `Resources/ResourceDefinitions/`
   - Tracks resource counts and capacities
   - Location: GameObject "ResourceManager"

2. **GridManager**
   - Singleton instance managing grid operations
   - Grid size: 20x20 cells
   - Cell size: 1 unit
   - Grid visualization enabled
   - Location: GameObject "GridManager"

### Test Machines

1. **TestExtractor_1**
   - Type: Extractor machine (resource generation)
   - Position: Grid (0, 0) - World (0.5, 0, 0.5)
   - Visual: Blue cube
   - Purpose: Test resource extraction and output buffer functionality

2. **TestProcessor_1**
   - Type: Processor machine (resource transformation)
   - Position: Grid (3, 0) - World (3.5, 0, 0.5)
   - Visual: Red cube
   - Purpose: Test recipe processing, input/output buffers, and state machine

### Scene Setup

- **Camera**: Positioned at (5, 10, -5) looking down at the grid
- **Lighting**: Directional light for clear visibility
- **Grid**: Visible grid lines for placement reference

## How to Use

### Opening the Scene

1. In Unity Editor, navigate to:
   ```
   Assets/_Project/Scenes/TestScenes/MachineFrameworkTest.unity
   ```
2. Double-click to open the scene

### Manual Testing Workflow

#### 1. Basic Initialization Test

```
1. Enter Play Mode
2. Check Console for initialization messages:
   - "ResourceManager initialized with X resources"
   - "GridManager initialized: 20x20 grid with cell size 1"
3. Verify no errors appear
```

#### 2. Machine State Testing

```
1. Select TestExtractor_1 in Hierarchy
2. In Inspector, observe:
   - Current State (should be Idle or WaitingForInput)
   - Processing Progress
   - Power status
3. Watch state transitions in Console
```

#### 3. Resource Flow Testing

```
1. Add resources to TestExtractor_1's output buffer:
   - Use ResourceManager.AddResource() in code
   - Or create a test UI button
2. Verify resources appear in output buffer
3. Check if machine transitions to WaitingForOutput when buffer is full
```

#### 4. Recipe Processing Testing

```
1. Assign a Recipe to TestProcessor_1:
   - Create a test Recipe ScriptableObject
   - Assign via Inspector
2. Add required input resources to input buffer
3. Observe processing:
   - State transitions to Processing
   - Progress increases over time
   - Outputs produced when complete
```

#### 5. Power Management Testing

```
1. Select a machine
2. Call SetPowered(false) via Inspector or code
3. Verify:
   - Machine transitions to NoPower state
   - Processing pauses (progress preserved)
4. Call SetPowered(true)
5. Verify:
   - Machine returns to previous state
   - Processing resumes from saved progress
```

#### 6. Grid Integration Testing

```
1. Verify machines occupy correct grid cells
2. Try to place another machine on occupied cells (should fail)
3. Move a machine and verify old cells are freed
4. Check grid visualization shows occupied cells
```

### Testing Checklist

- [ ] ResourceManager initializes successfully
- [ ] GridManager initializes successfully
- [ ] Test machines initialize without errors
- [ ] Machines generate unique IDs
- [ ] State machine transitions work correctly
- [ ] Input buffers accept resources
- [ ] Output buffers store produced resources
- [ ] Recipe processing completes successfully
- [ ] Power loss pauses processing
- [ ] Power restore resumes processing
- [ ] Grid cells are occupied correctly
- [ ] Grid cells are freed on machine destruction
- [ ] Events fire correctly (OnStateChanged, etc.)

## Extending the Test Scene

### Adding More Test Machines

1. Duplicate an existing test machine GameObject
2. Rename appropriately (e.g., "TestExtractor_2")
3. Move to a different grid position
4. Assign MachineData ScriptableObject if available

### Creating Test Recipes

1. Create a new Recipe ScriptableObject:
   ```
   Assets → Create → Santa → Recipe
   ```
2. Configure inputs and outputs
3. Set processing time and power consumption
4. Assign to a test machine

### Creating Test MachineData

1. Create a new MachineData ScriptableObject:
   ```
   Assets → Create → Santa → Machine Data
   ```
2. Configure machine properties
3. Assign to a test machine's machineData field

## Debugging Tips

### Console Messages

Watch for these key messages:
- `"Machine {id} transitioned from {old} to {new}"` - State changes
- `"Machine {id} entered Processing state"` - Processing started
- `"Machine {id} has no MachineData assigned!"` - Configuration error

### Inspector Debugging

While in Play Mode, select a machine and watch:
- `CurrentState` property
- `ProcessingProgress` (0.0 to 1.0)
- `EstimatedTimeRemaining` (seconds)
- `IsPowered` status

### Common Issues

**Issue**: Machines don't initialize
- **Solution**: Check MachineData is assigned in Inspector
- **Solution**: Verify ResourceManager and GridManager are in scene

**Issue**: Processing doesn't start
- **Solution**: Check if recipe is assigned
- **Solution**: Verify input resources are available
- **Solution**: Check if machine has power (IsPowered = true)

**Issue**: State doesn't change
- **Solution**: Ensure machine is enabled (isEnabled = true)
- **Solution**: Check Update() is being called (not paused)

## Performance Testing

### Frame Rate Monitoring

1. Open Profiler: `Window → Analysis → Profiler`
2. Enter Play Mode
3. Monitor:
   - CPU usage for machine updates
   - Memory allocations
   - Frame time (should be < 16ms for 60 FPS)

### Stress Testing

1. Duplicate test machines to create 10-20 instances
2. Spread across the grid
3. Assign recipes and start processing
4. Monitor performance in Profiler
5. Target: < 0.1ms per machine per frame

## Next Steps

After manual testing is complete:

1. **Create Unit Tests**: Convert manual tests to automated unit tests
2. **Create Integration Tests**: Test full production cycles
3. **Performance Tests**: Automated tests for 100+ machines
4. **Visual Feedback**: Add particle effects and animations
5. **UI Integration**: Create debug UI for runtime testing

## Related Documentation

- **Design Document**: `.kiro/specs/1.4-machine-framework/design.md`
- **Requirements**: `.kiro/specs/1.4-machine-framework/requirements.md`
- **Tasks**: `.kiro/specs/1.4-machine-framework/tasks.md`
- **Machine Base Class**: `Assets/_Project/Scripts/Machines/MachineBase.cs`
- **Test Machines**: `Assets/_Project/Scripts/Machines/TestExtractor.cs`, `TestProcessor.cs`

## Notes

- This scene is for development/testing only - exclude from release builds
- Grid visualization can be toggled via GridManager.SetGridVisualizationEnabled()
- Machines use placeholder cube visuals - replace with actual models later
- Recipe and MachineData ScriptableObjects need to be created separately

---

**Last Updated**: November 5, 2025  
**Status**: Ready for manual testing  
**Requirements**: All Machine Framework tasks 1-22 must be completed
