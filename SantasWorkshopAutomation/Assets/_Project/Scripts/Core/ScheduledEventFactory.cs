using System;
using System.Collections.Generic;
using UnityEngine;

namespace SantasWorkshop.Core
{
    /// <summary>
    /// Factory for reconstructing scheduled events from save data.
    /// Register event types here to enable save/load support for scheduled events.
    /// 
    /// Usage:
    /// 1. Register your event type: ScheduledEventFactory.RegisterEventType("MyEvent", () => MyCallback);
    /// 2. Schedule with type: TimeManager.Instance.ScheduleEvent(delay, "MyEvent", MyCallback);
    /// 3. Events will be automatically restored on load.
    /// </summary>
    public static class ScheduledEventFactory
    {
        private static Dictionary<string, Func<Action>> _eventFactories = new Dictionary<string, Func<Action>>();

        static ScheduledEventFactory()
        {
            // Register built-in event types
            RegisterEventType("SeasonalTransition", () => () => 
            {
                Debug.Log("Seasonal transition event triggered");
            });
            
            RegisterEventType("DayTransition", () => () => 
            {
                Debug.Log("Day transition event triggered");
            });
        }

        /// <summary>
        /// Registers a factory function for a specific event type.
        /// The factory function should return a new Action delegate that will be invoked when the event triggers.
        /// </summary>
        /// <param name="eventType">Unique identifier for the event type.</param>
        /// <param name="factory">Factory function that creates the callback.</param>
        public static void RegisterEventType(string eventType, Func<Action> factory)
        {
            if (string.IsNullOrEmpty(eventType))
            {
                Debug.LogError("Cannot register event type with null or empty name");
                return;
            }

            if (factory == null)
            {
                Debug.LogError($"Cannot register null factory for event type '{eventType}'");
                return;
            }

            _eventFactories[eventType] = factory;
            Debug.Log($"Registered event type: {eventType}");
        }

        /// <summary>
        /// Creates a callback for the specified event type.
        /// Returns null if the event type is not registered.
        /// </summary>
        /// <param name="eventType">The event type identifier.</param>
        /// <returns>A new Action delegate, or null if the type is not registered.</returns>
        public static Action CreateCallback(string eventType)
        {
            if (string.IsNullOrEmpty(eventType))
            {
                Debug.LogWarning("Cannot create callback for null or empty event type");
                return null;
            }

            if (_eventFactories.TryGetValue(eventType, out var factory))
            {
                return factory();
            }
            
            Debug.LogWarning($"Unknown event type '{eventType}'. Event will not be restored. Register it with ScheduledEventFactory.RegisterEventType()");
            return null;
        }

        /// <summary>
        /// Checks if an event type is registered.
        /// </summary>
        /// <param name="eventType">The event type identifier.</param>
        /// <returns>True if the event type is registered.</returns>
        public static bool IsRegistered(string eventType)
        {
            return !string.IsNullOrEmpty(eventType) && _eventFactories.ContainsKey(eventType);
        }

        /// <summary>
        /// Clears all registered event types. Use for testing only.
        /// </summary>
        public static void ClearRegistrations()
        {
            _eventFactories.Clear();
            Debug.Log("Cleared all event type registrations");
        }

        /// <summary>
        /// Gets the count of registered event types.
        /// </summary>
        public static int RegisteredCount => _eventFactories.Count;
    }
}
