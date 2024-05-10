﻿using System;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;
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
        public static int Phase2Transition_StayInvisibleTime => Utilities.SecondsToFrames(5.4f);

        /// <summary>
        /// The life ratio at which the Emperss transitions to her second phase.
        /// </summary>
        public static float Phase2LifeRatio => 0.6f;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_Phase2Transition()
        {
            StateMachine.RegisterTransition(EmpressAIType.Phase2Transition, null, false, () =>
            {
                return AITimer >= Phase2Transition_DisappearTime + Phase2Transition_StayInvisibleTime;
            });
            StateMachine.ApplyToAllStatesExcept(state =>
            {
                StateMachine.RegisterTransition(state, EmpressAIType.Phase2Transition, false, () => EnterPhase2AfterNextAttack);
            }, EmpressAIType.Phase2Transition);

            StateMachine.RegisterStateBehavior(EmpressAIType.Phase2Transition, DoBehavior_Phase2Transition);
        }

        /// <summary>
        /// Performs the Empress' second phase transition state.
        /// </summary>
        public void DoBehavior_Phase2Transition()
        {
            LeftHandFrame = EmpressHandFrame.HandPressedToChest;
            RightHandFrame = EmpressHandFrame.HandPressedToChest;
            NPC.dontTakeDamage = true;
            NPC.ShowNameOnHover = false;

            if (AITimer <= Phase2Transition_DisappearTime)
            {
                if (AITimer == 1)
                {
                    NPC.velocity -= NPC.SafeDirectionTo(Target.Center) * 70f;
                    SoundEngine.PlaySound(SoundID.NPCHit1);
                    SoundEngine.PlaySound(SoundID.Item160);
                    ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 13f);
                }

                float disapperInterpolant = Utilities.InverseLerp(0f, Phase2Transition_DisappearTime, AITimer);
                float opacitySwell = MathHelper.Lerp(1f, 1.6f, MathF.Sin(MathHelper.Pi * disapperInterpolant * 4f));

                NPC.velocity *= 0.85f;
                NPC.Opacity = Utilities.Saturate((1f - disapperInterpolant) * opacitySwell);
                return;
            }

            Phase2 = true;
            NPC.Opacity = 0f;
            NPC.Center = Target.Center - Vector2.UnitY * 375f;
            IdealDrizzleVolume = StandardDrizzleVolume + Utilities.InverseLerp(0f, 120f, AITimer - Phase2Transition_DisappearTime) * 0.3f;
        }
    }
}