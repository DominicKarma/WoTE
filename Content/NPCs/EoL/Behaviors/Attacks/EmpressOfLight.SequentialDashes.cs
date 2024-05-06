using System;
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
        public ref float SequentialDashes_DashCounter => ref NPC.ai[0];

        public ref float SequentialDashes_DashDirection => ref NPC.ai[1];

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_SequentialDashes()
        {
            StateMachine.RegisterTransition(EmpressAIType.SequentialDashes, null, false, () =>
            {
                return SequentialDashes_DashCounter >= 3f;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.SequentialDashes, DoBehavior_SequentialDashes);
        }

        /// <summary>
        /// Performs the Empress' SequentialDashes attack.
        /// </summary>
        public void DoBehavior_SequentialDashes()
        {
            int redirectTime = 20;
            int dashTime = 25;
            int slowDownTime = 20;

            if (AITimer <= redirectTime)
            {
                Vector2 hoverOffsetDirection = Target.SafeDirectionTo(NPC.Center);
                hoverOffsetDirection.Y = -MathF.Abs(hoverOffsetDirection.Y) * 0.91f;

                Vector2 hoverDestination = Target.Center + hoverOffsetDirection * new Vector2(600f, 400f);
                hoverDestination -= NPC.SafeDirectionTo(Target.Center) * Utilities.InverseLerp(-16f, -4f, redirectTime - AITimer).Squared() * 360f;

                NPC.SmoothFlyNear(hoverDestination, 0.12f, 0.9f);

                SequentialDashes_DashDirection = NPC.AngleTo(Target.Center);

                if (AITimer == redirectTime)
                {
                    SoundEngine.PlaySound(SoundID.Item163 with { MaxInstances = 0 });

                    NPC.oldRot = new float[NPC.oldRot.Length];
                    NPC.oldPos = new Vector2[NPC.oldPos.Length];

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 11; i++)
                        {
                            Vector2 rainbowVelocity = NPC.SafeDirectionTo(Target.Center).RotatedBy(MathHelper.TwoPi * i / 11f) * 12f;
                            Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, rainbowVelocity, ProjectileID.HallowBossLastingRainbow, 150, 0f, -1, 0f, i / 7f);
                        }
                    }
                }
            }
            else if (AITimer <= redirectTime + dashTime)
            {
                float dashInterpolant = Utilities.InverseLerp(0f, 9f, AITimer - redirectTime);
                NPC.velocity = Vector2.Lerp(NPC.velocity, SequentialDashes_DashDirection.ToRotationVector2() * 120f, dashInterpolant * 0.13f);
                DashAfterimageInterpolant = MathHelper.Lerp(DashAfterimageInterpolant, 1f, 0.3f);
            }
            else if (AITimer <= redirectTime + dashTime + slowDownTime)
            {
                NPC.velocity *= 0.81f;
                DashAfterimageInterpolant *= 0.81f;
            }
            else
            {
                AITimer = 0;
                TeleportTo(Target.Center + Main.rand.NextVector2CircularEdge(420f, 420f));
            }

            LeftHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
            RightHandFrame = EmpressHandFrame.OutstretchedDownwardHand;

            NPC.spriteDirection = NPC.velocity.X.NonZeroSign();
            NPC.rotation = NPC.velocity.X * 0.0015f;
        }
    }
}
