# Implementation Plan - Time & Simulation Manager

**Status**: Critical and High Priority Fixes Applied (November 6, 2025)

## Code Review Fixes Applied

### Critical Issues Fixed ✅
1. **ScheduledEvent converted to readonly struct** - Prevents mutations, reduces GC pressure, thread-safe
2. **Dictionary storage for O(1) lookups** - Changed from List to Dictionary<int, ScheduledEvent> for efficient event management
3. **Event cleanup mechanism added** - ClearAllEventSubscriptions() method prevents memory leaks
4. **Tick accumulator uses double precision** - Prevents drift over long play sessions
5. **Event factory for save/load** - ScheduledEventFactory enables event reconstruction from save data

### High Priority Fixes Applied ✅
1. **Test helper methods added** - ResetForTesting() for clean test state
2. **Conditional logging** - VERBOSE_LOGGING flag reduces console spam in production
3. **Named constants** - All magic numbers replaced with descriptive constants
4. **Calendar optimization** - Early exit pattern avoids unnecessary calculations
5. **Comprehensive documentation** - XML docs with memory leak warnings, usage guide created

### Files Modified
- `TimeManager.cs` - Complete rewrite with all fixes
- `ScheduledEvent.cs` - Converted to readonly struct
- `ScheduledEventFactory.cs` - New file for save/load support
- `TimeManager_Usage_Guide.md` - New comprehensive documentation

---

## Implementation Tasks

- [x] 1. Create core data structures and enums
  - Create SeasonalPhase enum with four phases (EarlyYear, Production, PreChristmas, ChristmasRush)
  - Create ScheduledEvent class with EventId, TriggerTime, TriggerDay, Callback, and IsCancelled properties
  - Create ScheduledEventHandle struct with EventId and IsValid property
  - Create TimeSaveData serializable class for persistence
  - Create ScheduledEventSaveData struct for event persistence
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7_

- [x] 2. Implement TimeManager singleton and basic structure
  - Create TimeManager class inheriting from MonoBehaviour in SantasWorkshop.Core namespace
  - Implement singleton pattern with Instance property and Awake initialization
  - Add DontDestroyOnLoad to persist across scenes
  - Add serialized fields for configuration (tick rate, seconds per day)
  - Initialize private fields for calendar, time tracking, and event scheduling
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_

- [x] 3. Implement calendar system
  - Add CurrentDay, CurrentMonth, DayOfMonth, and CurrentPhase properties
  - Implement UpdateCalendar method to calculate day from total game time
  - Implement UpdateMonthAndDay method for month/day calculation (30-day months, December 35 days)
  - Implement UpdateSeasonalPhase method with phase transitions at days 90, 270, 330
  - Add OnDayChanged event that fires when day increments
  - Add OnSeasonalPhaseChanged event that fires on phase transitions
  - _Requirements: 1.1, 1.6_

- [x] 4. Implement time speed controls
  - Add IsPaused, TimeSpeed, ScaledDeltaTime, and UnscaledDeltaTime properties
  - Implement SetTimeSpeed method with validation (clamp 0-10x, default 1x)
  - Implement Pause, Resume, and TogglePause methods
  - Add OnTimeSpeedChanged event
  - Initialize TimeSpeed to 1.0f and IsPaused to false
  - _Requirements: 1.2, 1.5_

- [x] 5. Implement simulation tick system
  - Add TickRate property and TicksPerSecond calculated property
  - Add OnSimulationTick static event
  - Implement tick accumulator pattern in Update method
  - Process ticks at fixed intervals (default 0.1s for 10 Hz)
  - Apply time speed multiplier to tick accumulation
  - Skip tick processing when paused
  - _Requirements: 1.3, 1.5_

- [x] 6. Implement event scheduling system
  - Add private list for scheduled events with next event ID counter
  - Implement ScheduleEvent method for delay-based scheduling
  - Implement ScheduleEventAtDay method for day-based scheduling
  - Implement CancelScheduledEvent method using event handle
  - Implement ProcessScheduledEvents method with sorted event processing
  - Limit event processing to 100 events per frame for performance
  - _Requirements: 1.4, 1.8_

- [x] 7. Implement Update loop integration
  - Implement Update method that processes time when not paused
  - Update UnscaledDeltaTime and ScaledDeltaTime each frame
  - Accumulate TotalRealTime and TotalGameTime
  - Call UpdateCalendar to check for day changes
  - Process simulation ticks using accumulator pattern
  - Process scheduled events each frame
  - _Requirements: 1.2, 1.3, 1.4, 1.5_

- [x] 8. Implement save/load system
  - Implement GetSaveData method to serialize current time state
  - Implement LoadSaveData method to restore from saved data
  - Add validation for loaded data (day range 1-365, non-negative times)
  - Handle null or corrupted save data gracefully with warnings
  - Preserve scheduled events in save data (note: callbacks cannot be serialized, only metadata)
  - _Requirements: 1.7_

- [x] 9. Add error handling and validation
  - Add validation in SetTimeSpeed for negative and excessive values
  - Add null checks in ScheduleEvent for callback parameter
  - Add validation in LoadSaveData for day range and time values
  - Add warning logs for invalid inputs with fallback behavior
  - Add error logs for critical failures
  - _Requirements: All requirements_

- [x] 10. Create unit tests for calendar system
  - Test day counter increments correctly over time
  - Test month and day-of-month calculations for all 365 days
  - Test seasonal phase transitions at days 90, 270, 330, and 365
  - Test OnDayChanged event fires when day increments
  - Test OnSeasonalPhaseChanged event fires on phase transitions
  - _Requirements: 1.1, 1.6_

- [x] 11. Create unit tests for time speed controls
  - Test pause functionality stops time progression
  - Test resume functionality restarts time progression
  - Test toggle pause switches between paused and running states
  - Test time speed multipliers (1x, 2x, 5x) affect ScaledDeltaTime correctly
  - Test OnTimeSpeedChanged event fires when speed changes
  - Test invalid time speed values are clamped appropriately
  - _Requirements: 1.2, 1.5_

- [x] 12. Create unit tests for simulation tick system
  - Test simulation ticks fire at correct intervals (10 Hz default)
  - Test OnSimulationTick event is invoked on each tick
  - Test tick accumulation works correctly with variable frame rates
  - Test ticks do not fire when paused
  - Test time speed multiplier affects tick rate
  - _Requirements: 1.3, 1.5_

- [x] 13. Create unit tests for event scheduling
  - Test ScheduleEvent creates events that trigger after specified delay
  - Test ScheduleEventAtDay creates events that trigger on specified day
  - Test events execute in correct order when multiple trigger simultaneously
  - Test CancelScheduledEvent prevents event from executing
  - Test event handles are valid after scheduling and invalid after cancellation
  - Test null callback returns invalid handle with error log
  - _Requirements: 1.4_

- [x] 14. Create unit tests for save/load system
  - Test GetSaveData serializes current time state correctly
  - Test LoadSaveData restores time state from saved data
  - Test LoadSaveData handles null data gracefully
  - Test LoadSaveData validates and corrects invalid day values
  - Test LoadSaveData validates and corrects negative time values
  - _Requirements: 1.7_

- [x] 15. Create performance tests
  - Test event scheduling with 1000 scheduled events
  - Test event processing stays under 100 events per frame limit
  - Test simulation tick overhead with 0, 10, and 100 subscribers
  - Test memory usage with many scheduled events
  - Test tick timing accuracy over extended periods
  - _Requirements: 1.8_

- [x] 16. Create integration test scene
  - Create test scene with TimeManager instance
  - Add test UI showing current day, month, phase, and time speed
  - Add buttons for pause/resume and time speed changes (1x, 2x, 5x)
  - Add test script that schedules events and logs when they trigger
  - Add visual indicators for simulation ticks
  - Test that all systems work together correctly
  - _Requirements: All requirements_
