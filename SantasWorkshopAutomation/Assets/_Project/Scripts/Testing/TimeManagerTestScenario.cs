using UnityEngine;
using SantasWorkshop.Core;

namespace SantasWorkshop.Testing
{
    /// <summary>
    /// Demonstrates various TimeManager features and scenarios for integration testing.
    /// Automatically schedules events and demonstrates time-based gameplay mechanics.
    /// </summary>
    public class TimeManagerTestScenario : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool autoStartTests = true;
        [SerializeField] private bool logVerbose = true;

        [Header("Test Scenarios")]
        [SerializeField] private bool testBasicScheduling = true;
        [SerializeField] private bool testDayBasedEvents = true;
        [SerializeField] private bool testSeasonalPhases = true;
        [SerializeField] private bool testEventCancellation = true;

        private TimeManager _timeManager;
        private int _testEventCounter = 0;

        private void Start()
        {
            _timeManager = TimeManager.Instance;
            
            if (_timeManager == null)
            {
                Debug.LogError("[TestScenario] TimeManager instance not found!");
                enabled = false;
                return;
            }

            Log("TimeManager Test Scenario started");

            if (autoStartTests)
            {
                StartTests();
            }
        }

        private void OnEnable()
        {
            TimeManager.OnSimulationTick += HandleSimulationTick;
            TimeManager.OnDayChanged += HandleDayChanged;
            TimeManager.OnSeasonalPhaseChanged += HandleSeasonalPhaseChanged;
        }

        private void OnDisable()
        {
            TimeManager.OnSimulationTick -= HandleSimulationTick;
            TimeManager.OnDayChanged -= HandleDayChanged;
            TimeManager.OnSeasonalPhaseChanged -= HandleSeasonalPhaseChanged;
        }

        public void StartTests()
        {
            Log("=== Starting TimeManager Integration Tests ===");

            if (testBasicScheduling)
                TestBasicScheduling();
            
            if (testDayBasedEvents)
                TestDayBasedEvents();
            
            if (testSeasonalPhases)
                TestSeasonalPhases();
            
            if (testEventCancellation)
                TestEventCancellation();

            Log("=== All tests scheduled ===");
        }

        #region Test Scenarios

        private void TestBasicScheduling()
        {
            Log("<color=yellow>--- Test: Basic Event Scheduling ---</color>");

            // Schedule events at various delays
            ScheduleTestEvent(1f, "1 second delay");
            ScheduleTestEvent(2f, "2 second delay");
            ScheduleTestEvent(5f, "5 second delay");
            ScheduleTestEvent(10f, "10 second delay");

            Log("Scheduled 4 events with various delays");
        }

        private void TestDayBasedEvents()
        {
            Log("<color=yellow>--- Test: Day-Based Event Scheduling ---</color>");

            // Schedule events for specific days
            int currentDay = _timeManager.CurrentDay;
            
            _timeManager.ScheduleEventAtDay(currentDay + 1, () => 
                Log("<color=cyan>Day-based event: Tomorrow arrived!</color>"));
            
            _timeManager.ScheduleEventAtDay(currentDay + 5, () => 
                Log("<color=cyan>Day-based event: 5 days later!</color>"));
            
            _timeManager.ScheduleEventAtDay(currentDay + 10, () => 
                Log("<color=cyan>Day-based event: 10 days later!</color>"));

            Log($"Scheduled 3 day-based events (current day: {currentDay})");
        }

        private void TestSeasonalPhases()
        {
            Log("<color=yellow>--- Test: Seasonal Phase Tracking ---</color>");
            Log($"Current phase: {_timeManager.CurrentPhase}");
            Log("Phase change events will be logged automatically");
        }

        private void TestEventCancellation()
        {
            Log("<color=yellow>--- Test: Event Cancellation ---</color>");

            // Schedule an event and immediately cancel it
            var handle1 = _timeManager.ScheduleEvent(3f, () => 
                Log("<color=red>ERROR: This event should have been cancelled!</color>"));
            
            _timeManager.CancelScheduledEvent(handle1);
            Log("Scheduled and cancelled event (should not trigger)");

            // Schedule an event and cancel it after a delay
            var handle2 = _timeManager.ScheduleEvent(8f, () => 
                Log("<color=red>ERROR: This event should have been cancelled!</color>"));
            
            _timeManager.ScheduleEvent(4f, () => 
            {
                _timeManager.CancelScheduledEvent(handle2);
                Log("<color=green>Cancelled delayed event</color>");
            });

            Log("Scheduled event to be cancelled after 4 seconds");
        }

        #endregion

        #region Helper Methods

        private void ScheduleTestEvent(float delay, string description)
        {
            int eventId = ++_testEventCounter;
            _timeManager.ScheduleEvent(delay, () => 
            {
                Log($"<color=green>Test Event #{eventId} triggered: {description}</color>");
            });
        }

        private void Log(string message)
        {
            if (logVerbose)
            {
                Debug.Log($"[TestScenario] {message}");
            }
        }

        #endregion

        #region Event Handlers

        private int _tickCount = 0;
        private float _lastTickLogTime = 0f;

        private void HandleSimulationTick()
        {
            _tickCount++;
            
            // Log tick count every 5 seconds
            if (_timeManager.TotalGameTime - _lastTickLogTime >= 5f)
            {
                Log($"Simulation ticks: {_tickCount} (Rate: {_timeManager.TicksPerSecond} Hz)");
                _lastTickLogTime = _timeManager.TotalGameTime;
            }
        }

        private void HandleDayChanged(int newDay)
        {
            Log($"<color=cyan>=== Day Changed: {newDay} (Month {_timeManager.CurrentMonth}, Day {_timeManager.DayOfMonth}) ===</color>");
        }

        private void HandleSeasonalPhaseChanged(SeasonalPhase newPhase)
        {
            Log($"<color=orange>=== Seasonal Phase Changed: {newPhase} ===</color>");
            
            // Log phase-specific information
            switch (newPhase)
            {
                case SeasonalPhase.EarlyYear:
                    Log("Early Year phase: Planning and preparation time");
                    break;
                case SeasonalPhase.Production:
                    Log("Production phase: Normal toy manufacturing");
                    break;
                case SeasonalPhase.PreChristmas:
                    Log("Pre-Christmas phase: Ramping up production");
                    break;
                case SeasonalPhase.ChristmasRush:
                    Log("Christmas Rush phase: Maximum production urgency!");
                    break;
            }
        }

        #endregion

        #region Public Test Methods (for manual testing)

        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            StartTests();
        }

        [ContextMenu("Test Time Speed Changes")]
        public void TestTimeSpeedChanges()
        {
            Log("=== Testing Time Speed Changes ===");
            
            _timeManager.SetTimeSpeed(1f);
            _timeManager.ScheduleEvent(2f, () => 
            {
                Log("Changing to 2x speed");
                _timeManager.SetTimeSpeed(2f);
            });
            
            _timeManager.ScheduleEvent(4f, () => 
            {
                Log("Changing to 5x speed");
                _timeManager.SetTimeSpeed(5f);
            });
            
            _timeManager.ScheduleEvent(6f, () => 
            {
                Log("Changing back to 1x speed");
                _timeManager.SetTimeSpeed(1f);
            });
        }

        [ContextMenu("Test Pause/Resume")]
        public void TestPauseResume()
        {
            Log("=== Testing Pause/Resume ===");
            
            _timeManager.ScheduleEvent(2f, () => 
            {
                Log("Pausing time for 3 real seconds");
                _timeManager.Pause();
                
                // Use real-time coroutine to resume after 3 seconds
                StartCoroutine(ResumeAfterDelay(3f));
            });
        }

        private System.Collections.IEnumerator ResumeAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            Log("Resuming time");
            _timeManager.Resume();
        }

        [ContextMenu("Stress Test: Schedule 100 Events")]
        public void StressTestScheduling()
        {
            Log("=== Stress Test: Scheduling 100 Events ===");
            
            for (int i = 0; i < 100; i++)
            {
                float delay = Random.Range(0.1f, 20f);
                int eventId = i;
                _timeManager.ScheduleEvent(delay, () => 
                {
                    if (eventId % 10 == 0) // Log every 10th event
                        Log($"Stress test event #{eventId} triggered");
                });
            }
            
            Log("Scheduled 100 events with random delays");
        }

        #endregion
    }
}
