using System;
using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.Particles;

namespace WoTE.Content.NPCs.EoL.Projectiles
{
    public class StarBolt : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<EmpressOfLight>
    {
        public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.BeforeProjectiles;

        /// <summary>
        /// How long this bolt has existed, in frames.
        /// </summary>
        public ref float Time => ref Projectile.ai[0];

        /// <summary>
        /// How long this bolt should last, in frames.
        /// </summary>
        public static int Lifetime => Utilities.SecondsToFrames(2.4f);

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1100;
        }

        public override void SetDefaults()
        {
            Projectile.width = 35;
            Projectile.height = 35;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.hide = true;
            Projectile.timeLeft = Lifetime;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            if (Time >= 120)
                Projectile.velocity *= 0.65f;
            else if (Projectile.velocity.Length() <= 70f)
                Projectile.velocity *= 1.07f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.velocity.Length() >= 12f)
                CreateParticles();

            Time++;
        }

        /// <summary>
        /// Creates various particles for this star bolt.
        /// </summary>
        public void CreateParticles()
        {
            float sinusoidalAngle = CalculateSinusoidalOffset(0.4f) * 1.2f;
            Vector2 particleVelocity = -Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(sinusoidalAngle) * Main.rand.NextFloat(2.5f, 3.3f) + Main.rand.NextVector2Circular(1.6f, 1.6f);
            Color particleColor = Main.hslToRgb(Main.rand.NextFloat(0.93f, 1.15f) % 1f, 1f, 0.7f) * 0.8f;

            BloomCircleParticle particle = new(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), particleVelocity, Vector2.One * Vector2.One * 0.045f, Color.Wheat, particleColor, 60, 1.8f, 1.75f);
            particle.Spawn();

            particle = new(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), -particleVelocity, Vector2.One * 0.045f, Color.Wheat, particleColor, 60, 1.8f, 1.75f);
            particle.Spawn();
        }

        public float BoltWidthFunction(float completionRatio)
        {
            float baseWidth = Projectile.width;
            float tipCutFactor = Utilities.InverseLerp(0.035f, 0.1f, completionRatio);
            float slownessFactor = Utils.Remap(Projectile.velocity.Length(), 1f, 5f, 0.18f, 1f);
            return baseWidth * tipCutFactor * slownessFactor;
        }

        public Color BoltColorFunction(float completionRatio)
        {
            float sineOffset = CalculateSinusoidalOffset(completionRatio);
            return Color.Lerp(Color.White, Color.Black, sineOffset * 0.5f + 0.5f);
        }

        public float CalculateSinusoidalOffset(float completionRatio)
        {
            return MathF.Sin(MathHelper.TwoPi * completionRatio + Main.GlobalTimeWrappedHourly * -12f + Projectile.identity) * Utilities.InverseLerp(0.01f, 0.9f, completionRatio);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            ManagedShader trailShader = ShaderManager.GetShader("WoTE.PrismaticBoltShader");
            trailShader.TrySetParameter("gradient", EmpressPalettes.StarBoltPalette);
            trailShader.TrySetParameter("gradientCount", EmpressPalettes.StarBoltPalette.Length);
            trailShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly * 1.2f + Projectile.identity * 1.9f);
            trailShader.SetTexture(MiscTexturesRegistry.WavyBlotchNoise.Value, 1, SamplerState.LinearWrap);
            trailShader.SetTexture(TextureAssets.Extra[ExtrasID.FlameLashTrailShape], 2, SamplerState.LinearWrap);
            trailShader.Apply();

            float perpendicularOffset = Utils.Remap(Projectile.velocity.Length(), 4f, 20f, 14f, 56f);
            Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * perpendicularOffset;
            Vector2[] trailPositions = new Vector2[Projectile.oldPos.Length];
            for (int i = 0; i < trailPositions.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float sine = CalculateSinusoidalOffset(i / (float)trailPositions.Length);
                trailPositions[i] = Projectile.oldPos[i] + perpendicular * sine;
            }

            PrimitiveSettings settings = new(BoltWidthFunction, BoltColorFunction, _ => Projectile.Size * 0.5f + Projectile.velocity.SafeNormalize(Vector2.Zero) * 40f, Pixelate: true, Shader: trailShader);
            PrimitiveRenderer.RenderTrail(trailPositions, settings, 25);
        }
    }
}
