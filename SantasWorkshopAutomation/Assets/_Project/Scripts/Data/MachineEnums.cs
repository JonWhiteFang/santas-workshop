namespace SantasWorkshop.Data
{
    /// <summary>
    /// Represents the current operational state of a machine.
    /// </summary>
    public enum MachineState
    {
        /// <summary>
        /// Machine is not powered or has been disabled.
        /// </summary>
        Offline,

        /// <summary>
        /// Machine is powered but not actively working.
        /// </summary>
        Idle,

        /// <summary>
        /// Machine is actively processing or producing.
        /// </summary>
        Working,

        /// <summary>
        /// Machine is blocked due to full output or empty input.
        /// </summary>
        Blocked,

        /// <summary>
        /// Machine has encountered a configuration or runtime error.
        /// </summary>
        Error
    }

    /// <summary>
    /// Categories of resources in the game.
    /// </summary>
    public enum ResourceCategory
    {
        RawMaterial,    // Wood, stone, iron ore
        Refined,        // Planks, iron ingots, plastic
        Component,      // Gears, circuits, fabric
        Toy,            // Final products
        Magic           // Magical resources
    }

    /// <summary>
    /// Categories of machines in the game.
    /// </summary>
    public enum MachineCategory
    {
        Extractor,      // Mining drills, harvesters
        Processor,      // Smelters, sawmills, refineries
        Assembler,      // Assembly machines
        Logistics,      // Conveyors, splitters, storage
        Power,          // Generators
        Utility         // Research labs, inspection stations
    }

    /// <summary>
    /// Research branches in the tech tree.
    /// </summary>
    public enum ResearchBranch
    {
        Automation,
        Energy,
        Materials,
        Toys,
        Logistics,
        ElfManagement,
        Magic,
        Aesthetics
    }
}
