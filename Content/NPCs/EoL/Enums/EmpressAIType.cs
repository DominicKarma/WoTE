namespace WoTE.Content.NPCs.EoL
{
    /// <summary>
    /// A representation of one of the Empress' AI states.
    /// </summary>
    public enum EmpressAIType
    {
        Awaken,

        // Attack states.
        SequentialDashes,
        BasicPrismaticBolts,
        ButterflyBurstDashes,
        ReleaseLanceWallsFromBelow,
        OutwardRainbows,
        ConvergingTerraprismas,
        TwirlingPetalSun,

        // Intermediate states.
        Teleport,
        ResetCycle,

        // Useful count constant.
        Count
    }
}
