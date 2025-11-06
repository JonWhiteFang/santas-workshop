using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Core;

namespace SantasWorkshop.Tests
{
    /// <summary>
    /// Unit tests for TimeManager time speed control functionality.
    /// Tests pause/resume, time speed multipliers, and related events.
    /// </summary>
    [TestFixture]
    public class TimeManagerSpeedControlTests
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

        #region Pause/Resume Tests

        [Test]
        public void IsPaused_StartsUnpaused()
        {
            // Assert
            Assert.IsFalse(_timeManager.IsPaused, "TimeManager should start unpaused");
        }

        [Test]
        public void Pause_StopsTimeProgression()
        {
            // Arrange
            float initialGameTime = _timeManager.TotalGameTime;
            
            // Act
            _timeManager.Pause();
            SimulateTime(1f);

            // Assert
            Assert.IsTrue(_timeManager.IsPaused, "TimeManager should be paused");
            Assert.AreEqual(initialGameTime, _timeManager.TotalGameTime, 
                "TotalGameTime should not change when paused");
        }

        [Test]
        public void Pause_StopsDayProgression()
        {
            // Arrange
            int initialDay = _timeManager.CurrentDay;
            
            // Act
            _timeManager.Pause();
            SimulateTime(120f); // Try to advance 2 days

            // Assert
            Assert.AreEqual(initialDay, _timeManager.CurrentDay, 
                "CurrentDay should not change when paused");
        }

        [Test]
        public void Resume_RestartsTimeProgression()
        {
            // Arrange
            _timeManager.Pause();
            float pausedGameTime = _timeManager.TotalGameTime;
            
            // Act
            _timeManager.Resume();
            SimulateTime(1f);

            // Assert
            Assert.IsFalse(_timeManager.IsPaused, "TimeManager should be unpaused");
            Assert.Greater(_timeManager.TotalGameTime, pausedGameTime, 
                "TotalGameTime should increase after resume");
        }

        [Test]
        public void TogglePause_SwitchesBetweenPausedAndRunning()
        {
            // Assert initial state
            Assert.IsFalse(_timeManager.IsPaused, "Should start unpaused");

            // Act - Toggle to paused
            _timeManager.TogglePause();
            Assert.IsTrue(_timeManager.IsPaused, "Should be paused after first toggle");

            // Act - Toggle back to running
            _timeManager.TogglePause();
            Assert.IsFalse(_timeManager.IsPaused, "Should be unpaused after second toggle");
        }

        [Test]
        public void Pause_CalledMultipleTimes_RemainsInPausedState()
        {
            // Act
            _timeManager.Pause();
            _timeManager.Pause();
            _timeManager.Pause();

            // Assert
            Assert.IsTrue(_timeManager.IsPaused, "Should remain paused after multiple Pause() calls");
        }

        [Test]
        public void Resume_CalledMultipleTimes_RemainsInRunningState()
        {
            // Act
            _timeManager.Resume();
            _timeManager.Resume();
            _timeManager.Resume();

            // Assert
            Assert.IsFalse(_timeManager.IsPaused, "Should remain unpaused after multiple Resume() calls");
        }

        #endregion

        #region Time Speed Multiplier Tests

        [Test]
        public void TimeSpeed_StartsAtOne()
        {
            // Assert
            Assert.AreEqual(1f, _timeManager.TimeSpeed, 0.001f, 
                "TimeManager should start with 1x time speed");
        }

        [Test]
        public void SetTimeSpeed_ChangesTimeSpeed()
        {
            // Act
            _timeManager.SetTimeSpeed(2f);

            // Assert
            Assert.AreEqual(2f, _timeManager.TimeSpeed, 0.001f, 
                "TimeSpeed should be 2x after SetTimeSpeed(2f)");
        }

        [Test]
        public void SetTimeSpeed_1x_AffectsScaledDeltaTime()
        {
            // Arrange
            _timeManager.SetTimeSpeed(1f);
            
            // Act
            SimulateTime(1f);

            // Assert
            Assert.AreEqual(_timeManager.UnscaledDeltaTime, _timeManager.ScaledDeltaTime, 0.001f,
                "ScaledDeltaTime should equal UnscaledDeltaTime at 1x speed");
        }

        [Test]
        public void SetTimeSpeed_2x_DoublesScaledDeltaTime()
        {
            // Arrange
            _timeManager.SetTimeSpeed(2f);
            
            // Act
            SimulateTime(1f);

            // Assert
            Assert.AreEqual(_timeManager.UnscaledDeltaTime * 2f, _timeManager.ScaledDeltaTime, 0.001f,
                "ScaledDeltaTime should be double UnscaledDeltaTime at 2x speed");
        }

        [Test]
        public void SetTimeSpeed_5x_MultipliesScaledDeltaTime()
        {
            // Arrange
            _timeManager.SetTimeSpeed(5f);
            
            // Act
            SimulateTime(1f);

            // Assert
            Assert.AreEqual(_timeManager.UnscaledDeltaTime * 5f, _timeManager.ScaledDeltaTime, 0.001f,
                "ScaledDeltaTime should be 5x UnscaledDeltaTime at 5x speed");
        }

        [Test]
        public void SetTimeSpeed_2x_DoublesDayProgression()
        {
            // Arrange
            _timeManager.SetTimeSpeed(2f);
            
            // Act - Simulate 30 seconds (should be 60 game seconds at 2x speed = 1 day)
            SimulateTime(30f);

            // Assert
            Assert.AreEqual(2, _timeManager.CurrentDay, 
                "Day should advance to 2 after 30 real seconds at 2x speed");
        }

        [Test]
        public void SetTimeSpeed_5x_AcceleratesDayProgression()
        {
            // Arrange
            _timeManager.SetTimeSpeed(5f);
            
            // Act - Simulate 12 seconds (should be 60 game seconds at 5x speed = 1 day)
            SimulateTime(12f);

            // Assert
            Assert.AreEqual(2, _timeManager.CurrentDay, 
                "Day should advance to 2 after 12 real seconds at 5x speed");
        }

        [Test]
        public void SetTimeSpeed_NegativeValue_ClampsToMinimum()
        {
            // Act
            _timeManager.SetTimeSpeed(-1f);

            // Assert
            Assert.AreEqual(1f, _timeManager.TimeSpeed, 0.001f, 
                "Negative time speed should be clamped to 1x");
        }

        [Test]
        public void SetTimeSpeed_ExcessiveValue_ClampsToMaximum()
        {
            // Act
            _timeManager.SetTimeSpeed(100f);

            // Assert
            Assert.AreEqual(10f, _timeManager.TimeSpeed, 0.001f, 
                "Time speed above 10x should be clamped to 10x");
        }

        [Test]
        public void SetTimeSpeed_Zero_IsValid()
        {
            // Act
            _timeManager.SetTimeSpeed(0f);

            // Assert
            Assert.AreEqual(0f, _timeManager.TimeSpeed, 0.001f, 
                "Time speed of 0x should be valid (effectively paused)");
        }

        [Test]
        public void SetTimeSpeed_NaN_FallsBackToDefault()
        {
            // Act
            _timeManager.SetTimeSpeed(float.NaN);

            // Assert
            Assert.AreEqual(1f, _timeManager.TimeSpeed, 0.001f, 
                "NaN time speed should fall back to 1x");
        }

        [Test]
        public void SetTimeSpeed_Infinity_FallsBackToDefault()
        {
            // Act
            _timeManager.SetTimeSpeed(float.PositiveInfinity);

            // Assert
            Assert.AreEqual(10f, _timeManager.TimeSpeed, 0.001f, 
                "Infinity time speed should be clamped to 10x");
        }

        #endregion

        #region OnTimeSpeedChanged Event Tests

        [Test]
        public void OnTimeSpeedChanged_FiresWhenSpeedChanges()
        {
            // Arrange
            float? newSpeed = null;
            TimeManager.OnTimeSpeedChanged += (speed) => newSpeed = speed;

            // Act
            _timeManager.SetTimeSpeed(2f);

            // Assert
            Assert.IsNotNull(newSpeed, "OnTimeSpeedChanged should fire");
            Assert.AreEqual(2f, newSpeed.Value, 0.001f, 
                "OnTimeSpeedChanged should report new speed of 2x");
        }

        [Test]
        public void OnTimeSpeedChanged_DoesNotFireWhenSpeedUnchanged()
        {
            // Arrange
            int eventCount = 0;
            _timeManager.SetTimeSpeed(2f);
            TimeManager.OnTimeSpeedChanged += (speed) => eventCount++;

            // Act - Set to same speed
            _timeManager.SetTimeSpeed(2f);

            // Assert
            Assert.AreEqual(0, eventCount, 
                "OnTimeSpeedChanged should not fire when speed doesn't change");
        }

        [Test]
        public void OnTimeSpeedChanged_FiresMultipleTimesForMultipleChanges()
        {
            // Arrange
            int eventCount = 0;
            TimeManager.OnTimeSpeedChanged += (speed) => eventCount++;

            // Act
            _timeManager.SetTimeSpeed(2f);
            _timeManager.SetTimeSpeed(5f);
            _timeManager.SetTimeSpeed(1f);

            // Assert
            Assert.AreEqual(3, eventCount, 
                "OnTimeSpeedChanged should fire 3 times for 3 speed changes");
        }

        [Test]
        public void OnTimeSpeedChanged_ReportsCorrectSpeed()
        {
            // Arrange
            float[] reportedSpeeds = new float[3];
            int index = 0;
            TimeManager.OnTimeSpeedChanged += (speed) => reportedSpeeds[index++] = speed;

            // Act
            _timeManager.SetTimeSpeed(2f);
            _timeManager.SetTimeSpeed(5f);
            _timeManager.SetTimeSpeed(1f);

            // Assert
            Assert.AreEqual(2f, reportedSpeeds[0], 0.001f, "First event should report 2x");
            Assert.AreEqual(5f, reportedSpeeds[1], 0.001f, "Second event should report 5x");
            Assert.AreEqual(1f, reportedSpeeds[2], 0.001f, "Third event should report 1x");
        }

        #endregion

        #region Combined Pause and Speed Tests

        [Test]
        public void PausedState_DoesNotAffectTimeSpeed()
        {
            // Arrange
            _timeManager.SetTimeSpeed(2f);
            
            // Act
            _timeManager.Pause();

            // Assert
            Assert.AreEqual(2f, _timeManager.TimeSpeed, 0.001f, 
                "TimeSpeed should remain 2x when paused");
        }

        [Test]
        public void TimeSpeed_DoesNotAffectPausedState()
        {
            // Arrange
            _timeManager.Pause();
            
            // Act
            _timeManager.SetTimeSpeed(5f);

            // Assert
            Assert.IsTrue(_timeManager.IsPaused, 
                "Should remain paused when changing time speed");
        }

        [Test]
        public void Resume_MaintainsTimeSpeed()
        {
            // Arrange
            _timeManager.SetTimeSpeed(3f);
            _timeManager.Pause();
            
            // Act
            _timeManager.Resume();

            // Assert
            Assert.AreEqual(3f, _timeManager.TimeSpeed, 0.001f, 
                "TimeSpeed should remain 3x after resume");
        }

        [Test]
        public void ScaledDeltaTime_IsZeroWhenPaused()
        {
            // Arrange
            _timeManager.SetTimeSpeed(5f);
            
            // Act
            _timeManager.Pause();
            SimulateTime(1f);

            // Assert
            Assert.AreEqual(0f, _timeManager.ScaledDeltaTime, 0.001f, 
                "ScaledDeltaTime should be 0 when paused, regardless of time speed");
        }

        [Test]
        public void UnscaledDeltaTime_IsUnaffectedByTimeSpeed()
        {
            // Arrange
            _timeManager.SetTimeSpeed(5f);
            
            // Act
            SimulateTime(1f);

            // Assert
            Assert.Greater(_timeManager.UnscaledDeltaTime, 0f, 
                "UnscaledDeltaTime should be > 0");
            Assert.Less(_timeManager.UnscaledDeltaTime, _timeManager.ScaledDeltaTime, 
                "UnscaledDeltaTime should be less than ScaledDeltaTime at 5x speed");
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
