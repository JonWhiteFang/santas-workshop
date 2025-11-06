using System;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Serializable data structure for persisting scheduled event metadata.
    /// Note: Callbacks cannot be serialized, so eventType is used to reconstruct them on load.
    /// </summary>
    [Serializable]
    public struct ScheduledEventSaveData
    {
        /// <summary>
        /// The unique identifier of the scheduled event.
        /// </summary>
        public int eventId;
        
        /// <summary>
        /// The total game time (in seconds) when this event should trigger.
        /// </summary>
        public float triggerTime;
        
        /// <summary>
        /// The game day (1-365) when this event should trigger.
        /// -1 if using time-based triggering instead.
        /// </summary>
        public int triggerDay;
        
        /// <summary>
        /// String identifier for the event type, used to reconstruct the callback on load.
        /// Examples: "ResearchComplete", "MissionDeadline", "SeasonalTransition"
        /// </summary>
        public string eventType;
    }
}
