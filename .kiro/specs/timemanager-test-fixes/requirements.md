# Requirements Document - TimeManager Test Fixes

## Introduction

The TimeManager system has 23 failing tests out of 23 tests run, indicating critical issues with event scheduling, time progression, simulation ticks, and performance. This specification addresses all test failures to ensure the TimeManager functions correctly and meets performance targets.

## Glossary

- **TimeManager**: Core system managing in-game time, day/night cycles, seasonal progression, and scheduled events
- **Scheduled Event**: A callback registered to execute after a specific delay or on a specific day
- **Simulation Tick**: Regular update callback fired at a fixed rate (10 Hz) for game systems
- **Time Speed**: Multiplier affecting how fast in-game time progresses (1x = normal, 5x = 5 times faster)
- **Event Handle**: Identifier returned when scheduling an event, used for cancellation
- **AdvanceTimeForTesting**: Test-only method to simulate time passage without real-time delays

---

## Requirements

### Requirement 1: Event Scheduling and Triggering

**User Story:** As a game system, I want to schedule events that trigger reliably after a specified delay, so that time-based gameplay mechanics work correctly.

#### Acceptance Criteria

1. WHEN THE TimeManager schedules an event with a 1-second delay, THE TimeManager SHALL trigger the event callback after 1 second of game time has elapsed
2. WHEN THE TimeManager schedules multiple events with the same delay, THE TimeManager SHALL trigger all event callbacks at the correct time
3. WHEN THE TimeManager advances time using AdvanceTimeForTesting, THE TimeManager SHALL process and trigger all events whose trigger time has been reached
4. WHEN THE TimeManager processes scheduled events, THE TimeManager SHALL remove triggered events from the pending events list
5. WHEN THE TimeManager schedules an event with a null callback, THE TimeManager SHALL return an invalid EventHandle and log an error without crashing

### Requirement 2: Event Scheduling by Day

**User Story:** As a game system, I want to schedule events to trigger on specific in-game days, so that seasonal and calendar-based mechanics work correctly.

#### Acceptance Criteria

1. WHEN THE TimeManager schedules an event for day 5, THE TimeManager SHALL not trigger the event before day 5
2. WHEN THE TimeManager advances to day 5, THE TimeManager SHALL trigger all events scheduled for day 5
3. WHEN THE TimeManager schedules an event for a past day, THE TimeManager SHALL trigger the event immediately on the next time update
4. WHEN THE TimeManager schedules a day-based event with a null callback, THE TimeManager SHALL return an invalid EventHandle and log an error

### Requirement 3: Event Cancellation

**User Story:** As a game system, I want to cancel scheduled events that are no longer needed, so that obsolete callbacks don't execute.

#### Acceptance Criteria

1. WHEN THE TimeManager cancels a scheduled event using a valid EventHandle, THE TimeManager SHALL remove the event from the pending events list
2. WHEN THE TimeManager cancels a scheduled event, THE TimeManager SHALL not trigger the cancelled event's callback
3. WHEN THE TimeManager cancels 1000 events, THE TimeManager SHALL complete the operation within 50 milliseconds
4. WHEN THE TimeManager cancels an event using an invalid EventHandle, THE TimeManager SHALL handle gracefully without errors

### Requirement 4: Event Exception Handling

**User Story:** As a game developer, I want the TimeManager to handle exceptions in event callbacks gracefully, so that one failing callback doesn't break the entire time system.

#### Acceptance Criteria

1. WHEN THE TimeManager executes an event callback that throws an exception, THE TimeManager SHALL log the error with stack trace
2. WHEN THE TimeManager encounters an exception in one event callback, THE TimeManager SHALL continue processing remaining scheduled events
3. WHEN THE TimeManager logs an event exception, THE TimeManager SHALL include the event ID and exception details in the log message

### Requirement 5: Pause and Resume Functionality

**User Story:** As a player, I want to pause the game and have time-based events resume correctly when unpaused, so that gameplay feels responsive and predictable.

#### Acceptance Criteria

1. WHEN THE TimeManager is paused, THE TimeManager SHALL not process scheduled events
2. WHEN THE TimeManager is paused, THE TimeManager SHALL not advance in-game time
3. WHEN THE TimeManager resumes after being paused, THE TimeManager SHALL process events that should have triggered during the pause
4. WHEN THE TimeManager resumes after being paused, THE TimeManager SHALL continue firing simulation ticks at the correct rate

### Requirement 6: Time Speed Control

**User Story:** As a player, I want to change the game speed, so that I can accelerate through slow periods or slow down during complex moments.

#### Acceptance Criteria

1. WHEN THE TimeManager sets time speed to 5x, THE TimeManager SHALL advance in-game time 5 times faster than real time
2. WHEN THE TimeManager sets time speed to 5x, THE TimeManager SHALL advance days 5 times faster (day 2 reached after 12 real seconds instead of 60)
3. WHEN THE TimeManager sets time speed to NaN, THE TimeManager SHALL fall back to 1x speed and log an error
4. WHEN THE TimeManager sets time speed to Infinity, THE TimeManager SHALL clamp to maximum speed (10x) and log a warning
5. WHEN THE TimeManager sets time speed to a negative value, THE TimeManager SHALL clamp to minimum speed (0.1x) and log a warning

### Requirement 7: Simulation Tick System

**User Story:** As a game system, I want to receive regular simulation tick callbacks at a fixed rate, so that I can update game logic consistently.

#### Acceptance Criteria

1. WHEN THE TimeManager runs for 0.5 seconds at 10 Hz tick rate, THE TimeManager SHALL fire exactly 5 simulation tick callbacks
2. WHEN THE TimeManager changes time speed from 1x to 2x, THE TimeManager SHALL fire simulation ticks at double the rate (20 Hz instead of 10 Hz)
3. WHEN THE TimeManager is paused, THE TimeManager SHALL not fire simulation tick callbacks
4. WHEN THE TimeManager resumes after pause, THE TimeManager SHALL resume firing simulation ticks at the correct rate
5. WHEN THE TimeManager unsubscribes a simulation tick listener, THE TimeManager SHALL not invoke that listener's callback in future ticks
6. WHEN THE TimeManager executes a simulation tick callback that throws an exception, THE TimeManager SHALL continue invoking other subscribed callbacks
7. WHEN THE TimeManager runs for 10 seconds at 10 Hz, THE TimeManager SHALL fire exactly 100 simulation ticks with minimal drift

### Requirement 8: Save and Load System

**User Story:** As a player, I want to save and load my game, so that time-based state persists correctly across sessions.

#### Acceptance Criteria

1. WHEN THE TimeManager loads save data with null reference, THE TimeManager SHALL reset to default values and log an error
2. WHEN THE TimeManager loads save data with NaN totalGameTime, THE TimeManager SHALL reset totalGameTime to 0 and log an error
3. WHEN THE TimeManager loads save data with Infinity totalRealTime, THE TimeManager SHALL reset totalRealTime to 0 and log an error
4. WHEN THE TimeManager loads save data with NaN timeSpeed, THE TimeManager SHALL fall back to 1x speed and log an error
5. WHEN THE TimeManager loads save data with Infinity timeSpeed, THE TimeManager SHALL clamp to 10x speed and log a warning

### Requirement 9: Performance - Event Scheduling

**User Story:** As a game developer, I want event scheduling to be fast, so that the game can handle many simultaneous events without performance degradation.

#### Acceptance Criteria

1. WHEN THE TimeManager schedules 1000 events with random delays, THE TimeManager SHALL complete the operation within 100 milliseconds
2. WHEN THE TimeManager schedules events, THE TimeManager SHALL use an efficient data structure (priority queue or sorted list) for O(log n) insertion

### Requirement 10: Performance - Event Cancellation

**User Story:** As a game developer, I want event cancellation to be fast, so that dynamic gameplay doesn't cause performance issues.

#### Acceptance Criteria

1. WHEN THE TimeManager cancels 1000 scheduled events, THE TimeManager SHALL complete the operation within 50 milliseconds
2. WHEN THE TimeManager cancels events, THE TimeManager SHALL use efficient lookup (dictionary or hash set) for O(1) or O(log n) removal

### Requirement 11: Performance - Memory Usage

**User Story:** As a game developer, I want the TimeManager to use memory efficiently, so that scheduling many events doesn't cause excessive memory allocation.

#### Acceptance Criteria

1. WHEN THE TimeManager schedules 1000 events, THE TimeManager SHALL increase memory usage by no more than 1024 KB (1 MB)
2. WHEN THE TimeManager cancels or triggers events, THE TimeManager SHALL release associated memory promptly
3. WHEN THE TimeManager processes events, THE TimeManager SHALL minimize garbage collection allocations

### Requirement 12: EventHandle Validation

**User Story:** As a game system, I want to validate EventHandles before using them, so that I can detect invalid or cancelled events.

#### Acceptance Criteria

1. WHEN THE TimeManager creates an EventHandle for a null callback, THE EventHandle SHALL report IsValid as false
2. WHEN THE TimeManager creates an EventHandle for a valid event, THE EventHandle SHALL report IsValid as true
3. WHEN THE TimeManager cancels an event, THE EventHandle SHALL report IsValid as false after cancellation

---

## Non-Functional Requirements

### Performance Targets

- Event scheduling: < 100ms for 1000 events
- Event cancellation: < 50ms for 1000 events  
- Memory usage: < 1 MB for 1000 scheduled events
- Simulation tick accuracy: < 1% drift over 10 seconds

### Reliability

- All 23 failing tests must pass
- No unhandled exceptions in event processing
- Graceful degradation with invalid input (NaN, Infinity, null)

### Maintainability

- Clear error messages with context (event ID, exception details)
- Efficient data structures for scalability
- Test-friendly design with AdvanceTimeForTesting support

---

## Test Coverage

All requirements must be validated by the existing test suite:

- **TimeManagerEventSchedulingTests**: Requirements 1, 2, 4, 12
- **TimeManagerSpeedControlTests**: Requirement 6
- **TimeManagerSimulationTickTests**: Requirement 7
- **TimeManagerSaveLoadTests**: Requirement 8
- **TimeManagerPerformanceTests**: Requirements 9, 10, 11
- **Integration tests**: Requirements 3, 5

---

## Success Criteria

1. All 23 failing tests pass without modification to test code
2. Performance targets met for 1000+ events
3. No unhandled log messages in tests (proper LogAssert usage where needed)
4. Time progression works correctly with AdvanceTimeForTesting
5. Simulation ticks fire at exact expected rates
6. Memory usage stays within acceptable bounds
