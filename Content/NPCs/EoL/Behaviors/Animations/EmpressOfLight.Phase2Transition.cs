﻿using System;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// Whether the Empress is in phase 1 but would like to enter phase 2.
        /// </summary>
        public bool EnterPhase2AfterNextAttack => Phase <= 0 && NPC.life <= NPC.lifeMax * Phase2LifeRatio;

        /// <summary>
        /// Whether the Empress is currently in phase 2 or not.
        /// </summary>
        public bool Phase2
        {
            get => Phase >= 1;
            set => Phase = value ? Math.Max(1, Phase) : 0;
        }

        /// <summary>
        /// How long the Empress spends disappearing during her second phase transition.
        /// </summary>
        public static int Phase2Transition_DisappearTime => Utilities.SecondsToFrames(2.5f);

        /// <summary>
        /// How long the Empress spends invisible as the rain pours during her second phase transition.
        /// </summary>
        public static int Phase2Transition_StayInvisibleTime => Utilities.SecondsToFrames(7.5f);

        /// <summary>
        /// The life ratio at which the Emperss transitions to her second phase.
        /// </summary>
        public static float Phase2LifeRatio => 0.6f;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_Phase2Transition()
        {
            StateMachine.RegisterTransition(EmpressAIType.Phase2Transition, EmpressAIType.OrbitReleasedTerraprismas, false, () =>
            {
                return AITimer >= 999999;
            });
            StateMachine.ApplyToAllStatesExcept(state =>
            {
                StateMachine.RegisterTransition(state, EmpressAIType.Phase2Transition, false, () => EnterPhase2AfterNextAttack && CurrentState != EmpressAIType.ButterflyBurstDashes);
            }, EmpressAIType.Phase2Transition, EmpressAIType.Die);

            StateMachine.RegisterStateBehavior(EmpressAIType.Phase2Transition, DoBehavior_Phase2Transition);
        }

        /// <summary>
        /// Performs the Empress' second phase transition state.
        /// </summary>
        public void DoBehavior_Phase2Transition()
        {
            if (Main.mouseRight && Main.mouseRightRelease)
                AITimer = 0;

            ZPosition = Utilities.InverseLerp(0f, 35f, AITimer).Squared() * 2.4f;
            NPC.SmoothFlyNear(Target.Center - Vector2.UnitY * 400f, ZPosition * 0.1f, 1f - ZPosition * 0.15f);

            LeftHandFrame = EmpressHandFrame.HandPressedToChest;
            RightHandFrame = EmpressHandFrame.HandPressedToChest;
            NPC.dontTakeDamage = true;
            NPC.ShowNameOnHover = false;
            IdealDrizzleVolume = StandardDrizzleVolume + Utilities.InverseLerp(0f, 120f, AITimer - Phase2Transition_DisappearTime) * 0.3f;
        }
    }
}
