namespace WoTE.Content.NPCs.EoL
{
    /// <summary>
    /// A representation of one of the Empress' AI states.
    /// </summary>
    public enum EmpressAIType
    {
        // Phase transition states.
        Awaken,
        Phase2Transition,

        // Attack states.
        SequentialDashes,
        BasicPrismaticBolts,
        PrismaticBoltDashes,
        ButterflyBurstDashes,
        OutwardRainbows,
        ConvergingTerraprismas,
        PrismaticBoltSpin,
        TwirlingPetalSun,
        RadialStarBurst,

        // Intermediate states.
        Teleport,
        ResetCycle,

        // Useful count constant.
        Count
    }
}
