namespace SantasWorkshop.Core
{
    /// <summary>
    /// Represents the different phases of the in-game year.
    /// Each phase affects gameplay mechanics and production requirements.
    /// </summary>
    public enum SeasonalPhase
    {
        /// <summary>
        /// Days 1-90: Slow production, planning phase
        /// </summary>
        EarlyYear,
        
        /// <summary>
        /// Days 91-270: Normal production
        /// </summary>
        Production,
        
        /// <summary>
        /// Days 271-330: Ramping up production
        /// </summary>
        PreChristmas,
        
        /// <summary>
        /// Days 331-365: High demand, time pressure
        /// </summary>
        ChristmasRush
    }
}
