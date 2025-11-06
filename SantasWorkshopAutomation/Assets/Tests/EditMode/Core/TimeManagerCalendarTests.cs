using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Core;
using System.Collections;
using UnityEngine.TestTools;

namespace SantasWorkshop.Tests
{
    /// <summary>
    /// Unit tests for TimeManager calendar system functionality.
    /// Tests day counter, month/day calculations, and seasonal phase transitions.
    /// </summary>
    [TestFixture]
    public class TimeManagerCalendarTests
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

        #region Day Counter Tests

        [Test]
        public void CurrentDay_StartsAtOne()
        {
            // Assert
            Assert.AreEqual(1, _timeManager.CurrentDay, "TimeManager should start at day 1");
        }

        [Test]
        public void CurrentDay_IncrementsAfterOneDay()
        {
            // Arrange
            int dayChangedCount = 0;
            int lastDay = 0;
            TimeManager.OnDayChanged += (day) =>
            {
                dayChangedCount++;
                lastDay = day;
            };

            // Act - Simulate 60 seconds (1 day at default 60 seconds per day)
            SimulateTime(60f);

            // Assert
            Assert.AreEqual(2, _timeManager.CurrentDay, "Day should increment to 2 after 60 seconds");
            Assert.AreEqual(1, dayChangedCount, "OnDayChanged should fire once");
            Assert.AreEqual(2, lastDay, "OnDayChanged should report day 2");
        }

        [Test]
        public void CurrentDay_IncrementsCorrectlyOverMultipleDays()
        {
            // Arrange
            int expectedDay = 10;
            float timeToSimulate = (expectedDay - 1) * 60f; // 9 days worth of time

            // Act
            SimulateTime(timeToSimulate);

            // Assert
            Assert.AreEqual(expectedDay, _timeManager.CurrentDay, 
                $"Day should be {expectedDay} after {timeToSimulate} seconds");
        }

        [Test]
        public void CurrentDay_ClampsToMaximum365()
        {
            // Arrange - Simulate 366 days worth of time
            float timeToSimulate = 366 * 60f;

            // Act
            SimulateTime(timeToSimulate);

            // Assert
            Assert.AreEqual(365, _timeManager.CurrentDay, "Day should clamp to maximum of 365");
        }

        [Test]
        public void OnDayChanged_FiresOncePerDay()
        {
            // Arrange
            int dayChangedCount = 0;
            TimeManager.OnDayChanged += (day) => dayChangedCount++;

            // Act - Simulate 5 days
            SimulateTime(5 * 60f);

            // Assert
            Assert.AreEqual(5, dayChangedCount, "OnDayChanged should fire 5 times for 5 days");
        }

        [Test]
        public void OnDayChanged_DoesNotFireWhenPaused()
        {
            // Arrange
            int dayChangedCount = 0;
            TimeManager.OnDayChanged += (day) => dayChangedCount++;
            _timeManager.Pause();

            // Act - Try to simulate time while paused
            SimulateTime(120f);

            // Assert
            Assert.AreEqual(0, dayChangedCount, "OnDayChanged should not fire when paused");
            Assert.AreEqual(1, _timeManager.CurrentDay, "Day should not change when paused");
        }

        #endregion

        #region Month and Day-of-Month Tests

        [Test]
        public void MonthAndDay_CalculatesCorrectlyForDay1()
        {
            // Assert
            Assert.AreEqual(1, _timeManager.CurrentMonth, "Month should be 1 (January) on day 1");
            Assert.AreEqual(1, _timeManager.DayOfMonth, "Day of month should be 1 on day 1");
        }

        [Test]
        public void MonthAndDay_CalculatesCorrectlyForDay30()
        {
            // Act
            SimulateTime(29 * 60f); // Day 30

            // Assert
            Assert.AreEqual(1, _timeManager.CurrentMonth, "Month should be 1 (January) on day 30");
            Assert.AreEqual(30, _timeManager.DayOfMonth, "Day of month should be 30 on day 30");
        }

        [Test]
        public void MonthAndDay_CalculatesCorrectlyForDay31()
        {
            // Act
            SimulateTime(30 * 60f); // Day 31

            // Assert
            Assert.AreEqual(2, _timeManager.CurrentMonth, "Month should be 2 (February) on day 31");
            Assert.AreEqual(1, _timeManager.DayOfMonth, "Day of month should be 1 on day 31");
        }

        [Test]
        public void MonthAndDay_CalculatesCorrectlyForDay90()
        {
            // Act
            SimulateTime(89 * 60f); // Day 90

            // Assert
            Assert.AreEqual(3, _timeManager.CurrentMonth, "Month should be 3 (March) on day 90");
            Assert.AreEqual(30, _timeManager.DayOfMonth, "Day of month should be 30 on day 90");
        }

        [Test]
        public void MonthAndDay_CalculatesCorrectlyForDay91()
        {
            // Act
            SimulateTime(90 * 60f); // Day 91

            // Assert
            Assert.AreEqual(4, _timeManager.CurrentMonth, "Month should be 4 (April) on day 91");
            Assert.AreEqual(1, _timeManager.DayOfMonth, "Day of month should be 1 on day 91");
        }

        [Test]
        public void MonthAndDay_CalculatesCorrectlyForDay330()
        {
            // Act
            SimulateTime(329 * 60f); // Day 330

            // Assert
            Assert.AreEqual(11, _timeManager.CurrentMonth, "Month should be 11 (November) on day 330");
            Assert.AreEqual(30, _timeManager.DayOfMonth, "Day of month should be 30 on day 330");
        }

        [Test]
        public void MonthAndDay_CalculatesCorrectlyForDay331_December()
        {
            // Act
            SimulateTime(330 * 60f); // Day 331

            // Assert
            Assert.AreEqual(12, _timeManager.CurrentMonth, "Month should be 12 (December) on day 331");
            Assert.AreEqual(1, _timeManager.DayOfMonth, "Day of month should be 1 on day 331");
        }

        [Test]
        public void MonthAndDay_CalculatesCorrectlyForDay365_December()
        {
            // Act
            SimulateTime(364 * 60f); // Day 365

            // Assert
            Assert.AreEqual(12, _timeManager.CurrentMonth, "Month should be 12 (December) on day 365");
            Assert.AreEqual(35, _timeManager.DayOfMonth, "Day of month should be 35 on day 365 (December has 35 days)");
        }

        [Test]
        public void MonthAndDay_AllDaysHaveValidMonthAndDay()
        {
            // Test all 365 days
            for (int day = 1; day <= 365; day++)
            {
                // Arrange
                _timeManager.ResetForTesting();
                
                // Act
                SimulateTime((day - 1) * 60f);

                // Assert
                Assert.GreaterOrEqual(_timeManager.CurrentMonth, 1, $"Month should be >= 1 on day {day}");
                Assert.LessOrEqual(_timeManager.CurrentMonth, 12, $"Month should be <= 12 on day {day}");
                Assert.GreaterOrEqual(_timeManager.DayOfMonth, 1, $"Day of month should be >= 1 on day {day}");
                
                if (_timeManager.CurrentMonth == 12)
                {
                    Assert.LessOrEqual(_timeManager.DayOfMonth, 35, $"Day of month should be <= 35 in December on day {day}");
                }
                else
                {
                    Assert.LessOrEqual(_timeManager.DayOfMonth, 30, $"Day of month should be <= 30 on day {day}");
                }
            }
        }

        #endregion

        #region Seasonal Phase Tests

        [Test]
        public void SeasonalPhase_StartsInEarlyYear()
        {
            // Assert
            Assert.AreEqual(SeasonalPhase.EarlyYear, _timeManager.CurrentPhase, 
                "TimeManager should start in EarlyYear phase");
        }

        [Test]
        public void SeasonalPhase_TransitionsToProductionAtDay91()
        {
            // Arrange
            SeasonalPhase? newPhase = null;
            TimeManager.OnSeasonalPhaseChanged += (phase) => newPhase = phase;

            // Act
            SimulateTime(90 * 60f); // Day 91

            // Assert
            Assert.AreEqual(SeasonalPhase.Production, _timeManager.CurrentPhase, 
                "Phase should be Production on day 91");
            Assert.AreEqual(SeasonalPhase.Production, newPhase, 
                "OnSeasonalPhaseChanged should fire with Production phase");
        }

        [Test]
        public void SeasonalPhase_TransitionsToPreChristmasAtDay271()
        {
            // Arrange
            SeasonalPhase? newPhase = null;
            TimeManager.OnSeasonalPhaseChanged += (phase) => newPhase = phase;

            // Act
            SimulateTime(270 * 60f); // Day 271

            // Assert
            Assert.AreEqual(SeasonalPhase.PreChristmas, _timeManager.CurrentPhase, 
                "Phase should be PreChristmas on day 271");
            Assert.AreEqual(SeasonalPhase.PreChristmas, newPhase, 
                "OnSeasonalPhaseChanged should fire with PreChristmas phase");
        }

        [Test]
        public void SeasonalPhase_TransitionsToChristmasRushAtDay331()
        {
            // Arrange
            SeasonalPhase? newPhase = null;
            TimeManager.OnSeasonalPhaseChanged += (phase) => newPhase = phase;

            // Act
            SimulateTime(330 * 60f); // Day 331

            // Assert
            Assert.AreEqual(SeasonalPhase.ChristmasRush, _timeManager.CurrentPhase, 
                "Phase should be ChristmasRush on day 331");
            Assert.AreEqual(SeasonalPhase.ChristmasRush, newPhase, 
                "OnSeasonalPhaseChanged should fire with ChristmasRush phase");
        }

        [Test]
        public void SeasonalPhase_EarlyYearCoversDay1ToDay90()
        {
            // Test boundary days
            Assert.AreEqual(SeasonalPhase.EarlyYear, _timeManager.CurrentPhase, "Day 1 should be EarlyYear");
            
            SimulateTime(89 * 60f); // Day 90
            Assert.AreEqual(SeasonalPhase.EarlyYear, _timeManager.CurrentPhase, "Day 90 should be EarlyYear");
        }

        [Test]
        public void SeasonalPhase_ProductionCoversDay91ToDay270()
        {
            // Test boundary days
            SimulateTime(90 * 60f); // Day 91
            Assert.AreEqual(SeasonalPhase.Production, _timeManager.CurrentPhase, "Day 91 should be Production");
            
            _timeManager.ResetForTesting();
            SimulateTime(269 * 60f); // Day 270
            Assert.AreEqual(SeasonalPhase.Production, _timeManager.CurrentPhase, "Day 270 should be Production");
        }

        [Test]
        public void SeasonalPhase_PreChristmasCoversDay271ToDay330()
        {
            // Test boundary days
            SimulateTime(270 * 60f); // Day 271
            Assert.AreEqual(SeasonalPhase.PreChristmas, _timeManager.CurrentPhase, "Day 271 should be PreChristmas");
            
            _timeManager.ResetForTesting();
            SimulateTime(329 * 60f); // Day 330
            Assert.AreEqual(SeasonalPhase.PreChristmas, _timeManager.CurrentPhase, "Day 330 should be PreChristmas");
        }

        [Test]
        public void SeasonalPhase_ChristmasRushCoversDay331ToDay365()
        {
            // Test boundary days
            SimulateTime(330 * 60f); // Day 331
            Assert.AreEqual(SeasonalPhase.ChristmasRush, _timeManager.CurrentPhase, "Day 331 should be ChristmasRush");
            
            _timeManager.ResetForTesting();
            SimulateTime(364 * 60f); // Day 365
            Assert.AreEqual(SeasonalPhase.ChristmasRush, _timeManager.CurrentPhase, "Day 365 should be ChristmasRush");
        }

        [Test]
        public void OnSeasonalPhaseChanged_FiresOnPhaseTransitions()
        {
            // Arrange
            int phaseChangedCount = 0;
            TimeManager.OnSeasonalPhaseChanged += (phase) => phaseChangedCount++;

            // Act - Simulate through all phases
            SimulateTime(364 * 60f); // Day 365 (through all phases)

            // Assert
            Assert.AreEqual(3, phaseChangedCount, 
                "OnSeasonalPhaseChanged should fire 3 times (EarlyYear→Production, Production→PreChristmas, PreChristmas→ChristmasRush)");
        }

        [Test]
        public void OnSeasonalPhaseChanged_DoesNotFireWithinSamePhase()
        {
            // Arrange
            int phaseChangedCount = 0;
            TimeManager.OnSeasonalPhaseChanged += (phase) => phaseChangedCount++;

            // Act - Simulate within EarlyYear phase
            SimulateTime(50 * 60f); // Day 51 (still in EarlyYear)

            // Assert
            Assert.AreEqual(0, phaseChangedCount, 
                "OnSeasonalPhaseChanged should not fire when staying within the same phase");
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
                // Note: In actual Unity tests, you might need to use UnityTest with yield return
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
