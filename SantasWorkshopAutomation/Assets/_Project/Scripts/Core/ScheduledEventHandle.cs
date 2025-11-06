namespace SantasWorkshop.Core
{
    /// <summary>
    /// A handle to a scheduled event that can be used to cancel it.
    /// Provides a lightweight way to reference scheduled events without holding the full event object.
    /// </summary>
    public struct ScheduledEventHandle
    {
        /// <summary>
        /// The unique identifier of the scheduled event.
        /// </summary>
        public int EventId { get; internal set; }
        
        /// <summary>
        /// Whether this handle references a valid event.
        /// Returns false if the event ID is invalid (≤ 0).
        /// </summary>
        public bool IsValid => EventId > 0;
    }
}
