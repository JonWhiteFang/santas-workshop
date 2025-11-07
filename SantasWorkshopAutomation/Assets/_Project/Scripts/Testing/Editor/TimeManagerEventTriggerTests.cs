using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Core;

namespace SantasWorkshop.Testing.Editor
{
    /// <summary>
    /// Tests for TimeManager event processing trigger logic.
    /// Verifies that events trigger correctly with epsilon-based floating-point comparison.
    /// </summary>
    public class TimeManagerEventTriggerTests
    {
        private GameObject _timeManagerObject;
        private TimeManager _timeManager;

        [SetUp]
        public void SetUp()
        {
            // Create TimeManager instance
            _timeManagerObject = new GameObject("TimeManager");
            _timeManager = _timeManagerObject.AddComponent<TimeManager>();
            
            // Reset for testing
            _timeManager.ResetForTesting();
        }

        [TearDown]
        public void TearDown()
        {
            if (_timeManagerObject != null)
            {
                Object.DestroyImmediate(_timeManagerObject);
            }
        }

        [Test]
        public void EventScheduling_TriggersAfterDelay()
        {
            // Arrange
            bool eventTriggered = false;
            _timeManager.ScheduleEvent(1f, () => eventTriggered = true);

            // Act
            _timeManager.AdvanceTimeForTesting(1f);

            // Assert
            Assert.IsTrue(eventTriggered, "Event should trigger after 1 second delay");
        }

        [Test]
        public void EventScheduling_TriggersWithFloatingPointPrecision()
        {
            // Arrange
            bool eventTriggered = false;
            _timeManager.ScheduleEvent(1f, () => eventTriggered = true);

            // Act - Advance time slightly less than 1 second (within epsilon)
            _timeManager.AdvanceTimeForTesting(0.9995f);

            // Assert - Event should still trigger due to epsilon tolerance
            Assert.IsTrue(eventTriggered, "Event should trigger even with minor floating-point drift");
        }

        [Test]
        public void EventScheduling_MultipleEvents_AllTrigger()
        {
            // Arrange
            int eventCount = 0;
            _timeManager.ScheduleEvent(1f, () => eventCount++);
            _timeManager.ScheduleEvent(1f, () => eventCount++);
            _timeManager.ScheduleEvent(1f, () => eventCount++);

            // Act
            _timeManager.AdvanceTimeForTesting(1f);

            // Assert
            Assert.AreEqual(3, eventCount, "All events with same delay should trigger");
        }

        [Test]
        public void EventScheduling_RemovesAfterTriggering()
        {
            // Arrange
            int eventCount = 0;
            _timeManager.ScheduleEvent(1f, () => eventCount++);

            // Act
            _timeManager.AdvanceTimeForTesting(1f);
            _timeManager.AdvanceTimeForTesting(1f); // Advance again

            // Assert
            Assert.AreEqual(1, eventCount, "Event should only trigger once and be removed");
        }

        [Test]
        public void DayBasedEvent_TriggersOnCorrectDay()
        {
            // Arrange
            bool eventTriggered = false;
            _timeManager.ScheduleEventAtDay(2, () => eventTriggered = true);

            // Act - Advance to day 2 (60 seconds per day by default)
            _timeManager.AdvanceTimeForTesting(60f);

            // Assert
            Assert.IsTrue(eventTriggered, "Day-based event should trigger on day 2");
        }

        [Test]
        public void DayBasedEvent_DoesNotTriggerBeforeDay()
        {
            // Arrange
            bool eventTriggered = false;
            _timeManager.ScheduleEventAtDay(5, () => eventTriggered = true);

            // Act - Advance to day 3
            _timeManager.AdvanceTimeForTesting(120f);

            // Assert
            Assert.IsFalse(eventTriggered, "Day-based event should not trigger before target day");
        }

        [Test]
        public void EventScheduling_UpdateCalendarBeforeProcessing()
        {
            // Arrange
            bool dayEventTriggered = false;
            _timeManager.ScheduleEventAtDay(2, () => dayEventTriggered = true);

            // Act - Advance exactly to day 2 boundary
            _timeManager.AdvanceTimeForTesting(60f);

            // Assert - Event should trigger because UpdateCalendar is called before ProcessScheduledEvents
            Assert.IsTrue(dayEventTriggered, "Day-based event should trigger when UpdateCalendar is called before ProcessScheduledEvents");
        }
    }
}
