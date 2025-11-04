namespace SantasWorkshop.Data
{
    /// <summary>
    /// Defines placement layers for vertical building support.
    /// Uses flags to allow objects to be placed on multiple layers.
    /// </summary>
    [System.Flags]
    public enum PlacementLayer
    {
        Ground = 1 << 0,      // Ground level (default)
        Elevated = 1 << 1,    // Elevated platforms
        Underground = 1 << 2  // Underground level
    }
}
