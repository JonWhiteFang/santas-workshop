using System;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Serializable data structure for persisting the TimeManager state.
    /// Contains all necessary information to restore the time system state on load.
    /// </summary>
    [Serializable]
    public class TimeSaveData
    {
        /// <summary>
        /// The current in-game day (1-365).
        /// </summary>
        public int currentDay;
        
        /// <summary>
        /// Total elapsed game time in seconds (affected by time speed).
        /// </summary>
        public float totalGameTime;
        
        /// <summary>
        /// Total elapsed real time in seconds (unaffected by time speed).
        /// </summary>
        public float totalRealTime;
        
        /// <summary>
        /// The current time speed multiplier (e.g., 1.0 = normal, 2.0 = 2x speed).
        /// </summary>
        public float timeSpeed;
        
        /// <summary>
        /// Whether the game time is currently paused.
        /// </summary>
        public bool isPaused;
        
        /// <summary>
        /// Array of scheduled events to be restored on load.
        /// Note: Callbacks cannot be serialized, only event metadata.
        /// </summary>
        public ScheduledEventSaveData[] scheduledEvents;
    }
}
