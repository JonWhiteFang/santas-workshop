# TimeManager Usage Guide

**Last Updated**: November 6, 2025  
**Component**: TimeManager (Core System)  
**Status**: Production Ready

---

## Overview

The TimeManager is the core time and simulation system for Santa's Workshop Automation. It provides:
- Fixed-rate simulation ticks (configurable Hz)
- Calendar progression (days, months, seasons)
- Time speed control (pause, 1x, 2x, 5x, etc.)
- Event scheduling with save/load support
- Memory-safe event subscriptions

---

## Basic Usage

### Subscribing to Simulation Ticks

**CRITICAL**: Always unsubscribe in `OnDisable()` to prevent memory leaks!

```csharp
using SantasWorkshop.Core;
using UnityEngine;

public class MySystem : MonoBehaviour
{
    private void OnEnable()
    {
        TimeManager.OnSimulationTick += HandleSimulationTick;
    }

    private void OnDisable()
    {
        TimeManager.OnSimulationTick -= HandleSimulationTick;
    }

    private void HandleSimulationTick()
    {
        // Update your system at fixed intervals (default: 10 Hz)
        // This is called independently of frame rate
    }
}
```

### Subscribing to Other Events

```csharp
public class CalendarUI : MonoBehaviour
{
    private void OnEnable()
    {
        TimeManager.OnDayChanged += HandleDayChanged;
        TimeManager.OnSeasonalPhaseChanged += HandlePhaseChanged;
        TimeManager.OnTimeSpeedChanged += HandleSpeedChanged;
    }

    private void OnDisable()
    {
        TimeManager.OnDayChanged -= HandleDayChanged;
        TimeManager.OnSeasonalPhaseChanged -= HandlePhaseChanged;
        TimeManager.OnTimeSpeedChanged -= HandleSpeedChanged;
    }

    private void HandleDayChanged(int newDay)
    {
        Debug.Log($"Day changed to {newDay}");
    }

    private void HandlePhaseChanged(SeasonalPhase newPhase)
    {
        Debug.Log($"Entered {newPhase} phase");
    }

    private void HandleSpeedChanged(float newSpeed)
    {
        Debug.Log($"Time speed changed to {newSpeed}x");
    }
}
```

---

## Scheduling Events

### Simple Event Scheduling

```csharp
// Schedule event after 5 seconds of game time
var handle = TimeManager.Instance.ScheduleEvent(5f, () => 
{
    Debug.Log("5 seconds have passed!");
});

// Schedule event at specific day
var handle2 = TimeManager.Instance.ScheduleEventAtDay(100, () => 
{
    Debug.Log("Day 100 reached!");
});

// Cancel event
TimeManager.Instance.CancelScheduledEvent(handle);
```

### Event Scheduling with Save/Load Support

To enable events to be saved and restored:

**Step 1**: Register your event type (do this once at startup):

```csharp
public class MissionManager : MonoBehaviour
{
    private void Awake()
    {
        // Register event types for save/load support
        ScheduledEventFactory.RegisterEventType("MissionDeadline", () => () => 
        {
            CheckMissionDeadlines();
        });
        
        ScheduledEventFactory.RegisterEventType("MissionReward", () => () => 
        {
            GrantMissionReward();
        });
    }

    private void CheckMissionDeadlines()
    {
        Debug.Log("Checking mission deadlines...");
        // Your logic here
    }

    private void GrantMissionReward()
    {
        Debug.Log("Granting mission reward...");
        // Your logic here
    }
}
```

**Step 2**: Schedule events with the event type:

```csharp
// Schedule with event type for save/load support
var handle = TimeManager.Instance.ScheduleEvent(
    delaySeconds: 300f,
    eventType: "MissionDeadline",
    callback: () => CheckMissionDeadlines()
);

// Schedule at specific day with event type
var handle2 = TimeManager.Instance.ScheduleEventAtDay(
    day: 100,
    eventType: "MissionReward",
    callback: () => GrantMissionReward()
);
```

**Step 3**: Events will automatically be saved and restored!

---

## Time Speed Control

### Basic Time Control

```csharp
// Pause time
TimeManager.Instance.Pause();

// Resume time
TimeManager.Instance.Resume();

// Toggle pause
TimeManager.Instance.TogglePause();

// Set time speed (0-10x)
TimeManager.Instance.SetTimeSpeed(2f); // 2x speed
TimeManager.Instance.SetTimeSpeed(5f); // 5x speed
TimeManager.Instance.SetTimeSpeed(0.5f); // Half speed
```

### Checking Time State

```csharp
// Check if paused
if (TimeManager.Instance.IsPaused)
{
    Debug.Log("Time is paused");
}

// Get current time speed
float speed = TimeManager.Instance.TimeSpeed;
Debug.Log($"Current speed: {speed}x");

// Get scaled delta time (affected by time speed)
float scaledDelta = TimeManager.Instance.ScaledDeltaTime;

// Get unscaled delta time (not affected by time speed)
float unscaledDelta = TimeManager.Instance.UnscaledDeltaTime;
```

---

## Calendar System

### Accessing Calendar Information

```csharp
// Get current day (1-365)
int day = TimeManager.Instance.CurrentDay;

// Get current month (1-12)
int month = TimeManager.Instance.CurrentMonth;

// Get day of month (1-30 or 1-35 for December)
int dayOfMonth = TimeManager.Instance.DayOfMonth;

// Get current seasonal phase
SeasonalPhase phase = TimeManager.Instance.CurrentPhase;

// Seasonal phases:
// - EarlyYear (Days 1-90)
// - Production (Days 91-270)
// - PreChristmas (Days 271-330)
// - ChristmasRush (Days 331-365)
```

### Responding to Calendar Changes

```csharp
public class SeasonalSystem : MonoBehaviour
{
    private void OnEnable()
    {
        TimeManager.OnDayChanged += HandleDayChanged;
        TimeManager.OnSeasonalPhaseChanged += HandlePhaseChanged;
    }

    private void OnDisable()
    {
        TimeManager.OnDayChanged -= HandleDayChanged;
        TimeManager.OnSeasonalPhaseChanged -= HandlePhaseChanged;
    }

    private void HandleDayChanged(int newDay)
    {
        if (newDay == 1)
        {
            Debug.Log("New year started!");
        }
        else if (newDay == 365)
        {
            Debug.Log("Last day of the year!");
        }
    }

    private void HandlePhaseChanged(SeasonalPhase newPhase)
    {
        switch (newPhase)
        {
            case SeasonalPhase.EarlyYear:
                Debug.Log("Early year - planning phase");
                break;
            case SeasonalPhase.Production:
                Debug.Log("Production phase - ramp up manufacturing");
                break;
            case SeasonalPhase.PreChristmas:
                Debug.Log("Pre-Christmas - final preparations");
                break;
            case SeasonalPhase.ChristmasRush:
                Debug.Log("Christmas Rush - all hands on deck!");
                break;
        }
    }
}
```

---

## Save/Load System

### Saving Time State

```csharp
public class SaveSystem : MonoBehaviour
{
    public void SaveGame()
    {
        // Get time state
        TimeSaveData timeData = TimeManager.Instance.GetSaveData();
        
        // Serialize to JSON
        string json = JsonUtility.ToJson(timeData, true);
        
        // Save to file
        System.IO.File.WriteAllText("save.json", json);
        
        Debug.Log($"Saved: Day {timeData.currentDay}, {timeData.scheduledEvents.Length} events");
    }
}
```

### Loading Time State

```csharp
public class SaveSystem : MonoBehaviour
{
    public void LoadGame()
    {
        // Load from file
        string json = System.IO.File.ReadAllText("save.json");
        
        // Deserialize from JSON
        TimeSaveData timeData = JsonUtility.FromJson<TimeSaveData>(json);
        
        // Restore time state
        TimeManager.Instance.LoadSaveData(timeData);
        
        Debug.Log($"Loaded: Day {timeData.currentDay}");
    }
}
```

---

## Advanced Usage

### Custom Tick Rates

```csharp
// Change tick rate (default: 10 Hz)
TimeManager.Instance.TickRate = 20f; // 20 ticks per second

// Get ticks per second
int tps = TimeManager.Instance.TicksPerSecond;
Debug.Log($"Ticks per second: {tps}");
```

### Accessing Elapsed Time

```csharp
// Total game time (affected by time speed)
float gameTime = TimeManager.Instance.TotalGameTime;

// Total real time (not affected by time speed)
float realTime = TimeManager.Instance.TotalRealTime;

Debug.Log($"Game time: {gameTime}s, Real time: {realTime}s");
```

### Testing Support

```csharp
#if UNITY_EDITOR
public class TimeManagerTests
{
    [Test]
    public void TestTimeManager()
    {
        // Reset to clean state for testing
        TimeManager.Instance.ResetForTesting();
        
        // Run your tests...
        
        // Clean up
        TimeManager.Instance.ClearAllEventSubscriptions();
    }
}
#endif
```

---

## Best Practices

### 1. Always Unsubscribe from Events

```csharp
// ✅ GOOD: Unsubscribe in OnDisable
private void OnEnable()
{
    TimeManager.OnSimulationTick += HandleTick;
}

private void OnDisable()
{
    TimeManager.OnSimulationTick -= HandleTick;
}

// ❌ BAD: Never unsubscribing (memory leak!)
private void Start()
{
    TimeManager.OnSimulationTick += HandleTick;
}
```

### 2. Use Simulation Ticks for Game Logic

```csharp
// ✅ GOOD: Use simulation ticks for consistent timing
private void OnEnable()
{
    TimeManager.OnSimulationTick += UpdateMachine;
}

private void UpdateMachine()
{
    // Runs at fixed intervals (10 Hz by default)
    // Independent of frame rate
}

// ❌ BAD: Using Update() for game logic (frame-rate dependent)
private void Update()
{
    UpdateMachine(); // Inconsistent timing!
}
```

### 3. Register Event Types for Save/Load

```csharp
// ✅ GOOD: Register event types for save/load support
private void Awake()
{
    ScheduledEventFactory.RegisterEventType("MyEvent", () => MyCallback);
}

var handle = TimeManager.Instance.ScheduleEvent(10f, "MyEvent", MyCallback);

// ❌ BAD: Not registering event type (events lost on save/load)
var handle = TimeManager.Instance.ScheduleEvent(10f, MyCallback);
```

### 4. Handle Time Speed Changes

```csharp
// ✅ GOOD: Use ScaledDeltaTime for gameplay
private void Update()
{
    float delta = TimeManager.Instance.ScaledDeltaTime;
    transform.position += velocity * delta; // Affected by time speed
}

// ✅ GOOD: Use UnscaledDeltaTime for UI
private void Update()
{
    float delta = TimeManager.Instance.UnscaledDeltaTime;
    uiAnimation.Update(delta); // Not affected by time speed
}
```

### 5. Test with Different Time Speeds

```csharp
// Test your systems at different speeds
TimeManager.Instance.SetTimeSpeed(0.1f); // Slow motion
TimeManager.Instance.SetTimeSpeed(1f);   // Normal
TimeManager.Instance.SetTimeSpeed(5f);   // Fast forward
TimeManager.Instance.SetTimeSpeed(10f);  // Maximum speed
```

---

## Common Pitfalls

### 1. Forgetting to Unsubscribe (Memory Leak)

**Problem**: Destroyed objects remain in memory if still subscribed to events.

**Solution**: Always unsubscribe in `OnDisable()` or `OnDestroy()`.

### 2. Scheduling Events Without Save/Load Support

**Problem**: Events are lost when saving/loading the game.

**Solution**: Register event types with `ScheduledEventFactory` and use the event type parameter.

### 3. Using Update() Instead of Simulation Ticks

**Problem**: Frame-rate dependent logic, inconsistent timing.

**Solution**: Use `OnSimulationTick` for game logic that needs consistent timing.

### 4. Not Handling Time Speed Changes

**Problem**: Systems break when time speed changes.

**Solution**: Use `ScaledDeltaTime` for gameplay, `UnscaledDeltaTime` for UI.

### 5. Assuming Fixed Frame Rate

**Problem**: Logic breaks at different frame rates.

**Solution**: Always use delta time, never assume 60 FPS.

---

## Performance Considerations

### Event Scheduling Limits

- Maximum 100 events processed per frame
- Use Dictionary for O(1) event lookups
- Cancelled events are cleaned up automatically

### Tick Rate Guidelines

- Default: 10 Hz (good for most systems)
- Low: 5 Hz (for slow-updating systems)
- High: 20-30 Hz (for fast-paced gameplay)
- Maximum: 100 Hz (not recommended, high CPU usage)

### Memory Management

- Events use struct (stack allocation, no GC pressure)
- Dictionary storage for efficient lookups
- Automatic cleanup of completed/cancelled events

---

## Troubleshooting

### Events Not Triggering

**Check**:
1. Is time paused? (`TimeManager.Instance.IsPaused`)
2. Is the event cancelled? (Check handle validity)
3. Is the trigger time/day correct?
4. Are there errors in the callback?

### Memory Leaks

**Check**:
1. Are you unsubscribing from events in `OnDisable()`?
2. Are you using static event references correctly?
3. Use Unity Profiler to check for leaked objects

### Save/Load Issues

**Check**:
1. Are event types registered with `ScheduledEventFactory`?
2. Is the save data valid (not null, not corrupted)?
3. Are callbacks being reconstructed correctly?

### Performance Issues

**Check**:
1. Is tick rate too high? (Lower to 5-10 Hz)
2. Are there too many scheduled events? (Limit to <1000)
3. Are event callbacks expensive? (Profile with Unity Profiler)

---

## Examples

### Complete Machine System Example

```csharp
using SantasWorkshop.Core;
using UnityEngine;

public class Machine : MonoBehaviour
{
    [SerializeField] private float productionTime = 10f;
    private ScheduledEventHandle _productionHandle;

    private void OnEnable()
    {
        TimeManager.OnSimulationTick += UpdateMachine;
    }

    private void OnDisable()
    {
        TimeManager.OnSimulationTick -= UpdateMachine;
        
        // Cancel any pending production
        if (_productionHandle.IsValid)
        {
            TimeManager.Instance.CancelScheduledEvent(_productionHandle);
        }
    }

    public void StartProduction()
    {
        // Schedule production completion
        _productionHandle = TimeManager.Instance.ScheduleEvent(
            delaySeconds: productionTime,
            eventType: "MachineProduction",
            callback: OnProductionComplete
        );
        
        Debug.Log($"Production started, will complete in {productionTime}s");
    }

    private void OnProductionComplete()
    {
        Debug.Log("Production complete!");
        // Spawn product, update inventory, etc.
    }

    private void UpdateMachine()
    {
        // Update machine state at fixed intervals
        // This runs at 10 Hz by default
    }
}
```

### Complete Calendar System Example

```csharp
using SantasWorkshop.Core;
using UnityEngine;

public class CalendarSystem : MonoBehaviour
{
    private void Awake()
    {
        // Register seasonal event types
        ScheduledEventFactory.RegisterEventType("SeasonStart", () => OnSeasonStart);
        ScheduledEventFactory.RegisterEventType("SeasonEnd", () => OnSeasonEnd);
    }

    private void OnEnable()
    {
        TimeManager.OnDayChanged += HandleDayChanged;
        TimeManager.OnSeasonalPhaseChanged += HandlePhaseChanged;
    }

    private void OnDisable()
    {
        TimeManager.OnDayChanged -= HandleDayChanged;
        TimeManager.OnSeasonalPhaseChanged -= HandlePhaseChanged;
    }

    private void Start()
    {
        // Schedule events for specific days
        TimeManager.Instance.ScheduleEventAtDay(90, "SeasonEnd", OnSeasonStart);
        TimeManager.Instance.ScheduleEventAtDay(270, "SeasonEnd", OnSeasonEnd);
    }

    private void HandleDayChanged(int newDay)
    {
        Debug.Log($"Day {newDay} - Month {TimeManager.Instance.CurrentMonth}, Day {TimeManager.Instance.DayOfMonth}");
    }

    private void HandlePhaseChanged(SeasonalPhase newPhase)
    {
        Debug.Log($"Entered {newPhase} phase");
    }

    private void OnSeasonStart()
    {
        Debug.Log("Season started!");
    }

    private void OnSeasonEnd()
    {
        Debug.Log("Season ended!");
    }
}
```

---

## API Reference

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `CurrentDay` | `int` | Current day (1-365) |
| `CurrentMonth` | `int` | Current month (1-12) |
| `DayOfMonth` | `int` | Day of month (1-30 or 1-35) |
| `CurrentPhase` | `SeasonalPhase` | Current seasonal phase |
| `IsPaused` | `bool` | Whether time is paused |
| `TimeSpeed` | `float` | Time speed multiplier (0-10x) |
| `ScaledDeltaTime` | `float` | Delta time affected by speed |
| `UnscaledDeltaTime` | `float` | Delta time not affected by speed |
| `TotalGameTime` | `float` | Total game time in seconds |
| `TotalRealTime` | `float` | Total real time in seconds |
| `TickRate` | `float` | Ticks per second |
| `TicksPerSecond` | `int` | Rounded ticks per second |

### Methods

| Method | Description |
|--------|-------------|
| `SetTimeSpeed(float)` | Set time speed (0-10x) |
| `Pause()` | Pause time |
| `Resume()` | Resume time |
| `TogglePause()` | Toggle pause state |
| `ScheduleEvent(float, Action)` | Schedule event after delay |
| `ScheduleEvent(float, string, Action)` | Schedule event with type |
| `ScheduleEventAtDay(int, Action)` | Schedule event at day |
| `ScheduleEventAtDay(int, string, Action)` | Schedule event at day with type |
| `CancelScheduledEvent(handle)` | Cancel scheduled event |
| `GetSaveData()` | Get save data |
| `LoadSaveData(data)` | Load save data |
| `ClearAllEventSubscriptions()` | Clear all event subscriptions |

### Events

| Event | Parameters | Description |
|-------|------------|-------------|
| `OnSimulationTick` | `Action` | Fixed-rate simulation tick |
| `OnTimeSpeedChanged` | `Action<float>` | Time speed changed |
| `OnSeasonalPhaseChanged` | `Action<SeasonalPhase>` | Phase changed |
| `OnDayChanged` | `Action<int>` | Day changed |

---

**Last Updated**: November 6, 2025  
**Version**: 2.0 (with all critical fixes applied)  
**Status**: Production Ready
