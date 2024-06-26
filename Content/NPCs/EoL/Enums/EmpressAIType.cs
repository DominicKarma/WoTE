﻿namespace WoTE.Content.NPCs.EoL
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
        VanillaPrismaticBolts,
        ButterflyBurstDashes,
        SpinSwirlRainbows,
        ConvergingTerraprismas,
        TwirlingPetalSun,
        RadialStarBurst,
        EventideLances,

        // Phase 2 attack states.
        VanillaPrismaticBolts2,
        PrismaticBoltSpin,
        OrbitReleasedTerraprismas,
        LanceWallSupport,
        BeatSyncedBolts,
        PrismaticOverload,

        // Intermediate states.
        Teleport,
        ResetCycle,

        // Enrage state. Used if the player kills a lacewing to summon the Empress.
        Enraged,

        // "The player died so I should probably go away" state.
        Vanish,

        // Death state.
        Die,

        // Useful count constant.
        Count
    }
}
