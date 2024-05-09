using System;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
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
        public static int SwirlingStarBurst_RedirectTime => Utilities.SecondsToFrames(0.583f);

        public static int SwirlingStarBurst_BurstDelay => Utilities.SecondsToFrames(0.5f);

        public static int SwirlingStarBurst_AttackRestartDelay => Utilities.SecondsToFrames(0.75f);

        public static int SwirlingStarBurst_BurstCount => 3;

        public ref float SwirlingStarBurst_HorizontalHoverDirection => ref NPC.ai[0];

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_SwirlingStarBurst()
        {
            StateMachine.RegisterTransition(EmpressAIType.SwirlingStarBurst, null, false, () =>
            {
                return AITimer >= (SwirlingStarBurst_RedirectTime + SwirlingStarBurst_BurstDelay + SwirlingStarBurst_AttackRestartDelay) * SwirlingStarBurst_BurstCount;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.SwirlingStarBurst, DoBehavior_SwirlingStarBurst);
        }

        /// <summary>
        /// Performs the Empress' Twirling Petal Sun attack.
        /// </summary>
        public void DoBehavior_SwirlingStarBurst()
        {
            LeftHandFrame = EmpressHandFrame.OpenHandDownwardArm;
            RightHandFrame = EmpressHandFrame.OpenHandDownwardArm;

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.rotation.AngleLerp(NPC.velocity.X * 0.0015f, 0.2f);

            int redirectTime = SwirlingStarBurst_RedirectTime;
            int boomDelay = SwirlingStarBurst_BurstDelay;
            int attackRestartDelay = SwirlingStarBurst_AttackRestartDelay;
            int wrappedTimer = AITimer % (redirectTime + boomDelay + attackRestartDelay);
            if (wrappedTimer <= redirectTime)
            {
                bool swapTeleportHappened = false;
                if (AITimer == 1)
                {
                    SwirlingStarBurst_HorizontalHoverDirection = NPC.OnRightSideOf(Target).ToDirectionInt();
                    NPC.netUpdate = true;
                }
                else if (wrappedTimer == 1)
                {
                    SwirlingStarBurst_HorizontalHoverDirection *= -1f;
                    swapTeleportHappened = true;
                }

                if (AITimer <= redirectTime)
                {
                    Vector2 teleportDestination = Target.Center - Vector2.UnitY * 150f;
                    if (AITimer == redirectTime / 2 && !NPC.WithinRange(teleportDestination, 300f))
                        TeleportTo(teleportDestination);

                    NPC.velocity *= 0.95f;
                    return;
                }

                float flySpeedInterpolant = 1f - wrappedTimer / (float)redirectTime;
                Vector2 hoverDestination = Target.Center + new Vector2(SwirlingStarBurst_HorizontalHoverDirection * 400f, 100f);
                NPC.Center = Vector2.Lerp(NPC.Center, hoverDestination, 0.2f);
                NPC.velocity += NPC.SafeDirectionTo(hoverDestination) * flySpeedInterpolant * 40f;

                DashAfterimageInterpolant = 1f;

                if (swapTeleportHappened && NPC.OnRightSideOf(Target.Center).ToDirectionInt() == SwirlingStarBurst_HorizontalHoverDirection)
                    SwirlingStarBurst_HorizontalHoverDirection *= -1f;

                if (NPC.velocity.AngleBetween(NPC.SafeDirectionTo(hoverDestination)) >= MathHelper.PiOver2)
                {
                    AITimer += redirectTime - wrappedTimer + 1;
                    NPC.velocity *= 0.25f;
                    DashAfterimageInterpolant *= 0.4f;
                    NPC.netUpdate = true;
                }
            }
            else
            {
                float idealVerticalSpeed = Utilities.InverseLerpBump(0f, 0.6f, 0.8f, 1f, (wrappedTimer - redirectTime) / (float)boomDelay).Squared() * -20f;
                NPC.velocity.X *= 0.5f;
                NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, idealVerticalSpeed, 0.33f);
                DashAfterimageInterpolant *= 0.95f;

                // TODO -- Charge-up particles.
            }

            if (wrappedTimer == redirectTime + boomDelay)
            {
                SoundEngine.PlaySound(SoundID.Item122);
                SoundEngine.PlaySound(SoundID.Item160);
                ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 7.2f);
                ModContent.GetInstance<DistortionMetaball>().CreateParticle(NPC.Center, Vector2.Zero, 70f, 2f, 0.1f, 0.009f);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float shootOffsetAngle = NPC.AngleTo(Target.Center);
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 shootVelocity = StarPolarEquation(5, MathHelper.TwoPi * i / 30f + shootOffsetAngle) * 0.45f;
                        Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, shootVelocity, ModContent.ProjectileType<StarBolt>(), 200, 0f);
                    }
                }
            }

            if (wrappedTimer >= redirectTime + boomDelay)
            {
                LeftHandFrame = EmpressHandFrame.HandPressedToChest;
                RightHandFrame = EmpressHandFrame.HandPressedToChest;
            }
        }

        /// <summary>
        /// A polar equation for a star petal with a given amount of points.
        /// </summary>
        /// <param name="pointCount">The amount of points the star should have.</param>
        /// <param name="angle">The input angle for the polar equation.</param>
        public static Vector2 StarPolarEquation(int pointCount, float angle)
        {
            float spacedAngle = angle;

            // There should be a star point that looks directly upward. However, that isn't the case for odd star counts with the equation below.
            // To address this, a -90 degree rotation is performed.
            if (pointCount % 2 != 0)
                spacedAngle -= MathHelper.PiOver2;

            // Refer to desmos to view the resulting shape this creates. It's basically a black box of trig otherwise.
            float sqrt3 = 1.732051f;
            float numerator = MathF.Cos(MathHelper.Pi * (pointCount + 1f) / pointCount);
            float starAdjustedAngle = MathF.Asin(MathF.Cos(pointCount * spacedAngle)) * 2f;
            float denominator = MathF.Cos((starAdjustedAngle + MathHelper.PiOver2 * pointCount) / (pointCount * 2f));
            Vector2 result = angle.ToRotationVector2() * numerator / denominator / sqrt3;
            return result;
        }
    }
}
