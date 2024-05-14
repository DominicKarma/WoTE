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

        public Vector2[] RelatveOldPositions;

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
            Projectile.width = Main.rand?.Next(84, 120) ?? 120;
            Projectile.height = Projectile.width;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.hide = true;
            Projectile.timeLeft = Lifetime + 70;
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
            Vector2 end = EmpressOfLight.Myself.Center;

            float proximityInterpolant = Utilities.InverseLerp(70f, 200f, Projectile.Distance(end));
            float sine = MathF.Sin(MathHelper.Pi * Utilities.InverseLerp(0f, Lifetime + 4f, Time));
            float spinInterpolant = MathF.Pow(Utilities.InverseLerp(0f, Lifetime, Time), 0.7f);
            Projectile.Center = Vector2.Lerp(Start, end, spinInterpolant) + (end - Start).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * sine * MathF.Sqrt(proximityInterpolant) * 400f;
            Projectile.Opacity = Utilities.InverseLerp(0f, 36f, Time);
            Projectile.velocity *= 0.5f;
            if (Time >= Lifetime)
                Projectile.scale *= 0.98f;

            for (int i = RelatveOldPositions.Length - 1; i >= 1; i--)
                RelatveOldPositions[i] = RelatveOldPositions[i - 1];
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
            float endPointFade = Utilities.InverseLerpBump(0.05f, 0.3f, 0.7f, 0.95f, completionRatio);
            return new Color(182, 170, 255, 0) * endPointFade * 0.6f;
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            if (EmpressOfLight.Myself is null)
                return;

            ManagedShader trailShader = ShaderManager.GetShader("WoTE.ConvergingMoonlightShader");
            trailShader.SetTexture(MiscTexturesRegistry.WavyBlotchNoise.Value, 1, SamplerState.LinearWrap);
            trailShader.Apply();

            PrimitiveSettings settings = new(MoonlightWidthFunction, MoonlightColorFunction, _ => EmpressOfLight.Myself.Center, Pixelate: true, Shader: trailShader);
            PrimitiveRenderer.RenderTrail(RelatveOldPositions, settings, 105);
        }
    }
}
