using System;
using Luminance.Assets;
using Luminance.Common.Easings;
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
        /// <summary>
        /// The intensity of afterimage visuals as a 0-1 interpolant.
        /// </summary>
        public float DashAfterimageInterpolant
        {
            get;
            set;
        }

        public override void FindFrame(int frameHeight)
        {

        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
        {
            Main.spriteBatch.PrepareForShaders();

            float illusionInterpolant = (1f - Utilities.InverseLerpBump(0f, 0.2f, 0.8f, 1f, TeleportCompletionRatio)) * DashAfterimageInterpolant;
            float cutoffYInterpolant = EasingCurves.Quadratic.Evaluate(EasingType.InOut, TeleportCompletionRatio);
            if (TeleportCompletionRatio > 0f && TeleportCompletionRatio < 1f)
            {
                DrawInstance(NPC.Center - screenPos, Color.White * (1f - illusionInterpolant), NPC.rotation, cutoffYInterpolant, false);
                DrawInstance(TeleportDestination - screenPos, Color.White, NPC.rotation, 1f - cutoffYInterpolant, false);
            }
            else
                DrawInstance(NPC.Center - screenPos, Color.White, NPC.rotation, 0f, false);

            if (DashAfterimageInterpolant > 0f)
                DrawAfterimageVisuals(screenPos, illusionInterpolant);

            Main.spriteBatch.ResetToDefault();

            return false;
        }

        /// <summary>
        /// Draws a single instance of the Empress at a given position in screen space.
        /// </summary>
        /// <param name="drawPosition">The draw position of the Empress instance.</param>
        /// <param name="color">The color of the Empress instance.</param>
        /// <param name="cutoffY">The instance's rotation.</param>
        /// <param name="cutoffY">The Y cutoff interpolant value.</param>
        /// <param name="invertDisappearanceDirection">Whether the direction of disappearance should be inverted.</param>
        public void DrawInstance(Vector2 drawPosition, Color color, float rotation, float cutoffY, bool invertDisappearanceDirection)
        {
            ManagedShader teleportShader = ShaderManager.GetShader("WoTE.EmpressTeleportDisappearShader");
            teleportShader.TrySetParameter("cutoffY", cutoffY);
            teleportShader.TrySetParameter("invertDisappearanceDirection", invertDisappearanceDirection);
            teleportShader.SetTexture(MiscTexturesRegistry.TurbulentNoise.Value, 1, SamplerState.LinearWrap);
            teleportShader.Apply();

            SpriteEffects direction = NPC.spriteDirection.ToSpriteDirection();
            Main.EntitySpriteDraw(EmpressOfLightTargetManager.EmpressTarget, drawPosition, null, NPC.GetAlpha(color), rotation, EmpressOfLightTargetManager.EmpressTarget.Size() * 0.5f, NPC.scale, direction, 0f);

            DrawTeleportRing(drawPosition, cutoffY, invertDisappearanceDirection);
        }

        /// <summary>
        /// Draws the Empress' dash visuals.
        /// </summary>
        /// <param name="screenPos">The position of the camera. Used to offset world positions into screen space.</param>
        /// <param name="illusionInterpolant">The illusion interpolant. Used for the vanilla game swirling illusions visual.</param>
        public void DrawAfterimageVisuals(Vector2 screenPos, float illusionInterpolant)
        {
            for (int i = NPC.oldPos.Length - 1; i >= 1; i--)
            {
                float opacity = Utilities.InverseLerp(NPC.oldPos.Length, 1f, i) * DashAfterimageInterpolant * 0.8f;
                Vector2 drawPosition = Vector2.Lerp(NPC.oldPos[i] + NPC.Size * 0.5f, NPC.Center, 0f) - screenPos;
                Color afterimageColor = Main.hslToRgb((i + 5f) / 10f - Main.GlobalTimeWrappedHourly * 0.2f, 0.7f, 0.5f, 0) * opacity;
                DrawInstance(drawPosition, afterimageColor, NPC.oldRot[i], 0f, false);
            }

            for (int i = 0; i < 25; i++)
            {
                float time = (float)Main.timeForVisualEffects / 60f;
                Vector2 illusionDrawPosition = NPC.Center - screenPos - NPC.velocity * i * 0.23f;
                Vector3 illusionOffset = Vector3.Transform(Vector3.Forward,
                    Matrix.CreateRotationX((time - 0.3f + i * 0.1f) * MathHelper.TwoPi * 0.7f) *
                    Matrix.CreateRotationY((time - 0.8f + i * 0.3f) * MathHelper.TwoPi * 0.7f) *
                    Matrix.CreateRotationZ((time + i * 0.5f) * MathHelper.TwoPi * 0.1f));
                illusionDrawPosition += NPC.scale * new Vector2(illusionOffset.X, illusionOffset.Y) * illusionInterpolant * 150f;

                Color illusionColor = Main.hslToRgb((i + 5f) / 10f, 0.7f, 0.5f) * illusionInterpolant;

                DrawInstance(illusionDrawPosition, illusionColor with { A = 0 }, NPC.rotation, 0f, false);
            }
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
            if (Phase2)
                DrawTentacles(drawPosition);
            Main.EntitySpriteDraw(texture, drawPosition, NPC.frame, Color.White, 0f, NPC.frame.Size() * 0.5f, 1f, 0);
            if (Phase2)
                DrawDress(drawPosition);
            DrawHands(drawPosition);
        }

        /// <summary>
        /// Draws the Empress' backglow.
        /// </summary>
        /// <param name="drawPosition">The draw position of the backglow.</param>
        public void DrawBackglow(Vector2 drawPosition)
        {
            float backglowOpacity = MathHelper.Lerp(0.5f, 0.03f, MathF.Sqrt(DashAfterimageInterpolant));
            float backglowScale = 1f - Utilities.InverseLerpBump(0f, 0.5f, 0.6f, 1f, TeleportCompletionRatio);
            Color rainbow = Main.hslToRgb(Main.GlobalTimeWrappedHourly * 0.5f % 1f, 1f, 0.5f, 0);
            Texture2D backglow = MiscTexturesRegistry.BloomCircleSmall.Value;
            Main.EntitySpriteDraw(backglow, drawPosition, null, Color.Wheat with { A = 0 } * backglowOpacity * 0.25f, 0f, backglow.Size() * 0.5f, backglowScale * 4.1f, 0);
            Main.EntitySpriteDraw(backglow, drawPosition, null, Color.Wheat with { A = 0 } * backglowOpacity * 0.67f, 0f, backglow.Size() * 0.5f, backglowScale * 2.85f, 0);
            Main.EntitySpriteDraw(backglow, drawPosition, null, rainbow * backglowOpacity * 0.7f, 0f, backglow.Size() * 0.5f, backglowScale * 1.5f, 0);
            Main.EntitySpriteDraw(backglow, drawPosition, null, Color.Wheat with { A = 0 } * backglowOpacity, 0f, backglow.Size() * 0.5f, backglowScale * 0.8f, 0);
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

            Main.EntitySpriteDraw(wingsTexture, drawPosition, wingsFrame, Color.White, 0f, wingsFrame.Size() * 0.5f, 2f, 0);

            Main.spriteBatch.End();
            EmpressOfLightTargetManager.BeginSpriteBatch(SpriteSortMode.Immediate);

            ManagedShader gradientShader = ShaderManager.GetShader("WoTE.EmpressWingGradientShader");
            gradientShader.SetTexture(TextureAssets.Extra[ExtrasID.HallowBossGradient], 1, SamplerState.LinearWrap);
            gradientShader.Apply();

            Main.EntitySpriteDraw(wingsColorShapeTexture, drawPosition, wingsFrame, Color.White, 0f, wingsFrame.Size() * 0.5f, 2f, 0);

            Main.spriteBatch.End();
            EmpressOfLightTargetManager.BeginSpriteBatch(SpriteSortMode.Deferred);
        }

        /// <summary>
        /// Draws the Empress' tentacles.
        /// </summary>
        /// <param name="drawPosition">The draw position of the tentacles.</param>
        public void DrawTentacles(Vector2 drawPosition)
        {
            Color tentacleColor = Color.Gold * 0.6f;
            tentacleColor.A = 0;

            Texture2D tentaclesTexture = TextureAssets.Extra[ExtrasID.HallowBossTentacles].Value;
            Rectangle tantaclesFrame = tentaclesTexture.Frame(1, 8, 0, (int)(Main.GlobalTimeWrappedHourly * 15f) % 8);
            Main.EntitySpriteDraw(tentaclesTexture, drawPosition, tantaclesFrame, Color.White, 0f, tantaclesFrame.Size() * 0.5f, 1f, 0);
            for (int i = 0; i < 4; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 4f).ToRotationVector2() * 2f;
                Main.EntitySpriteDraw(tentaclesTexture, drawPosition + drawOffset, tantaclesFrame, tentacleColor, 0f, tantaclesFrame.Size() * 0.5f, 1f, 0);
            }
        }

        /// <summary>
        /// Draws the Empress' arms.
        /// </summary>
        /// <param name="drawPosition">The draw position of the arms.</param>
        public void DrawHands(Vector2 drawPosition)
        {
            Texture2D leftHandTexture = TextureAssets.Extra[ExtrasID.HallowBossArmsLeft].Value;
            Rectangle leftHandFrame = leftHandTexture.Frame(1, 7, 0, (int)LeftHandFrame);
            Main.EntitySpriteDraw(leftHandTexture, drawPosition, leftHandFrame, Color.White, 0f, leftHandFrame.Size() * 0.5f, 1f, 0);

            Texture2D rightHandTexture = TextureAssets.Extra[ExtrasID.HallowBossArmsRight].Value;
            Rectangle rightHandFrame = rightHandTexture.Frame(1, 7, 0, (int)RightHandFrame);
            Main.EntitySpriteDraw(rightHandTexture, drawPosition, rightHandFrame, Color.White, 0f, rightHandFrame.Size() * 0.5f, 1f, 0);
        }

        /// <summary>
        /// Draws the Empress' glowing dress.
        /// </summary>
        /// <param name="drawPosition">The draw position of the dress.</param>
        public void DrawDress(Vector2 drawPosition)
        {
            Texture2D dressTexture = TextureAssets.Extra[ExtrasID.HallowBossSkirt].Value;
            Main.EntitySpriteDraw(dressTexture, drawPosition, null, Color.White, 0f, dressTexture.Size() * 0.5f, 1f, 0);
        }
    }
}
