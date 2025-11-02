using SantasWorkshop.Data;

namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Core interface that all machines must implement.
    /// Defines the contract for machine lifecycle and state management.
    /// </summary>
    public interface IMachine
    {
        /// <summary>
        /// Gets the unique identifier for this machine type.
        /// </summary>
        string MachineId { get; }

        /// <summary>
        /// Gets the current operational state of the machine.
        /// </summary>
        MachineState State { get; }

        /// <summary>
        /// Initializes the machine with configuration data.
        /// Called once when the machine is first created or loaded.
        /// </summary>
        /// <param name="data">Machine configuration data</param>
        void Initialize(MachineData data);

        /// <summary>
        /// Updates the machine's logic each frame or simulation tick.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last tick</param>
        void Tick(float deltaTime);

        /// <summary>
        /// Shuts down the machine and cleans up resources.
        /// Called when the machine is destroyed or disabled.
        /// </summary>
        void Shutdown();
    }
}
