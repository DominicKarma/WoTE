using System;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using WoTE.Content.Particles;
using WoTE.Content.Particles.Metaballs;

namespace WoTE.Content.NPCs.EoL.Projectiles
{
    public class PrismaticBurst : ModProjectile
    {
        /// <summary>
        /// The lifetime ratio of this burst as a 0-1 interpolant.
        /// </summary>
        public float LifetimeRatio => 1f - Projectile.timeLeft / (float)Lifetime;

        /// <summary>
        /// The current radius of this burst.
        /// </summary>
        public ref float Radius => ref Projectile.ai[0];

        /// <summary>
        /// The ideal, maximum radius of this burst.
        /// </summary>
        public static float IdealRadius => 600f;

        /// <summary>
        /// How long this burst should exist for, in frames.
        /// </summary>
        public static int Lifetime => Utilities.SecondsToFrames(0.25f);

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.timeLeft = Lifetime;
        }

        public override void AI()
        {
            // Make screen shove effects happen on the first frame.
            if (Projectile.localAI[0] == 0f)
            {
                ModContent.GetInstance<DistortionMetaball>().CreateParticle(Projectile.Center + Main.rand.NextVector2Circular(50f, 50f), Vector2.Zero, 30f, 0.75f, 0.25f, 0.02f);
                Projectile.localAI[0] = 1f;
            }

            Radius = MathHelper.Lerp(Radius, IdealRadius, 0.075f);
            Projectile.Opacity = Utilities.InverseLerp(2f, 10f, Projectile.timeLeft);
            if (Projectile.Opacity <= 0.8f)
                Projectile.damage = 0;

            Vector2 originalSize = Projectile.Size;
            Projectile.Size = Vector2.One * Radius * 1.1f;
            Projectile.position -= (Projectile.Size - originalSize) * 0.5f;

            // Randomly create small fire particles.
            float fireVelocityArc = MathHelper.Pi * Utilities.InverseLerp(Lifetime, 0f, Projectile.timeLeft) * 0.67f;
            for (int i = 0; i < 8; i++)
            {
                Vector2 particleSpawnPosition = Projectile.Center + Main.rand.NextVector2Unit() * Radius * Projectile.scale * Main.rand.NextFloat(0.4f, 0.56f);
                Vector2 particleVelocity = (particleSpawnPosition - Projectile.Center).SafeNormalize(Vector2.UnitY).RotatedBy(fireVelocityArc) * Main.rand.NextFloat(2f, 11f);
                BloomCircleParticle particle = new(particleSpawnPosition, particleVelocity, new Vector2(1f, 0.5f) * Main.rand.NextFloat(0.03f, 0.1f), Color.White, Color.Lerp(Color.DeepSkyBlue, Color.HotPink, Main.rand.NextFloat(0.8f)), Main.rand.Next(25, 44), 2f);
                particle.Spawn();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
            DrawData burstDrawData = new(MiscTexturesRegistry.TurbulentNoise.Value, new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y), Color.White * Projectile.Opacity);

            var shockwaveShader = ShaderManager.GetShader("WoTE.ShockwaveShader");
            shockwaveShader.TrySetParameter("shockwaveColor", Color.Lerp(Color.DeepSkyBlue, Color.HotPink, MathF.Pow(1f - LifetimeRatio, 0.95f)));
            shockwaveShader.TrySetParameter("screenSize", screenSize);
            shockwaveShader.TrySetParameter("explosionDistance", Radius * Projectile.scale * 0.5f);
            shockwaveShader.TrySetParameter("projectilePosition", Projectile.Center - Main.screenPosition);
            shockwaveShader.TrySetParameter("shockwaveOpacityFactor", Projectile.Opacity);
            shockwaveShader.Apply();
            burstDrawData.Draw(Main.spriteBatch);

            Main.spriteBatch.ResetToDefault();
            return false;
        }
    }
}
