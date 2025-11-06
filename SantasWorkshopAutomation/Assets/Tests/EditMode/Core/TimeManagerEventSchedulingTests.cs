using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Core;
using System;

namespace SantasWorkshop.Tests
{
    /// <summary>
    /// Unit tests for TimeManager event scheduling functionality.
    /// Tests delay-based and day-based event scheduling, execution order, and cancellation.
    /// </summary>
    [TestFixture]
    public class TimeManagerEventSchedulingTests
    {
        private GameObject _timeManagerObject;
        private TimeManager _timeManager;

        [SetUp]
        public void SetUp()
        {
            // Create TimeManager instance for testing
            _timeManagerObject = new GameObject("TimeManager");
            _timeManager = _timeManagerObject.AddComponent<TimeManager>();
            
            // Wait for Awake to complete
            _timeManager.ResetForTesting();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up
            if (_timeManagerObject != null)
            {
                Object.DestroyImmediate(_timeManagerObject);
            }
            
            // Clear singleton reference
            if (TimeManager.Instance != null)
            {
                var instanceObj = TimeManager.Instance.gameObject;
                Object.DestroyImmediate(instanceObj);
            }
        }

        #region ScheduleEvent (Delay-Based) Tests

        [Test]
        public void ScheduleEvent_ReturnsValidHandle()
        {
            // Act
            var handle = _timeManager.ScheduleEvent(1f, () => { });

            // Assert
            Assert.IsTrue(handle.IsValid, "ScheduleEvent should return a valid handle");
            Assert.Greater(handle.EventId, 0, "Event ID should be positive");
        }

        [Test]
        public void ScheduleEvent_TriggersAfterDelay()
        {
            // Arrange
            bool eventTriggered = false;
            _timeManager.ScheduleEvent(1f, () => eventTriggered = true);

            // Act - Simulate 0.5 seconds (event should not trigger yet)
            SimulateTime(0.5f);
            Assert.IsFalse(eventTriggered, "Event should not trigger before delay");

            // Act - Simulate another 0.5 seconds (event should trigger now)
            SimulateTime(0.5f);

            // Assert
            Assert.IsTrue(eventTriggered, "Event should trigger after 1 second delay");
        }

        [Test]
        public void ScheduleEvent_ImmediateExecution_WithZeroDelay()
        {
            // Arrange
            bool eventTriggered = false;
            _timeManager.ScheduleEvent(0f, () => eventTriggered = true);

            // Act - Simulate minimal time
            SimulateTime(0.02f);

            // Assert
            Assert.IsTrue(eventTriggered, "Event with 0 delay should trigger immediately");
        }

        [Test]
        public void ScheduleEvent_NegativeDelay_TriggersImmediately()
        {
            // Arrange
            bool eventTriggered = false;
            _timeManager.ScheduleEvent(-1f, () => eventTriggered = true);

            // Act
            SimulateTime(0.02f);

            // Assert
            Assert.IsTrue(eventTriggered, "Event with negative delay should trigger immediately");
        }

        [Test]
        public void ScheduleEvent_NullCallback_ReturnsInvalidHandle()
        {
            // Act
            var handle = _timeManager.ScheduleEvent(1f, null);

            // Assert
            Assert.IsFalse(handle.IsValid, "ScheduleEvent with null callback should return invalid handle");
        }

        [Test]
        public void ScheduleEvent_MultipleEvents_AllTrigger()
        {
            // Arrange
            int event1Count = 0;
            int event2Count = 0;
            int event3Count = 0;
            
            _timeManager.ScheduleEvent(0.5f, () => event1Count++);
            _timeManager.ScheduleEvent(1.0f, () => event2Count++);
            _timeManager.ScheduleEvent(1.5f, () => event3Count++);

            // Act
            SimulateTime(2f);

            // Assert
            Assert.AreEqual(1, event1Count, "Event 1 should trigger once");
            Assert.AreEqual(1, event2Count, "Event 2 should trigger once");
            Assert.AreEqual(1, event3Count, "Event 3 should trigger once");
        }

        [Test]
        public void ScheduleEvent_ExecutesInCorrectOrder()
        {
            // Arrange
            string executionOrder = "";
            
            _timeManager.ScheduleEvent(1f, () => executionOrder += "A");
            _timeManager.ScheduleEvent(1f, () => executionOrder += "B");
            _timeManager.ScheduleEvent(1f, () => executionOrder += "C");

            // Act
            SimulateTime(1.1f);

            // Assert
            Assert.AreEqual("ABC", executionOrder, 
                "Events scheduled at same time should execute in order they were scheduled");
        }

        #endregion

        #region ScheduleEventAtDay Tests

        [Test]
        public void ScheduleEventAtDay_ReturnsValidHandle()
        {
            // Act
            var handle = _timeManager.ScheduleEventAtDay(10, () => { });

            // Assert
            Assert.IsTrue(handle.IsValid, "ScheduleEventAtDay should return a valid handle");
            Assert.Greater(handle.EventId, 0, "Event ID should be positive");
        }

        [Test]
        public void ScheduleEventAtDay_TriggersOnSpecifiedDay()
        {
            // Arrange
            bool eventTriggered = false;
            _timeManager.ScheduleEventAtDay(5, () => eventTriggered = true);

            // Act - Simulate to day 4 (event should not trigger yet)
            SimulateTime(4 * 60f);
            Assert.IsFalse(eventTriggered, "Event should not trigger before day 5");

            // Act - Simulate to day 5 (event should trigger now)
            SimulateTime(1 * 60f);

            // Assert
            Assert.IsTrue(eventTriggered, "Event should trigger on day 5");
        }

        [Test]
        public void ScheduleEventAtDay_PastDay_TriggersImmediately()
        {
            // Arrange
            SimulateTime(5 * 60f); // Advance to day 6
            bool eventTriggered = false;
            _timeManager.ScheduleEventAtDay(3, () => eventTriggered = true);

            // Act
            SimulateTime(0.02f);

            // Assert
            Assert.IsTrue(eventTriggered, "Event scheduled for past day should trigger immediately");
        }

        [Test]
        public void ScheduleEventAtDay_CurrentDay_TriggersImmediately()
        {
            // Arrange
            SimulateTime(2 * 60f); // Advance to day 3
            bool eventTriggered = false;
            _timeManager.ScheduleEventAtDay(3, () => eventTriggered = true);

            // Act
            SimulateTime(0.02f);

            // Assert
            Assert.IsTrue(eventTriggered, "Event scheduled for current day should trigger immediately");
        }

        [Test]
        public void ScheduleEventAtDay_InvalidDay_ClampsToValidRange()
        {
            // Act
            var handle1 = _timeManager.ScheduleEventAtDay(0, () => { });
            var handle2 = _timeManager.ScheduleEventAtDay(366, () => { });

            // Assert
            Assert.IsTrue(handle1.IsValid, "Event with day 0 should be clamped and scheduled");
            Assert.IsTrue(handle2.IsValid, "Event with day 366 should be clamped and scheduled");
        }

        [Test]
        public void ScheduleEventAtDay_NullCallback_ReturnsInvalidHandle()
        {
            // Act
            var handle = _timeManager.ScheduleEventAtDay(10, null);

            // Assert
            Assert.IsFalse(handle.IsValid, "ScheduleEventAtDay with null callback should return invalid handle");
        }

        [Test]
        public void ScheduleEventAtDay_MultipleEventsOnSameDay_AllTrigger()
        {
            // Arrange
            int event1Count = 0;
            int event2Count = 0;
            int event3Count = 0;
            
            _timeManager.ScheduleEventAtDay(5, () => event1Count++);
            _timeManager.ScheduleEventAtDay(5, () => event2Count++);
            _timeManager.ScheduleEventAtDay(5, () => event3Count++);

            // Act
            SimulateTime(5 * 60f);

            // Assert
            Assert.AreEqual(1, event1Count, "Event 1 should trigger once");
            Assert.AreEqual(1, event2Count, "Event 2 should trigger once");
            Assert.AreEqual(1, event3Count, "Event 3 should trigger once");
        }

        #endregion

        #region Event Cancellation Tests

        [Test]
        public void CancelScheduledEvent_PreventsEventExecution()
        {
            // Arrange
            bool eventTriggered = false;
            var handle = _timeManager.ScheduleEvent(1f, () => eventTriggered = true);

            // Act
            _timeManager.CancelScheduledEvent(handle);
            SimulateTime(2f);

            // Assert
            Assert.IsFalse(eventTriggered, "Cancelled event should not trigger");
        }

        [Test]
        public void CancelScheduledEvent_InvalidHandle_DoesNotThrow()
        {
            // Arrange
            var invalidHandle = new ScheduledEventHandle { EventId = -1 };

            // Act & Assert
            Assert.DoesNotThrow(() => _timeManager.CancelScheduledEvent(invalidHandle),
                "Cancelling invalid handle should not throw exception");
        }

        [Test]
        public void CancelScheduledEvent_AlreadyExecutedEvent_DoesNotThrow()
        {
            // Arrange
            var handle = _timeManager.ScheduleEvent(0.5f, () => { });
            SimulateTime(1f); // Event executes

            // Act & Assert
            Assert.DoesNotThrow(() => _timeManager.CancelScheduledEvent(handle),
                "Cancelling already executed event should not throw exception");
        }

        [Test]
        public void CancelScheduledEvent_OnlyAffectsTargetEvent()
        {
            // Arrange
            bool event1Triggered = false;
            bool event2Triggered = false;
            bool event3Triggered = false;
            
            var handle1 = _timeManager.ScheduleEvent(1f, () => event1Triggered = true);
            var handle2 = _timeManager.ScheduleEvent(1f, () => event2Triggered = true);
            var handle3 = _timeManager.ScheduleEvent(1f, () => event3Triggered = true);

            // Act
            _timeManager.CancelScheduledEvent(handle2);
            SimulateTime(2f);

            // Assert
            Assert.IsTrue(event1Triggered, "Event 1 should trigger");
            Assert.IsFalse(event2Triggered, "Event 2 should be cancelled");
            Assert.IsTrue(event3Triggered, "Event 3 should trigger");
        }

        [Test]
        public void CancelScheduledEvent_BeforeExecution_Works()
        {
            // Arrange
            bool eventTriggered = false;
            var handle = _timeManager.ScheduleEvent(2f, () => eventTriggered = true);

            // Act - Cancel before event triggers
            SimulateTime(1f);
            _timeManager.CancelScheduledEvent(handle);
            SimulateTime(2f);

            // Assert
            Assert.IsFalse(eventTriggered, "Event should not trigger after cancellation");
        }

        #endregion

        #region Event Execution Order Tests

        [Test]
        public void Events_ExecuteInScheduleOrder_WhenTriggerSimultaneously()
        {
            // Arrange
            string executionOrder = "";
            
            _timeManager.ScheduleEvent(1f, () => executionOrder += "1");
            _timeManager.ScheduleEvent(1f, () => executionOrder += "2");
            _timeManager.ScheduleEvent(1f, () => executionOrder += "3");
            _timeManager.ScheduleEvent(1f, () => executionOrder += "4");

            // Act
            SimulateTime(1.1f);

            // Assert
            Assert.AreEqual("1234", executionOrder, 
                "Events should execute in the order they were scheduled");
        }

        [Test]
        public void Events_ExecuteInTimeOrder_WhenDifferentDelays()
        {
            // Arrange
            string executionOrder = "";
            
            _timeManager.ScheduleEvent(1.5f, () => executionOrder += "C");
            _timeManager.ScheduleEvent(0.5f, () => executionOrder += "A");
            _timeManager.ScheduleEvent(1.0f, () => executionOrder += "B");

            // Act
            SimulateTime(2f);

            // Assert
            Assert.AreEqual("ABC", executionOrder, 
                "Events should execute in time order (earliest first)");
        }

        [Test]
        public void Events_MixedDelayAndDay_ExecuteCorrectly()
        {
            // Arrange
            string executionOrder = "";
            
            _timeManager.ScheduleEvent(30f, () => executionOrder += "A"); // 30 seconds
            _timeManager.ScheduleEventAtDay(2, () => executionOrder += "B"); // Day 2 (60 seconds)
            _timeManager.ScheduleEvent(90f, () => executionOrder += "C"); // 90 seconds

            // Act
            SimulateTime(100f);

            // Assert
            Assert.AreEqual("ABC", executionOrder, 
                "Mixed delay and day events should execute in correct time order");
        }

        #endregion

        #region Event Behavior with Time Speed Tests

        [Test]
        public void ScheduledEvents_RespectTimeSpeed()
        {
            // Arrange
            _timeManager.SetTimeSpeed(2f);
            bool eventTriggered = false;
            _timeManager.ScheduleEvent(1f, () => eventTriggered = true);

            // Act - Simulate 0.5 real seconds (1 game second at 2x speed)
            SimulateTime(0.5f);

            // Assert
            Assert.IsTrue(eventTriggered, 
                "Event with 1s delay should trigger after 0.5 real seconds at 2x speed");
        }

        [Test]
        public void ScheduledEvents_FireWhenTimeSpeedChanges()
        {
            // Arrange
            bool eventTriggered = false;
            _timeManager.ScheduleEvent(1f, () => eventTriggered = true);

            // Act - Change speed mid-simulation
            SimulateTime(0.5f); // 0.5 game seconds at 1x
            _timeManager.SetTimeSpeed(2f);
            SimulateTime(0.25f); // 0.5 game seconds at 2x (total: 1 game second)

            // Assert
            Assert.IsTrue(eventTriggered, 
                "Event should trigger correctly even when time speed changes");
        }

        [Test]
        public void ScheduledEvents_DoNotFireWhenPaused()
        {
            // Arrange
            bool eventTriggered = false;
            _timeManager.ScheduleEvent(0.5f, () => eventTriggered = true);
            _timeManager.Pause();

            // Act
            SimulateTime(1f);

            // Assert
            Assert.IsFalse(eventTriggered, "Event should not fire when paused");
        }

        [Test]
        public void ScheduledEvents_FireAfterUnpause()
        {
            // Arrange
            bool eventTriggered = false;
            _timeManager.ScheduleEvent(1f, () => eventTriggered = true);

            // Act - Pause, wait, resume
            SimulateTime(0.5f);
            _timeManager.Pause();
            SimulateTime(1f); // Time doesn't advance while paused
            _timeManager.Resume();
            SimulateTime(0.5f);

            // Assert
            Assert.IsTrue(eventTriggered, "Event should fire after unpause");
        }

        #endregion

        #region Event Handle Tests

        [Test]
        public void EventHandle_IsValid_TrueForScheduledEvent()
        {
            // Act
            var handle = _timeManager.ScheduleEvent(1f, () => { });

            // Assert
            Assert.IsTrue(handle.IsValid, "Handle for scheduled event should be valid");
        }

        [Test]
        public void EventHandle_IsValid_FalseForNullCallback()
        {
            // Act
            var handle = _timeManager.ScheduleEvent(1f, null);

            // Assert
            Assert.IsFalse(handle.IsValid, "Handle for null callback should be invalid");
        }

        [Test]
        public void EventHandle_UniqueIds_ForMultipleEvents()
        {
            // Act
            var handle1 = _timeManager.ScheduleEvent(1f, () => { });
            var handle2 = _timeManager.ScheduleEvent(1f, () => { });
            var handle3 = _timeManager.ScheduleEvent(1f, () => { });

            // Assert
            Assert.AreNotEqual(handle1.EventId, handle2.EventId, "Event IDs should be unique");
            Assert.AreNotEqual(handle2.EventId, handle3.EventId, "Event IDs should be unique");
            Assert.AreNotEqual(handle1.EventId, handle3.EventId, "Event IDs should be unique");
        }

        #endregion

        #region Performance and Edge Case Tests

        [Test]
        public void EventScheduling_HandlesManyEvents()
        {
            // Arrange
            int eventCount = 0;
            const int totalEvents = 100;
            
            for (int i = 0; i < totalEvents; i++)
            {
                _timeManager.ScheduleEvent(1f, () => eventCount++);
            }

            // Act
            SimulateTime(2f);

            // Assert
            Assert.AreEqual(totalEvents, eventCount, 
                $"All {totalEvents} events should trigger");
        }

        [Test]
        public void EventScheduling_HandlesEventThatSchedulesAnotherEvent()
        {
            // Arrange
            int chainCount = 0;
            Action scheduleNext = null;
            scheduleNext = () =>
            {
                chainCount++;
                if (chainCount < 5)
                {
                    _timeManager.ScheduleEvent(0.1f, scheduleNext);
                }
            };

            // Act
            _timeManager.ScheduleEvent(0.1f, scheduleNext);
            SimulateTime(1f);

            // Assert
            Assert.AreEqual(5, chainCount, "Event chain should execute 5 times");
        }

        [Test]
        public void EventScheduling_HandlesEventException()
        {
            // Arrange
            bool event1Triggered = false;
            bool event2Triggered = false;
            
            _timeManager.ScheduleEvent(1f, () => throw new Exception("Test exception"));
            _timeManager.ScheduleEvent(1f, () => event1Triggered = true);
            _timeManager.ScheduleEvent(1f, () => event2Triggered = true);

            // Act
            SimulateTime(2f);

            // Assert
            Assert.IsTrue(event1Triggered, "Event 1 should trigger despite exception in previous event");
            Assert.IsTrue(event2Triggered, "Event 2 should trigger despite exception in previous event");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Simulates time progression by calling Update repeatedly.
        /// </summary>
        /// <param name="seconds">Number of seconds to simulate.</param>
        private void SimulateTime(float seconds)
        {
            const float fixedDeltaTime = 0.02f; // 50 FPS
            float elapsed = 0f;

            while (elapsed < seconds)
            {
                float deltaTime = Mathf.Min(fixedDeltaTime, seconds - elapsed);
                
                // Manually set Time.deltaTime for testing
                typeof(Time).GetField("deltaTime", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                    ?.SetValue(null, deltaTime);
                
                // Call Update through reflection since it's private
                var updateMethod = typeof(TimeManager).GetMethod("Update", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                updateMethod?.Invoke(_timeManager, null);
                
                elapsed += deltaTime;
            }
        }

        #endregion
    }
}
