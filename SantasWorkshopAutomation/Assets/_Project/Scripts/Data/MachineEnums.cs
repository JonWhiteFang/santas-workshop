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
        /// <summary>
        /// Raw materials extracted from resource nodes (e.g., Wood, Iron Ore, Coal).
        /// </summary>
        RawMaterial,

        /// <summary>
        /// Processed materials refined from raw materials (e.g., Planks, Iron Ingots, Steel).
        /// </summary>
        Refined,

        /// <summary>
        /// Crafted components used in assembly (e.g., Gears, Circuits, Paint).
        /// </summary>
        Component,

        /// <summary>
        /// Specialized components for toy production (e.g., Wheels, Arms, Bodies).
        /// </summary>
        ToyPart,

        /// <summary>
        /// Complete finished toys ready for delivery (e.g., Wooden Train, Teddy Bear, Robot).
        /// </summary>
        FinishedToy,

        /// <summary>
        /// Magical resources and enchantments (e.g., Magic Dust, Enchanted Crystals).
        /// </summary>
        Magic,

        /// <summary>
        /// Resources used for power generation (e.g., Coal, Magic Crystals).
        /// </summary>
        Energy
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
