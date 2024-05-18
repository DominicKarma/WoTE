using System;
using Luminance.Assets;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        public Vector2 EventideLances_UndirectionedBowOffset => new Vector2(-72f, -14f).RotatedBy(NPC.rotation);

        public ref float EventideLances_BowDirection => ref NPC.ai[0];

        public ref float EventideLances_BowGlimmerInterpolant => ref NPC.ai[1];

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

            NPC.spriteDirection = NPC.OnRightSideOf(Target).ToDirectionInt();
            NPC.rotation = NPC.velocity.X * 0.0035f;

            EventideLances_BowDirection = NPC.AngleTo(Target.Center);

            Vector2 hoverDestination = Target.Center + Vector2.UnitX * NPC.OnRightSideOf(Target).ToDirectionInt() * 400f;
            NPC.SmoothFlyNearWithSlowdownRadius(hoverDestination, 0.2f, 0.8f, 76f);

            EventideLances_BowGlimmerInterpolant = Utilities.InverseLerp(0f, 60f, AITimer % 150f);
        }

        public void DoBehavior_EventideLances_DrawBowString(Vector2 drawPosition)
        {
            if (CurrentState != EmpressAIType.EventideLances)
                return;

            Vector2 eventidePosition = drawPosition + EventideLances_UndirectionedBowOffset;

            float angleOffset = EventideLances_BowDirection * NPC.spriteDirection;
            if (NPC.spriteDirection == -1)
                angleOffset += MathHelper.Pi;

            Vector2 eventideTop = eventidePosition - Vector2.UnitY.RotatedBy(NPC.rotation + angleOffset) * 37f;
            Vector2 eventideBottom = eventidePosition + Vector2.UnitY.RotatedBy(NPC.rotation + angleOffset) * 37f;
            Vector2 stringEnd = drawPosition + new Vector2(2f, -36f).RotatedBy(NPC.rotation);

            Utils.DrawLine(Main.spriteBatch, eventideTop + Main.screenPosition, stringEnd + Main.screenPosition, Color.DeepSkyBlue, Color.Wheat, 2f);
            Utils.DrawLine(Main.spriteBatch, eventideBottom + Main.screenPosition, stringEnd + Main.screenPosition, Color.HotPink, Color.Wheat, 2f);
        }

        public void DoBehavior_EventideLances_DrawBow(Vector2 drawPosition)
        {
            if (CurrentState != EmpressAIType.EventideLances)
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
