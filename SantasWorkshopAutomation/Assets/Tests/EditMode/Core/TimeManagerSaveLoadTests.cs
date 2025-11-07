using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Core;
using System;

namespace SantasWorkshop.Tests
{
    /// <summary>
    /// Unit tests for TimeManager save/load system functionality.
    /// Tests serialization, deserialization, data validation, and error handling.
    /// </summary>
    [TestFixture]
    public class TimeManagerSaveLoadTests
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
                UnityEngine.Object.DestroyImmediate(_timeManagerObject);
            }
            
            // Clear singleton reference
            if (TimeManager.Instance != null)
            {
                var instanceObj = TimeManager.Instance.gameObject;
                UnityEngine.Object.DestroyImmediate(instanceObj);
            }
        }

        #region GetSaveData Tests

        [Test]
        public void GetSaveData_ReturnsNonNullData()
        {
            // Act
            var saveData = _timeManager.GetSaveData();

            // Assert
            Assert.IsNotNull(saveData, "GetSaveData should return non-null data");
        }

        [Test]
        public void GetSaveData_SerializesCurrentDay()
        {
            // Arrange
            SimulateTime(5 * 60f); // Advance to day 6

            // Act
            var saveData = _timeManager.GetSaveData();

            // Assert
            Assert.AreEqual(6, saveData.currentDay, "Save data should contain current day");
        }

        [Test]
        public void GetSaveData_SerializesTotalGameTime()
        {
            // Arrange
            SimulateTime(100f);
            float expectedGameTime = _timeManager.TotalGameTime;

            // Act
            var saveData = _timeManager.GetSaveData();

            // Assert
            Assert.AreEqual(expectedGameTime, saveData.totalGameTime, 0.001f, 
                "Save data should contain total game time");
        }

        [Test]
        public void GetSaveData_SerializesTotalRealTime()
        {
            // Arrange
            SimulateTime(100f);
            float expectedRealTime = _timeManager.TotalRealTime;

            // Act
            var saveData = _timeManager.GetSaveData();

            // Assert
            Assert.AreEqual(expectedRealTime, saveData.totalRealTime, 0.001f, 
                "Save data should contain total real time");
        }

        [Test]
        public void GetSaveData_SerializesTimeSpeed()
        {
            // Arrange
            _timeManager.SetTimeSpeed(2.5f);

            // Act
            var saveData = _timeManager.GetSaveData();

            // Assert
            Assert.AreEqual(2.5f, saveData.timeSpeed, 0.001f, 
                "Save data should contain time speed");
        }

        [Test]
        public void GetSaveData_SerializesPausedState()
        {
            // Arrange
            _timeManager.Pause();

            // Act
            var saveData = _timeManager.GetSaveData();

            // Assert
            Assert.IsTrue(saveData.isPaused, "Save data should contain paused state");
        }

        [Test]
        public void GetSaveData_SerializesScheduledEvents()
        {
            // Arrange
            _timeManager.ScheduleEvent(10f, "TestEvent1", () => { });
            _timeManager.ScheduleEvent(20f, "TestEvent2", () => { });
            _timeManager.ScheduleEventAtDay(5, "TestEvent3", () => { });

            // Act
            var saveData = _timeManager.GetSaveData();

            // Assert
            Assert.IsNotNull(saveData.scheduledEvents, "Save data should contain scheduled events array");
            Assert.AreEqual(3, saveData.scheduledEvents.Length, 
                "Save data should contain all 3 scheduled events");
        }

        [Test]
        public void GetSaveData_ScheduledEventData_ContainsCorrectInfo()
        {
            // Arrange
            _timeManager.ScheduleEvent(10f, "TestEvent", () => { });

            // Act
            var saveData = _timeManager.GetSaveData();

            // Assert
            Assert.AreEqual(1, saveData.scheduledEvents.Length);
            var eventData = saveData.scheduledEvents[0];
            Assert.Greater(eventData.eventId, 0, "Event ID should be positive");
            Assert.AreEqual("TestEvent", eventData.eventType, "Event type should be preserved");
        }

        #endregion

        #region LoadSaveData Tests

        [Test]
        public void LoadSaveData_RestoresCurrentDay()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 100,
                totalGameTime = 0f,
                totalRealTime = 0f,
                timeSpeed = 1f,
                isPaused = false,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(100, _timeManager.CurrentDay, "Current day should be restored");
        }

        [Test]
        public void LoadSaveData_RestoresTotalGameTime()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 1,
                totalGameTime = 500f,
                totalRealTime = 0f,
                timeSpeed = 1f,
                isPaused = false,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(500f, _timeManager.TotalGameTime, 0.001f, 
                "Total game time should be restored");
        }

        [Test]
        public void LoadSaveData_RestoresTotalRealTime()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 1,
                totalGameTime = 0f,
                totalRealTime = 300f,
                timeSpeed = 1f,
                isPaused = false,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(300f, _timeManager.TotalRealTime, 0.001f, 
                "Total real time should be restored");
        }

        [Test]
        public void LoadSaveData_RestoresTimeSpeed()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 1,
                totalGameTime = 0f,
                totalRealTime = 0f,
                timeSpeed = 3.5f,
                isPaused = false,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(3.5f, _timeManager.TimeSpeed, 0.001f, 
                "Time speed should be restored");
        }

        [Test]
        public void LoadSaveData_RestoresPausedState()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 1,
                totalGameTime = 0f,
                totalRealTime = 0f,
                timeSpeed = 1f,
                isPaused = true,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.IsTrue(_timeManager.IsPaused, "Paused state should be restored");
        }

        [Test]
        public void LoadSaveData_RestoresMonthAndDayOfMonth()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 91, // Day 91 = April 1st
                totalGameTime = 0f,
                totalRealTime = 0f,
                timeSpeed = 1f,
                isPaused = false,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(4, _timeManager.CurrentMonth, "Month should be calculated correctly");
            Assert.AreEqual(1, _timeManager.DayOfMonth, "Day of month should be calculated correctly");
        }

        [Test]
        public void LoadSaveData_RestoresSeasonalPhase()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 331, // Day 331 = ChristmasRush phase
                totalGameTime = 0f,
                totalRealTime = 0f,
                timeSpeed = 1f,
                isPaused = false,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(SeasonalPhase.ChristmasRush, _timeManager.CurrentPhase, 
                "Seasonal phase should be calculated correctly");
        }

        #endregion

        #region Save/Load Round-Trip Tests

        [Test]
        public void SaveLoad_RoundTrip_PreservesAllState()
        {
            // Arrange - Set up complex state
            SimulateTime(5 * 60f); // Day 6
            _timeManager.SetTimeSpeed(2.5f);
            _timeManager.Pause();

            // Act - Save and load
            var saveData = _timeManager.GetSaveData();
            _timeManager.ResetForTesting();
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(6, _timeManager.CurrentDay, "Day should be preserved");
            Assert.AreEqual(2.5f, _timeManager.TimeSpeed, 0.001f, "Time speed should be preserved");
            Assert.IsTrue(_timeManager.IsPaused, "Paused state should be preserved");
        }

        [Test]
        public void SaveLoad_RoundTrip_PreservesTimeValues()
        {
            // Arrange
            SimulateTime(123.45f);
            float originalGameTime = _timeManager.TotalGameTime;
            float originalRealTime = _timeManager.TotalRealTime;

            // Act
            var saveData = _timeManager.GetSaveData();
            _timeManager.ResetForTesting();
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(originalGameTime, _timeManager.TotalGameTime, 0.001f, 
                "Total game time should be preserved");
            Assert.AreEqual(originalRealTime, _timeManager.TotalRealTime, 0.001f, 
                "Total real time should be preserved");
        }

        [Test]
        public void SaveLoad_RoundTrip_PreservesCalendarState()
        {
            // Arrange
            SimulateTime(150 * 60f); // Day 151
            int originalDay = _timeManager.CurrentDay;
            int originalMonth = _timeManager.CurrentMonth;
            int originalDayOfMonth = _timeManager.DayOfMonth;
            SeasonalPhase originalPhase = _timeManager.CurrentPhase;

            // Act
            var saveData = _timeManager.GetSaveData();
            _timeManager.ResetForTesting();
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(originalDay, _timeManager.CurrentDay, "Day should be preserved");
            Assert.AreEqual(originalMonth, _timeManager.CurrentMonth, "Month should be preserved");
            Assert.AreEqual(originalDayOfMonth, _timeManager.DayOfMonth, "Day of month should be preserved");
            Assert.AreEqual(originalPhase, _timeManager.CurrentPhase, "Seasonal phase should be preserved");
        }

        #endregion

        #region Null and Invalid Data Handling Tests

        [Test]
        public void LoadSaveData_NullData_ResetsToDefaults()
        {
            // Arrange
            SimulateTime(10 * 60f); // Advance state
            _timeManager.SetTimeSpeed(5f);

            // Act
            _timeManager.LoadSaveData(null);

            // Assert
            Assert.AreEqual(1, _timeManager.CurrentDay, "Should reset to day 1");
            Assert.AreEqual(1f, _timeManager.TimeSpeed, 0.001f, "Should reset to 1x speed");
            Assert.AreEqual(0f, _timeManager.TotalGameTime, 0.001f, "Should reset game time to 0");
        }

        [Test]
        public void LoadSaveData_InvalidDay_ClampsToValidRange()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 500, // Invalid (> 365)
                totalGameTime = 0f,
                totalRealTime = 0f,
                timeSpeed = 1f,
                isPaused = false,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.LessOrEqual(_timeManager.CurrentDay, 365, 
                "Day should be clamped to maximum of 365");
            Assert.GreaterOrEqual(_timeManager.CurrentDay, 1, 
                "Day should be clamped to minimum of 1");
        }

        [Test]
        public void LoadSaveData_NegativeDay_ClampsToValidRange()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = -10,
                totalGameTime = 0f,
                totalRealTime = 0f,
                timeSpeed = 1f,
                isPaused = false,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.GreaterOrEqual(_timeManager.CurrentDay, 1, 
                "Negative day should be clamped to minimum of 1");
        }

        [Test]
        public void LoadSaveData_NegativeTotalGameTime_ResetsToZero()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 1,
                totalGameTime = -100f,
                totalRealTime = 0f,
                timeSpeed = 1f,
                isPaused = false,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.GreaterOrEqual(_timeManager.TotalGameTime, 0f, 
                "Negative game time should be reset to 0");
        }

        [Test]
        public void LoadSaveData_NegativeTotalRealTime_ResetsToZero()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 1,
                totalGameTime = 0f,
                totalRealTime = -50f,
                timeSpeed = 1f,
                isPaused = false,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.GreaterOrEqual(_timeManager.TotalRealTime, 0f, 
                "Negative real time should be reset to 0");
        }

        [Test]
        public void LoadSaveData_InvalidTimeSpeed_ClampsToValidRange()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 1,
                totalGameTime = 0f,
                totalRealTime = 0f,
                timeSpeed = 100f, // Invalid (> 10)
                isPaused = false,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.LessOrEqual(_timeManager.TimeSpeed, 10f, 
                "Time speed should be clamped to maximum of 10x");
        }

        [Test]
        public void LoadSaveData_NaNTimeSpeed_FallsBackToDefault()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 1,
                totalGameTime = 0f,
                totalRealTime = 0f,
                timeSpeed = float.NaN,
                isPaused = false,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(1f, _timeManager.TimeSpeed, 0.001f, 
                "NaN time speed should fall back to 1x");
        }

        [Test]
        public void LoadSaveData_InfinityTimeSpeed_ClampsToMaximum()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 1,
                totalGameTime = 0f,
                totalRealTime = 0f,
                timeSpeed = float.PositiveInfinity,
                isPaused = false,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(10f, _timeManager.TimeSpeed, 0.001f, 
                "Infinity time speed should be clamped to 10x");
        }

        [Test]
        public void LoadSaveData_NullScheduledEventsArray_HandlesGracefully()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 1,
                totalGameTime = 0f,
                totalRealTime = 0f,
                timeSpeed = 1f,
                isPaused = false,
                scheduledEvents = null
            };

            // Act & Assert
            Assert.DoesNotThrow(() => _timeManager.LoadSaveData(saveData),
                "Should handle null scheduled events array gracefully");
        }

        [Test]
        public void LoadSaveData_NaNTotalGameTime_ResetsToZero()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 1,
                totalGameTime = float.NaN,
                totalRealTime = 0f,
                timeSpeed = 1f,
                isPaused = false,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(0f, _timeManager.TotalGameTime, 0.001f, 
                "NaN game time should be reset to 0");
        }

        [Test]
        public void LoadSaveData_InfinityTotalRealTime_ResetsToZero()
        {
            // Arrange
            var saveData = new TimeSaveData
            {
                currentDay = 1,
                totalGameTime = 0f,
                totalRealTime = float.PositiveInfinity,
                timeSpeed = 1f,
                isPaused = false,
                scheduledEvents = new ScheduledEventSaveData[0]
            };

            // Act
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(0f, _timeManager.TotalRealTime, 0.001f, 
                "Infinity real time should be reset to 0");
        }

        #endregion

        #region Scheduled Events Save/Load Tests

        [Test]
        public void SaveLoad_ScheduledEvents_PreservesEventCount()
        {
            // Arrange
            _timeManager.ScheduleEvent(10f, "Event1", () => { });
            _timeManager.ScheduleEvent(20f, "Event2", () => { });
            _timeManager.ScheduleEventAtDay(5, "Event3", () => { });

            // Act
            var saveData = _timeManager.GetSaveData();
            _timeManager.ResetForTesting();
            
            // Register event types with factory
            ScheduledEventFactory.RegisterEventType("Event1", () => () => { });
            ScheduledEventFactory.RegisterEventType("Event2", () => () => { });
            ScheduledEventFactory.RegisterEventType("Event3", () => () => { });
            
            _timeManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(3, saveData.scheduledEvents.Length, 
                "Should preserve all 3 scheduled events");
        }

        [Test]
        public void SaveLoad_ScheduledEvents_TriggerAfterLoad()
        {
            // Arrange
            bool eventTriggered = false;
            ScheduledEventFactory.RegisterEventType("TestEvent", () => () => eventTriggered = true);
            _timeManager.ScheduleEvent(1f, "TestEvent", () => eventTriggered = true);

            // Act - Save, reset, load
            var saveData = _timeManager.GetSaveData();
            _timeManager.ResetForTesting();
            _timeManager.LoadSaveData(saveData);
            
            // Simulate time to trigger event
            SimulateTime(2f);

            // Assert
            Assert.IsTrue(eventTriggered, "Scheduled event should trigger after load");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Simulates time progression by calling AdvanceTimeForTesting repeatedly.
        /// </summary>
        /// <param name="seconds">Number of seconds to simulate.</param>
        private void SimulateTime(float seconds)
        {
            const float fixedDeltaTime = 0.02f; // 50 FPS
            float elapsed = 0f;

            while (elapsed < seconds)
            {
                float deltaTime = Mathf.Min(fixedDeltaTime, seconds - elapsed);
                _timeManager.AdvanceTimeForTesting(deltaTime);
                elapsed += deltaTime;
            }
        }

        #endregion
    }
}
