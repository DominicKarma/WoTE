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
        public Vector2 EventideLances_UndirectionedBowOffset => new Vector2(-50f, -14f).RotatedBy(NPC.rotation);

        public Vector2 EventideLances_DirectionedBowOffset => new Vector2(NPC.spriteDirection * -50f, -14f).RotatedBy(NPC.rotation);

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
        public int EventideLances_BowGleamTime => Utilities.SecondsToFrames(EventideLances_TeleportCounter <= 0f ? 1.35f : (0.5f - Main.dayTime.ToInt() * 0.167f));

        /// <summary>
        /// How long the Empress' bow should wait after the bows gleam animation before firing during her Eventide Lances attack.
        /// </summary>
        public static int EventideLances_ShootDelay => Utilities.SecondsToFrames(Main.dayTime ? 0.12f : 0.25f);

        /// <summary>
        /// How long the Empress' rainbow rift arrows should last before disappearing.
        /// </summary>
        public static int EventideLances_RiftArrowLifetime => Utilities.SecondsToFrames(0.5f);

        /// <summary>
        /// The amount of lances the Empress should shoot during her Eventide Lances attack when firing.
        /// </summary>
        public static int EventideLances_LancesOnEachSide => 5;

        /// <summary>
        /// The spacing of lances shot by the Empress during her Eventide Lances attack.
        /// </summary>
        public static float EventideLances_LanceSpacing => MathHelper.ToRadians(16.7f);

        /// <summary>
        /// The minimum speed of lances shot by the Empress during her Eventide Lances attack.
        /// </summary>
        public static float EventideLances_MinLanceSpeed => 9f;

        /// <summary>
        /// The maximum speed of lances shot by the Empress during her Eventide Lances attack.
        /// </summary>
        public static float EventideLances_MaxLanceSpeed => 18.6f;

        /// <summary>
        /// The hover offset of the Empress during her Eventide Lances attack.
        /// </summary>
        public static float EventideLances_HoverOffset => Main.dayTime ? 615f : 510f;

        // NOTE -- With how intense the timings of the daytime variant of this attack are, it's best to have it be more consistent with a stronger horizontal bias.
        // Be careful when changing this.
        /// <summary>
        /// How much the Empress prefers being horizontally offset relative to the target during her Eventide Lances attack. A value 0 equates to being fully omnidirectional, a value of 1 equates to being fully to the side of the target.
        /// </summary>
        public static float EventideLances_HorizontalHoverBias => Main.dayTime ? 0.85f : 0.4f;

        /// <summary>
        /// The amount of teleports the Empress should perform during her Eventide Lances attack before transitioning to a different attack.
        /// </summary>
        public static int EventideLances_TeleportCount => Main.dayTime ? 6 : 4;

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

            bool waitingForBeat = AITimer <= 1 && MusicTimer % BeatSyncedBolts_ShootRate != 23 && Main.dayTime && MusicTimer >= 1 && EventideLances_TeleportCounter <= 0f;
            if (waitingForBeat)
                AITimer = 0;

            // Aim the bow and make it gleam at first.
            float bowAimInterpolant = Utilities.InverseLerp(0f, -7f, AITimer - EventideLances_BowGleamTime);
            Vector2 eventideEnd = NPC.Center + EventideLances_DirectionedBowOffset;
            if (bowAimInterpolant > 0f)
            {
                float idealDirection = eventideEnd.AngleTo(Target.Center);
                EventideLances_BowDirection = EventideLances_BowDirection.AngleLerp(idealDirection, bowAimInterpolant * 0.42f).AngleTowards(idealDirection, bowAimInterpolant * 0.32f);
            }
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
            Vector2 horizontalHoverOffset = new Vector2(NPC.OnRightSideOf(Target).ToDirectionInt(), -0.196f) * EventideLances_HoverOffset;
            Vector2 omnidirectionalHoverOffset = Target.SafeDirectionTo(NPC.Center) * horizontalHoverOffset.Length();
            Vector2 hoverDestination = Target.Center + Vector2.Lerp(omnidirectionalHoverOffset, horizontalHoverOffset, EventideLances_HorizontalHoverBias);
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
            NPC.dontTakeDamage = true;
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
                for (int i = -EventideLances_LancesOnEachSide; i <= EventideLances_LancesOnEachSide; i++)
                {
                    if (i == 0)
                    {
                        Vector2 arrowVelocity = EventideLances_BowDirection.ToRotationVector2() * 32f;
                        Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), eventideEnd, arrowVelocity, ModContent.ProjectileType<RainbowRiftArrow>(), LightLanceDamage, 0f, -1, 0.45f);
                        continue;
                    }

                    // The i - 1f and EventideLances_LancesOnEachSide - 1f parts exist to account for the fact that the 0th iteration in the loop will not fire a typical lance, thus ensuring
                    // that the interpolation is correct starting at 1, rather than never actually starting at the EventideLances_MaxLanceSpeed value.
                    float lanceFireOffsetAngle = i * EventideLances_LanceSpacing;
                    float lanceSpeed = MathHelper.Lerp(EventideLances_MaxLanceSpeed, EventideLances_MinLanceSpeed, MathF.Abs(i - 1f) / (EventideLances_LancesOnEachSide - 1f));
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

            NPC.netSpam = 0;
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
            Vector2 stringEnd = drawPosition + new Vector2(16f, -22f).RotatedBy(NPC.rotation);
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

            Color flareColorA = Palette.MulticolorLerp(EmpressPaletteType.PrismaticBolt, 0f);
            Color flareColorB = Palette.MulticolorLerp(EmpressPaletteType.PrismaticBolt, 0.33f) * 1.6f;
            Color flareColorC = Palette.MulticolorLerp(EmpressPaletteType.PrismaticBolt, 0.66f);

            Main.spriteBatch.Draw(bloom, flarePosition, null, flareColorA with { A = 0 } * flareOpacity * 0.3f, 0f, bloom.Size() * 0.5f, flareScale * 1.9f, 0, 0f);
            Main.spriteBatch.Draw(bloom, flarePosition, null, flareColorB with { A = 0 } * flareOpacity * 0.54f, 0f, bloom.Size() * 0.5f, flareScale, 0, 0f);
            Main.spriteBatch.Draw(flare, flarePosition, null, flareColorC with { A = 0 } * flareOpacity, flareRotation, flare.Size() * 0.5f, flareScale, 0, 0f);
        }
    }
}
