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
        TwirlingPetalSun,
        RadialStarBurst,

        // Phase 2 attack states.
        BasicPrismaticBolts2,
        PrismaticBoltSpin,
        OrbitReleasedTerraprismas,
        LanceWallSupport,
        BeatSyncedBolts,
        PrismaticOverload,

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
