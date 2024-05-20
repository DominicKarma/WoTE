using System;
using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL.Projectiles
{
    public class ConvergingMoonlight : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<EmpressOfLight>
    {
        /// <summary>
        /// How long this moonlight has existed for, in frames.
        /// </summary>
        public ref float Time => ref Projectile.ai[0];

        /// <summary>
        /// The starting position of the moonlight.
        /// </summary>
        public Vector2 Start;

        /// <summary>
        /// The set of past positions relative to the Empress.
        /// </summary>
        public Vector2[] RelatveOldPositions;

        /// <summary>
        /// How long this moonlight should exist for, in frames.
        /// </summary>
        public static int Lifetime => Utilities.SecondsToFrames(0.8f);

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 96;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;
        }

        public override void SetDefaults()
        {
            Projectile.width = Main.rand?.Next(36, 100) ?? 100;
            Projectile.height = Projectile.width;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.hide = true;
            Projectile.timeLeft = Lifetime + 45;
            RelatveOldPositions = new Vector2[Lifetime];

            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            if (EmpressOfLight.Myself is null)
            {
                Projectile.Kill();
                return;
            }

            if (Start == Vector2.Zero)
                Start = Projectile.Center;

            float spinInterpolant = MathF.Pow(Utilities.InverseLerp(0f, Lifetime, Time), 1.3f);

            Vector2 end = EmpressOfLight.Myself.Center - Vector2.UnitY;
            Vector2 linearOffset = end - Start;
            Vector2 rotatedOffset = linearOffset.RotatedBy(MathHelper.PiOver2 * (1f - spinInterpolant.Squared()));

            Projectile.Center = Start + rotatedOffset * spinInterpolant;
            Projectile.velocity *= 0.5f;
            Projectile.Opacity = Utilities.InverseLerp(0f, 36f, Time) * Utilities.InverseLerp(45f, 10f, Time - Lifetime);

            for (int i = RelatveOldPositions.Length - 1; i >= 1; i--)
            {
                Vector2 offset = RelatveOldPositions[i - 1].SafeDirectionTo(RelatveOldPositions[i]);
                RelatveOldPositions[i] = RelatveOldPositions[i - 1] + offset * 12f;
            }
            if (Time <= Lifetime)
                RelatveOldPositions[0] = Projectile.Center - EmpressOfLight.Myself.Center;

            Time++;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;

        public float MoonlightWidthFunction(float completionRatio)
        {
            float baseWidth = Projectile.width;
            return baseWidth * Projectile.scale;
        }

        public Color MoonlightColorFunction(float completionRatio)
        {
            return new Color(182, 170, 255) * Utilities.InverseLerpBump(0f, 0.4f, 0.1f, 0.9f, completionRatio) * Projectile.Opacity * Utilities.Convert01To010(completionRatio);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            if (EmpressOfLight.Myself is null)
                return;

            ManagedShader trailShader = ShaderManager.GetShader("WoTE.ConvergingMoonlightShader");
            trailShader.SetTexture(MiscTexturesRegistry.WavyBlotchNoise.Value, 1, SamplerState.LinearWrap);
            trailShader.Apply();

            PrimitiveSettings settings = new(MoonlightWidthFunction, MoonlightColorFunction, _ => EmpressOfLight.Myself.Center, Pixelate: true, Shader: trailShader);
            PrimitiveRenderer.RenderTrail(RelatveOldPositions, settings, 35);
        }
    }
}
