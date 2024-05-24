using System;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL.Projectiles;
using WoTE.Content.Particles.Metaballs;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// Whether the Empress started her Spin Swirl Rainbows attack on the right side of the target.
        /// </summary>
        public bool SpinSwirlRainbows_StartedOnRightSideOfTarget
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value.ToInt();
        }

        /// <summary>
        /// The hover offset angle during the Empress' Spin Swirl Rainbows attack.
        /// </summary>
        public ref float SpinSwirlRainbows_SpinAngleAngle => ref NPC.ai[2];

        /// <summary>
        /// How long the Empress spends redirecting during her Spin Swirl Rainbows attack.
        /// </summary>
        public int SpinSwirlRainbows_RainbowShootDelay => Utilities.SecondsToFrames(ByPhase(0.9f, 0.78f));

        /// <summary>
        /// How long the Empress spends releasing rainbows on her finger during her Spin Swirl Rainbows attack.
        /// </summary>
        public int SpinSwirlRainbows_RainbowShootTime => Utilities.SecondsToFrames(ByPhase(2f, 2.3f));

        /// <summary>
        /// How long the Empress waits before choosing a new attack during her Spin Swirl Rainbows attack.
        /// </summary>
        public static int SpinSwirlRainbows_AttackTransitionDelay => Utilities.SecondsToFrames(0.75f);

        /// <summary>
        /// How speed of rainbows summoned during the Empress' Spin Swirl Rainbows attack.
        /// </summary>
        public float SpinSwirlRainbows_RainbowSpeed => ByPhase(3f, 5f);

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_SpinSwirlRainbows()
        {
            StateMachine.RegisterTransition(EmpressAIType.SpinSwirlRainbows, null, false, () =>
            {
                return AITimer >= SpinSwirlRainbows_RainbowShootDelay + SpinSwirlRainbows_RainbowShootTime + SpinSwirlRainbows_AttackTransitionDelay;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.SpinSwirlRainbows, DoBehavior_SpinSwirlRainbows);
        }

        /// <summary>
        /// Performs the Empress' Spin Swirl Rainbows attack.
        /// </summary>
        public void DoBehavior_SpinSwirlRainbows()
        {
            LeftHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
            RightHandFrame = EmpressHandFrame.PointingUp;

            if (AITimer <= SpinSwirlRainbows_RainbowShootDelay)
            {
                if (AITimer == 2)
                {
                    SoundEngine.PlaySound(SoundID.Item160, NPC.Center);
                    NPC.oldPos = new Vector2[NPC.oldPos.Length];
                    NPC.oldRot = new float[NPC.oldRot.Length];

                    SpinSwirlRainbows_SpinAngleAngle = Main.rand.NextFloat(MathHelper.PiOver2);

                    // Ensure that the spin ends up on the same side of the target as the lance wall if it's in use.
                    if (PerformingLanceWallSupport)
                        SpinSwirlRainbows_SpinAngleAngle = MathHelper.PiOver2 * (LanceWallXPosition >= Target.Center.X).ToDirectionInt();

                    SpinSwirlRainbows_StartedOnRightSideOfTarget = NPC.OnRightSideOf(Target.Center);
                    NPC.netUpdate = true;
                }

                float dashInterpolant = AITimer / (float)SpinSwirlRainbows_RainbowShootDelay;
                float dashSpeedInterpolant = Utilities.Saturate(Utilities.Convert01To010(dashInterpolant));
                float dashArc = MathHelper.TwoPi * MathF.Pow(dashSpeedInterpolant, 0.49f) / SpinSwirlRainbows_RainbowShootDelay;
                SpinSwirlRainbows_SpinAngleAngle += dashArc * Utilities.Convert01To010(dashInterpolant) * SpinSwirlRainbows_StartedOnRightSideOfTarget.ToDirectionInt();

                Vector2 hoverDestination = Target.Center + Vector2.UnitY.RotatedBy(SpinSwirlRainbows_SpinAngleAngle) * 950f;
                NPC.SmoothFlyNearWithSlowdownRadius(hoverDestination, (1f - dashInterpolant) * 0.15f + 0.06f, 0.67f, 30f);
                DashAfterimageInterpolant = MathHelper.Lerp(DashAfterimageInterpolant, Utilities.InverseLerp(4f, 30f, NPC.velocity.Length()), 0.2f);

                if (AITimer == SpinSwirlRainbows_RainbowShootDelay)
                    SoundEngine.PlaySound(SoundID.Item163);

                if (DashAfterimageInterpolant >= 0.4f)
                {
                    PerformVFXForMultiplayer(() =>
                    {
                        ModContent.GetInstance<DistortionMetaball>().CreateParticle(NPC.Center + Main.rand.NextVector2Circular(50f, 50f), Vector2.Zero, 30f, 0.75f, 0.25f, 0.02f);
                    });
                }
            }
            else
            {
                NPC.Center = Vector2.Lerp(NPC.Center, Target.Center, 0.013f);
                if (!NPC.WithinRange(Target.Center, 75f))
                    NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.SafeDirectionTo(Target.Center) * 2f, 0.08f);
                else
                    NPC.velocity *= 0.98f;

                Vector2 handPosition = NPC.Center + new Vector2(NPC.spriteDirection * 42f, -68f).RotatedBy(NPC.rotation);
                if (Main.netMode != NetmodeID.MultiplayerClient && AITimer <= SpinSwirlRainbows_RainbowShootTime)
                {
                    Vector2 rainbowVelocity = (MathHelper.TwoPi * AITimer / 25f).ToRotationVector2() * SpinSwirlRainbows_RainbowSpeed;
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), handPosition, rainbowVelocity, ModContent.ProjectileType<AcceleratingRainbow>(), AcceleratingRainbowDamage, 0f, -1, AITimer / 20f % 1f);
                }
            }

            // The PerformingLanceWallSupport check is because she already performs a teleport at the end of that state, and as such this second one would be unnecessary.
            if (AITimer == 1 && !PerformingLanceWallSupport)
            {
                TeleportTo(Target.Center - Vector2.UnitX * Target.direction * 400f);
                NPC.velocity = Vector2.Zero;
            }

            NPC.spriteDirection = NPC.velocity.X.NonZeroSign();
            NPC.rotation = NPC.velocity.X * 0.003f;
        }
    }
}
