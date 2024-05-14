using System.Linq;
using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.Particles;

namespace WoTE.Content.NPCs.EoL.Projectiles
{
    public class DazzlingDeathray : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<EmpressOfLight>
    {
        public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.AfterNPCs;

        /// <summary>
        /// The length of this deathray.
        /// </summary>
        public ref float DeathrayLength => ref Projectile.ai[0];

        /// <summary>
        /// How long this deathray has existed for, in frames.
        /// </summary>
        public ref float Time => ref Projectile.ai[2];

        /// <summary>
        /// The maximum length that this deathray can extend.
        /// </summary>
        public static float MaxLength => 5000f;

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

        public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = (int)MaxLength + 640;

        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 70;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.hide = true;
            Projectile.timeLeft = EmpressOfLight.Phase2Transition_ShootDeathrayTime;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            if (EmpressOfLight.Myself is null)
            {
                Projectile.Kill();
                return;
            }

            ScreenShakeSystem.StartShake(Utilities.InverseLerp(25f, 0f, Time) * 7f + 1.6f);

            Projectile.Opacity = Utilities.InverseLerp(0f, 12f, Time);
            Projectile.scale = Utilities.InverseLerp(0f, 28f, Time).Squared() * Utilities.InverseLerp(0f, 15f, Projectile.timeLeft);
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Time == 1)
            {
                SoundEngine.PlaySound(SoundID.Item122);
                SoundEngine.PlaySound(SoundID.Item164);
            }

            float[] distanceSamples = new float[10];
            Collision.LaserScan(Projectile.Center, Projectile.velocity, Projectile.width, MaxLength, distanceSamples);
            float idealLength = distanceSamples.Average();
            if (!Collision.CanHit(EmpressOfLight.Myself, Main.player[EmpressOfLight.Myself.target]))
                idealLength = MaxLength;

            DeathrayLength = MathHelper.Clamp(DeathrayLength + 120f, 0f, idealLength);

            Projectile.Center = EmpressOfLight.Myself.Center;

            float currentDirection = Projectile.velocity.ToRotation();
            float idealDirection = Projectile.AngleTo(Main.player[EmpressOfLight.Myself.target].Center);
            Projectile.velocity = currentDirection.AngleLerp(idealDirection, 0.02f).ToRotationVector2();

            CreateCenterAndEndLacewings();
            CreatePerpendicularEnergy();

            Time++;
        }

        /// <summary>
        /// Creates lacewings at the starting and ending point of this deathray.
        /// </summary>
        public void CreateCenterAndEndLacewings()
        {
            for (int i = 0; i < 3; i++)
            {
                int lacewingLifetime = Main.rand.Next(18, 45);
                float lacewingScale = Main.rand.NextFloat(0.4f, 1.15f);
                Color lacewingColor = Color.Lerp(Color.Yellow, Color.LightGoldenrodYellow, Main.rand.NextFloat()) * 0.6f;
                Vector2 lacewingSpawnPosition = Projectile.Center + Projectile.velocity * Main.rand.NextFloat(DeathrayLength * 0.1f);
                Vector2 lacewingVelocity = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(30f, 250f);
                if (Main.rand.NextBool())
                {
                    lacewingSpawnPosition = Projectile.Center + Projectile.velocity * DeathrayLength + Main.rand.NextVector2Circular(30f, 30f);
                    lacewingVelocity = Main.rand.NextVector2Circular(32f, 32f);
                }

                PrismaticLacewingParticle lacewing = new(lacewingSpawnPosition, lacewingVelocity, lacewingColor, lacewingLifetime, Vector2.One * lacewingScale);
                lacewing.Spawn();
            }
        }

        /// <summary>
        /// Creates energy particles perpendicular along this deathray.
        /// </summary>
        public void CreatePerpendicularEnergy()
        {
            for (int i = 0; i < 6; i++)
            {
                float pixelScale = Main.rand.NextFloat(0.7f, 2.4f);
                Vector2 pixelSpawnPosition = Projectile.Center + Projectile.velocity * Main.rand.NextFloat(DeathrayLength * 0.825f);
                Vector2 pixelVelocity = Projectile.velocity.RotatedBy(Main.rand.NextFromList(-MathHelper.PiOver2, MathHelper.PiOver2)) * Main.rand.NextFloat(8f, 19f) / pixelScale;
                Color pixelBloomColor = Utilities.MulticolorLerp(Main.rand.NextFloat(0.75f), Color.LightGoldenrodYellow, Color.Yellow, Color.Orange) * 0.6f;

                BloomPixelParticle bloom = new(pixelSpawnPosition, pixelVelocity, Color.White, pixelBloomColor, Main.rand.Next(20, 45), Vector2.One * pixelScale);
                bloom.Spawn();
            }
        }

        public override Color? GetAlpha(Color lightColor) => lightColor * Projectile.Opacity;

        public float DeathrayWidthFunction(float completionRatio)
        {
            float baseWidth = Projectile.width * Projectile.scale;
            return baseWidth;
        }

        public Color DeathrayColorFunction(float completionRatio)
        {
            return Projectile.GetAlpha(new(255, 239, 182)) * Utilities.InverseLerp(0f, 0.1f, completionRatio);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Time <= 30f)
                return false;

            float _ = 0f;
            Vector2 start = Projectile.Center;
            Vector2 end = start + Projectile.velocity * DeathrayLength;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, Projectile.scale * Projectile.width * 0.9f, ref _);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            ManagedShader trailShader = ShaderManager.GetShader("WoTE.DazzlingDeathrayShader");
            trailShader.SetTexture(MiscTexturesRegistry.WavyBlotchNoise.Value, 1, SamplerState.AnisotropicWrap);
            trailShader.TrySetParameter("localTime", -Main.GlobalTimeWrappedHourly + Projectile.identity * 0.383f);
            trailShader.Apply();

            var laserPoints = Projectile.GetLaserControlPoints(10, DeathrayLength);
            PrimitiveSettings settings = new(DeathrayWidthFunction, DeathrayColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true, Shader: trailShader);
            PrimitiveRenderer.RenderTrail(laserPoints, settings, 54);
        }
    }
}
