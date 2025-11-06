using System;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Represents a time-based event that triggers at a specific game time or after a delay.
    /// Immutable struct for better performance and safety.
    /// </summary>
    public readonly struct ScheduledEvent
    {
        /// <summary>
        /// Unique identifier for this event.
        /// </summary>
        public int EventId { get; }
        
        /// <summary>
        /// The total game time (in seconds) when this event should trigger.
        /// Used for delay-based scheduling.
        /// </summary>
        public float TriggerTime { get; }
        
        /// <summary>
        /// The game day (1-365) when this event should trigger.
        /// Null if using time-based triggering instead.
        /// </summary>
        public int? TriggerDay { get; }
        
        /// <summary>
        /// The callback action to invoke when the event triggers.
        /// </summary>
        public Action Callback { get; }
        
        /// <summary>
        /// String identifier for the event type, used for save/load support.
        /// </summary>
        public string EventType { get; }

        /// <summary>
        /// Creates a new ScheduledEvent.
        /// </summary>
        public ScheduledEvent(int eventId, float triggerTime, int? triggerDay, Action callback, string eventType = "Generic")
        {
            EventId = eventId;
            TriggerTime = triggerTime;
            TriggerDay = triggerDay;
            Callback = callback;
            EventType = eventType;
        }
    }
}
