using System;
using System.Collections.Generic;
using UnityEngine;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Singleton system responsible for managing game time, calendar progression, and time speed controls.
    /// Provides a fixed-interval simulation tick for all game systems and handles event scheduling.
    /// 
    /// SAVE/LOAD PATTERN FOR SCHEDULED EVENTS:
    /// Scheduled events cannot be directly serialized because Action delegates are not serializable.
    /// To enable save/load support for your scheduled events:
    /// 
    /// 1. Register your event type with ScheduledEventFactory:
    ///    ScheduledEventFactory.RegisterEventType("MyEventType", () => MyCallback);
    /// 
    /// 2. Use the event type when scheduling:
    ///    var handle = TimeManager.Instance.ScheduleEvent(delay, "MyEventType", MyCallback);
    /// 
    /// 3. The event will be automatically restored on load using the factory.
    /// 
    /// MEMORY LEAK PREVENTION:
    /// Always unsubscribe from static events in OnDisable() or OnDestroy():
    /// <code>
    /// private void OnEnable() => TimeManager.OnSimulationTick += HandleTick;
    /// private void OnDisable() => TimeManager.OnSimulationTick -= HandleTick;
    /// </code>
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        #region Constants

        // Time Configuration
        private const float SECONDS_PER_YEAR = 31536000f; // 365 days * 24 hours * 60 minutes * 60 seconds
        private const float MIN_TICK_RATE = 0.1f;
        private const float MAX_TICK_RATE = 100f;
        private const float MIN_TIME_SPEED = 0f;
        private const float MAX_TIME_SPEED = 10f;

        // Calendar Configuration
        private const int MIN_DAY = 1;
        private const int MAX_DAY = 365;
        private const int DAYS_PER_MONTH = 30;
        private const int DECEMBER_DAYS = 35;
        private const int DECEMBER_START_DAY = 331;

        // Seasonal Phase Boundaries
        private const int EARLY_YEAR_END = 90;
        private const int PRODUCTION_END = 270;
        private const int PRE_CHRISTMAS_END = 330;

        // Performance Limits
        private const int MAX_EVENTS_PER_FRAME = 100;
        private const int MAX_TICKS_PER_FRAME = 1000;

        // Validation Thresholds
        private const float TIME_SPEED_EPSILON = 0.001f;

        #endregion

        #region Singleton

        /// <summary>
        /// Singleton instance of the TimeManager.
        /// </summary>
        public static TimeManager Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [Header("Time Configuration")]
        [Tooltip("Number of ticks per second (default: 10 Hz)")]
        [SerializeField] private float tickRate = 10f;

        [Tooltip("Number of real seconds per in-game day (default: 60 seconds)")]
        [SerializeField] private float secondsPerDay = 60f;

        [Header("Initial State")]
        [Tooltip("Starting day of the year (1-365)")]
        [SerializeField] private int startingDay = 1;

        [Tooltip("Initial time speed multiplier (default: 1x)")]
        [SerializeField] private float initialTimeSpeed = 1f;

        [Tooltip("Start paused")]
        [SerializeField] private bool startPaused = false;

        #endregion

        #region Calendar Properties

        /// <summary>
        /// Current day of the year (1-365).
        /// </summary>
        public int CurrentDay { get; private set; }

        /// <summary>
        /// Current month (1-12).
        /// </summary>
        public int CurrentMonth { get; private set; }

        /// <summary>
        /// Day of the current month (1-30 or 1-35 for December).
        /// </summary>
        public int DayOfMonth { get; private set; }

        /// <summary>
        /// Current seasonal phase based on the day counter.
        /// </summary>
        public SeasonalPhase CurrentPhase { get; private set; }

        #endregion

        #region Time Speed Properties

        /// <summary>
        /// Whether time progression is currently paused.
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// Current time speed multiplier (1x = normal, 2x = fast, 5x = very fast).
        /// </summary>
        public float TimeSpeed { get; private set; }

        /// <summary>
        /// Delta time scaled by time speed multiplier (for gameplay systems).
        /// </summary>
        public float ScaledDeltaTime { get; private set; }

        /// <summary>
        /// Unscaled delta time (for UI and non-gameplay systems).
        /// </summary>
        public float UnscaledDeltaTime { get; private set; }

        #endregion

        #region Elapsed Time

        /// <summary>
        /// Total elapsed game time in seconds (affected by time speed).
        /// </summary>
        public float TotalGameTime { get; private set; }

        /// <summary>
        /// Total elapsed real time in seconds (not affected by time speed).
        /// </summary>
        public float TotalRealTime { get; private set; }

        #endregion

        #region Simulation Tick

        /// <summary>
        /// Tick rate in ticks per second (configurable).
        /// </summary>
        public float TickRate
        {
            get => tickRate;
            set
            {
                if (value <= 0f)
                {
                    LogWarning($"Invalid tick rate {value}. Must be > 0. Using minimum {MIN_TICK_RATE} Hz.");
                    value = MIN_TICK_RATE;
                }
                
                if (value > MAX_TICK_RATE)
                {
                    LogWarning($"Tick rate {value} exceeds recommended maximum. Clamping to {MAX_TICK_RATE} Hz for performance.");
                    value = MAX_TICK_RATE;
                }
                
                tickRate = value;
                _tickInterval = 1.0 / tickRate;
                LogVerbose($"Tick rate changed to {tickRate} Hz (interval: {_tickInterval:F4}s)");
            }
        }

        /// <summary>
        /// Number of ticks per second (derived from TickRate).
        /// </summary>
        public int TicksPerSecond => Mathf.RoundToInt(tickRate);

        #endregion

        #region Events

        /// <summary>
        /// Fired at fixed intervals for simulation updates.
        /// IMPORTANT: Always unsubscribe in OnDisable() or OnDestroy() to prevent memory leaks.
        /// </summary>
        /// <example>
        /// <code>
        /// private void OnEnable() => TimeManager.OnSimulationTick += HandleTick;
        /// private void OnDisable() => TimeManager.OnSimulationTick -= HandleTick;
        /// </code>
        /// </example>
        public static event Action OnSimulationTick;

        /// <summary>
        /// Fired when time speed changes.
        /// IMPORTANT: Always unsubscribe in OnDisable() or OnDestroy() to prevent memory leaks.
        /// </summary>
        public static event Action<float> OnTimeSpeedChanged;

        /// <summary>
        /// Fired when entering a new seasonal phase.
        /// IMPORTANT: Always unsubscribe in OnDisable() or OnDestroy() to prevent memory leaks.
        /// </summary>
        public static event Action<SeasonalPhase> OnSeasonalPhaseChanged;

        /// <summary>
        /// Fired when the day increments.
        /// IMPORTANT: Always unsubscribe in OnDisable() or OnDestroy() to prevent memory leaks.
        /// </summary>
        public static event Action<int> OnDayChanged;

        #endregion

        #region Private Fields

        // Tick accumulator for fixed timestep (using double for precision)
        private double _tickAccumulator = 0.0;
        private double _tickInterval = 0.1; // 10 Hz default

        // Event scheduling (Dictionary for O(1) lookups)
        private Dictionary<int, ScheduledEvent> _scheduledEvents = new Dictionary<int, ScheduledEvent>();
        private HashSet<int> _cancelledEventIds = new HashSet<int>();
        private int _nextEventId = 1;

        // Calendar optimization
        private float _nextDayThreshold = 0f;

        #endregion

        #region Logging Configuration
#if UNITY_EDITOR
        private const bool VERBOSE_LOGGING = true;
#else
        private const bool VERBOSE_LOGGING = false;
#endif

        private void LogVerbose(string message)
        {
            if (VERBOSE_LOGGING)
                Debug.Log($"[TimeManager] {message}");
        }

        private void LogInfo(string message)
        {
            Debug.Log($"[TimeManager] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[TimeManager] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[TimeManager] {message}");
        }
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                LogWarning("Multiple TimeManager instances detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Validate and initialize configuration
            if (tickRate <= 0f)
            {
                LogWarning($"Invalid tick rate {tickRate} in configuration. Using default 10 Hz.");
                tickRate = 10f;
            }
            else if (tickRate > MAX_TICK_RATE)
            {
                LogWarning($"Tick rate {tickRate} exceeds recommended maximum. Clamping to {MAX_TICK_RATE} Hz.");
                tickRate = MAX_TICK_RATE;
            }
            _tickInterval = 1.0 / tickRate;

            if (secondsPerDay <= 0f)
            {
                LogWarning($"Invalid secondsPerDay {secondsPerDay} in configuration. Using default 60 seconds.");
                secondsPerDay = 60f;
            }

            // Initialize calendar with validation
            if (startingDay < MIN_DAY || startingDay > MAX_DAY)
            {
                LogWarning($"Invalid starting day {startingDay} in configuration. Using day 1.");
                startingDay = 1;
            }
            CurrentDay = startingDay;
            UpdateMonthAndDay();
            UpdateSeasonalPhase();

            // Initialize day threshold
            _nextDayThreshold = CurrentDay * secondsPerDay;

            // Initialize time speed with validation
            if (float.IsNaN(initialTimeSpeed) || float.IsInfinity(initialTimeSpeed))
            {
                LogWarning($"Invalid initial time speed {initialTimeSpeed} in configuration. Using 1x.");
                initialTimeSpeed = 1f;
            }
            TimeSpeed = Mathf.Clamp(initialTimeSpeed, MIN_TIME_SPEED, MAX_TIME_SPEED);
            IsPaused = startPaused;

            // Initialize time tracking
            TotalGameTime = 0f;
            TotalRealTime = 0f;
            ScaledDeltaTime = 0f;
            UnscaledDeltaTime = 0f;

            LogInfo($"Initialized: Day {CurrentDay}, Phase {CurrentPhase}, Speed {TimeSpeed}x, TickRate: {tickRate} Hz");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ClearAllEventSubscriptions();
                Instance = null;
            }
        }

        #endregion

        #region Public Methods - Time Speed Control

        /// <summary>
        /// Sets the time speed multiplier.
        /// </summary>
        /// <param name="speed">Speed multiplier (0-10x, clamped automatically).</param>
        public void SetTimeSpeed(float speed)
        {
            if (float.IsNaN(speed) || float.IsInfinity(speed))
            {
                LogError($"Invalid time speed {speed} (NaN or Infinity). Using 1x as fallback.");
                speed = 1f;
            }
            else if (speed < MIN_TIME_SPEED)
            {
                LogWarning($"Invalid time speed {speed}. Must be >= {MIN_TIME_SPEED}. Using 1x as fallback.");
                speed = 1f;
            }
            else if (speed > MAX_TIME_SPEED)
            {
                LogWarning($"Time speed {speed} exceeds maximum of {MAX_TIME_SPEED}x. Clamping to {MAX_TIME_SPEED}x.");
                speed = MAX_TIME_SPEED;
            }

            float oldSpeed = TimeSpeed;
            TimeSpeed = speed;
            
            if (Mathf.Abs(oldSpeed - speed) > TIME_SPEED_EPSILON)
            {
                OnTimeSpeedChanged?.Invoke(speed);
                LogVerbose($"Time speed changed from {oldSpeed:F2}x to {speed:F2}x");
            }
        }

        /// <summary>
        /// Pauses time progression.
        /// </summary>
        public void Pause()
        {
            if (!IsPaused)
            {
                IsPaused = true;
                LogVerbose("Time paused");
            }
        }

        /// <summary>
        /// Resumes time progression.
        /// </summary>
        public void Resume()
        {
            if (IsPaused)
            {
                IsPaused = false;
                LogVerbose("Time resumed");
            }
        }

        /// <summary>
        /// Toggles pause state.
        /// </summary>
        public void TogglePause()
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }

        #endregion

        #region Public Methods - Event Scheduling

        /// <summary>
        /// Schedules an event to trigger after a specified delay.
        /// </summary>
        /// <param name="delaySeconds">Delay in game seconds.</param>
        /// <param name="callback">Callback to invoke when event triggers.</param>
        /// <returns>Handle to the scheduled event.</returns>
        public ScheduledEventHandle ScheduleEvent(float delaySeconds, Action callback)
        {
            return ScheduleEvent(delaySeconds, "Generic", callback);
        }

        /// <summary>
        /// Schedules an event to trigger after a specified delay with a type identifier for save/load support.
        /// </summary>
        /// <param name="delaySeconds">Delay in game seconds.</param>
        /// <param name="eventType">Event type identifier for save/load reconstruction.</param>
        /// <param name="callback">Callback to invoke when event triggers.</param>
        /// <returns>Handle to the scheduled event.</returns>
        public ScheduledEventHandle ScheduleEvent(float delaySeconds, string eventType, Action callback)
        {
            if (callback == null)
            {
                LogError("Cannot schedule event with null callback. Event will not be created.");
                return new ScheduledEventHandle { EventId = -1 };
            }

            if (float.IsNaN(delaySeconds) || float.IsInfinity(delaySeconds))
            {
                LogError($"Invalid delay {delaySeconds} (NaN or Infinity). Event will not be created.");
                return new ScheduledEventHandle { EventId = -1 };
            }

            if (delaySeconds < 0f)
            {
                LogWarning($"Negative delay {delaySeconds:F2}s. Scheduling for immediate execution (0s delay).");
                delaySeconds = 0f;
            }

            if (delaySeconds > SECONDS_PER_YEAR)
            {
                LogWarning($"Very large delay {delaySeconds:F0}s (>{SECONDS_PER_YEAR}s). This may never trigger.");
            }

            var evt = new ScheduledEvent(
                eventId: _nextEventId++,
                triggerTime: TotalGameTime + delaySeconds,
                triggerDay: null,
                callback: callback,
                eventType: eventType
            );

            _scheduledEvents.Add(evt.EventId, evt);
            LogVerbose($"Scheduled event {evt.EventId} (type: {eventType}) to trigger in {delaySeconds:F2}s");
            return new ScheduledEventHandle { EventId = evt.EventId };
        }

        /// <summary>
        /// Schedules an event to trigger at a specific game day.
        /// </summary>
        /// <param name="day">Target day (1-365).</param>
        /// <param name="callback">Callback to invoke when event triggers.</param>
        /// <returns>Handle to the scheduled event.</returns>
        public ScheduledEventHandle ScheduleEventAtDay(int day, Action callback)
        {
            return ScheduleEventAtDay(day, "Generic", callback);
        }

        /// <summary>
        /// Schedules an event to trigger at a specific game day with a type identifier for save/load support.
        /// </summary>
        /// <param name="day">Target day (1-365).</param>
        /// <param name="eventType">Event type identifier for save/load reconstruction.</param>
        /// <param name="callback">Callback to invoke when event triggers.</param>
        /// <returns>Handle to the scheduled event.</returns>
        public ScheduledEventHandle ScheduleEventAtDay(int day, string eventType, Action callback)
        {
            if (callback == null)
            {
                LogError("Cannot schedule event with null callback. Event will not be created.");
                return new ScheduledEventHandle { EventId = -1 };
            }

            if (day < MIN_DAY || day > MAX_DAY)
            {
                LogWarning($"Invalid day {day}. Must be {MIN_DAY}-{MAX_DAY}. Clamping to valid range.");
                day = Mathf.Clamp(day, MIN_DAY, MAX_DAY);
            }

            if (day < CurrentDay)
            {
                LogWarning($"Scheduling event for day {day}, which is in the past (current day: {CurrentDay}). Event will trigger immediately.");
            }

            var evt = new ScheduledEvent(
                eventId: _nextEventId++,
                triggerTime: 0f,
                triggerDay: day,
                callback: callback,
                eventType: eventType
            );

            _scheduledEvents.Add(evt.EventId, evt);
            LogVerbose($"Scheduled event {evt.EventId} (type: {eventType}) to trigger on day {day}");
            return new ScheduledEventHandle { EventId = evt.EventId };
        }

        /// <summary>
        /// Cancels a scheduled event.
        /// </summary>
        /// <param name="handle">Handle to the event to cancel.</param>
        public void CancelScheduledEvent(ScheduledEventHandle handle)
        {
            if (!handle.IsValid)
            {
                LogWarning($"Cannot cancel event with invalid handle (EventId: {handle.EventId}).");
                return;
            }

            if (_scheduledEvents.ContainsKey(handle.EventId))
            {
                _cancelledEventIds.Add(handle.EventId);
                LogVerbose($"Cancelled event {handle.EventId}");
            }
            else
            {
                LogWarning($"Event {handle.EventId} not found for cancellation. It may have already been executed or removed.");
            }
        }

        #endregion

        #region Public Methods - Save/Load

        /// <summary>
        /// Serializes current time state to a data structure.
        /// </summary>
        /// <returns>Save data containing current time state.</returns>
        public TimeSaveData GetSaveData()
        {
            var saveData = new TimeSaveData
            {
                currentDay = CurrentDay,
                totalGameTime = TotalGameTime,
                totalRealTime = TotalRealTime,
                timeSpeed = TimeSpeed,
                isPaused = IsPaused,
                scheduledEvents = new ScheduledEventSaveData[_scheduledEvents.Count]
            };

            int index = 0;
            foreach (var kvp in _scheduledEvents)
            {
                var evt = kvp.Value;
                saveData.scheduledEvents[index++] = new ScheduledEventSaveData
                {
                    eventId = evt.EventId,
                    triggerTime = evt.TriggerTime,
                    triggerDay = evt.TriggerDay ?? -1,
                    eventType = evt.EventType
                };
            }

            return saveData;
        }

        /// <summary>
        /// Restores time state from saved data.
        /// </summary>
        /// <param name="data">Save data to load.</param>
        public void LoadSaveData(TimeSaveData data)
        {
            if (data == null)
            {
                LogError("Cannot load null save data. Resetting to default values.");
                ResetToDefaults();
                return;
            }

            bool hasErrors = false;

            // Validate day range
            if (data.currentDay < MIN_DAY || data.currentDay > MAX_DAY)
            {
                LogWarning($"Invalid day {data.currentDay} in save data (must be {MIN_DAY}-{MAX_DAY}). Clamping to valid range.");
                data.currentDay = Mathf.Clamp(data.currentDay, MIN_DAY, MAX_DAY);
                hasErrors = true;
            }

            // Validate time values
            if (float.IsNaN(data.totalGameTime) || float.IsInfinity(data.totalGameTime))
            {
                LogError($"Invalid totalGameTime {data.totalGameTime} (NaN or Infinity). Resetting to 0.");
                data.totalGameTime = 0f;
                hasErrors = true;
            }
            else if (data.totalGameTime < 0f)
            {
                LogWarning($"Negative totalGameTime {data.totalGameTime} in save data. Resetting to 0.");
                data.totalGameTime = 0f;
                hasErrors = true;
            }

            if (float.IsNaN(data.totalRealTime) || float.IsInfinity(data.totalRealTime))
            {
                LogError($"Invalid totalRealTime {data.totalRealTime} (NaN or Infinity). Resetting to 0.");
                data.totalRealTime = 0f;
                hasErrors = true;
            }
            else if (data.totalRealTime < 0f)
            {
                LogWarning($"Negative totalRealTime {data.totalRealTime} in save data. Resetting to 0.");
                data.totalRealTime = 0f;
                hasErrors = true;
            }

            // Validate time speed
            if (float.IsNaN(data.timeSpeed) || float.IsInfinity(data.timeSpeed))
            {
                LogError($"Invalid timeSpeed {data.timeSpeed} (NaN or Infinity). Using 1x as fallback.");
                data.timeSpeed = 1f;
                hasErrors = true;
            }
            else if (data.timeSpeed < MIN_TIME_SPEED || data.timeSpeed > MAX_TIME_SPEED)
            {
                LogWarning($"Invalid timeSpeed {data.timeSpeed} in save data (must be {MIN_TIME_SPEED}-{MAX_TIME_SPEED}). Clamping to valid range.");
                data.timeSpeed = Mathf.Clamp(data.timeSpeed, MIN_TIME_SPEED, MAX_TIME_SPEED);
                hasErrors = true;
            }

            // Validate scheduled events array
            if (data.scheduledEvents == null)
            {
                LogWarning("Scheduled events array is null in save data. Initializing empty array.");
                data.scheduledEvents = new ScheduledEventSaveData[0];
                hasErrors = true;
            }

            // Apply validated data
            CurrentDay = data.currentDay;
            TotalGameTime = data.totalGameTime;
            TotalRealTime = data.totalRealTime;
            UpdateMonthAndDay();
            UpdateSeasonalPhase();
            SetTimeSpeed(data.timeSpeed);

            // Update day threshold
            _nextDayThreshold = CurrentDay * secondsPerDay;

            if (data.isPaused)
                Pause();
            else
                Resume();

            // Restore scheduled events using factory
            _scheduledEvents.Clear();
            _cancelledEventIds.Clear();
            
            int restoredCount = 0;
            foreach (var eventData in data.scheduledEvents)
            {
                Action callback = ScheduledEventFactory.CreateCallback(eventData.eventType);
                if (callback != null)
                {
                    var evt = new ScheduledEvent(
                        eventId: eventData.eventId,
                        triggerTime: eventData.triggerTime,
                        triggerDay: eventData.triggerDay >= 0 ? (int?)eventData.triggerDay : null,
                        callback: callback,
                        eventType: eventData.eventType
                    );
                    _scheduledEvents.Add(evt.EventId, evt);
                    
                    // Update next event ID to avoid conflicts
                    if (evt.EventId >= _nextEventId)
                        _nextEventId = evt.EventId + 1;
                    
                    restoredCount++;
                }
            }

            if (hasErrors)
            {
                LogWarning($"TimeManager loaded with validation errors. Some values were corrected. Day {CurrentDay}, Phase {CurrentPhase}, Speed {TimeSpeed}x, Events: {restoredCount}/{data.scheduledEvents.Length}");
            }
            else
            {
                LogInfo($"TimeManager loaded successfully: Day {CurrentDay}, Phase {CurrentPhase}, Speed {TimeSpeed}x, Paused: {IsPaused}, Events: {restoredCount}");
            }
        }

        #endregion

        #region Public Methods - Event Cleanup

        /// <summary>
        /// Clears all event subscriptions. Call this when resetting the game or loading a new scene.
        /// This prevents memory leaks from dangling event subscriptions.
        /// </summary>
        public void ClearAllEventSubscriptions()
        {
            OnSimulationTick = null;
            OnTimeSpeedChanged = null;
            OnSeasonalPhaseChanged = null;
            OnDayChanged = null;
            LogVerbose("All event subscriptions cleared");
        }

#if UNITY_EDITOR
        /// <summary>
        /// Resets the TimeManager to a clean state for testing.
        /// Only available in editor/test builds.
        /// </summary>
        public void ResetForTesting()
        {
            ResetToDefaults();
            ClearAllEventSubscriptions();
            _scheduledEvents.Clear();
            _cancelledEventIds.Clear();
            _nextEventId = 1;
            _tickAccumulator = 0.0;
            _nextDayThreshold = CurrentDay * secondsPerDay;
            LogInfo("TimeManager reset for testing");
        }
#endif

        #endregion

        #region Unity Update Loop

        private void Update()
        {
            if (IsPaused)
                return;

            // Update delta times
            UnscaledDeltaTime = Time.deltaTime;
            ScaledDeltaTime = UnscaledDeltaTime * TimeSpeed;

            // Accumulate elapsed time
            TotalRealTime += UnscaledDeltaTime;
            TotalGameTime += ScaledDeltaTime;

            // Update calendar (check for day changes)
            UpdateCalendar();

            // Process simulation ticks
            ProcessSimulationTicks();

            // Process scheduled events
            ProcessScheduledEvents();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Resets time manager to default values.
        /// </summary>
        private void ResetToDefaults()
        {
            CurrentDay = 1;
            TotalGameTime = 0f;
            TotalRealTime = 0f;
            TimeSpeed = 1f;
            IsPaused = false;
            UpdateMonthAndDay();
            UpdateSeasonalPhase();
            _scheduledEvents.Clear();
            _cancelledEventIds.Clear();
            _nextDayThreshold = secondsPerDay;
        }

        /// <summary>
        /// Updates the calendar by calculating the current day from total game time.
        /// Fires OnDayChanged event when the day increments.
        /// Optimized to avoid calculations when day hasn't changed.
        /// </summary>
        private void UpdateCalendar()
        {
            // Early exit if we haven't reached the next day threshold
            if (TotalGameTime < _nextDayThreshold)
                return;
            
            // Calculate new day
            int newDay = Mathf.FloorToInt(TotalGameTime / secondsPerDay) + 1;
            newDay = Mathf.Clamp(newDay, MIN_DAY, MAX_DAY);
            
            // Update threshold for next day
            _nextDayThreshold = newDay * secondsPerDay;

            if (newDay != CurrentDay)
            {
                CurrentDay = newDay;
                UpdateMonthAndDay();
                UpdateSeasonalPhase();
                OnDayChanged?.Invoke(CurrentDay);
                LogVerbose($"Day changed to {CurrentDay} (Month {CurrentMonth}, Day {DayOfMonth})");
            }
        }

        /// <summary>
        /// Processes simulation ticks using accumulator pattern with double precision.
        /// </summary>
        private void ProcessSimulationTicks()
        {
            _tickAccumulator += (double)ScaledDeltaTime;
            
            int ticksToProcess = 0;
            while (_tickAccumulator >= _tickInterval)
            {
                _tickAccumulator -= _tickInterval;
                ticksToProcess++;
                
                // Safety limit to prevent infinite loops from precision errors
                if (ticksToProcess > MAX_TICKS_PER_FRAME)
                {
                    LogError($"Tick accumulator overflow detected. Resetting. Accumulator: {_tickAccumulator}");
                    _tickAccumulator = 0.0;
                    break;
                }
            }
            
            // Invoke events after calculating count to avoid issues if handlers modify time
            for (int i = 0; i < ticksToProcess; i++)
            {
                try
                {
                    OnSimulationTick?.Invoke();
                }
                catch (Exception ex)
                {
                    LogError($"Error in simulation tick subscriber: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Processes scheduled events that are ready to trigger.
        /// Limits processing to MAX_EVENTS_PER_FRAME events per frame for performance.
        /// Uses Dictionary for O(1) lookups and HashSet for cancelled events.
        /// </summary>
        private void ProcessScheduledEvents()
        {
            int eventsProcessed = 0;
            var eventsToRemove = new List<int>();

            foreach (var kvp in _scheduledEvents)
            {
                if (eventsProcessed >= MAX_EVENTS_PER_FRAME)
                    break;

                int eventId = kvp.Key;
                var evt = kvp.Value;

                // Skip cancelled events
                if (_cancelledEventIds.Contains(eventId))
                {
                    eventsToRemove.Add(eventId);
                    continue;
                }

                bool shouldTrigger = evt.TriggerDay.HasValue 
                    ? CurrentDay >= evt.TriggerDay.Value 
                    : TotalGameTime >= evt.TriggerTime;

                if (shouldTrigger)
                {
                    try
                    {
                        evt.Callback?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error executing scheduled event {eventId}: {ex.Message}\n{ex.StackTrace}");
                    }
                    
                    eventsToRemove.Add(eventId);
                    eventsProcessed++;
                }
            }

            // Clean up processed and cancelled events
            foreach (int id in eventsToRemove)
            {
                _scheduledEvents.Remove(id);
                _cancelledEventIds.Remove(id);
            }
            
            if (eventsProcessed >= MAX_EVENTS_PER_FRAME && _scheduledEvents.Count > 0)
            {
                LogWarning($"Reached maximum events per frame limit ({MAX_EVENTS_PER_FRAME}). {_scheduledEvents.Count} events remaining.");
            }
        }

        /// <summary>
        /// Updates month and day-of-month based on current day.
        /// </summary>
        private void UpdateMonthAndDay()
        {
            // Simple 30-day months (except December with 35 days)
            if (CurrentDay <= PRE_CHRISTMAS_END)
            {
                CurrentMonth = ((CurrentDay - 1) / DAYS_PER_MONTH) + 1;
                DayOfMonth = ((CurrentDay - 1) % DAYS_PER_MONTH) + 1;
            }
            else
            {
                CurrentMonth = 12;
                DayOfMonth = CurrentDay - PRE_CHRISTMAS_END;
            }
        }

        /// <summary>
        /// Updates seasonal phase based on current day.
        /// </summary>
        private void UpdateSeasonalPhase()
        {
            SeasonalPhase newPhase;

            if (CurrentDay <= EARLY_YEAR_END)
                newPhase = SeasonalPhase.EarlyYear;
            else if (CurrentDay <= PRODUCTION_END)
                newPhase = SeasonalPhase.Production;
            else if (CurrentDay <= PRE_CHRISTMAS_END)
                newPhase = SeasonalPhase.PreChristmas;
            else
                newPhase = SeasonalPhase.ChristmasRush;

            if (newPhase != CurrentPhase)
            {
                CurrentPhase = newPhase;
                OnSeasonalPhaseChanged?.Invoke(newPhase);
                LogVerbose($"Seasonal phase changed to {newPhase}");
            }
        }

        #endregion
    }
}
