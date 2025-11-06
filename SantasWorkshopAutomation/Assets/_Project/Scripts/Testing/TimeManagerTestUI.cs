using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SantasWorkshop.Core;

namespace SantasWorkshop.Testing
{
    /// <summary>
    /// Test UI for TimeManager integration testing.
    /// Displays current time state and provides controls for time speed and pause.
    /// </summary>
    public class TimeManagerTestUI : MonoBehaviour
    {
        [Header("Display Elements")]
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI monthText;
        [SerializeField] private TextMeshProUGUI phaseText;
        [SerializeField] private TextMeshProUGUI timeSpeedText;
        [SerializeField] private TextMeshProUGUI gameTimeText;
        [SerializeField] private TextMeshProUGUI realTimeText;
        [SerializeField] private TextMeshProUGUI pauseStatusText;
        [SerializeField] private TextMeshProUGUI tickIndicator;

        [Header("Control Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button speed1xButton;
        [SerializeField] private Button speed2xButton;
        [SerializeField] private Button speed5xButton;
        [SerializeField] private Button scheduleEventButton;

        [Header("Event Log")]
        [SerializeField] private TextMeshProUGUI eventLogText;
        [SerializeField] private ScrollRect eventLogScrollRect;

        private TimeManager _timeManager;
        private System.Text.StringBuilder _eventLog = new System.Text.StringBuilder();
        private int _eventCounter = 0;
        private float _tickIndicatorTimer = 0f;
        private const float TICK_INDICATOR_DURATION = 0.1f;

        private void Awake()
        {
            // Setup button listeners
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseButtonClicked);
            
            if (speed1xButton != null)
                speed1xButton.onClick.AddListener(() => OnSpeedButtonClicked(1f));
            
            if (speed2xButton != null)
                speed2xButton.onClick.AddListener(() => OnSpeedButtonClicked(2f));
            
            if (speed5xButton != null)
                speed5xButton.onClick.AddListener(() => OnSpeedButtonClicked(5f));
            
            if (scheduleEventButton != null)
                scheduleEventButton.onClick.AddListener(OnScheduleEventButtonClicked);
        }

        private void OnEnable()
        {
            // Subscribe to TimeManager events
            TimeManager.OnSimulationTick += HandleSimulationTick;
            TimeManager.OnTimeSpeedChanged += HandleTimeSpeedChanged;
            TimeManager.OnSeasonalPhaseChanged += HandleSeasonalPhaseChanged;
            TimeManager.OnDayChanged += HandleDayChanged;
        }

        private void OnDisable()
        {
            // Unsubscribe from TimeManager events
            TimeManager.OnSimulationTick -= HandleSimulationTick;
            TimeManager.OnTimeSpeedChanged -= HandleTimeSpeedChanged;
            TimeManager.OnSeasonalPhaseChanged -= HandleSeasonalPhaseChanged;
            TimeManager.OnDayChanged -= HandleDayChanged;
        }

        private void Start()
        {
            _timeManager = TimeManager.Instance;
            
            if (_timeManager == null)
            {
                Debug.LogError("TimeManager instance not found! Make sure TimeManager is in the scene.");
                enabled = false;
                return;
            }

            LogEvent("TimeManager Test UI initialized");
            UpdateDisplay();
        }

        private void Update()
        {
            if (_timeManager == null)
                return;

            UpdateDisplay();
            UpdateTickIndicator();
        }

        private void UpdateDisplay()
        {
            // Update calendar display
            if (dayText != null)
                dayText.text = $"Day: {_timeManager.CurrentDay}";
            
            if (monthText != null)
                monthText.text = $"Month: {_timeManager.CurrentMonth} (Day {_timeManager.DayOfMonth})";
            
            if (phaseText != null)
                phaseText.text = $"Phase: {_timeManager.CurrentPhase}";

            // Update time display
            if (timeSpeedText != null)
                timeSpeedText.text = $"Speed: {_timeManager.TimeSpeed:F1}x";
            
            if (gameTimeText != null)
                gameTimeText.text = $"Game Time: {FormatTime(_timeManager.TotalGameTime)}";
            
            if (realTimeText != null)
                realTimeText.text = $"Real Time: {FormatTime(_timeManager.TotalRealTime)}";

            // Update pause status
            if (pauseStatusText != null)
            {
                pauseStatusText.text = _timeManager.IsPaused ? "PAUSED" : "RUNNING";
                pauseStatusText.color = _timeManager.IsPaused ? Color.red : Color.green;
            }

            // Update pause button text
            if (pauseButton != null)
            {
                var buttonText = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = _timeManager.IsPaused ? "Resume" : "Pause";
            }
        }

        private void UpdateTickIndicator()
        {
            if (tickIndicator == null)
                return;

            if (_tickIndicatorTimer > 0f)
            {
                _tickIndicatorTimer -= Time.unscaledDeltaTime;
                tickIndicator.color = Color.Lerp(Color.clear, Color.yellow, _tickIndicatorTimer / TICK_INDICATOR_DURATION);
            }
            else
            {
                tickIndicator.color = Color.clear;
            }
        }

        private string FormatTime(float seconds)
        {
            int hours = Mathf.FloorToInt(seconds / 3600f);
            int minutes = Mathf.FloorToInt((seconds % 3600f) / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{hours:D2}:{minutes:D2}:{secs:D2}";
        }

        #region Button Handlers

        private void OnPauseButtonClicked()
        {
            if (_timeManager == null)
                return;

            _timeManager.TogglePause();
            LogEvent(_timeManager.IsPaused ? "Time paused" : "Time resumed");
        }

        private void OnSpeedButtonClicked(float speed)
        {
            if (_timeManager == null)
                return;

            _timeManager.SetTimeSpeed(speed);
            LogEvent($"Time speed set to {speed}x");
        }

        private void OnScheduleEventButtonClicked()
        {
            if (_timeManager == null)
                return;

            _eventCounter++;
            float delay = Random.Range(1f, 10f);
            int eventId = _eventCounter;

            _timeManager.ScheduleEvent(delay, () => 
            {
                LogEvent($"Scheduled event #{eventId} triggered!");
            });

            LogEvent($"Scheduled event #{eventId} to trigger in {delay:F1}s");
        }

        #endregion

        #region Event Handlers

        private void HandleSimulationTick()
        {
            // Flash tick indicator
            _tickIndicatorTimer = TICK_INDICATOR_DURATION;
        }

        private void HandleTimeSpeedChanged(float newSpeed)
        {
            LogEvent($"Time speed changed to {newSpeed:F1}x");
        }

        private void HandleSeasonalPhaseChanged(SeasonalPhase newPhase)
        {
            LogEvent($"<color=orange>Seasonal phase changed to {newPhase}</color>");
        }

        private void HandleDayChanged(int newDay)
        {
            LogEvent($"<color=cyan>Day changed to {newDay}</color>");
        }

        #endregion

        #region Event Logging

        private void LogEvent(string message)
        {
            string timestamp = FormatTime(_timeManager != null ? _timeManager.TotalGameTime : 0f);
            _eventLog.AppendLine($"[{timestamp}] {message}");

            // Keep log size manageable (last 50 lines)
            string[] lines = _eventLog.ToString().Split('\n');
            if (lines.Length > 50)
            {
                _eventLog.Clear();
                for (int i = lines.Length - 50; i < lines.Length; i++)
                {
                    _eventLog.AppendLine(lines[i]);
                }
            }

            if (eventLogText != null)
            {
                eventLogText.text = _eventLog.ToString();
                
                // Scroll to bottom
                if (eventLogScrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    eventLogScrollRect.verticalNormalizedPosition = 0f;
                }
            }
        }

        #endregion
    }
}
