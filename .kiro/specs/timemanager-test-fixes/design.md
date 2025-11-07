# Design Document - TimeManager Test Fixes

## Overview

This design document outlines the architectural changes needed to fix 23 failing TimeManager tests. The core issues are:

1. **Event Processing Logic**: Scheduled events not triggering when time advances
2. **Simulation Tick Timing**: Off-by-one errors in tick counting
3. **Performance**: Event scheduling/cancellation exceeding performance targets
4. **Memory Management**: Excessive memory allocation for scheduled events
5. **Test Integration**: Missing LogAssert expectations for error messages

The solution focuses on fixing the event processing loop, improving data structures for performance, and ensuring proper time advancement in test scenarios.

---

## Architecture

### Current Architecture Issues

```
TimeManager (Singleton)
├── Update() Loop
│   ├── UpdateCalendar() - ✅ Working
│   ├── ProcessSimulationTicks() - ⚠️ Off-by-one errors
│   └── ProcessScheduledEvents() - ❌ Not triggering events
├── Event Storage: Dictionary<int, ScheduledEvent> - ⚠️ Performance issues
└── AdvanceTimeForTesting() - ❌ Not processing events correctly
```

### Root Cause Analysis

**Problem 1: Event Processing Not Triggering**
- `ProcessScheduledEvents()` collects events to trigger but the trigger condition logic may have precision issues
- Events scheduled with `TriggerTime` may not trigger due to floating-point comparison issues
- Day-based events may not trigger if `CurrentDay` doesn't update before event processing

**Problem 2: Simulation Tick Off-by-One**
- Tick accumulator using double precision but may have rounding errors
- Expected 5 ticks in 0.5s at 10 Hz, getting 4 ticks suggests accumulator not reaching threshold
- Issue: `_tickAccumulator >= _tickInterval` may need to be `>` instead of `>=`

**Problem 3: Performance Issues**
- Dictionary iteration for event processing is O(n) for every frame
- No priority queue for time-ordered event processing
- Event cancellation is O(1) but scheduling 1000 events takes too long

**Problem 4: Memory Usage**
- Each `ScheduledEvent` object allocates memory
- No object pooling for frequently created/destroyed events
- Delegate allocations for callbacks

---

## Components and Interfaces

### 1. ScheduledEvent Data Structure

**Current Implementation:**
```csharp
private class ScheduledEvent
{
    public int EventId;
    public float TriggerTime;
    public int? TriggerDay;
    public Action Callback;
    public string EventType;
}
```

**Issues:**
- No efficient ordering for time-based processing
- Mixed time-based and day-based events in same structure

**Proposed Changes:**
- Keep current structure (it's adequate)
- Add comparison logic for sorting
- Separate time-based and day-based event collections

### 2. Event Storage Data Structures

**Current:**
```csharp
private Dictionary<int, ScheduledEvent> _scheduledEvents;
private List<int> _eventsToRemove;
```

**Proposed:**
```csharp
// Separate collections for different event types
private Dictionary<int, ScheduledEvent> _timeBasedEvents;  // Events triggered by game time
private Dictionary<int, ScheduledEvent> _dayBasedEvents;   // Events triggered by day
private SortedList<float, List<int>> _eventsByTriggerTime; // Time-ordered index
private List<int> _eventsToRemove;                         // Reused buffer
```

**Benefits:**
- O(log n) insertion with sorted structure
- O(1) lookup by event ID
- Efficient range queries for "all events before time X"
- Separate processing paths for time vs day events

### 3. Event Processing Algorithm

**Current Algorithm:**
```csharp
foreach (var kvp in _scheduledEvents)
{
    if (shouldTrigger)
    {
        _eventsToRemove.Add(eventId);
    }
}
foreach (int eventId in _eventsToRemove)
{
    // Execute and remove
}
```

**Issues:**
- Iterates all events every frame (O(n))
- No early exit when no events are ready
- Floating-point comparison issues

**Proposed Algorithm:**
```csharp
// Process time-based events
while (_eventsByTriggerTime.Count > 0)
{
    var firstKey = _eventsByTriggerTime.Keys[0];
    if (firstKey > TotalGameTime + EPSILON)
        break; // No more events ready
    
    var eventIds = _eventsByTriggerTime[firstKey];
    foreach (var eventId in eventIds)
    {
        // Execute event
    }
    _eventsByTriggerTime.RemoveAt(0);
}

// Process day-based events
foreach (var kvp in _dayBasedEvents)
{
    if (CurrentDay >= kvp.Value.TriggerDay.Value)
    {
        // Execute event
    }
}
```

**Benefits:**
- O(1) check if any events are ready
- O(k) processing where k = number of ready events
- Epsilon-based comparison for floating-point safety

### 4. Simulation Tick Timing Fix

**Current Logic:**
```csharp
while (_tickAccumulator >= _tickInterval)
{
    _tickAccumulator -= _tickInterval;
    ticksToProcess++;
}
```

**Issue:**
- Accumulator precision loss
- Potential off-by-one due to >= vs >

**Proposed Fix:**
```csharp
// Use epsilon for floating-point comparison
const double TICK_EPSILON = 0.0001;

while (_tickAccumulator >= _tickInterval - TICK_EPSILON)
{
    _tickAccumulator -= _tickInterval;
    ticksToProcess++;
    
    if (ticksToProcess > MAX_TICKS_PER_FRAME)
        break;
}
```

**Benefits:**
- Accounts for floating-point precision
- Ensures ticks fire when expected
- Maintains safety limit

### 5. AdvanceTimeForTesting Fix

**Current Implementation:**
```csharp
public void AdvanceTimeForTesting(float deltaTime)
{
    if (IsPaused) return;
    
    UnscaledDeltaTime = deltaTime;
    ScaledDeltaTime = deltaTime * TimeSpeed;
    TotalRealTime += UnscaledDeltaTime;
    TotalGameTime += ScaledDeltaTime;
    
    UpdateCalendar();
    ProcessSimulationTicks();
    ProcessScheduledEvents();
}
```

**Issue:**
- `UpdateCalendar()` may not update `CurrentDay` before `ProcessScheduledEvents()`
- Day threshold check may prevent day advancement

**Proposed Fix:**
```csharp
public void AdvanceTimeForTesting(float deltaTime)
{
    if (IsPaused) return;
    
    UnscaledDeltaTime = deltaTime;
    ScaledDeltaTime = deltaTime * TimeSpeed;
    TotalRealTime += UnscaledDeltaTime;
    TotalGameTime += ScaledDeltaTime;
    
    // Force calendar update by recalculating day from total time
    UpdateCalendar();
    
    // Process events AFTER calendar update
    ProcessSimulationTicks();
    ProcessScheduledEvents();
}
```

---

## Data Models

### ScheduledEvent Class

```csharp
private class ScheduledEvent
{
    public int EventId { get; }
    public float TriggerTime { get; }
    public int? TriggerDay { get; }
    public Action Callback { get; }
    public string EventType { get; }
    
    public ScheduledEvent(int eventId, float triggerTime, int? triggerDay, 
                          Action callback, string eventType)
    {
        EventId = eventId;
        TriggerTime = triggerTime;
        TriggerDay = triggerDay;
        Callback = callback;
        EventType = eventType;
    }
    
    public bool IsTimeBasedEvent => !TriggerDay.HasValue;
    public bool IsDayBasedEvent => TriggerDay.HasValue;
}
```

### EventHandle Validation

```csharp
public struct ScheduledEventHandle
{
    public int EventId { get; set; }
    
    public bool IsValid => EventId > 0 && 
                          TimeManager.Instance != null && 
                          TimeManager.Instance.HasEvent(EventId);
}
```

**New Method:**
```csharp
public bool HasEvent(int eventId)
{
    return _timeBasedEvents.ContainsKey(eventId) || 
           _dayBasedEvents.ContainsKey(eventId);
}
```

---

## Error Handling

### 1. Null Callback Handling

**Current:** Logs error, returns invalid handle
**Status:** ✅ Correct, but tests need LogAssert

**Test Fix:**
```csharp
[Test]
public void ScheduleEvent_NullCallback_ReturnsInvalidHandle()
{
    LogAssert.Expect(LogType.Error, new Regex("Cannot schedule event with null callback"));
    var handle = _timeManager.ScheduleEvent(1f, null);
    Assert.IsFalse(handle.IsValid);
}
```

### 2. Event Exception Handling

**Current:** Catches exceptions, logs error, continues
**Status:** ✅ Correct, but tests need LogAssert

**Test Fix:**
```csharp
[Test]
public void EventScheduling_HandlesEventException()
{
    LogAssert.Expect(LogType.Error, new Regex("Error executing scheduled event"));
    // ... rest of test
}
```

### 3. Invalid Time Speed Handling

**Current:** Validates and clamps, logs error/warning
**Status:** ✅ Correct, but tests need LogAssert

**Test Fix:**
```csharp
[Test]
public void SetTimeSpeed_NaN_FallsBackToDefault()
{
    LogAssert.Expect(LogType.Error, new Regex("Invalid time speed NaN"));
    _timeManager.SetTimeSpeed(float.NaN);
    Assert.AreEqual(1f, _timeManager.TimeSpeed, 0.001f);
}
```

---

## Testing Strategy

### Unit Test Categories

1. **Event Scheduling Tests** (7 tests)
   - Fix: Ensure events trigger after time advancement
   - Fix: Add LogAssert for null callback tests
   - Fix: Ensure day-based events trigger on correct day

2. **Simulation Tick Tests** (5 tests)
   - Fix: Adjust tick accumulator logic for exact counts
   - Fix: Ensure pause/resume works correctly
   - Fix: Handle time speed changes during simulation

3. **Save/Load Tests** (5 tests)
   - Fix: Add LogAssert for error message tests
   - Fix: Ensure validation logic matches test expectations

4. **Performance Tests** (3 tests)
   - Fix: Optimize event scheduling with sorted structure
   - Fix: Optimize event cancellation with direct removal
   - Fix: Reduce memory allocations

5. **Speed Control Tests** (3 tests)
   - Fix: Add LogAssert for validation error tests
   - Fix: Ensure time speed affects day progression

### Test Execution Strategy

1. Run tests to identify specific failures
2. Fix event processing logic first (highest impact)
3. Fix simulation tick timing
4. Add LogAssert expectations
5. Optimize performance
6. Verify all tests pass

---

## Performance Optimization

### Event Scheduling Performance

**Target:** < 100ms for 1000 events

**Current Performance:** ~208ms (FAILING)

**Optimization Strategy:**
1. Use `SortedList<float, List<int>>` for time-ordered events
2. Batch event insertions when possible
3. Pre-allocate event ID lists

**Expected Performance:** ~50ms (2x improvement)

### Event Cancellation Performance

**Target:** < 50ms for 1000 events

**Current Performance:** ~202ms (FAILING)

**Optimization Strategy:**
1. Keep `Dictionary<int, ScheduledEvent>` for O(1) lookup
2. Remove from sorted list efficiently
3. Avoid linear searches

**Expected Performance:** ~25ms (8x improvement)

### Memory Usage

**Target:** < 1024 KB for 1000 events

**Current Usage:** ~4332 KB (FAILING)

**Optimization Strategy:**
1. Reduce `ScheduledEvent` object size
2. Use struct instead of class where possible
3. Pool event ID lists
4. Avoid delegate allocations

**Expected Usage:** ~800 KB (5x improvement)

---

## Implementation Plan

### Phase 1: Core Event Processing Fixes

1. **Separate event collections**
   - Split `_scheduledEvents` into `_timeBasedEvents` and `_dayBasedEvents`
   - Add `_eventsByTriggerTime` sorted index

2. **Fix ProcessScheduledEvents()**
   - Use epsilon-based floating-point comparison
   - Process time-based events from sorted structure
   - Process day-based events separately

3. **Fix UpdateCalendar()**
   - Ensure day updates before event processing
   - Use epsilon for day threshold comparison

### Phase 2: Simulation Tick Fixes

1. **Fix tick accumulator logic**
   - Add epsilon to comparison
   - Ensure exact tick counts

2. **Fix pause/resume**
   - Verify tick accumulator doesn't drift
   - Test resume behavior

### Phase 3: Test Integration Fixes

1. **Add LogAssert expectations**
   - Identify all tests with unhandled log messages
   - Add appropriate `LogAssert.Expect()` calls

2. **Verify AdvanceTimeForTesting**
   - Ensure calendar updates correctly
   - Ensure events process correctly

### Phase 4: Performance Optimization

1. **Optimize event scheduling**
   - Implement sorted list structure
   - Benchmark performance

2. **Optimize event cancellation**
   - Ensure O(1) removal from dictionary
   - Optimize sorted list removal

3. **Reduce memory usage**
   - Analyze allocation sources
   - Implement pooling if needed

---

## Validation Criteria

### Functional Validation

- ✅ All 23 tests pass without modification
- ✅ Events trigger at correct times
- ✅ Simulation ticks fire at exact rates
- ✅ Day progression works correctly
- ✅ Pause/resume works correctly
- ✅ Save/load preserves state

### Performance Validation

- ✅ Event scheduling: < 100ms for 1000 events
- ✅ Event cancellation: < 50ms for 1000 events
- ✅ Memory usage: < 1024 KB for 1000 events
- ✅ Tick accuracy: < 1% drift over 10 seconds

### Code Quality

- ✅ No unhandled exceptions
- ✅ Clear error messages
- ✅ Efficient data structures
- ✅ Maintainable code

---

## Risk Analysis

### High Risk

1. **Breaking existing functionality**
   - Mitigation: Run all tests after each change
   - Mitigation: Make incremental changes

2. **Performance regression**
   - Mitigation: Benchmark before and after
   - Mitigation: Profile memory usage

### Medium Risk

1. **Floating-point precision issues**
   - Mitigation: Use epsilon-based comparisons
   - Mitigation: Use double precision where needed

2. **Test flakiness**
   - Mitigation: Ensure deterministic behavior
   - Mitigation: Avoid timing-dependent tests

### Low Risk

1. **API changes**
   - Mitigation: Keep public API unchanged
   - Mitigation: Only modify internal implementation

---

## Summary

The TimeManager test fixes require:

1. **Event Processing**: Fix trigger logic with epsilon-based comparisons and separate collections
2. **Simulation Ticks**: Fix accumulator logic for exact tick counts
3. **Test Integration**: Add LogAssert expectations for error messages
4. **Performance**: Optimize with sorted data structures and efficient algorithms
5. **Memory**: Reduce allocations through better data structures

All changes maintain backward compatibility and focus on internal implementation improvements. The fixes are incremental and testable at each stage.
