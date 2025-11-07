using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Core;
using System;

namespace SantasWorkshop.Tests
{
    /// <summary>
    /// Unit tests for TimeManager simulation tick system functionality.
    /// Tests tick rate, tick events, and tick behavior with time speed and pause.
    /// </summary>
    [TestFixture]
    public class TimeManagerSimulationTickTests
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

        #region Tick Rate Configuration Tests

        [Test]
        public void TickRate_DefaultsTo10Hz()
        {
            // Assert
            Assert.AreEqual(10f, _timeManager.TickRate, 0.001f, 
                "Default tick rate should be 10 Hz");
        }

        [Test]
        public void TicksPerSecond_MatchesTickRate()
        {
            // Assert
            Assert.AreEqual(10, _timeManager.TicksPerSecond, 
                "TicksPerSecond should match TickRate (10)");
        }

        [Test]
        public void TickRate_CanBeChanged()
        {
            // Act
            _timeManager.TickRate = 20f;

            // Assert
            Assert.AreEqual(20f, _timeManager.TickRate, 0.001f, 
                "TickRate should be changeable to 20 Hz");
            Assert.AreEqual(20, _timeManager.TicksPerSecond, 
                "TicksPerSecond should update to 20");
        }

        [Test]
        public void TickRate_NegativeValue_ClampsToMinimum()
        {
            // Act
            _timeManager.TickRate = -5f;

            // Assert
            Assert.AreEqual(0.1f, _timeManager.TickRate, 0.001f, 
                "Negative tick rate should be clamped to minimum (0.1 Hz)");
        }

        [Test]
        public void TickRate_ZeroValue_ClampsToMinimum()
        {
            // Act
            _timeManager.TickRate = 0f;

            // Assert
            Assert.AreEqual(0.1f, _timeManager.TickRate, 0.001f, 
                "Zero tick rate should be clamped to minimum (0.1 Hz)");
        }

        [Test]
        public void TickRate_ExcessiveValue_ClampsToMaximum()
        {
            // Act
            _timeManager.TickRate = 1000f;

            // Assert
            Assert.AreEqual(100f, _timeManager.TickRate, 0.001f, 
                "Excessive tick rate should be clamped to maximum (100 Hz)");
        }

        #endregion

        #region Tick Event Tests

        [Test]
        public void OnSimulationTick_FiresAtCorrectIntervals()
        {
            // Arrange
            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;

            // Act - Simulate 1 second (should trigger 10 ticks at 10 Hz)
            SimulateTime(1f);

            // Assert
            Assert.AreEqual(10, tickCount, 
                "OnSimulationTick should fire 10 times in 1 second at 10 Hz");
        }

        [Test]
        public void OnSimulationTick_FiresCorrectlyWithCustomTickRate()
        {
            // Arrange
            _timeManager.TickRate = 20f;
            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;

            // Act - Simulate 1 second (should trigger 20 ticks at 20 Hz)
            SimulateTime(1f);

            // Assert
            Assert.AreEqual(20, tickCount, 
                "OnSimulationTick should fire 20 times in 1 second at 20 Hz");
        }

        [Test]
        public void OnSimulationTick_FiresCorrectlyWithSlowTickRate()
        {
            // Arrange
            _timeManager.TickRate = 2f;
            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;

            // Act - Simulate 1 second (should trigger 2 ticks at 2 Hz)
            SimulateTime(1f);

            // Assert
            Assert.AreEqual(2, tickCount, 
                "OnSimulationTick should fire 2 times in 1 second at 2 Hz");
        }

        [Test]
        public void OnSimulationTick_DoesNotFireWhenPaused()
        {
            // Arrange
            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;
            _timeManager.Pause();

            // Act - Try to simulate time while paused
            SimulateTime(1f);

            // Assert
            Assert.AreEqual(0, tickCount, 
                "OnSimulationTick should not fire when paused");
        }

        [Test]
        public void OnSimulationTick_ResumesAfterUnpause()
        {
            // Arrange
            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;
            
            // Act - Pause, simulate, resume, simulate
            _timeManager.Pause();
            SimulateTime(0.5f);
            _timeManager.Resume();
            SimulateTime(0.5f);

            // Assert
            Assert.AreEqual(5, tickCount, 
                "OnSimulationTick should fire 5 times for 0.5 seconds at 10 Hz after resume");
        }

        [Test]
        public void OnSimulationTick_HandlesMultipleSubscribers()
        {
            // Arrange
            int subscriber1Count = 0;
            int subscriber2Count = 0;
            int subscriber3Count = 0;
            
            TimeManager.OnSimulationTick += () => subscriber1Count++;
            TimeManager.OnSimulationTick += () => subscriber2Count++;
            TimeManager.OnSimulationTick += () => subscriber3Count++;

            // Act
            SimulateTime(1f);

            // Assert
            Assert.AreEqual(10, subscriber1Count, "Subscriber 1 should receive 10 ticks");
            Assert.AreEqual(10, subscriber2Count, "Subscriber 2 should receive 10 ticks");
            Assert.AreEqual(10, subscriber3Count, "Subscriber 3 should receive 10 ticks");
        }

        [Test]
        public void OnSimulationTick_UnsubscribeWorks()
        {
            // Arrange
            int tickCount = 0;
            Action handler = () => tickCount++;
            TimeManager.OnSimulationTick += handler;

            // Act - Simulate, unsubscribe, simulate again
            SimulateTime(0.5f);
            TimeManager.OnSimulationTick -= handler;
            SimulateTime(0.5f);

            // Assert
            Assert.AreEqual(5, tickCount, 
                "Should only count ticks before unsubscribe (5 ticks in 0.5s at 10 Hz)");
        }

        #endregion

        #region Tick Accumulation Tests

        [Test]
        public void TickAccumulation_WorksWithVariableFrameRates()
        {
            // Arrange
            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;

            // Act - Simulate with varying delta times
            SimulateTimeWithVariableDelta(1f, new float[] { 0.016f, 0.033f, 0.016f, 0.050f });

            // Assert
            Assert.AreEqual(10, tickCount, 
                "Should fire 10 ticks in 1 second regardless of variable frame rate");
        }

        [Test]
        public void TickAccumulation_HandlesLargeFrameDeltas()
        {
            // Arrange
            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;

            // Act - Simulate with one large frame delta (0.5 seconds)
            SimulateTimeWithVariableDelta(0.5f, new float[] { 0.5f });

            // Assert
            Assert.AreEqual(5, tickCount, 
                "Should fire 5 ticks for 0.5 second frame delta at 10 Hz");
        }

        [Test]
        public void TickAccumulation_HandlesSmallFrameDeltas()
        {
            // Arrange
            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;

            // Act - Simulate with many small frame deltas (0.001 seconds each)
            SimulateTimeWithVariableDelta(1f, new float[] { 0.001f });

            // Assert
            Assert.AreEqual(10, tickCount, 
                "Should fire 10 ticks in 1 second with small frame deltas");
        }

        #endregion

        #region Time Speed Multiplier Tests

        [Test]
        public void TimeSpeed_2x_DoublesTickRate()
        {
            // Arrange
            _timeManager.SetTimeSpeed(2f);
            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;

            // Act - Simulate 1 real second (should be 2 game seconds at 2x speed)
            SimulateTime(1f);

            // Assert
            Assert.AreEqual(20, tickCount, 
                "Should fire 20 ticks in 1 real second at 2x speed (20 game ticks)");
        }

        [Test]
        public void TimeSpeed_5x_MultipliesTickRate()
        {
            // Arrange
            _timeManager.SetTimeSpeed(5f);
            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;

            // Act - Simulate 1 real second (should be 5 game seconds at 5x speed)
            SimulateTime(1f);

            // Assert
            Assert.AreEqual(50, tickCount, 
                "Should fire 50 ticks in 1 real second at 5x speed (50 game ticks)");
        }

        [Test]
        public void TimeSpeed_HalfSpeed_HalvesTickRate()
        {
            // Arrange
            _timeManager.SetTimeSpeed(0.5f);
            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;

            // Act - Simulate 1 real second (should be 0.5 game seconds at 0.5x speed)
            SimulateTime(1f);

            // Assert
            Assert.AreEqual(5, tickCount, 
                "Should fire 5 ticks in 1 real second at 0.5x speed (5 game ticks)");
        }

        [Test]
        public void TimeSpeed_Zero_StopsTicks()
        {
            // Arrange
            _timeManager.SetTimeSpeed(0f);
            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;

            // Act
            SimulateTime(1f);

            // Assert
            Assert.AreEqual(0, tickCount, 
                "Should fire 0 ticks at 0x speed (time stopped)");
        }

        [Test]
        public void TimeSpeed_ChangeDuringSimulation_AffectsTickRate()
        {
            // Arrange
            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;

            // Act - Simulate at 1x, then 2x
            SimulateTime(0.5f); // 5 ticks at 1x
            _timeManager.SetTimeSpeed(2f);
            SimulateTime(0.5f); // 10 ticks at 2x

            // Assert
            Assert.AreEqual(15, tickCount, 
                "Should fire 15 ticks total (5 at 1x + 10 at 2x)");
        }

        #endregion

        #region Performance and Edge Case Tests

        [Test]
        public void TickSystem_HandlesNoSubscribers()
        {
            // Act - Simulate without any subscribers
            SimulateTime(1f);

            // Assert - Should not throw exceptions
            Assert.Pass("Tick system should handle no subscribers gracefully");
        }

        [Test]
        public void TickSystem_HandlesSubscriberException()
        {
            // Arrange
            int successfulTickCount = 0;
            TimeManager.OnSimulationTick += () => throw new Exception("Test exception");
            TimeManager.OnSimulationTick += () => successfulTickCount++;

            // Act
            SimulateTime(1f);

            // Assert
            Assert.AreEqual(10, successfulTickCount, 
                "Other subscribers should still receive ticks even if one throws exception");
        }

        [Test]
        public void TickSystem_HandlesRapidTickRateChanges()
        {
            // Arrange
            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;

            // Act - Change tick rate multiple times during simulation
            _timeManager.TickRate = 10f;
            SimulateTime(0.2f);
            _timeManager.TickRate = 20f;
            SimulateTime(0.2f);
            _timeManager.TickRate = 5f;
            SimulateTime(0.2f);

            // Assert
            int expectedTicks = 2 + 4 + 1; // 0.2s at 10Hz + 0.2s at 20Hz + 0.2s at 5Hz
            Assert.AreEqual(expectedTicks, tickCount, 
                $"Should fire {expectedTicks} ticks with varying tick rates");
        }

        [Test]
        public void TickSystem_MaintainsAccuracyOverLongPeriods()
        {
            // Arrange
            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;

            // Act - Simulate 10 seconds
            SimulateTime(10f);

            // Assert
            Assert.AreEqual(100, tickCount, 
                "Should maintain accuracy over long periods (100 ticks in 10 seconds at 10 Hz)");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Simulates time progression by calling Update repeatedly with fixed delta time.
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

        /// <summary>
        /// Simulates time progression with variable delta times.
        /// </summary>
        /// <param name="totalSeconds">Total seconds to simulate.</param>
        /// <param name="deltaPattern">Pattern of delta times to use (cycles through).</param>
        private void SimulateTimeWithVariableDelta(float totalSeconds, float[] deltaPattern)
        {
            float elapsed = 0f;
            int patternIndex = 0;

            while (elapsed < totalSeconds)
            {
                float deltaTime = Mathf.Min(deltaPattern[patternIndex], totalSeconds - elapsed);
                
                // Manually set Time.deltaTime for testing
                typeof(Time).GetField("deltaTime", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                    ?.SetValue(null, deltaTime);
                
                // Call Update through reflection since it's private
                var updateMethod = typeof(TimeManager).GetMethod("Update", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                updateMethod?.Invoke(_timeManager, null);
                
                elapsed += deltaTime;
                patternIndex = (patternIndex + 1) % deltaPattern.Length;
            }
        }

        #endregion
    }
}
