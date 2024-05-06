using System;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        public override void FindFrame(int frameHeight)
        {

        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
        {
            Main.spriteBatch.PrepareForShaders();

            if (TeleportCompletionRatio > 0f && TeleportCompletionRatio < 1f)
            {
                DrawInstance(NPC.Center - screenPos, TeleportCompletionRatio, false);
                DrawInstance(TeleportDestination - screenPos, 1f - TeleportCompletionRatio, false);
            }
            else
                DrawInstance(NPC.Center - screenPos, 0f, false);
            Main.spriteBatch.ResetToDefault();

            return false;
        }

        /// <summary>
        /// Draws a single instance of the Empress at a given position in screen space.
        /// </summary>
        /// <param name="drawPosition">The draw position of the Empress instance.</param>
        /// <param name="cutoffY">The Y cutoff interpolant value.</param>
        /// <param name="invertDisappearanceDirection">Whether the direction of disappearance should be inverted.</param>
        public void DrawInstance(Vector2 drawPosition, float cutoffY, bool invertDisappearanceDirection)
        {
            ManagedShader teleportShader = ShaderManager.GetShader("WoTE.EmpressTeleportDisappearShader");
            teleportShader.TrySetParameter("cutoffY", cutoffY);
            teleportShader.TrySetParameter("invertDisappearanceDirection", invertDisappearanceDirection);
            teleportShader.SetTexture(MiscTexturesRegistry.TurbulentNoise.Value, 1, SamplerState.LinearWrap);
            teleportShader.Apply();

            SpriteEffects direction = NPC.spriteDirection.ToSpriteDirection();
            Main.EntitySpriteDraw(EmpressOfLightTargetManager.EmpressTarget, drawPosition, null, NPC.GetAlpha(Color.White), 0f, EmpressOfLightTargetManager.EmpressTarget.Size() * 0.5f, NPC.scale, direction, 0f);

            DrawTeleportRing(drawPosition, cutoffY, invertDisappearanceDirection);
        }

        /// <summary>
        /// Draws a teleportation ring for an empress of light instance
        /// </summary>
        /// <param name="drawPosition">The draw position of the Empress instance.</param>
        /// <param name="cutoffY">The Y cutoff interpolant value.</param>
        /// <param name="invertDisappearanceDirection">Whether the direction of disappearance should be inverted.</param>
        public void DrawTeleportRing(Vector2 drawPosition, float cutoffY, bool invertDisappearanceDirection)
        {
            if (cutoffY <= 0f || cutoffY >= 1f)
                return;

            Texture2D gradient = TextureAssets.Extra[ExtrasID.HallowBossGradient].Value;
            float teleportRingOpacity = Utilities.InverseLerpBump(0f, 0.25f, 0.85f, 1f, cutoffY).Cubed();
            Vector2 teleportRingPosition = drawPosition + Vector2.UnitY * (cutoffY - 0.5f) * 570f;
            ManagedShader teleportRingShader = ShaderManager.GetShader("WoTE.EmpressTeleportRingShader");
            teleportRingShader.SetTexture(MiscTexturesRegistry.DendriticNoiseZoomedOut.Value, 1, SamplerState.LinearWrap);
            teleportRingShader.TrySetParameter("pulsationIntensity", 1f - cutoffY);
            teleportRingShader.TrySetParameter("invertDisappearanceDirection", invertDisappearanceDirection);
            teleportRingShader.Apply();

            float ringGrowInterpolant = Utilities.InverseLerpBump(0f, 0.5f, 0.8f, 1f, cutoffY);
            if (invertDisappearanceDirection)
                ringGrowInterpolant = 1f - ringGrowInterpolant;

            Vector2 ringScale = new Vector2(320f, 600f) * MathF.Pow(ringGrowInterpolant, 0.7f) * NPC.scale;
            Color ringColor = Main.hslToRgb(Main.GlobalTimeWrappedHourly * 2f % 1f, 1f, 0.95f);
            Main.EntitySpriteDraw(gradient, teleportRingPosition, null, NPC.GetAlpha(ringColor) * teleportRingOpacity, 0f, gradient.Size() * 0.5f, ringScale / gradient.Size(), SpriteEffects.FlipVertically);
        }

        /// <summary>
        /// Draws the Empress to her designated render target.
        /// </summary>
        /// <param name="drawPosition">The Empress' draw position in the render target.</param>
        public void DrawToTarget(Vector2 drawPosition)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;

            DrawBackglow(drawPosition);
            DrawWings(drawPosition);
            Main.EntitySpriteDraw(texture, drawPosition, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() * 0.5f, 1f, 0);
            DrawHands(drawPosition);
        }

        /// <summary>
        /// Draws the Empress' backglow.
        /// </summary>
        /// <param name="drawPosition">The draw position of the backglow.</param>
        public void DrawBackglow(Vector2 drawPosition)
        {
            float backglowScale = 1f - Utilities.InverseLerpBump(0f, 0.5f, 0.6f, 1f, TeleportCompletionRatio);
            Color rainbow = Main.hslToRgb(Main.GlobalTimeWrappedHourly * 0.5f % 1f, 1f, 0.5f, 0);
            Texture2D backglow = MiscTexturesRegistry.BloomCircleSmall.Value;
            Main.EntitySpriteDraw(backglow, drawPosition, null, Color.Wheat with { A = 0 } * 0.25f, NPC.rotation, backglow.Size() * 0.5f, backglowScale * 4.1f, 0);
            Main.EntitySpriteDraw(backglow, drawPosition, null, Color.Wheat with { A = 0 } * 0.67f, NPC.rotation, backglow.Size() * 0.5f, backglowScale * 2.85f, 0);
            Main.EntitySpriteDraw(backglow, drawPosition, null, rainbow * 0.7f, NPC.rotation, backglow.Size() * 0.5f, backglowScale * 1.5f, 0);
            Main.EntitySpriteDraw(backglow, drawPosition, null, Color.Wheat with { A = 0 }, NPC.rotation, backglow.Size() * 0.5f, backglowScale * 0.8f, 0);
        }

        /// <summary>
        /// Draws the Empress' wings.
        /// </summary>
        /// <param name="drawPosition">The draw position of the wings.</param>
        public void DrawWings(Vector2 drawPosition)
        {
            Texture2D wingsTexture = TextureAssets.Extra[ExtrasID.HallowBossWingsBack].Value;
            Texture2D wingsColorShapeTexture = TextureAssets.Extra[ExtrasID.HallowBossWings].Value;
            Rectangle wingsFrame = wingsTexture.Frame(1, 11, 0, (int)(Main.GlobalTimeWrappedHourly * 15f) % 11);

            Main.EntitySpriteDraw(wingsTexture, drawPosition, wingsFrame, Color.White, NPC.rotation, wingsFrame.Size() * 0.5f, 2f, 0);

            Main.spriteBatch.End();
            EmpressOfLightTargetManager.BeginSpriteBatch(SpriteSortMode.Immediate);

            ManagedShader gradientShader = ShaderManager.GetShader("WoTE.EmpressWingGradientShader");
            gradientShader.SetTexture(TextureAssets.Extra[ExtrasID.HallowBossGradient], 1, SamplerState.LinearWrap);
            gradientShader.Apply();

            Main.EntitySpriteDraw(wingsColorShapeTexture, drawPosition, wingsFrame, Color.White, NPC.rotation, wingsFrame.Size() * 0.5f, 2f, 0);

            Main.spriteBatch.End();
            EmpressOfLightTargetManager.BeginSpriteBatch(SpriteSortMode.Deferred);
        }

        /// <summary>
        /// Draws the Empress' arms.
        /// </summary>
        /// <param name="drawPosition">The draw position of the arms.</param>
        public void DrawHands(Vector2 drawPosition)
        {
            Texture2D leftHandTexture = TextureAssets.Extra[ExtrasID.HallowBossArmsLeft].Value;
            Rectangle leftHandFrame = leftHandTexture.Frame(1, 7, 0, (int)LeftHandFrame);
            Main.EntitySpriteDraw(leftHandTexture, drawPosition, leftHandFrame, Color.White, NPC.rotation, leftHandFrame.Size() * 0.5f, 1f, 0);

            Texture2D rightHandTexture = TextureAssets.Extra[ExtrasID.HallowBossArmsRight].Value;
            Rectangle rightHandFrame = rightHandTexture.Frame(1, 7, 0, (int)RightHandFrame);
            Main.EntitySpriteDraw(rightHandTexture, drawPosition, rightHandFrame, Color.White, NPC.rotation, rightHandFrame.Size() * 0.5f, 1f, 0);
        }
    }
}
