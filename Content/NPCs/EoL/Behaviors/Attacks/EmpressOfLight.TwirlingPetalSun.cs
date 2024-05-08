using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        public static int TwirlingPetalSun_TwirlTime => Utilities.SecondsToFrames(1.75f);

        public static int TwirlingPetalSun_FlareTransformTime => Utilities.SecondsToFrames(1.35f);

        public static int TwirlingPetalSun_FlareRetractTime => Utilities.SecondsToFrames(0.39f);

        public static int TwirlingPetalSun_BurstTime => Utilities.SecondsToFrames(0.1f);

        public static int TwirlingPetalSun_AttackTransitionDelay => Utilities.SecondsToFrames(2f);

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_TwirlingPetalSun()
        {
            StateMachine.RegisterTransition(EmpressAIType.TwirlingPetalSun, null, false, () =>
            {
                return AITimer >= TwirlingPetalSun_TwirlTime + TwirlingPetalSun_FlareTransformTime + TwirlingPetalSun_FlareRetractTime + TwirlingPetalSun_BurstTime + TwirlingPetalSun_AttackTransitionDelay;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.TwirlingPetalSun, DoBehavior_TwirlingPetalSun);
        }

        /// <summary>
        /// Performs the Empress' Twirling Petal Sun attack.
        /// </summary>
        public void DoBehavior_TwirlingPetalSun()
        {
            LeftHandFrame = EmpressHandFrame.HandPressedToChest;
            RightHandFrame = EmpressHandFrame.HandPressedToChest;

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;

            if (AITimer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item159 with { MaxInstances = 0 });

                float petalOffsetAngle = NPC.AngleTo(Target.Center);
                for (int i = 0; i < 8; i++)
                {
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DazzlingPetal>(), 200, 0f, -1, MathHelper.TwoPi * i / 8f + petalOffsetAngle);
                }
                AITimer = 2;
            }

            if (AITimer == TwirlingPetalSun_TwirlTime + TwirlingPetalSun_FlareTransformTime + TwirlingPetalSun_FlareRetractTime + TwirlingPetalSun_BurstTime)
            {
                SoundEngine.PlaySound(SoundID.Item74);
                for (int i = 0; i < 12; i++)
                {
                    Vector2 boltVelocity = (MathHelper.TwoPi * i / 12f).ToRotationVector2() * 12f + Main.rand.NextVector2Circular(3.5f, 3.5f);
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, boltVelocity, ModContent.ProjectileType<PrismaticBolt>(), 200, 0f, -1, NPC.target);
                }
            }

            NPC.Center = Vector2.Lerp(NPC.Center, Target.Center, 0.012f);
            NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.SafeDirectionTo(Target.Center) * 2.5f, 0.03f);
        }
    }
}
