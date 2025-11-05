namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Interface for entities that consume power from the power grid.
    /// </summary>
    public interface IPowerConsumer
    {
        /// <summary>
        /// Gets the current power consumption in watts.
        /// </summary>
        float PowerConsumption { get; }
        
        /// <summary>
        /// Gets whether the consumer currently has sufficient power.
        /// </summary>
        bool IsPowered { get; }
        
        /// <summary>
        /// Sets the powered state of the consumer.
        /// Called by the power grid when power availability changes.
        /// </summary>
        /// <param name="powered">True if power is available, false otherwise.</param>
        void SetPowered(bool powered);
    }
}
