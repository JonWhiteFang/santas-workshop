# TimeManager Integration Test Scene

**Purpose**: Integration testing and demonstration of TimeManager functionality

## Overview

This test scene provides a complete integration test environment for the TimeManager system. It includes:

- **Visual UI** showing current time state (day, month, phase, time speed)
- **Interactive controls** for pause/resume and time speed changes
- **Event scheduling** demonstration with visual feedback
- **Automated test scenarios** that run on scene start
- **Event logging** to track all TimeManager events

## Scene Setup Instructions

### 1. Create the Test Scene

1. Create a new scene: `File → New Scene`
2. Save as: `Assets/_Project/Scenes/TestScenes/TestScene_TimeManager.unity`

### 2. Add TimeManager

1. Create empty GameObject: `GameObject → Create Empty`
2. Rename to: `TimeManager`
3. Add component: `TimeManager` (from SantasWorkshop.Core)
4. Configure settings:
   - Tick Rate: 10 (default)
   - Seconds Per Day: 60 (1 minute per day for testing)
   - Starting Day: 1
   - Initial Time Speed: 1
   - Start Paused: false

### 3. Add Test Scenario

1. Create empty GameObject: `GameObject → Create Empty`
2. Rename to: `TestScenario`
3. Add component: `TimeManagerTestScenario` (from SantasWorkshop.Testing)
4. Configure settings:
   - Auto Start Tests: true
   - Log Verbose: true
   - Enable all test scenarios

### 4. Create UI Canvas

1. Create UI Canvas: `GameObject → UI → Canvas`
2. Set Canvas Scaler to "Scale With Screen Size"
3. Reference Resolution: 1920x1080

### 5. Add Display Panel

Create a panel for displaying time information:

1. Create Panel: `Right-click Canvas → UI → Panel`
2. Rename to: `DisplayPanel`
3. Position: Top-left corner
4. Size: 400x300

Add TextMeshPro text elements (children of DisplayPanel):
- `DayText` - "Day: 1"
- `MonthText` - "Month: 1 (Day 1)"
- `PhaseText` - "Phase: EarlyYear"
- `TimeSpeedText` - "Speed: 1.0x"
- `GameTimeText` - "Game Time: 00:00:00"
- `RealTimeText` - "Real Time: 00:00:00"
- `PauseStatusText` - "RUNNING" (green color)
- `TickIndicator` - "●" (yellow, will flash on ticks)

### 6. Add Control Panel

Create a panel for control buttons:

1. Create Panel: `Right-click Canvas → UI → Panel`
2. Rename to: `ControlPanel`
3. Position: Bottom-left corner
4. Size: 400x200

Add buttons (children of ControlPanel):
- `PauseButton` - "Pause"
- `Speed1xButton` - "1x Speed"
- `Speed2xButton` - "2x Speed"
- `Speed5xButton` - "5x Speed"
- `ScheduleEventButton` - "Schedule Random Event"

### 7. Add Event Log Panel

Create a panel for event logging:

1. Create Panel: `Right-click Canvas → UI → Panel`
2. Rename to: `EventLogPanel`
3. Position: Right side of screen
4. Size: 600x800

Add components:
- Add `ScrollRect` component
- Create child `Viewport` → `Content` → `EventLogText` (TextMeshPro)
- Configure ScrollRect to scroll vertically

### 8. Connect UI to TimeManagerTestUI

1. Create empty GameObject: `GameObject → Create Empty`
2. Rename to: `TestUI`
3. Add component: `TimeManagerTestUI` (from SantasWorkshop.Testing)
4. Drag and drop UI elements to corresponding fields:
   - Display Elements: All text fields from DisplayPanel
   - Control Buttons: All buttons from ControlPanel
   - Event Log: EventLogText and EventLogScrollRect

## Using the Test Scene

### Running Tests

1. **Play the scene**: Press Play button in Unity Editor
2. **Automatic tests**: Test scenarios will run automatically if `Auto Start Tests` is enabled
3. **Manual tests**: Use buttons in the UI to interact with TimeManager

### UI Controls

- **Pause/Resume**: Toggle time progression
- **1x/2x/5x Speed**: Change time speed multiplier
- **Schedule Random Event**: Schedule an event with random delay (1-10 seconds)

### Observing Behavior

Watch for:
- **Day counter** incrementing (every 60 seconds at 1x speed)
- **Month and day-of-month** calculations
- **Seasonal phase** transitions at days 90, 270, 330
- **Tick indicator** flashing yellow (10 times per second)
- **Event log** showing all TimeManager events
- **Scheduled events** triggering at correct times

### Test Scenarios

The `TimeManagerTestScenario` component runs these tests automatically:

1. **Basic Scheduling**: Events at 1s, 2s, 5s, 10s delays
2. **Day-Based Events**: Events scheduled for specific days
3. **Seasonal Phases**: Tracks phase changes
4. **Event Cancellation**: Tests cancelling scheduled events

### Manual Testing

Use the context menu on `TimeManagerTestScenario`:
- Right-click component → "Run All Tests"
- Right-click component → "Test Time Speed Changes"
- Right-click component → "Test Pause/Resume"
- Right-click component → "Stress Test: Schedule 100 Events"

## Expected Behavior

### Normal Operation

- Time progresses smoothly
- Day counter increments every 60 seconds (at 1x speed)
- Tick indicator flashes 10 times per second
- Events trigger at scheduled times
- UI updates every frame

### Time Speed Changes

- 2x speed: Day increments every 30 seconds
- 5x speed: Day increments every 12 seconds
- All scheduled events respect time speed

### Pause/Resume

- When paused:
  - Day counter stops
  - Tick indicator stops flashing
  - Scheduled events don't trigger
  - Real time continues (UI still updates)
- When resumed:
  - Everything continues from where it stopped

### Seasonal Phase Transitions

- Day 1-90: EarlyYear
- Day 91-270: Production
- Day 271-330: PreChristmas
- Day 331-365: ChristmasRush

## Performance Monitoring

Watch the Console for performance logs:
- Tick rate accuracy
- Event processing times
- Memory usage (if verbose logging enabled)

## Troubleshooting

### TimeManager not found

- Ensure TimeManager GameObject exists in scene
- Ensure TimeManager component is attached
- Check that TimeManager.Instance is not null

### UI not updating

- Ensure TimeManagerTestUI component is attached
- Check that all UI references are assigned
- Verify Canvas is set to Screen Space - Overlay

### Events not triggering

- Check Console for errors
- Verify time is not paused
- Ensure time speed is > 0
- Check that events are being scheduled (see event log)

### Tick indicator not flashing

- Verify tick rate is set correctly (default: 10 Hz)
- Check that time is not paused
- Ensure OnSimulationTick event is subscribed

## Performance Targets

The test scene should maintain:
- **60 FPS** with all systems running
- **<1ms** per frame for TimeManager updates
- **100+ scheduled events** without performance degradation
- **Accurate tick timing** (±5% deviation)

## Cleanup

When done testing:
- Stop Play mode
- Save scene if you made changes
- Exclude test scene from builds (File → Build Settings → Scenes)

## Related Documentation

- TimeManager API: `Assets/_Project/Scripts/Core/TimeManager.cs`
- Usage Guide: `Documentation/Systems/TimeManager_Usage_Guide.md`
- Performance Tests: `Assets/Tests/EditMode/Core/TimeManagerPerformanceTests.cs`
- Unit Tests: `Assets/Tests/EditMode/Core/TimeManager*Tests.cs`

---

**Last Updated**: November 6, 2025  
**Version**: 1.0
