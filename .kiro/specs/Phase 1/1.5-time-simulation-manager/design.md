# Design Document - Time & Simulation Manager

## Overview

The Time & Simulation Manager is a singleton MonoBehaviour that serves as the central time authority for Santa's Workshop Automation. It manages the in-game calendar (365-day yearly cycle), provides time speed controls (pause, 1x, 2x, 5x), drives a fixed-interval simulation tick for all game systems, and handles event scheduling for time-based gameplay mechanics.

The system is designed to be lightweight, performant, and easy to integrate with other game systems through events and public APIs.

## Architecture

### System Components

```
TimeManager (Singleton MonoBehaviour)
├── Calendar System
│   ├── Day Counter (1-365)
│   ├── Month/Day Calculation
│   └── Seasonal Phase Tracking
├── Time Speed Control
│   ├── Pause State
│   ├── Speed Multipliers (1x, 2x, 5x)
│   └── Speed Change Events
├── Simulation Tick
│   ├── Fixed Tick Rate (10 Hz default)
│   ├── Delta Time Accumulation
│   └── Tick Event Broadcasting
├── Event Scheduler
│   ├── Scheduled Event Queue
│   ├── Event Execution
│   └── Event Cancellation
└── Persistence
    ├── Save Data Serialization
    └── Load Data Restoration
```

### Integration Points

- **Machine Framework**: Subscribes to simulation ticks for production updates
- **Power Grid**: Uses simulation ticks for power consumption calculations
- **Research System**: Schedules research completion events
- **Mission System**: Tracks time-based objectives and deadlines
- **Seasonal Cycle**: Responds to seasonal phase changes
- **Save/Load System**: Serializes and restores time state

## Components and Interfaces

### TimeManager Class

```csharp
namespace SantasWorkshop.Core
{
    public class TimeManager : MonoBehaviour
    {
        // Singleton
        public static TimeManager Instance { get; private set; }
        
        // Calendar Properties
        public int CurrentDay { get; private set; }
        public int CurrentMonth { get; private set; }
        public int DayOfMonth { get; private set; }
        public SeasonalPhase CurrentPhase { get; private set; }
        
        // Time Speed Properties
        public bool IsPaused { get; private set; }
        public float TimeSpeed { get; private set; }
        public float ScaledDeltaTime { get; private set; }
        public float UnscaledDeltaTime { get; private set; }
        
        // Elapsed Time
        public float TotalGameTime { get; private set; }
        public float TotalRealTime { get; private set; }
        
        // Simulation Tick
        public float TickRate { get; set; }
        public int TicksPerSecond { get; private set; }
        
        // Events
        public static event Action OnSimulationTick;
        public static event Action<float> OnTimeSpeedChanged;
        public static event Action<SeasonalPhase> OnSeasonalPhaseChanged;
        public static event Action<int> OnDayChanged;
        
        // Public Methods
        public void SetTimeSpeed(float speed);
        public void Pause();
        public void Resume();
        public void TogglePause();
        
        public ScheduledEventHandle ScheduleEvent(float delaySeconds, Action callback);
        public ScheduledEventHandle ScheduleEventAtDay(int day, Action callback);
        public void CancelScheduledEvent(ScheduledEventHandle handle);
        
        public TimeSaveData GetSaveData();
        public void LoadSaveData(TimeSaveData data);
    }
}
```

### SeasonalPhase Enum

```csharp
public enum SeasonalPhase
{
    EarlyYear,      // Days 1-90: Slow production, planning phase
    Production,     // Days 91-270: Normal production
    PreChristmas,   // Days 271-330: Ramping up production
    ChristmasRush,  // Days 331-365: High demand, time pressure
}
```

### ScheduledEvent Structure

```csharp
public class ScheduledEvent
{
    public int EventId { get; set; }
    public float TriggerTime { get; set; }
    public int? TriggerDay { get; set; }
    public Action Callback { get; set; }
    public bool IsCancelled { get; set; }
}

public struct ScheduledEventHandle
{
    public int EventId { get; internal set; }
    public bool IsValid => EventId > 0;
}
```

### TimeSaveData Structure

```csharp
[System.Serializable]
public class TimeSaveData
{
    public int currentDay;
    public float totalGameTime;
    public float totalRealTime;
    public float timeSpeed;
    public bool isPaused;
    public ScheduledEventSaveData[] scheduledEvents;
}

[System.Serializable]
public struct ScheduledEventSaveData
{
    public int eventId;
    public float triggerTime;
    public int triggerDay;
    public string eventType; // For deserializing callbacks
}
```

## Data Models

### Calendar System

The calendar uses a simple day counter (1-365) and calculates month/day on demand:

```
Month Lengths (30-day months for simplicity):
- January: Days 1-30
- February: Days 31-60
- March: Days 61-90
- April: Days 91-120
- May: Days 121-150
- June: Days 151-180
- July: Days 181-210
- August: Days 211-240
- September: Days 241-270
- October: Days 271-300
- November: Days 301-330
- December: Days 331-365 (35 days for Christmas Rush)
```

### Seasonal Phase Ranges

```
EarlyYear: Days 1-90 (January-March)
Production: Days 91-270 (April-September)
PreChristmas: Days 271-330 (October-November)
ChristmasRush: Days 331-365 (December)
```

### Time Speed Multipliers

```
Paused: 0x (time stopped)
Normal: 1x (1 game second = 1 real second)
Fast: 2x (1 game second = 0.5 real seconds)
VeryFast: 5x (1 game second = 0.2 real seconds)
```

## Error Handling

### Invalid Time Speed

```csharp
public void SetTimeSpeed(float speed)
{
    if (speed < 0f)
    {
        Debug.LogWarning($"Invalid time speed {speed}. Must be >= 0. Using 1x.");
        speed = 1f;
    }
    
    if (speed > 10f)
    {
        Debug.LogWarning($"Time speed {speed} exceeds maximum. Clamping to 10x.");
        speed = 10f;
    }
    
    TimeSpeed = speed;
    OnTimeSpeedChanged?.Invoke(speed);
}
```

### Corrupted Save Data

```csharp
public void LoadSaveData(TimeSaveData data)
{
    if (data == null)
    {
        Debug.LogError("Cannot load null save data. Using default values.");
        ResetToDefaults();
        return;
    }
    
    // Validate day range
    if (data.currentDay < 1 || data.currentDay > 365)
    {
        Debug.LogWarning($"Invalid day {data.currentDay} in save data. Resetting to day 1.");
        data.currentDay = 1;
    }
    
    // Validate time values
    if (data.totalGameTime < 0f || data.totalRealTime < 0f)
    {
        Debug.LogWarning("Negative time values in save data. Resetting to 0.");
        data.totalGameTime = Mathf.Max(0f, data.totalGameTime);
        data.totalRealTime = Mathf.Max(0f, data.totalRealTime);
    }
    
    // Apply validated data
    CurrentDay = data.currentDay;
    TotalGameTime = data.totalGameTime;
    TotalRealTime = data.totalRealTime;
    SetTimeSpeed(data.timeSpeed);
    
    if (data.isPaused)
        Pause();
    else
        Resume();
}
```

### Event Scheduling Errors

```csharp
public ScheduledEventHandle ScheduleEvent(float delaySeconds, Action callback)
{
    if (callback == null)
    {
        Debug.LogError("Cannot schedule event with null callback.");
        return new ScheduledEventHandle { EventId = -1 };
    }
    
    if (delaySeconds < 0f)
    {
        Debug.LogWarning($"Negative delay {delaySeconds}. Scheduling for immediate execution.");
        delaySeconds = 0f;
    }
    
    // Create and queue event
    var evt = CreateScheduledEvent(TotalGameTime + delaySeconds, callback);
    _scheduledEvents.Add(evt);
    
    return new ScheduledEventHandle { EventId = evt.EventId };
}
```

## Testing Strategy

### Unit Tests

**Calendar System Tests**:
- Test day counter increments correctly
- Test month/day calculation for all 365 days
- Test seasonal phase transitions at correct days
- Test year rollover (day 365 → day 1)

**Time Speed Tests**:
- Test pause/resume functionality
- Test time speed multipliers (1x, 2x, 5x)
- Test scaled delta time calculations
- Test time speed change events

**Simulation Tick Tests**:
- Test tick rate configuration
- Test tick event firing at correct intervals
- Test tick accumulation with variable frame rates
- Test tick behavior when paused

**Event Scheduling Tests**:
- Test scheduling events with delays
- Test scheduling events at specific days
- Test event execution order
- Test event cancellation
- Test event persistence across save/load

**Save/Load Tests**:
- Test serialization of time state
- Test restoration from save data
- Test handling of corrupted save data
- Test scheduled event persistence

### Integration Tests

**Machine Integration**:
- Test machines update on simulation ticks
- Test machine production respects time speed
- Test machines pause when time is paused

**Research Integration**:
- Test research completion events trigger correctly
- Test research progress respects time speed

**Mission Integration**:
- Test time-based mission objectives
- Test mission deadlines

### Performance Tests

**Event Scheduling Performance**:
- Test with 1000 scheduled events
- Test event processing time per frame
- Test memory usage with many events

**Tick Performance**:
- Test tick overhead with no subscribers
- Test tick overhead with 100 subscribers
- Test tick timing accuracy

## Implementation Notes

### Simulation Tick Implementation

The simulation tick uses a fixed timestep accumulator pattern:

```csharp
private float _tickAccumulator = 0f;
private float _tickInterval = 0.1f; // 10 Hz

private void Update()
{
    if (IsPaused)
        return;
    
    UnscaledDeltaTime = Time.deltaTime;
    ScaledDeltaTime = UnscaledDeltaTime * TimeSpeed;
    
    TotalRealTime += UnscaledDeltaTime;
    TotalGameTime += ScaledDeltaTime;
    
    // Accumulate time for ticks
    _tickAccumulator += ScaledDeltaTime;
    
    // Process ticks
    while (_tickAccumulator >= _tickInterval)
    {
        _tickAccumulator -= _tickInterval;
        ProcessSimulationTick();
    }
    
    // Update calendar
    UpdateCalendar();
    
    // Process scheduled events
    ProcessScheduledEvents();
}
```

### Event Queue Optimization

Use a sorted list for efficient event processing:

```csharp
private List<ScheduledEvent> _scheduledEvents = new List<ScheduledEvent>();

private void ProcessScheduledEvents()
{
    // Events are sorted by trigger time
    // Process only events that should trigger now
    int eventsProcessed = 0;
    const int MAX_EVENTS_PER_FRAME = 100;
    
    for (int i = 0; i < _scheduledEvents.Count && eventsProcessed < MAX_EVENTS_PER_FRAME; i++)
    {
        var evt = _scheduledEvents[i];
        
        if (evt.IsCancelled)
        {
            _scheduledEvents.RemoveAt(i);
            i--;
            continue;
        }
        
        bool shouldTrigger = false;
        
        if (evt.TriggerDay.HasValue)
        {
            shouldTrigger = CurrentDay >= evt.TriggerDay.Value;
        }
        else
        {
            shouldTrigger = TotalGameTime >= evt.TriggerTime;
        }
        
        if (shouldTrigger)
        {
            evt.Callback?.Invoke();
            _scheduledEvents.RemoveAt(i);
            i--;
            eventsProcessed++;
        }
        else
        {
            // Events are sorted, so we can break early
            break;
        }
    }
}
```

### Calendar Update Logic

```csharp
private void UpdateCalendar()
{
    // Calculate day from total game time
    // Assuming 1 game day = 60 real seconds at 1x speed
    const float SECONDS_PER_DAY = 60f;
    int newDay = Mathf.FloorToInt(TotalGameTime / SECONDS_PER_DAY) + 1;
    
    // Clamp to 1-365
    newDay = Mathf.Clamp(newDay, 1, 365);
    
    if (newDay != CurrentDay)
    {
        CurrentDay = newDay;
        UpdateMonthAndDay();
        UpdateSeasonalPhase();
        OnDayChanged?.Invoke(CurrentDay);
    }
}

private void UpdateMonthAndDay()
{
    // Simple 30-day months (except December with 35 days)
    if (CurrentDay <= 330)
    {
        CurrentMonth = ((CurrentDay - 1) / 30) + 1;
        DayOfMonth = ((CurrentDay - 1) % 30) + 1;
    }
    else
    {
        CurrentMonth = 12;
        DayOfMonth = CurrentDay - 330;
    }
}

private void UpdateSeasonalPhase()
{
    SeasonalPhase newPhase;
    
    if (CurrentDay <= 90)
        newPhase = SeasonalPhase.EarlyYear;
    else if (CurrentDay <= 270)
        newPhase = SeasonalPhase.Production;
    else if (CurrentDay <= 330)
        newPhase = SeasonalPhase.PreChristmas;
    else
        newPhase = SeasonalPhase.ChristmasRush;
    
    if (newPhase != CurrentPhase)
    {
        CurrentPhase = newPhase;
        OnSeasonalPhaseChanged?.Invoke(newPhase);
    }
}
```

## Performance Considerations

### Memory Management

- Use object pooling for scheduled events to avoid allocations
- Limit event processing to 100 events per frame to prevent frame drops
- Use sorted list for efficient event queue management

### CPU Optimization

- Cache tick interval calculation
- Early exit from event processing when no events are ready
- Minimize allocations in Update loop

### Scalability

- System should handle 1000+ scheduled events without performance impact
- Tick system should support 100+ subscribers without frame drops
- Calendar calculations should be O(1) complexity

## Future Enhancements

1. **Variable Day Length**: Allow configuration of seconds per day
2. **Time Zones**: Support for different time zones in multiplayer
3. **Time Dilation**: Smooth transitions between time speeds
4. **Event Priorities**: Priority queue for event execution order
5. **Debug Visualization**: In-game debug panel showing time state and scheduled events
6. **Analytics**: Track time speed usage and player pacing preferences
