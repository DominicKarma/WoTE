using System;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.Particles.Metaballs;

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
        public static int SequentialDashes_RedirectTime => Utilities.SecondsToFrames(0.23f);

        /// <summary>
        /// How long the Empress spends dashing during her Sequential Dashes attack.
        /// </summary>
        public static int SequentialDashes_DashTime => Utilities.SecondsToFrames(0.32f);

        /// <summary>
        /// How long the Empress spends slowing down during her Sequential Dashes attack, after her dash.
        /// </summary>
        public static int SequentialDashes_SlowDownTime => Utilities.SecondsToFrames(0.15f);

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
                float dashInterpolant = Utilities.InverseLerp(0f, 10f, AITimer - SequentialDashes_RedirectTime);
                float targetDirectionErringInterpolant = Utilities.InverseLerp(0f, SequentialDashes_DashTime, AITimer - SequentialDashes_RedirectTime) * Utilities.InverseLerp(200f, 400f, NPC.Distance(Target.Center));
                Vector2 targetDirectionErring = NPC.SafeDirectionTo(Target.Center) * targetDirectionErringInterpolant * 150f;
                Vector2 idealVelocity = SequentialDashes_DashDirection.ToRotationVector2() * 125f + targetDirectionErring;

                NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, dashInterpolant * 0.2f);
                NPC.damage = NPC.defDamage;
                DashAfterimageInterpolant = MathHelper.Lerp(DashAfterimageInterpolant, 1f, 0.3f);

                if (AITimer % 5 == 0)
                    ModContent.GetInstance<DistortionMetaball>().CreateParticle(NPC.Center + Main.rand.NextVector2Circular(75f, 75f) - NPC.velocity, Vector2.Zero, 120f, 1f, 0.1f, 0.014f);
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
                TeleportTo(Target.Center + Main.rand.NextVector2CircularEdge(500f, 420f), (int)(DefaultTeleportDuration * 1.15f));
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
            // Reset the old position and rotation arrays, to ensure that the dash starts off with a brand new
            // afterimage state.
            NPC.oldRot = new float[NPC.oldRot.Length];
            NPC.oldPos = new Vector2[NPC.oldPos.Length];

            // Release everlasting rainbows outward.
            // TODO -- Probably replace this with something better sometime later?
            SoundEngine.PlaySound(SoundID.Item160 with { MaxInstances = 0 });

            SequentialDashes_DashDirection = NPC.AngleTo(Target.Center);
            NPC.netUpdate = true;

            ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 7f);
        }
    }
}
