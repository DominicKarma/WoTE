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
        /// <summary>
        /// The amount of dashes the Empress has performed so far during the Sequential Dashes attack.
        /// </summary>
        public ref float SequentialDashes_DashCounter => ref NPC.ai[0];

        /// <summary>
        /// The direction the Empress will attempt to dash in during the Sequential Dashes attack when she's ready.
        /// </summary>
        public ref float SequentialDashes_DashDirection => ref NPC.ai[1];

        /// <summary>
        /// How long the Empress spends redirecting during her Sequential Dashes attack.
        /// </summary>
        public static int SequentialDashes_RedirectTime => Utilities.SecondsToFrames(0.333f);

        /// <summary>
        /// How long the Empress spends dashing during her Sequential Dashes attack.
        /// </summary>
        public static int SequentialDashes_DashTime => Utilities.SecondsToFrames(0.433f);

        /// <summary>
        /// How long the Empress spends slowing down during her Sequential Dashes attack, after her dash.
        /// </summary>
        public static int SequentialDashes_SlowDownTime => Utilities.SecondsToFrames(0.433f);

        /// <summary>
        /// The amount of dashes that the Empress should perform during her Sequential Dashes attack before choosing a new attack. 
        /// </summary>
        public static int SequentialDashes_DashCount => 3;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_SequentialDashes()
        {
            StateMachine.RegisterTransition(EmpressAIType.SequentialDashes, null, false, () =>
            {
                return SequentialDashes_DashCounter >= SequentialDashes_DashCount;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.SequentialDashes, DoBehavior_SequentialDashes);
        }

        /// <summary>
        /// Performs the Empress' Sequential Dashes attack.
        /// </summary>
        public void DoBehavior_SequentialDashes()
        {
            if (AITimer <= SequentialDashes_RedirectTime)
            {
                DoBehavior_SequentialDashes_Redirect();

                if (AITimer == SequentialDashes_RedirectTime)
                    DoBehavior_SequentialDashes_PerformDashEffects();
            }
            else if (AITimer <= SequentialDashes_RedirectTime + SequentialDashes_DashTime)
            {
                float dashInterpolant = Utilities.InverseLerp(0f, 9f, AITimer - SequentialDashes_RedirectTime);
                NPC.velocity = Vector2.Lerp(NPC.velocity, SequentialDashes_DashDirection.ToRotationVector2() * 120f, dashInterpolant * 0.13f);
                DashAfterimageInterpolant = MathHelper.Lerp(DashAfterimageInterpolant, 1f, 0.3f);
            }
            else if (AITimer <= SequentialDashes_RedirectTime + SequentialDashes_DashTime + SequentialDashes_SlowDownTime)
            {
                NPC.velocity *= 0.81f;
                DashAfterimageInterpolant *= 0.81f;
            }
            else
            {
                AITimer = 0;
                SequentialDashes_DashCounter++;
                TeleportTo(Target.Center + Main.rand.NextVector2CircularEdge(500f, 420f));
                NPC.netUpdate = true;
            }

            LeftHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
            RightHandFrame = EmpressHandFrame.OutstretchedDownwardHand;

            NPC.spriteDirection = NPC.velocity.X.NonZeroSign();
            NPC.rotation = NPC.velocity.X * 0.00175f;
        }

        /// <summary>
        /// Performs initial redirecting behaviors for the Empress' Sequential Dashes attack.
        /// </summary>
        public void DoBehavior_SequentialDashes_Redirect()
        {
            // Determine which direction the Empress should attempt to hover relative to the target.
            // The abs(x) * -0.92 calculation serves two functions:
            // 1. It ensures that the Empress always attempts to stay above the player, not below them.
            // 2. It makes the vertical hover offset regress to a horizontal one.
            Vector2 hoverOffsetDirection = Target.SafeDirectionTo(NPC.Center);
            hoverOffsetDirection.Y = MathF.Abs(hoverOffsetDirection.Y) * -0.92f;

            float backwardsWindUpOffset = Utilities.InverseLerp(-16f, -4f, SequentialDashes_RedirectTime - AITimer).Squared() * 360f;
            Vector2 hoverDestination = Target.Center + hoverOffsetDirection * new Vector2(600f, 400f) - NPC.SafeDirectionTo(Target.Center) * backwardsWindUpOffset;

            NPC.SmoothFlyNear(hoverDestination, 0.125f, 0.875f);
        }

        /// <summary>
        /// Performs frame-one effects for the Empress' dashes during the Sequential Dashes attack.
        /// </summary>
        public void DoBehavior_SequentialDashes_PerformDashEffects()
        {
            SoundEngine.PlaySound(SoundID.Item163 with { MaxInstances = 0 });

            // Reset the old position and rotation arrays, to ensure that the dash starts off with a brand new
            // afterimage state.
            NPC.oldRot = new float[NPC.oldRot.Length];
            NPC.oldPos = new Vector2[NPC.oldPos.Length];

            // Release everlasting rainbows outward.
            // TODO -- Probably replace this with something better sometime later?
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 11; i++)
                {
                    Vector2 rainbowVelocity = NPC.SafeDirectionTo(Target.Center).RotatedBy(MathHelper.TwoPi * i / 11f) * 12f;
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, rainbowVelocity, ProjectileID.HallowBossLastingRainbow, 150, 0f, -1, 0f, i / 7f);
                }
            }

            SequentialDashes_DashDirection = NPC.AngleTo(Target.Center);
            NPC.netUpdate = true;
        }
    }
}
