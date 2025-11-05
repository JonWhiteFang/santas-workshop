namespace SantasWorkshop.Data
{
    /// <summary>
    /// Enum representing different types of resources in the game.
    /// NOTE: This is a temporary enum for backward compatibility with example files.
    /// The actual resource system uses string-based resourceId in ResourceData ScriptableObjects.
    /// </summary>
    public enum ResourceType
    {
        // Raw Materials
        IronOre,
        CopperOre,
        Wood,
        Coal,
        Sand,
        Clay,
        
        // Refined Goods
        IronIngot,
        CopperIngot,
        SteelIngot,
        Planks,
        Glass,
        Bricks,
        
        // Components
        IronGear,
        CopperWire,
        Circuit,
        Paint_Red,
        Paint_Blue,
        Paint_Green,
        
        // Toy Parts
        WoodenWheel,
        PlasticBody,
        MetalArm,
        
        // Finished Toys
        WoodenTrain,
        TeddyBear,
        RobotToy,
        DollHouse,
        
        // Magic
        MagicDust,
        EnchantedCrystal,
        
        // Energy
        CoalFuel,
        MagicCrystalFuel
    }
}
