# Implementation Tasks - TimeManager Test Fixes

This document outlines the implementation tasks to fix all 23 failing TimeManager tests. Tasks are ordered to maximize incremental progress and minimize risk.

---

## Task List

- [ ] 1. Add LogAssert expectations to failing tests
  - Add `LogAssert.Expect()` calls for all tests with unhandled log messages
  - This will make tests pass that are only failing due to missing log expectations
  - _Requirements: 1.5, 2.4, 6.3, 6.4, 6.5, 8.2, 8.3, 8.4, 8.5, 12.1_

- [ ] 1.1 Fix EventScheduling tests with null callbacks
  - Add `LogAssert.Expect(LogType.Error, new Regex("Cannot schedule event with null callback"))` to `ScheduleEvent_NullCallback_ReturnsInvalidHandle`
  - Add same expectation to `EventHandle_IsValid_FalseForNullCallback`
  - Add same expectation to `ScheduleEventAtDay_NullCallback_ReturnsInvalidHandle`
  - _Requirements: 1.5, 12.1_

- [ ] 1.2 Fix EventScheduling test with exception handling
  - Add `LogAssert.Expect(LogType.Error, new Regex("Error executing scheduled event"))` to `EventScheduling_HandlesEventException`
  - _Requirements: 4.1, 4.2, 4.3_

- [ ] 1.3 Fix SaveLoad tests with validation errors
  - Add `LogAssert.Expect(LogType.Error, new Regex("Cannot load null save data"))` to `LoadSaveData_NullData_ResetsToDefaults`
  - Add `LogAssert.Expect(LogType.Error, new Regex("Invalid totalGameTime"))` to `LoadSaveData_NaNTotalGameTime_ResetsToZero`
  - Add `LogAssert.Expect(LogType.Error, new Regex("Invalid totalRealTime"))` to `LoadSaveData_InfinityTotalRealTime_ResetsToZero`
  - Add `LogAssert.Expect(LogType.Error, new Regex("Invalid timeSpeed"))` to `LoadSaveData_NaNTimeSpeed_FallsBackToDefault`
  - _Requirements: 8.1, 8.2, 8.3, 8.4_

- [ ] 1.4 Fix SpeedControl tests with validation errors
  - Add `LogAssert.Expect(LogType.Error, new Regex("Invalid time speed"))` to `SetTimeSpeed_NaN_FallsBackToDefault`
  - Add `LogAssert.Expect(LogType.Warning, new Regex("Time speed .* exceeds maximum"))` to `SetTimeSpeed_Infinity_FallsBackToDefault`
  - _Requirements: 6.3, 6.4_

- [ ] 2. Fix event processing trigger logic
  - Modify `ProcessScheduledEvents()` to correctly trigger events when time advances
  - Add epsilon-based floating-point comparison for trigger time checks
  - Ensure events are removed after triggering
  - _Requirements: 1.1, 1.2, 1.3, 1.4_

- [ ] 2.1 Add floating-point epsilon constant
  - Add `private const float EVENT_TRIGGER_EPSILON = 0.001f;` to TimeManager constants
  - Document why epsilon is needed (floating-point precision)
  - _Requirements: 1.1, 1.3_

- [ ] 2.2 Fix time-based event trigger condition
  - Change `TotalGameTime >= evt.TriggerTime` to `TotalGameTime >= evt.TriggerTime - EVENT_TRIGGER_EPSILON`
  - This ensures events trigger even with minor floating-point drift
  - _Requirements: 1.1, 1.2, 1.3_

- [ ] 2.3 Fix day-based event trigger condition
  - Ensure `UpdateCalendar()` is called before `ProcessScheduledEvents()` in all code paths
  - Verify day-based events trigger immediately when day matches
  - _Requirements: 2.1, 2.2, 2.3_

- [ ] 2.4 Verify event removal after triggering
  - Ensure `_scheduledEvents.Remove(eventId)` is called after event execution
  - Verify events don't trigger multiple times
  - _Requirements: 1.4_

- [ ] 3. Fix simulation tick timing accuracy
  - Adjust tick accumulator logic to produce exact tick counts
  - Fix off-by-one errors in tick counting
  - _Requirements: 7.1, 7.2, 7.7_

- [ ] 3.1 Add tick epsilon constant
  - Add `private const double TICK_EPSILON = 0.0001;` to TimeManager constants
  - Document purpose (floating-point precision for tick timing)
  - _Requirements: 7.1, 7.7_

- [ ] 3.2 Fix tick accumulator comparison
  - Change `while (_tickAccumulator >= _tickInterval)` to `while (_tickAccumulator >= _tickInterval - TICK_EPSILON)`
  - This ensures ticks fire when expected despite floating-point precision
  - _Requirements: 7.1, 7.7_

- [ ] 3.3 Verify tick count accuracy
  - Test that 0.5 seconds at 10 Hz produces exactly 5 ticks
  - Test that 10 seconds at 10 Hz produces exactly 100 ticks
  - _Requirements: 7.1, 7.7_

- [ ] 4. Fix pause/resume event triggering
  - Ensure scheduled events fire correctly after unpause
  - Ensure simulation ticks resume at correct rate after unpause
  - _Requirements: 5.3, 5.4, 7.4_

- [ ] 4.1 Verify pause behavior
  - Ensure `ProcessScheduledEvents()` returns early when paused
  - Ensure `ProcessSimulationTicks()` returns early when paused
  - _Requirements: 5.1, 5.2, 7.3_

- [ ] 4.2 Fix resume behavior for scheduled events
  - When resuming, ensure events that should have triggered during pause are processed
  - Current implementation should handle this correctly, verify with tests
  - _Requirements: 5.3_

- [ ] 4.3 Fix resume behavior for simulation ticks
  - Verify tick accumulator doesn't accumulate during pause
  - Verify ticks resume at correct rate after unpause
  - _Requirements: 5.4, 7.4_

- [ ] 5. Fix time speed affecting day progression
  - Ensure time speed multiplier correctly affects day advancement
  - Fix test expecting day 2 after 12 real seconds at 5x speed
  - _Requirements: 6.1, 6.2_

- [ ] 5.1 Verify ScaledDeltaTime calculation
  - Ensure `ScaledDeltaTime = UnscaledDeltaTime * TimeSpeed` is correct
  - Ensure `TotalGameTime += ScaledDeltaTime` is correct
  - _Requirements: 6.1_

- [ ] 5.2 Verify day threshold calculation
  - Ensure `_nextDayThreshold = CurrentDay * secondsPerDay` is correct
  - Ensure day advances when `TotalGameTime >= _nextDayThreshold`
  - _Requirements: 6.2_

- [ ] 5.3 Fix UpdateCalendar day calculation
  - Verify `int newDay = Mathf.FloorToInt(TotalGameTime / secondsPerDay) + 1;` is correct
  - Add epsilon if needed for floating-point precision
  - _Requirements: 6.2_

- [ ] 6. Fix time speed change affecting simulation ticks
  - Ensure simulation tick rate changes when time speed changes
  - Fix test expecting 15 ticks (5 at 1x + 10 at 2x)
  - _Requirements: 7.2_

- [ ] 6.1 Verify tick rate calculation with time speed
  - Current implementation: ticks accumulate based on `ScaledDeltaTime`
  - This should automatically handle time speed changes
  - Verify with test that changes speed mid-simulation
  - _Requirements: 7.2_

- [ ] 6.2 Debug tick count discrepancy
  - Test expects 15 ticks but gets 14
  - Likely due to epsilon issue in tick accumulator
  - Verify fix from task 3.2 resolves this
  - _Requirements: 7.2_

- [ ] 7. Fix simulation tick exception handling
  - Ensure other subscribers receive ticks even if one throws exception
  - Fix test expecting 10 ticks for second subscriber when first throws
  - _Requirements: 7.6_

- [ ] 7.1 Verify exception handling in ProcessSimulationTicks
  - Current implementation has try-catch around `tickDelegate.Invoke()`
  - Should continue processing remaining ticks after exception
  - _Requirements: 7.6_

- [ ] 7.2 Debug why second subscriber gets 0 ticks
  - Test expects second subscriber to get 10 ticks
  - First subscriber throws exception
  - Likely issue: exception breaks out of loop instead of continuing
  - Verify exception handling continues to next tick iteration
  - _Requirements: 7.6_

- [ ] 8. Fix simulation tick unsubscribe behavior
  - Ensure unsubscribed listeners don't receive ticks
  - Fix test expecting 5 ticks before unsubscribe, 0 after
  - _Requirements: 7.5_

- [ ] 8.1 Verify unsubscribe timing
  - Test unsubscribes after 0.5 seconds (5 ticks)
  - Then advances another 0.5 seconds (should be 0 more ticks)
  - Current implementation should handle this correctly
  - _Requirements: 7.5_

- [ ] 8.2 Debug tick count after unsubscribe
  - Test expects 5 ticks but gets 4
  - Likely same epsilon issue as task 3.2
  - Verify fix from task 3.2 resolves this
  - _Requirements: 7.5_

- [ ] 9. Optimize event scheduling performance
  - Improve event scheduling to meet < 100ms target for 1000 events
  - Current: ~208ms, Target: < 100ms
  - _Requirements: 9.1, 9.2_

- [ ] 9.1 Profile current event scheduling performance
  - Identify bottlenecks in `ScheduleEvent()` method
  - Measure time for Dictionary.Add operation
  - Measure time for event object creation
  - _Requirements: 9.1_

- [ ] 9.2 Optimize event scheduling algorithm
  - Consider pre-allocating Dictionary capacity
  - Consider reducing ScheduledEvent object size
  - Consider using struct instead of class for ScheduledEvent
  - _Requirements: 9.1, 9.2_

- [ ] 9.3 Benchmark optimized implementation
  - Verify 1000 events schedule in < 100ms
  - Compare before/after performance
  - _Requirements: 9.1_

- [ ] 10. Optimize event cancellation performance
  - Improve event cancellation to meet < 50ms target for 1000 events
  - Current: ~202ms, Target: < 50ms
  - _Requirements: 10.1, 10.2_

- [ ] 10.1 Profile current event cancellation performance
  - Identify bottlenecks in `CancelScheduledEvent()` method
  - Measure time for Dictionary.Remove operation
  - Identify any linear searches or iterations
  - _Requirements: 10.1_

- [ ] 10.2 Optimize event cancellation algorithm
  - Verify Dictionary.Remove is O(1)
  - Remove any unnecessary lookups or validations
  - Consider batch cancellation if applicable
  - _Requirements: 10.1, 10.2_

- [ ] 10.3 Benchmark optimized implementation
  - Verify 1000 events cancel in < 50ms
  - Compare before/after performance
  - _Requirements: 10.1_

- [ ] 11. Optimize memory usage for scheduled events
  - Reduce memory usage to meet < 1024 KB target for 1000 events
  - Current: ~4332 KB, Target: < 1024 KB
  - _Requirements: 11.1, 11.2, 11.3_

- [ ] 11.1 Profile current memory usage
  - Measure size of ScheduledEvent objects
  - Identify sources of allocations
  - Check for delegate allocations
  - _Requirements: 11.1_

- [ ] 11.2 Reduce ScheduledEvent memory footprint
  - Consider using struct instead of class
  - Remove unnecessary fields
  - Use value types where possible
  - _Requirements: 11.1, 11.2_

- [ ] 11.3 Benchmark optimized memory usage
  - Verify 1000 events use < 1024 KB
  - Compare before/after memory usage
  - _Requirements: 11.1_

- [ ] 12. Fix EventHandle validation
  - Ensure EventHandle.IsValid returns correct value
  - Add HasEvent() method to TimeManager
  - _Requirements: 12.1, 12.2, 12.3_

- [ ] 12.1 Add HasEvent() method to TimeManager
  - Add `public bool HasEvent(int eventId)` method
  - Return `_scheduledEvents.ContainsKey(eventId)`
  - _Requirements: 12.2_

- [ ] 12.2 Update ScheduledEventHandle.IsValid property
  - Change to: `EventId > 0 && TimeManager.Instance != null && TimeManager.Instance.HasEvent(EventId)`
  - This provides accurate validation
  - _Requirements: 12.1, 12.2, 12.3_

- [ ] 12.3 Verify EventHandle validation tests pass
  - Test null callback returns invalid handle
  - Test valid event returns valid handle
  - Test cancelled event returns invalid handle
  - _Requirements: 12.1, 12.2, 12.3_

- [ ] 13. Run all tests and verify fixes
  - Execute full test suite
  - Verify all 23 tests pass
  - Document any remaining issues
  - _Requirements: All_

- [ ] 13.1 Run TimeManagerEventSchedulingTests
  - Verify all 7 failing tests now pass
  - Document results
  - _Requirements: 1, 2, 4, 12_

- [ ] 13.2 Run TimeManagerSimulationTickTests
  - Verify all 5 failing tests now pass
  - Document results
  - _Requirements: 7_

- [ ] 13.3 Run TimeManagerSaveLoadTests
  - Verify all 5 failing tests now pass
  - Document results
  - _Requirements: 8_

- [ ] 13.4 Run TimeManagerSpeedControlTests
  - Verify all 3 failing tests now pass
  - Document results
  - _Requirements: 6_

- [ ] 13.5 Run TimeManagerPerformanceTests
  - Verify all 3 failing tests now pass
  - Document results
  - _Requirements: 9, 10, 11_

---

## Task Execution Notes

### Priority Order

1. **High Priority** (Tasks 1-2): LogAssert fixes and event processing - fixes most tests quickly
2. **Medium Priority** (Tasks 3-8): Timing and behavior fixes - fixes remaining functional tests
3. **Low Priority** (Tasks 9-11): Performance optimizations - fixes performance tests
4. **Final** (Tasks 12-13): Validation and verification

### Dependencies

- Task 2 depends on Task 1 (LogAssert must be in place first)
- Tasks 3-8 can be done in parallel after Task 2
- Tasks 9-11 can be done in parallel after Tasks 1-8
- Task 12 can be done in parallel with Tasks 9-11
- Task 13 depends on all previous tasks

### Estimated Effort

- Task 1: 30 minutes (straightforward LogAssert additions)
- Task 2: 1 hour (event processing logic fixes)
- Task 3: 1 hour (tick timing fixes)
- Tasks 4-8: 2 hours (various behavior fixes)
- Tasks 9-11: 3 hours (performance optimizations)
- Task 12: 30 minutes (validation fixes)
- Task 13: 30 minutes (verification)

**Total: ~8.5 hours**

### Testing Strategy

- Run tests after each major task completion
- Don't proceed to next task if current task breaks existing tests
- Use `AdvanceTimeForTesting()` to verify time advancement works correctly
- Profile performance before and after optimizations

---

## Success Criteria

- ✅ All 23 failing tests pass
- ✅ No test code modifications required
- ✅ Performance targets met (< 100ms scheduling, < 50ms cancellation, < 1024 KB memory)
- ✅ No new bugs introduced
- ✅ Code remains maintainable and well-documented
