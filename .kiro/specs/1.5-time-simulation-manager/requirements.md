# Requirements Document - Time & Simulation Manager

## Introduction

The Time & Simulation Manager is a core system that controls the flow of time in Santa's Workshop Automation. It provides a calendar system tracking in-game days and seasons, time speed controls for player convenience, a consistent simulation tick for all game systems, and an event scheduling system for time-based gameplay mechanics.

## Glossary

- **TimeManager**: The singleton system responsible for managing game time, calendar progression, and time speed controls
- **SimulationTick**: A fixed-interval update cycle that drives all time-dependent game systems
- **GameDay**: A single in-game day, part of the 365-day yearly cycle
- **TimeSpeed**: A multiplier controlling how fast game time progresses relative to real time
- **ScheduledEvent**: A time-based event that triggers at a specific game time or after a delay
- **SeasonalPhase**: A period within the game year (e.g., Production, Christmas Rush)
- **Calendar**: The system tracking current day, month, and season within the game year

## Requirements

### Requirement 1: Calendar System

**User Story:** As a player, I want to see the current in-game date and season, so that I can plan my factory operations around the yearly cycle.

#### Acceptance Criteria

1. THE TimeManager SHALL maintain a current day counter from 1 to 365 representing the in-game year
2. THE TimeManager SHALL provide month and day-of-month calculations based on the current day counter
3. THE TimeManager SHALL track the current seasonal phase based on the day counter
4. THE TimeManager SHALL expose read-only properties for current day, month, and season
5. THE TimeManager SHALL reset the calendar to day 1 when reaching day 366

### Requirement 2: Time Speed Controls

**User Story:** As a player, I want to control the speed of time (pause, normal, fast, very fast), so that I can manage my factory at my own pace.

#### Acceptance Criteria

1. THE TimeManager SHALL support pause functionality that stops all time progression
2. THE TimeManager SHALL support at least three time speed multipliers (1x, 2x, 5x)
3. WHEN the player changes time speed, THE TimeManager SHALL apply the new multiplier to all time-dependent systems
4. THE TimeManager SHALL maintain the current time speed setting across scene transitions
5. THE TimeManager SHALL fire an event when time speed changes

### Requirement 3: Simulation Tick System

**User Story:** As a developer, I want a consistent simulation tick system, so that all game systems update in a synchronized and predictable manner.

#### Acceptance Criteria

1. THE TimeManager SHALL provide a fixed-interval simulation tick independent of frame rate
2. THE TimeManager SHALL expose a tick rate configuration (default 10 ticks per second)
3. THE TimeManager SHALL fire a simulation tick event that systems can subscribe to
4. THE TimeManager SHALL accumulate delta time and trigger ticks at consistent intervals
5. THE TimeManager SHALL apply time speed multipliers to the simulation tick rate

### Requirement 4: Event Scheduling System

**User Story:** As a developer, I want to schedule events to occur at specific game times or after delays, so that I can implement time-based gameplay mechanics.

#### Acceptance Criteria

1. THE TimeManager SHALL allow scheduling events to trigger after a specified game time delay
2. THE TimeManager SHALL allow scheduling events to trigger at a specific game day
3. THE TimeManager SHALL execute scheduled events in the order they were scheduled when multiple events trigger simultaneously
4. THE TimeManager SHALL provide a method to cancel scheduled events before they trigger
5. THE TimeManager SHALL fire scheduled events even when time speed is changed

### Requirement 5: Time Progression and Delta Time

**User Story:** As a developer, I want access to scaled delta time values, so that I can implement time-dependent behaviors that respect time speed settings.

#### Acceptance Criteria

1. THE TimeManager SHALL provide a scaled delta time value that accounts for time speed multipliers
2. THE TimeManager SHALL provide an unscaled delta time value for UI and non-gameplay systems
3. THE TimeManager SHALL track total elapsed game time in seconds
4. THE TimeManager SHALL track total elapsed real time in seconds
5. THE TimeManager SHALL expose the current time speed multiplier as a read-only property

### Requirement 6: Seasonal Phase Tracking

**User Story:** As a player, I want the game to recognize different phases of the year (e.g., Production, Christmas Rush), so that gameplay mechanics can change based on the season.

#### Acceptance Criteria

1. THE TimeManager SHALL define at least two seasonal phases (Production and ChristmasRush)
2. THE TimeManager SHALL automatically transition between seasonal phases based on the current day
3. THE TimeManager SHALL fire an event when entering a new seasonal phase
4. THE TimeManager SHALL expose the current seasonal phase as a read-only property
5. THE TimeManager SHALL allow configuration of day ranges for each seasonal phase

### Requirement 7: Persistence and Save/Load Support

**User Story:** As a player, I want my current game time and calendar progress to be saved, so that I can resume exactly where I left off.

#### Acceptance Criteria

1. THE TimeManager SHALL provide a method to serialize current time state to a data structure
2. THE TimeManager SHALL provide a method to restore time state from saved data
3. THE TimeManager SHALL preserve current day, elapsed time, and time speed in save data
4. THE TimeManager SHALL preserve all scheduled events in save data
5. THE TimeManager SHALL validate loaded time data and handle corrupted or invalid data gracefully

### Requirement 8: Performance and Optimization

**User Story:** As a developer, I want the time system to be performant, so that it doesn't impact frame rate even with many scheduled events.

#### Acceptance Criteria

1. THE TimeManager SHALL process scheduled events efficiently using sorted data structures
2. THE TimeManager SHALL limit event processing to a maximum number per frame to prevent frame drops
3. THE TimeManager SHALL use object pooling for scheduled event data structures
4. THE TimeManager SHALL provide performance metrics (events processed per frame, tick timing)
5. THE TimeManager SHALL handle at least 1000 scheduled events without performance degradation
