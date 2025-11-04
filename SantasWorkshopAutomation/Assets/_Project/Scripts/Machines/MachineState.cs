namespace SantasWorkshop.Machines
{
    /// <summary>
    /// Defines the operational states a machine can be in.
    /// </summary>
    public enum MachineState
    {
        /// <summary>
        /// Machine is ready but has no work to do.
        /// </summary>
        Idle,
        
        /// <summary>
        /// Machine has a recipe selected but is waiting for input resources.
        /// </summary>
        WaitingForInput,
        
        /// <summary>
        /// Machine is actively processing a recipe.
        /// </summary>
        Processing,
        
        /// <summary>
        /// Machine has completed processing but output buffer is full.
        /// </summary>
        WaitingForOutput,
        
        /// <summary>
        /// Machine has insufficient power to operate.
        /// </summary>
        NoPower,
        
        /// <summary>
        /// Machine has been manually disabled by the player.
        /// </summary>
        Disabled
    }
}
