﻿using System;
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
using WoTE.Content.NPCs.EoL.Projectiles;

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

        /// <summary>
        /// The scale of the Empress' butterfly projection.
        /// </summary>
        public float ButterflyProjectionScale
        {
            get;
            set;
        }

        /// <summary>
        /// The opacity of the Empress' butterfly projection.
        /// </summary>
        public float ButterflyProjectionOpacity
        {
            get;
            set;
        }

        /// <summary>
        /// The amount of directional blur that should be applied to the Empress.
        /// </summary>
        public float BlurInterpolant
        {
            get;
            set;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
        {
            if (Palette is null)
                return false;

            if (ButterflyProjectionOpacity > 0f && ButterflyProjectionScale > 0f)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                DrawButterflyProjection(NPC.Center - screenPos, 1f);
                Main.spriteBatch.ResetToDefault();
            }

            Main.spriteBatch.PrepareForShaders();

            float illusionInterpolant = (1f - Utilities.InverseLerpBump(0f, 0.2f, 0.8f, 1f, TeleportCompletionRatio)) * DashAfterimageInterpolant;
            float cutoffYInterpolant = EasingCurves.Quadratic.Evaluate(EasingType.InOut, TeleportCompletionRatio);
            if (TeleportCompletionRatio > 0f && CurrentState == EmpressAIType.Teleport)
            {
                DrawInstance(NPC.Center - screenPos, Color.White * (1f - illusionInterpolant), ZPosition, NPC.rotation, cutoffYInterpolant, false);
                DrawInstance(TeleportDestination - screenPos, Color.White, NPC.rotation, ZPosition, 1f - cutoffYInterpolant, false);
            }
            else
                DrawInstance(NPC.Center - screenPos, Color.White, ZPosition, NPC.rotation, 0f, false);

            if (DashAfterimageInterpolant > 0f)
                DrawAfterimageVisuals(screenPos, illusionInterpolant);

            Main.spriteBatch.ResetToDefault();

            return false;
        }

        public void DrawButterflyProjection(Vector2 baseDrawPosition, float opacity)
        {
            float flapScale = MathHelper.Lerp(0.4f, 1f, Utilities.Cos01(MathHelper.TwoPi * Main.GlobalTimeWrappedHourly * 0.85f));
            float bloomScaleFactor = ButterflyProjectionOpacity;
            float bloomOpacity = MathHelper.Lerp(0.4f, 1f, flapScale) * opacity;
            Color bloomColor = Palette.MulticolorLerp(EmpressPaletteType.ButterflyAvatar, 0.5f) with { A = 0 };
            Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
            Main.spriteBatch.Draw(bloom, baseDrawPosition, null, Color.White with { A = 0 } * bloomOpacity * 0.43f, 0f, bloom.Size() * 0.5f, bloomScaleFactor * 6f, 0, 0f);
            Main.spriteBatch.Draw(bloom, baseDrawPosition, null, bloomColor * bloomOpacity * 0.8f, 0f, bloom.Size() * 0.5f, bloomScaleFactor * 9f, 0, 0f);

            Texture2D body = ModContent.Request<Texture2D>("WoTE/Content/NPCs/EoL/Rendering/ButterflyProjectionBody").Value;
            Texture2D wing = ModContent.Request<Texture2D>("WoTE/Content/NPCs/EoL/Rendering/ButterflyProjectionWing").Value;

            float[] blurWeights = new float[7];
            for (int i = 0; i < blurWeights.Length; i++)
                blurWeights[i] = Utilities.GaussianDistribution(i - (int)(blurWeights.Length * 0.5f), 0.5f) / 11f;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -10f, 10f);
            Vector4[] avatarPalette = Palette.Get(EmpressPaletteType.ButterflyAvatar);
            ManagedShader avatarShader = ShaderManager.GetShader("WoTE.EmpressButterflyAvatarShader");
            avatarShader.TrySetParameter("horizontalScale", flapScale);
            avatarShader.TrySetParameter("gradientCount", avatarPalette.Length);
            avatarShader.TrySetParameter("gradient", avatarPalette);
            avatarShader.TrySetParameter("blurOffset", 0.003f);
            avatarShader.TrySetParameter("blurWeights", blurWeights);
            avatarShader.TrySetParameter("center", baseDrawPosition);
            avatarShader.TrySetParameter("uWorldViewProjection", Main.GameViewMatrix.TransformationMatrix * projection);
            avatarShader.Apply();

            Vector2 baseScale = Vector2.One * ButterflyProjectionScale;
            Vector2 wingScale = Vector2.One * baseScale;
            Vector2 wingDrawPosition = baseDrawPosition + Vector2.UnitY * 54f;
            Main.EntitySpriteDraw(wing, wingDrawPosition - Vector2.UnitX * ((1f - flapScale) * 100f + 36f) * baseScale.X, null, Color.White * ButterflyProjectionOpacity * opacity, 0f, wing.Size() * new Vector2(0f, 0.5f), wingScale, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(wing, wingDrawPosition + Vector2.UnitX * ((1f - flapScale) * 100f + 36f) * baseScale.X, null, Color.White * ButterflyProjectionOpacity * opacity, 0f, wing.Size() * new Vector2(1f, 0.5f), wingScale, SpriteEffects.FlipHorizontally, 0f);

            avatarShader.TrySetParameter("horizontalScale", 1f);
            avatarShader.Apply();

            Main.EntitySpriteDraw(body, baseDrawPosition, null, Color.White * ButterflyProjectionOpacity * opacity, 0f, body.Size() * 0.5f, baseScale * new Vector2(1f, 0.81f), 0, 0f);
        }

        /// <summary>
        /// Draws a single instance of the Empress at a given position in screen space.
        /// </summary>
        /// <param name="drawPosition">The draw position of the Empress instance.</param>
        /// <param name="color">The color of the Empress instance.</param>
        /// <param name="z">The Z position of the Empress instance.</param>
        /// <param name="cutoffY">The instance's rotation.</param>
        /// <param name="cutoffY">The Y cutoff interpolant value.</param>
        /// <param name="invertDisappearanceDirection">Whether the direction of disappearance should be inverted.</param>
        public void DrawInstance(Vector2 drawPosition, Color color, float z, float rotation, float cutoffY, bool invertDisappearanceDirection)
        {
            float scale = NPC.scale / (z + 1f);
            float backgroundFadeInterpolant = Utilities.InverseLerp(1f, 2.6f, z);
            float defocusInterpolant = Utilities.InverseLerp(0.3f, 2.4f, z);
            float opacity = MathHelper.Lerp(1f, 0.43f, backgroundFadeInterpolant);

            float[] defocusBlurWeights = new float[5];
            for (int i = 0; i < defocusBlurWeights.Length; i++)
                defocusBlurWeights[i] = Utilities.GaussianDistribution(i - (int)(defocusBlurWeights.Length * 0.5f), 0.8f) / 7f;

            float[] directionalBlurWeights = new float[18];
            for (int i = 0; i < directionalBlurWeights.Length; i++)
                directionalBlurWeights[i] = Utilities.GaussianDistribution(i - (int)(directionalBlurWeights.Length * 0.5f), 6f);

            ManagedShader postProcessingShader = ShaderManager.GetShader("WoTE.EmpressPostProcessingShader");
            postProcessingShader.TrySetParameter("cutoffY", cutoffY);
            postProcessingShader.TrySetParameter("invertDisappearanceDirection", invertDisappearanceDirection);
            postProcessingShader.TrySetParameter("defocusBlurOffset", defocusInterpolant * 0.004f);
            postProcessingShader.TrySetParameter("defocusBlurWeights", defocusBlurWeights);
            postProcessingShader.TrySetParameter("directionBlurInterpolant", BlurInterpolant);
            postProcessingShader.TrySetParameter("directionalBlurOffset", NPC.velocity.SafeNormalize(rotation.ToRotationVector2()) * BlurInterpolant * 0.0016f);
            postProcessingShader.TrySetParameter("directionalBlurWeights", directionalBlurWeights);
            postProcessingShader.SetTexture(MiscTexturesRegistry.TurbulentNoise.Value, 1, SamplerState.LinearWrap);
            postProcessingShader.Apply();

            SpriteEffects direction = NPC.spriteDirection.ToSpriteDirection();
            Main.EntitySpriteDraw(EmpressOfLightTargetManager.EmpressTarget, drawPosition, null, NPC.GetAlpha(color) * opacity, rotation, EmpressOfLightTargetManager.EmpressTarget.Size() * 0.5f, scale, direction, 0f);

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
                Color afterimageColor = Palette.MulticolorLerp(EmpressPaletteType.RainbowArrow, (i + 5f) / 10f - Main.GlobalTimeWrappedHourly * 0.2f) * opacity;
                DrawInstance(drawPosition, afterimageColor with { A = 0 }, OldZPositions[i], NPC.oldRot[i], 0f, false);
            }

            for (int i = 0; i < 25; i++)
            {
                float time = (float)Main.timeForVisualEffects / 60f;

                Matrix transformX = Matrix.CreateRotationX((time - 0.3f + i * 0.1f) * MathHelper.TwoPi * 0.7f);
                Matrix transformY = Matrix.CreateRotationY((time - 0.8f + i * 0.3f) * MathHelper.TwoPi * 0.7f);
                Matrix transformZ = Matrix.CreateRotationZ((time + i * 0.5f) * MathHelper.TwoPi * 0.1f);
                Vector2 illusionDrawPosition = NPC.Center - screenPos - NPC.velocity * i * 0.23f;
                Vector3 illusionOffset = Vector3.Transform(Vector3.Forward, transformX * transformY * transformZ) * illusionInterpolant * 150f;
                illusionDrawPosition += NPC.scale / (ZPosition + 1f) * new Vector2(illusionOffset.X, illusionOffset.Y);
                Color illusionColor = Palette.MulticolorLerp(EmpressPaletteType.RainbowArrow, (i + 5f) / 10f) * illusionInterpolant;

                DrawInstance(illusionDrawPosition, illusionColor with { A = 0 }, ZPosition, NPC.rotation, 0f, false);
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

            Vector4[] teleportRingPalette = Palette.Get(EmpressPaletteType.PrismaticBolt);
            Texture2D gradient = TextureAssets.Extra[ExtrasID.HallowBossGradient].Value;
            float teleportRingOpacity = Utilities.InverseLerpBump(0f, 0.25f, 0.85f, 1f, cutoffY).Cubed();
            Vector2 teleportRingPosition = drawPosition + Vector2.UnitY * (cutoffY - 0.5f) * 570f;
            ManagedShader teleportRingShader = ShaderManager.GetShader("WoTE.EmpressTeleportRingShader");
            teleportRingShader.SetTexture(MiscTexturesRegistry.DendriticNoiseZoomedOut.Value, 1, SamplerState.LinearWrap);
            teleportRingShader.TrySetParameter("pulsationIntensity", 1f - cutoffY);
            teleportRingShader.TrySetParameter("invertDisappearanceDirection", invertDisappearanceDirection);
            teleportRingShader.TrySetParameter("gradient", teleportRingPalette);
            teleportRingShader.TrySetParameter("gradientCount", teleportRingPalette.Length);
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
            if (Palette is null)
                return;

            Texture2D texture = ModContent.Request<Texture2D>(Palette.BodyTextureOverride ?? "WoTE/Content/NPCs/EoL/Rendering/EmpressBody").Value;
            Texture2D eyes = ModContent.Request<Texture2D>("WoTE/Content/NPCs/EoL/Rendering/EmpressEyes").Value;

            DrawBackglow(drawPosition);
            DrawWings(drawPosition);
            if (Phase2)
                DrawTentacles(drawPosition);

            Main.EntitySpriteDraw(texture, drawPosition, null, Color.White, 0f, texture.Size() * 0.5f, 1f, 0);
            Main.EntitySpriteDraw(eyes, drawPosition, null, Color.Magenta, 0f, eyes.Size() * 0.5f, 1f, 0);

            if (Phase2 || Palette != EmpressPalettes.Default)
                DrawDress(drawPosition);

            DrawHands(drawPosition);
            DoBehavior_EventideLances_DrawBow(drawPosition);
        }

        /// <summary>
        /// Draws the Empress' backglow.
        /// </summary>
        /// <param name="drawPosition">The draw position of the backglow.</param>
        public void DrawBackglow(Vector2 drawPosition)
        {
            float backglowOpacity = MathHelper.Lerp(0.5f, 0.03f, MathF.Sqrt(DashAfterimageInterpolant));
            backglowOpacity = MathHelper.Lerp(backglowOpacity, 0.03f, Utilities.InverseLerp(0.3f, 0.9f, ZPosition));

            float backglowScale = 1f - Utilities.InverseLerpBump(0f, 0.5f, 0.6f, 1f, TeleportCompletionRatio);
            Color rainbow = Palette.MulticolorLerp(EmpressPaletteType.Wings, Main.GlobalTimeWrappedHourly * 0.5f) with { A = 0 };
            Color baseGlow = Palette.MulticolorLerp(EmpressPaletteType.Wings, 0.7f) with { A = 0 };
            Texture2D backglow = MiscTexturesRegistry.BloomCircleSmall.Value;
            Main.EntitySpriteDraw(backglow, drawPosition, null, baseGlow * backglowOpacity * 0.25f, 0f, backglow.Size() * 0.5f, backglowScale * 4.1f, 0);
            Main.EntitySpriteDraw(backglow, drawPosition, null, baseGlow * backglowOpacity * 0.67f, 0f, backglow.Size() * 0.5f, backglowScale * 2.85f, 0);
            Main.EntitySpriteDraw(backglow, drawPosition, null, rainbow * backglowOpacity * 0.7f, 0f, backglow.Size() * 0.5f, backglowScale * 1.5f, 0);
            Main.EntitySpriteDraw(backglow, drawPosition, null, baseGlow * backglowOpacity, 0f, backglow.Size() * 0.5f, backglowScale * 0.8f, 0);
        }

        /// <summary>
        /// Draws the Empress' wings.
        /// </summary>
        /// <param name="drawPosition">The draw position of the wings.</param>
        public void DrawWings(Vector2 drawPosition)
        {
            Texture2D wingsTexture = ModContent.Request<Texture2D>(Palette.WingTextureOverride ?? "WoTE/Content/NPCs/EoL/Rendering/EmpressWings").Value;
            Texture2D wingsColorShapeTexture = ModContent.Request<Texture2D>("WoTE/Content/NPCs/EoL/Rendering/EmpressWingsGreyscale").Value;
            Rectangle wingsFrame = wingsTexture.Frame(1, 11, 0, (int)(Main.GlobalTimeWrappedHourly * 15f) % 11);

            Main.EntitySpriteDraw(wingsTexture, drawPosition, wingsFrame, Color.White, 0f, wingsFrame.Size() * 0.5f, 2f, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

            float timeOffset = Main.GlobalTimeWrappedHourly;
            if (Palette == EmpressPalettes.DaytimePaletteSet)
                timeOffset = 0.6f;
            if (Palette == EmpressPalettes.BloodMoonPaletteSet)
                timeOffset = 0.24f;

            Vector4[] wingsPalette = Palette.Get(EmpressPaletteType.Wings);
            ManagedShader gradientShader = ShaderManager.GetShader("WoTE.EmpressWingGradientShader");
            gradientShader.TrySetParameter("gradient", wingsPalette);
            gradientShader.TrySetParameter("gradientCount", wingsPalette.Length);
            gradientShader.TrySetParameter("timeOffset", timeOffset);
            gradientShader.SetTexture(TextureAssets.Projectile[ModContent.ProjectileType<StarBolt>()], 1, SamplerState.PointWrap);
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
            Texture2D tentaclesTexture = ModContent.Request<Texture2D>(Palette.TentaclesTextureOverride ?? "WoTE/Content/NPCs/EoL/Rendering/Tentacles").Value;
            Rectangle tantaclesFrame = tentaclesTexture.Frame(1, 8, 0, (int)(Main.GlobalTimeWrappedHourly * 15f) % 8);
            Main.EntitySpriteDraw(tentaclesTexture, drawPosition, tantaclesFrame, Color.White, 0f, tantaclesFrame.Size() * 0.5f, 1f, 0);
        }

        /// <summary>
        /// Draws the Empress' arms.
        /// </summary>
        /// <param name="drawPosition">The draw position of the arms.</param>
        public void DrawHands(Vector2 drawPosition)
        {
            GetHandDrawData((int)LeftHandFrame, out Texture2D leftHandTexture, out Rectangle leftHandFrame);
            Main.EntitySpriteDraw(leftHandTexture, drawPosition, leftHandFrame, Color.White, 0f, leftHandFrame.Size() * 0.5f, 1f, SpriteEffects.None);

            DoBehavior_EventideLances_DrawBowString(drawPosition);

            GetHandDrawData((int)RightHandFrame, out Texture2D rightHandTexture, out Rectangle rightHandFrame);
            Main.EntitySpriteDraw(rightHandTexture, drawPosition, rightHandFrame, Color.White, 0f, rightHandFrame.Size() * 0.5f, 1f, SpriteEffects.FlipHorizontally);
        }

        /// <summary>
        /// Collects draw data for a given arm frame that the Empress uses.
        /// </summary>
        /// <param name="frameY">The arm's Y frame.</param>
        /// <param name="handTexture">The selected hand texture.</param>
        /// <param name="handFrame">The selected hand frame rectangle.</param>
        public void GetHandDrawData(int frameY, out Texture2D handTexture, out Rectangle handFrame)
        {
            handTexture = ModContent.Request<Texture2D>(Palette.ArmTextureOverride ?? "WoTE/Content/NPCs/EoL/Rendering/Arm").Value;
            handFrame = handTexture.Frame(1, 8, 0, frameY);
        }

        /// <summary>
        /// Draws the Empress' glowing dress.
        /// </summary>
        /// <param name="drawPosition">The draw position of the dress.</param>
        public void DrawDress(Vector2 drawPosition)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

            float scrollTime = Main.GlobalTimeWrappedHourly;
            if (Palette == EmpressPalettes.BloodMoonPaletteSet)
                scrollTime = 0.38f;

            Vector4[] dressPalette = Palette.Get(EmpressPaletteType.Phase2Dress);
            Texture2D dressTexture = ModContent.Request<Texture2D>("WoTE/Content/NPCs/EoL/Rendering/EmpressDressGlow").Value;
            ManagedShader gradientShader = ShaderManager.GetShader("WoTE.EmpressDressShader");
            gradientShader.TrySetParameter("gradient", dressPalette);
            gradientShader.TrySetParameter("gradientCount", dressPalette.Length);
            gradientShader.TrySetParameter("baseTextureSize", dressTexture.Size());
            gradientShader.TrySetParameter("scrollTime", scrollTime);
            gradientShader.Apply();

            Main.EntitySpriteDraw(dressTexture, drawPosition, null, Color.White, 0f, dressTexture.Size() * 0.5f, 1f, 0);

            Main.spriteBatch.End();
            EmpressOfLightTargetManager.BeginSpriteBatch(SpriteSortMode.Deferred);
        }

        public override void DrawBehind(int index)
        {
            if (ButterflyProjectionScale > 0f)
                Main.instance.DrawCacheNPCsMoonMoon.Add(index);
            else if (ZPosition >= 0.45f)
                Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
            else
                Main.instance.DrawCacheNPCsMoonMoon.Add(index);
        }
    }
}
