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
        /// <summary>
        /// How long the Empress spends redirecting during her Radial Star Burst attack.
        /// </summary>
        public static int RadialStarBurst_RedirectTime => Utilities.SecondsToFrames(0.583f);

        /// <summary>
        /// How long the Empress waits before releasing star bursts during her Radial Star Burst attack.
        /// </summary>
        public static int RadialStarBurst_BurstDelay => Utilities.SecondsToFrames(0.5f);

        /// <summary>
        /// How long the Empress waits after firing star bursts to either fire another one or choose a new attack during her Radial Star Burst attack.
        /// </summary>
        public static int RadialStarBurst_AttackRestartDelay => Utilities.SecondsToFrames(0.75f);

        /// <summary>
        /// The amount of bursts the Empress performs during her Radial Star Burst attack before choosing a new attack.
        /// </summary>
        public static int RadialStarBurst_BurstCount => 2;

        /// <summary>
        /// The horizontal hover direction as used for redirect positions during her Radial Star Burst attack.
        /// </summary>
        public ref float RadialStarBurst_HorizontalHoverDirection => ref NPC.ai[0];

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_RadialStarBurst()
        {
            StateMachine.RegisterTransition(EmpressAIType.RadialStarBurst, null, false, () =>
            {
                return AITimer >= (RadialStarBurst_RedirectTime + RadialStarBurst_BurstDelay + RadialStarBurst_AttackRestartDelay) * RadialStarBurst_BurstCount;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.RadialStarBurst, DoBehavior_RadialStarBurst);
        }

        /// <summary>
        /// Performs the Empress' Twirling Petal Sun attack.
        /// </summary>
        public void DoBehavior_RadialStarBurst()
        {
            LeftHandFrame = EmpressHandFrame.OpenHandDownwardArm;
            RightHandFrame = EmpressHandFrame.OpenHandDownwardArm;

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.rotation.AngleLerp(NPC.velocity.X * 0.0015f, 0.2f);

            int redirectTime = RadialStarBurst_RedirectTime;
            int boomDelay = RadialStarBurst_BurstDelay;
            int attackRestartDelay = RadialStarBurst_AttackRestartDelay;
            int wrappedTimer = AITimer % (redirectTime + boomDelay + attackRestartDelay);
            if (wrappedTimer <= redirectTime)
                DoBehavior_RadialStarBurst_Redirect(wrappedTimer);
            else
            {
                float idealVerticalSpeed = Utilities.InverseLerpBump(0f, 0.6f, 0.8f, 1f, (wrappedTimer - redirectTime) / (float)boomDelay).Squared() * -20f;
                NPC.velocity.X *= 0.5f;
                NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, idealVerticalSpeed, 0.33f);
                DashAfterimageInterpolant *= 0.95f;
            }

            if (wrappedTimer == redirectTime + boomDelay)
                DoBehavior_RadialStarBurst_ShootBurstProjectiles();

            if (wrappedTimer >= redirectTime + boomDelay)
            {
                LeftHandFrame = EmpressHandFrame.HandPressedToChest;
                RightHandFrame = EmpressHandFrame.HandPressedToChest;
            }
        }

        /// <summary>
        /// Handles redirecting behaviors during the Empress' Radial Star Burst attack.
        /// </summary>
        /// <param name="wrappedTimer">The cyclically wrapped AI timer.</param>
        public void DoBehavior_RadialStarBurst_Redirect(int wrappedTimer)
        {
            int redirectTime = RadialStarBurst_RedirectTime;
            bool swapTeleportHappened = false;

            if (AITimer == 1)
            {
                RadialStarBurst_HorizontalHoverDirection = NPC.OnRightSideOf(Target).ToDirectionInt();
                NPC.netUpdate = true;
            }
            else if (wrappedTimer == 1)
            {
                RadialStarBurst_HorizontalHoverDirection *= -1f;
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
            Vector2 hoverDestination = Target.Center + new Vector2(RadialStarBurst_HorizontalHoverDirection * 400f, 100f - Utilities.Convert01To010(wrappedTimer / (float)redirectTime) * 150f);
            NPC.Center = Vector2.Lerp(NPC.Center, hoverDestination, 0.2f);
            NPC.velocity += NPC.SafeDirectionTo(hoverDestination) * flySpeedInterpolant * 40f;

            DashAfterimageInterpolant = 1f;

            if (swapTeleportHappened && NPC.OnRightSideOf(Target.Center).ToDirectionInt() == RadialStarBurst_HorizontalHoverDirection)
                RadialStarBurst_HorizontalHoverDirection *= -1f;

            if (NPC.velocity.AngleBetween(NPC.SafeDirectionTo(hoverDestination)) >= MathHelper.PiOver2)
            {
                AITimer += redirectTime - wrappedTimer + 1;
                NPC.velocity *= 0.25f;
                DashAfterimageInterpolant *= 0.4f;
                NPC.netUpdate = true;
            }
        }

        /// <summary>
        /// Handles burst shot behaviors for the Empress during her Radial Star Bursts attack.
        /// </summary>
        public void DoBehavior_RadialStarBurst_ShootBurstProjectiles()
        {
            SoundEngine.PlaySound(SoundID.Item122);
            SoundEngine.PlaySound(SoundID.Item160);
            ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 7.2f);
            ModContent.GetInstance<DistortionMetaball>().CreateParticle(NPC.Center, Vector2.Zero, 70f, 2f, 0.1f, 0.009f);

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            float shootOffsetAngle = NPC.AngleTo(Target.Center);
            for (int i = 0; i < 18; i++)
            {
                float shootAngle = MathHelper.TwoPi * i / 18f + shootOffsetAngle;
                Vector2 shootVelocity = shootAngle.ToRotationVector2() * 0.3f;

                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, shootVelocity, ModContent.ProjectileType<StarBolt>(), StarBurstDamage, 0f);
            }

            for (int i = 0; i < 9; i++)
            {
                Vector2 shootVelocity = (MathHelper.TwoPi * i / 9f).ToRotationVector2() * 6.4f;
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, shootVelocity, ModContent.ProjectileType<PrismaticBolt>(), PrismaticBoltDamage, 0f, -1, NPC.target);
            }

            for (int i = 0; i < 7; i++)
            {
                Vector2 shootVelocity = -NPC.SafeDirectionTo(Target.Center).RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(1f, 1.7f);
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, shootVelocity, ModContent.ProjectileType<PrismaticBolt>(), PrismaticBoltDamage, 0f, -1, NPC.target);
            }
        }
    }
}
