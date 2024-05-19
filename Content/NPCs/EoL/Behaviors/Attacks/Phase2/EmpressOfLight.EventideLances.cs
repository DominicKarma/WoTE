using System;
using Luminance.Assets;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL.Projectiles;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        public Vector2 EventideLances_UndirectionedBowOffset => new Vector2(-72f, -14f).RotatedBy(NPC.rotation);

        public Vector2 EventideLances_DirectionedBowOffset => new Vector2(NPC.spriteDirection * -72f, -14f).RotatedBy(NPC.rotation);

        /// <summary>
        /// Whether the Empress is visibly using her bow during her Eventide Lances attack.
        /// </summary>
        public bool EventideLances_UsingBow
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value.ToInt();
        }

        /// <summary>
        /// The direction of the Empress' bow during her Eventide Lances attack.
        /// </summary>
        public ref float EventideLances_BowDirection => ref NPC.ai[1];

        /// <summary>
        /// The glimmer interpolant of the Empress' bow during her Eventide Lances attack.
        /// </summary>
        public ref float EventideLances_BowGlimmerInterpolant => ref NPC.ai[2];

        /// <summary>
        /// The amount of teleports the Empress has performed so far during her Eventide Lances attack.
        /// </summary>
        public ref float EventideLances_TeleportCounter => ref NPC.ai[3];

        /// <summary>
        /// How long the Empress' bow should spend performing its gleam animation during her Eventide Lances attack.
        /// </summary>
        public int EventideLances_BowGleamTime => Utilities.SecondsToFrames(EventideLances_TeleportCounter <= 0f ? 1.2f : 0.5f);

        /// <summary>
        /// How long the Empress' bow should wait after the bows gleam animation before firing during her Eventide Lances attack.
        /// </summary>
        public static int EventideLances_ShootDelay => Utilities.SecondsToFrames(0.25f);

        /// <summary>
        /// How long the Empress' rainbow rift arrows should last before disappearing.
        /// </summary>
        public static int EventideLances_RiftArrowLifetime => Utilities.SecondsToFrames(0.5f);

        /// <summary>
        /// The amount of teleports the Empress should perform during her Eventide Lances attack before transitioning to a different attack.
        /// </summary>
        public static int EventideLances_TeleportCount => 4;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_EventideLances()
        {
            StateMachine.RegisterTransition(EmpressAIType.EventideLances, null, false, () =>
            {
                return EventideLances_TeleportCounter >= EventideLances_TeleportCount && AITimer >= 12;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.EventideLances, DoBehavior_EventideLances);
        }

        /// <summary>
        /// Performs the Empress' Eventide Lances attack.
        /// </summary>
        public void DoBehavior_EventideLances()
        {
            // Reset things.
            LeftHandFrame = EmpressHandFrame.FistedOutstretchedArm;
            RightHandFrame = EmpressHandFrame.UpwardGrip;
            EventideLances_UsingBow = true;
            if (EventideLances_TeleportCounter >= EventideLances_TeleportCount)
            {
                LeftHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
                RightHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
                EventideLances_UsingBow = false;
            }

            // Aim the bow and make it gleam at first.
            Vector2 eventideEnd = NPC.Center + EventideLances_DirectionedBowOffset;
            if (AITimer <= EventideLances_BowGleamTime)
                EventideLances_BowDirection = eventideEnd.AngleTo(Target.Center);
            EventideLances_BowGlimmerInterpolant = Utilities.InverseLerp(0f, EventideLances_BowGleamTime, AITimer);

            if (AITimer >= EventideLances_BowGleamTime + EventideLances_ShootDelay)
            {
                DoBehavior_EventideLances_HandlePostShootEffects();

                if (AITimer == EventideLances_BowGleamTime + EventideLances_ShootDelay)
                    DoBehavior_EventideLances_Shoot(eventideEnd);
            }
            else
            {
                NPC.spriteDirection = NPC.OnRightSideOf(Target).ToDirectionInt();
                NPC.Opacity = 1f;
            }

            DoBehavior_EventideLances_HoverNearTarget();
        }

        /// <summary>
        /// Makes the Empress hover near the target during her Eventide Lances attack.
        /// </summary>
        public void DoBehavior_EventideLances_HoverNearTarget()
        {
            NPC.rotation = NPC.velocity.X * 0.0035f;

            float hoverSpeedInterpolant = Utilities.InverseLerpBump(0f, 4f, EventideLances_BowGleamTime, EventideLances_BowGleamTime + 8f, AITimer);
            Vector2 horizontalHoverOffset = new Vector2(NPC.OnRightSideOf(Target).ToDirectionInt() * 510f, -100f);
            Vector2 omnidirectionalHoverOffset = Target.SafeDirectionTo(NPC.Center) * 550f;
            Vector2 hoverDestination = Target.Center + Vector2.Lerp(horizontalHoverOffset, omnidirectionalHoverOffset, 0.6f);
            NPC.SmoothFlyNearWithSlowdownRadius(hoverDestination, hoverSpeedInterpolant * 0.55f, 1f - hoverSpeedInterpolant * 0.3f, 120f);
            NPC.velocity *= MathHelper.Lerp(0.7f, 1f, Utilities.InverseLerp(0f, 15f, AITimer - EventideLances_BowGleamTime));

            float idealDashAfterimageInterpolant = Utilities.InverseLerp(32f, 80f, NPC.velocity.Length());
            DashAfterimageInterpolant = MathHelper.Lerp(DashAfterimageInterpolant, idealDashAfterimageInterpolant, 0.12f);
        }

        /// <summary>
        /// Handles after-shooting effects for the Empress during her Eventide Lances attack, making her fade out, cease recoiling and use different hand poses.
        /// </summary>
        public void DoBehavior_EventideLances_HandlePostShootEffects()
        {
            NPC.velocity *= 0.6f;

            LeftHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
            RightHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
            EventideLances_UsingBow = false;

            NPC.Opacity = Utilities.InverseLerp(8f, 0f, AITimer - EventideLances_BowGleamTime - EventideLances_ShootDelay);
        }

        /// <summary>
        /// Shoots lances and arrows from the Empress' bow during her Eventide Lances attack.
        /// </summary>
        /// <param name="eventideEnd">The end position of the Eventide bow.</param>
        public void DoBehavior_EventideLances_Shoot(Vector2 eventideEnd)
        {
            SoundEngine.PlaySound(SoundID.Item122);
            SoundEngine.PlaySound(SoundID.Item163);
            ScreenShakeSystem.StartShake(50f, MathHelper.Pi * 0.16f, -NPC.SafeDirectionTo(Target.Center), 2f);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = -5; i <= 5; i++)
                {
                    if (i == 0)
                    {
                        Vector2 arrowVelocity = EventideLances_BowDirection.ToRotationVector2() * 32f;
                        Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), eventideEnd, arrowVelocity, ModContent.ProjectileType<RainbowRiftArrow>(), LightLanceDamage, 0f, -1, 0.45f);
                        continue;
                    }

                    float lanceFireOffsetAngle = i * 0.29f;
                    float lanceSpeed = 21f - MathF.Abs(i) * 2.4f;
                    Vector2 lanceVelocity = (EventideLances_BowDirection + lanceFireOffsetAngle).ToRotationVector2() * lanceSpeed;

                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), eventideEnd, lanceVelocity, ModContent.ProjectileType<LightLance>(), LightLanceDamage, 0f, -1, 0f, Main.rand.NextFloat());
                }
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), eventideEnd, Vector2.Zero, ModContent.ProjectileType<PrismaticBurst>(), 0, 0f);

                // Apply recoil.
                NPC.velocity -= EventideLances_BowDirection.ToRotationVector2() * 120f;
                NPC.netUpdate = true;
            }
        }

        /// <summary>
        /// Handles an immediate teleport during the Empress' Eventide Lances attack.
        /// </summary>
        /// <param name="teleportPosition">The position the Empress should teleport to.</param>
        public void DoBehavior_EventideLances_TeleportTo(Vector2 teleportPosition)
        {
            AITimer = 0;
            NPC.Center = teleportPosition;
            NPC.velocity = -Vector2.UnitY * 40f;

            NPC.oldPos = new Vector2[NPC.oldPos.Length];
            NPC.oldRot = new float[NPC.oldRot.Length];

            EventideLances_TeleportCounter++;

            NPC.netUpdate = true;
        }

        /// <summary>
        /// Draws the string of the Empress' bow during her Eventide Lances attack, assuming she's using it.
        /// </summary>
        /// <param name="drawPosition">The base draw position of the Empress.</param>
        public void DoBehavior_EventideLances_DrawBowString(Vector2 drawPosition)
        {
            if (CurrentState != EmpressAIType.EventideLances || !EventideLances_UsingBow)
                return;

            Vector2 eventidePosition = drawPosition + EventideLances_UndirectionedBowOffset;

            float angleOffset = EventideLances_BowDirection * NPC.spriteDirection;
            if (NPC.spriteDirection == -1)
                angleOffset += MathHelper.Pi;

            Vector2 eventideTop = eventidePosition + new Vector2(-14f, -37f).RotatedBy(NPC.rotation + angleOffset);
            Vector2 eventideBottom = eventidePosition + new Vector2(-14f, 37f).RotatedBy(NPC.rotation + angleOffset);
            Vector2 stringEnd = drawPosition + new Vector2(2f, -36f).RotatedBy(NPC.rotation);
            if (RightHandFrame != EmpressHandFrame.UpwardGrip)
                stringEnd = (eventideTop + eventideBottom) * 0.5f;

            Utils.DrawLine(Main.spriteBatch, eventideTop + Main.screenPosition, stringEnd + Main.screenPosition, Color.DeepSkyBlue, Color.Wheat, 2f);
            Utils.DrawLine(Main.spriteBatch, eventideBottom + Main.screenPosition, stringEnd + Main.screenPosition, Color.HotPink, Color.Wheat, 2f);
        }

        /// <summary>
        /// Draws the Empress' bow during her Eventide Lances attack, assuming she's using it.
        /// </summary>
        /// <param name="drawPosition">The base draw position of the Empress.</param>
        public void DoBehavior_EventideLances_DrawBow(Vector2 drawPosition)
        {
            if (CurrentState != EmpressAIType.EventideLances || !EventideLances_UsingBow)
                return;

            float rotation = EventideLances_BowDirection * NPC.spriteDirection + NPC.rotation;
            if (NPC.spriteDirection == 1)
                rotation += MathHelper.Pi;

            Texture2D eventide = ModContent.Request<Texture2D>("WoTE/Content/NPCs/EoL/Rendering/Eventide").Value;
            Vector2 eventidePosition = drawPosition + EventideLances_UndirectionedBowOffset;
            Main.spriteBatch.Draw(eventide, eventidePosition, null, Color.White, rotation, eventide.Size() * 0.5f, NPC.scale * 1.5f, SpriteEffects.FlipHorizontally, 0f);

            if (EventideLances_BowGlimmerInterpolant > 0f && EventideLances_BowGlimmerInterpolant < 1f)
                DoBehavior_EventideLances_DrawBowGleam(drawPosition);
        }

        /// <summary>
        /// Draws the gleam of the Empress' bow during her Eventide Lances attack, assuming she's using it.
        /// </summary>
        /// <param name="drawPosition">The base draw position of the Empress.</param>
        public void DoBehavior_EventideLances_DrawBowGleam(Vector2 drawPosition)
        {
            Texture2D flare = MiscTexturesRegistry.ShineFlareTexture.Value;
            Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;

            float flareOpacity = Utilities.InverseLerp(1f, 0.75f, EventideLances_BowGlimmerInterpolant);
            float flareScale = MathF.Pow(Utilities.Convert01To010(EventideLances_BowGlimmerInterpolant), 1.4f) * 0.7f + 0.1f;
            float flareRotation = MathHelper.SmoothStep(0f, MathHelper.TwoPi, MathF.Pow(EventideLances_BowGlimmerInterpolant, 0.2f)) + MathHelper.PiOver4;
            Vector2 flarePosition = drawPosition + EventideLances_UndirectionedBowOffset + Vector2.UnitX.RotatedBy(NPC.rotation) * -18f;

            Main.spriteBatch.Draw(bloom, flarePosition, null, Color.Cyan with { A = 0 } * flareOpacity * 0.3f, 0f, bloom.Size() * 0.5f, flareScale * 1.9f, 0, 0f);
            Main.spriteBatch.Draw(bloom, flarePosition, null, Color.Wheat with { A = 0 } * flareOpacity * 0.54f, 0f, bloom.Size() * 0.5f, flareScale, 0, 0f);
            Main.spriteBatch.Draw(flare, flarePosition, null, Color.LightCyan with { A = 0 } * flareOpacity, flareRotation, flare.Size() * 0.5f, flareScale, 0, 0f);
        }
    }
}
