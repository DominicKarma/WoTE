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

        // Phase 2 attack states.
        BasicPrismaticBolts2,
        DazzlingTornadoes,
        OrbitReleasedTerraprismas,
        LanceWallSupport,
        BeatSyncedBolts,

        // Intermediate states.
        Teleport,
        ResetCycle,

        // "The player died so I should probably go away" state.
        Vanish,

        // Death state.
        Die,

        // Useful count constant.
        Count
    }
}
