using System;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        public static int Phase2Transition_DisappearTime => Utilities.SecondsToFrames(1.5f);

        public static int Phase2Transition_StayInvisibleTime => Utilities.SecondsToFrames(6f);

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_Phase2Transition()
        {
            StateMachine.RegisterTransition(EmpressAIType.Phase2Transition, null, false, () =>
            {
                return AITimer >= Phase2Transition_DisappearTime + Phase2Transition_StayInvisibleTime;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.Phase2Transition, DoBehavior_Phase2Transition);
        }

        /// <summary>
        /// Performs the Empress' Phase 2 Transition state.
        /// </summary>
        public void DoBehavior_Phase2Transition()
        {
            NPC.dontTakeDamage = true;
            NPC.ShowNameOnHover = false;

            if (AITimer <= Phase2Transition_DisappearTime)
            {
                float disapperInterpolant = Utilities.InverseLerp(0f, AITimer, Phase2Transition_DisappearTime);
                float opacitySwell = MathHelper.Lerp(1f, 1.67f, MathF.Sin(MathHelper.TwoPi * disapperInterpolant * 3f));

                NPC.velocity *= 0.65f;
                NPC.Opacity = Utilities.Saturate(disapperInterpolant * opacitySwell);
                return;
            }

            NPC.Opacity = 0f;
            NPC.Center = Target.Center - Vector2.UnitY * 375f;
        }
    }
}
