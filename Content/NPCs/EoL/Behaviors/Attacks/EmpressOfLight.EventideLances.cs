using System;
using Luminance.Assets;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL.Projectiles;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        public Vector2 EventideLances_UndirectionedBowOffset => new Vector2(-72f, -14f).RotatedBy(NPC.rotation);

        public Vector2 EventideLances_DirectionedBowOffset => new Vector2(NPC.spriteDirection * -72f, -14f).RotatedBy(NPC.rotation);

        public bool EventideLances_UsingBow
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value.ToInt();
        }

        public ref float EventideLances_BowDirection => ref NPC.ai[1];

        public ref float EventideLances_BowGlimmerInterpolant => ref NPC.ai[2];

        public static int EventideLances_BowGleamTime => Utilities.SecondsToFrames(0.95f);

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_EventideLances()
        {
            StateMachine.RegisterTransition(EmpressAIType.EventideLances, null, false, () =>
            {
                return AITimer >= 99999999;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.EventideLances, DoBehavior_EventideLances);
        }

        /// <summary>
        /// Performs the Empress' RadialStarBurst attack.
        /// </summary>
        public void DoBehavior_EventideLances()
        {
            LeftHandFrame = EmpressHandFrame.FistedOutstretchedArm;
            RightHandFrame = EmpressHandFrame.UpwardGrip;
            EventideLances_UsingBow = true;

            NPC.spriteDirection = NPC.OnRightSideOf(Target).ToDirectionInt();
            NPC.rotation = NPC.velocity.X * 0.0035f;

            float hoverSpeedInterpolant = Utilities.InverseLerpBump(0f, 4f, EventideLances_BowGleamTime, EventideLances_BowGleamTime + 8f, AITimer);
            Vector2 hoverDestination = Target.Center + new Vector2(NPC.OnRightSideOf(Target).ToDirectionInt() * 400f, -100f);
            NPC.SmoothFlyNearWithSlowdownRadius(hoverDestination, hoverSpeedInterpolant * 0.55f, 1f - hoverSpeedInterpolant * 0.3f, 45f);
            NPC.velocity *= MathHelper.Lerp(0.7f, 1f, Utilities.InverseLerp(0f, 15f, AITimer - EventideLances_BowGleamTime));

            if (AITimer <= EventideLances_BowGleamTime)
                EventideLances_BowDirection = NPC.AngleTo(Target.Center);

            float idealDashAfterimageInterpolant = Utilities.InverseLerp(32f, 80f, NPC.velocity.Length());
            DashAfterimageInterpolant = MathHelper.Lerp(DashAfterimageInterpolant, idealDashAfterimageInterpolant, 0.12f);

            EventideLances_BowGlimmerInterpolant = Utilities.InverseLerp(0f, EventideLances_BowGleamTime, AITimer);

            if (AITimer == EventideLances_BowGleamTime + 15)
            {
                ScreenShakeSystem.StartShake(50f, MathHelper.Pi * 0.16f, -NPC.SafeDirectionTo(Target.Center), 2f);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 eventideEnd = NPC.Center + EventideLances_DirectionedBowOffset;

                    for (int i = -3; i <= 3; i++)
                    {
                        if (i == 0)
                            continue;

                        float lanceFireOffsetAngle = i * 0.12f;
                        float lanceSpeed = 22f - MathF.Abs(i) * 1.4f;
                        Vector2 lanceVelocity = (EventideLances_BowDirection + lanceFireOffsetAngle).ToRotationVector2() * lanceSpeed;

                        Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), eventideEnd, lanceVelocity, ModContent.ProjectileType<LightLance>(), LightLanceDamage, 0f, -1, 0f, Main.rand.NextFloat());
                    }
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), eventideEnd, Vector2.Zero, ModContent.ProjectileType<PrismaticBurst>(), 0, 0f);

                    NPC.velocity -= EventideLances_BowDirection.ToRotationVector2() * 140f;
                    NPC.netUpdate = true;
                }
            }

            if (AITimer >= EventideLances_BowGleamTime + 15)
            {
                NPC.velocity *= 0.6f;

                LeftHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
                RightHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
                EventideLances_UsingBow = false;
            }

            if (AITimer >= 150)
                AITimer = 0;
        }

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
