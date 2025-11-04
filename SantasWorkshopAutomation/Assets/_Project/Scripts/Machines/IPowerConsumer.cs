namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Interface for machines that consume electrical power.
    /// Allows the power grid system to manage electricity distribution.
    /// </summary>
    public interface IPowerConsumer
    {
        /// <summary>
        /// Gets the current power consumption in watts.
        /// Returns 0 if the machine is not actively consuming power.
        /// </summary>
        float PowerConsumption { get; }
        
        /// <summary>
        /// Gets whether the machine currently has sufficient power.
        /// </summary>
        bool IsPowered { get; }
        
        /// <summary>
        /// Sets the powered state of the machine.
        /// Called by the power grid when power availability changes.
        /// </summary>
        /// <param name="powered">True if power is available, false otherwise.</param>
        void SetPowered(bool powered);
    }
}
