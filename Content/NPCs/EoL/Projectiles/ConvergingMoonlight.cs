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
        /// How long this lance has existed for, in frames.
        /// </summary>
        public ref float Time => ref Projectile.ai[0];

        public Vector2 Start;

        public static int Lifetime => Utilities.SecondsToFrames(1.1f);

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 96;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 70;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.hide = true;
            Projectile.timeLeft = Lifetime + 45;

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
            Vector2 end = EmpressOfLight.Myself.Center;

            float proximityInterpolant = Utilities.InverseLerp(70f, 200f, Projectile.Distance(end));
            float sine = MathF.Sin(MathHelper.Pi * Utilities.InverseLerp(0f, Lifetime + 4f, Time));
            Projectile.Center = Vector2.Lerp(Start, end, Utilities.InverseLerp(0f, Lifetime, Time)) + (end - Start).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * sine * MathF.Sqrt(proximityInterpolant) * 270f;
            Projectile.Opacity = Utilities.InverseLerp(10f, 0f, Time - Lifetime);
            Projectile.velocity *= 0.5f;

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
            float endPointFade = Utilities.InverseLerpBump(0.05f, 0.3f, 0.7f, 0.95f, completionRatio);
            float opacity = Utilities.InverseLerp(completionRatio, 1f, Projectile.Opacity);
            return new Color(182, 170, 255, 0) * endPointFade * opacity * 0.75f;
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            ManagedShader trailShader = ShaderManager.GetShader("WoTE.ConvergingMoonlightShader");
            trailShader.SetTexture(MiscTexturesRegistry.DendriticNoiseZoomedOut.Value, 1, SamplerState.LinearWrap);
            trailShader.Apply();

            PrimitiveSettings settings = new(MoonlightWidthFunction, MoonlightColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true, Shader: trailShader);
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 22);
        }
    }
}
