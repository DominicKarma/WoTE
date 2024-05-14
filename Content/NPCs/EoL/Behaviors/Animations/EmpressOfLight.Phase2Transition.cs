using System;
using Luminance.Common.Easings;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Common.ShapeCurves;
using WoTE.Content.NPCs.EoL.Projectiles;
using WoTE.Content.Particles;

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
        /// How long the Empress waits during her second phase transition while charging energy.
        /// </summary>
        public static int Phase2Transition_EnergyChargeUpTime => Utilities.SecondsToFrames(9f);

        /// <summary>
        /// How long the Empress spends idle while entering her avatar form during her second phase.
        /// </summary>
        public static int Phase2Transition_ShootCycleDelay => Utilities.SecondsToFrames(1.5f);

        /// <summary>
        /// How long the Empress spends flying about in her avatar form during each cycle.
        /// </summary>
        public static int Phase2Transition_FlyAroundCycleTime => Utilities.SecondsToFrames(2f);

        /// <summary>
        /// How long the Empress spends releasing laser beams and rainbows during her avatar form.
        /// </summary>
        public static int Phase2Transition_ShootDeathrayTime => Utilities.SecondsToFrames(3f);

        /// <summary>
        /// How amount of laser cycles the Empress performs in her avatar form during the second phase transition.
        /// </summary>
        public static int Phase2Transition_ShootCycleCount => 3;

        /// <summary>
        /// The life ratio at which the Emperss transitions to her second phase.
        /// </summary>
        public static float Phase2LifeRatio => 0.6f;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_Phase2Transition()
        {
            StateMachine.RegisterTransition(EmpressAIType.Phase2Transition, EmpressAIType.OrbitReleasedTerraprismas, false, () =>
            {
                return AITimer >= Phase2Transition_EnergyChargeUpTime + Phase2Transition_ShootCycleDelay + (Phase2Transition_FlyAroundCycleTime + Phase2Transition_ShootDeathrayTime) * Phase2Transition_ShootCycleCount + 90;
            }, () =>
            {
                Phase2 = true;
                TeleportTo(Target.Center - Vector2.UnitY * 240f);
                ZPosition = 0f;
                NPC.netUpdate = true;
            });
            StateMachine.ApplyToAllStatesExcept(state =>
            {
                StateMachine.RegisterTransition(state, EmpressAIType.Phase2Transition, false, () => EnterPhase2AfterNextAttack && CurrentState != EmpressAIType.ButterflyBurstDashes);
            }, EmpressAIType.Phase2Transition, EmpressAIType.Die, EmpressAIType.Vanish);

            StateMachine.RegisterStateBehavior(EmpressAIType.Phase2Transition, DoBehavior_Phase2Transition);
        }

        /// <summary>
        /// Performs the Empress' second phase transition state.
        /// </summary>
        public void DoBehavior_Phase2Transition()
        {
            if (Main.mouseRight && Main.mouseRightRelease)
            {
                ButterflyProjectionScale = 0f;
                ButterflyProjectionOpacity = 0f;
                AITimer = 0;
                NPC.Opacity = 1f;
            }

            if (AITimer == 1)
                SoundEngine.PlaySound(SoundID.Item159);

            bool shootingLasers = AITimer >= Phase2Transition_EnergyChargeUpTime + Phase2Transition_ShootCycleDelay;

            float maxZPosition = MathHelper.Lerp(5f, 1.1f, Utilities.Sin01(MathHelper.TwoPi * AITimer / 60f).Cubed());
            ZPosition = EasingCurves.Cubic.Evaluate(EasingType.InOut, Utilities.InverseLerp(0f, 60f, AITimer)) * maxZPosition;
            if (AITimer <= 120)
                NPC.SmoothFlyNear(Target.Center - Vector2.UnitY * 270f, ZPosition * 0.1f, 1f - ZPosition * 0.15f);
            else if (AITimer <= 180)
                NPC.velocity *= 0.9f;

            NPC.rotation = MathHelper.Lerp(NPC.rotation, 0f, 0.3f);

            float appearanceInterpolant = Utilities.InverseLerpBump(0f, 0.4f, 0.7f, 0.75f, AITimer / (float)Phase2Transition_EnergyChargeUpTime).Squared();
            if (Main.netMode != NetmodeID.MultiplayerClient && ZPosition >= 2f && AITimer % 3 == 0 && appearanceInterpolant >= 0.5f)
            {
                Vector2 moonlightPosition = NPC.Center + (MathHelper.TwoPi * AITimer / 30f).ToRotationVector2() * Main.rand.NextFloat(1200f, 1300f) * new Vector2(1f, 0.6f);
                Vector2 moonlightVelocity = moonlightPosition.SafeDirectionTo(NPC.Center).RotatedBy(MathHelper.PiOver2) * 32f;
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), moonlightPosition, moonlightVelocity, ModContent.ProjectileType<ConvergingMoonlight>(), 0, 0f);
            }

            for (int i = 0; i < appearanceInterpolant.Squared() * 16f; i++)
            {
                float pixelScale = Main.rand.NextFloat(1f, 5f);
                Vector2 pixelSpawnPosition = NPC.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(900f, 1256f);
                Vector2 pixelVelocity = pixelSpawnPosition.SafeDirectionTo(NPC.Center).RotatedBy(MathHelper.PiOver4) * Main.rand.NextFloat(12f, 30f) / pixelScale;
                Color pixelBloomColor = Utilities.MulticolorLerp(Main.rand.NextFloat(), Color.Yellow, Color.HotPink, Color.Violet, Color.DeepSkyBlue) * 0.6f;

                BloomPixelParticle bloom = new(pixelSpawnPosition, pixelVelocity, Color.White, pixelBloomColor, Main.rand.Next(150, 210), Vector2.One * pixelScale, () => NPC.Center);
                bloom.Spawn();
            }

            if (AITimer >= Phase2Transition_EnergyChargeUpTime)
            {
                if (AITimer == Phase2Transition_EnergyChargeUpTime + 10)
                {
                    SoundEngine.PlaySound(SoundID.Item160);
                    ScreenShakeSystem.StartShake(17.4f);
                }

                ButterflyProjectionScale = MathHelper.Lerp(ButterflyProjectionScale, 3f, 0.04f);
                ButterflyProjectionOpacity = MathHelper.Lerp(ButterflyProjectionOpacity, 1f, 0.2f);
                NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 0.15f, 0.15f);

                if (shootingLasers)
                    DoBehavior_Phase2Transition_ShootLasers(AITimer - Phase2Transition_EnergyChargeUpTime - Phase2Transition_ShootCycleDelay);
                else
                    NPC.SmoothFlyNear(Target.Center - Vector2.UnitY * 250f, 0.04f, 0.85f);

                if (Main.rand.NextBool() && ShapeCurveManager.TryFind("Butterfly", out ShapeCurve butterflyCurve))
                {
                    int lacewingLifetime = Main.rand.Next(25, 36);
                    float lacewingScale = Main.rand.NextFloat(0.4f, 1.15f);
                    Color lacewingColor = Color.Lerp(Color.Yellow, Color.LightGoldenrodYellow, Main.rand.NextFloat());
                    Vector2 lacewingVelocity = (Main.rand.Next(butterflyCurve.ShapePoints) - Vector2.One * 0.5f) * new Vector2(1.75f, 0.9f) * Main.rand.NextFloat(30f, 83f);
                    PrismaticLacewingParticle lacewing = new(NPC.Center, lacewingVelocity, lacewingColor, lacewingLifetime, Vector2.One * lacewingScale);
                    lacewing.Spawn();

                    for (int i = 0; i < 5; i++)
                    {
                        BloomPixelParticle pixel = new(NPC.Center, lacewingVelocity.RotatedByRandom(0.4f) * 0.35f + Main.rand.NextVector2Circular(5f, 5f), Color.White, lacewingColor * 0.45f, 45, Vector2.One * Main.rand.NextFloat(1.5f, 4f));
                        pixel.Spawn();
                    }
                }
            }

            ScreenShakeSystem.SetUniversalRumble(MathF.Sqrt(appearanceInterpolant) * 6f, MathHelper.TwoPi, null, 0.2f);

            LeftHandFrame = EmpressHandFrame.HandPressedToChest;
            RightHandFrame = EmpressHandFrame.HandPressedToChest;
            NPC.dontTakeDamage = true;
            NPC.ShowNameOnHover = false;
            IdealDrizzleVolume = StandardDrizzleVolume + Utilities.InverseLerp(0f, 120f, AITimer) * 0.3f;

            DashAfterimageInterpolant = MathHelper.Lerp(DashAfterimageInterpolant, Utilities.InverseLerp(0f, 120f, AITimer), 0.055f);

            // God.
            // This ensures that Noxus' apparent position isn't as responsive to camera movements if he's in the background, giving a pseudo-parallax visual.
            // Idea is basically Noxus going
            // "Oh? You moved 30 pixels in this direction? Well I'm in the background bozo so I'm gonna follow you and go in the same direction by, say, 27 pixels. This will make it look like I only moved 3 pixels"
            // This obviously doesn't work in multiplayer, and as such it does not run there.
            if (Main.netMode == NetmodeID.SinglePlayer && !shootingLasers)
            {
                float parallax = 0.8f;
                Vector2 targetOffset = Target.velocity;
                if (NPC.HasPlayerTarget)
                {
                    Player playerTarget = Main.player[NPC.TranslatedTargetIndex];
                    targetOffset = playerTarget.position - playerTarget.oldPosition;
                }
                NPC.position += targetOffset * Utilities.Saturate(parallax);
            }
        }

        public void DoBehavior_Phase2Transition_ShootLasers(int localTimer)
        {
            if (localTimer >= (Phase2Transition_FlyAroundCycleTime + Phase2Transition_ShootDeathrayTime) * Phase2Transition_ShootCycleCount)
            {
                for (int i = 0; i < 11; i++)
                {
                    float pixelScale = Main.rand.NextFloat(1f, 5f);
                    Vector2 pixelSpawnPosition = Target.Center + new Vector2(Main.rand.NextFloatDirection() * 1000f, -Main.rand.NextFloat(800f, 1000f));
                    Vector2 pixelVelocity = Vector2.UnitY * Main.rand.NextFloat(12f, 30f) / pixelScale;
                    Color pixelBloomColor = Utilities.MulticolorLerp(Main.rand.NextFloat(), Color.Yellow, Color.HotPink, Color.Violet, Color.DeepSkyBlue) * 0.6f;

                    BloomPixelParticle bloom = new(pixelSpawnPosition, pixelVelocity, Color.White, pixelBloomColor, Main.rand.Next(150, 210), Vector2.One * pixelScale);
                    bloom.Spawn();
                }

                NPC.velocity.X *= 1.04f;
                NPC.velocity.Y -= 3f;
                ZPosition = MathHelper.Lerp(ZPosition, 0f, 0.2f);
                return;
            }

            int wrappedTimer = localTimer % (Phase2Transition_FlyAroundCycleTime + Phase2Transition_ShootDeathrayTime);
            if (wrappedTimer <= Phase2Transition_FlyAroundCycleTime)
            {
                if (wrappedTimer <= Phase2Transition_FlyAroundCycleTime - 45)
                    NPC.velocity += NPC.SafeDirectionTo(Target.Center) * 1.1f;
                else
                    NPC.velocity *= 0.98f;

                return;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient && wrappedTimer == Phase2Transition_FlyAroundCycleTime + 1)
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, NPC.SafeDirectionTo(Target.Center), ModContent.ProjectileType<DazzlingDeathray>(), DazzlingDeathrayDamage, 0f);

            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer % 2 == 0)
            {
                Vector2 starBurstVelocity = (MathHelper.TwoPi * AITimer / 20.333f).ToRotationVector2() * 3f;
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, starBurstVelocity, ModContent.ProjectileType<AcceleratingRainbow>(), AcceleratingRainbowDamage, 0f);
            }

            NPC.velocity *= 0.95f;
            NPC.Center = Vector2.Lerp(NPC.Center, Target.Center, 0.009f);
        }
    }
}
