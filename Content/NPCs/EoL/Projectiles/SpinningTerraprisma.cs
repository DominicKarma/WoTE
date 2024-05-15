using System.IO;
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
using WoTE.Content.Particles.Metaballs;
using static WoTE.Content.NPCs.EoL.EmpressOfLight;

namespace WoTE.Content.NPCs.EoL.Projectiles
{
    public class SpinningTerraprisma : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<EmpressOfLight>
    {
        /// <summary>
        /// The bloom texture for this sword.
        /// </summary>
        internal static LazyAsset<Texture2D> BloomTexture;

        /// <summary>
        /// The spin center position for this sword.
        /// </summary>
        public Vector2 SpinCenter;

        /// <summary>
        /// The intensity of dash visuals.
        /// </summary>
        public ref float DashVisualsIntensity => ref Projectile.localAI[0];

        /// <summary>
        /// The hue interpolant of this sword.
        /// </summary>
        public ref float HueInterpolant => ref Projectile.ai[0];

        /// <summary>
        /// The spin angle for this sword.
        /// </summary>
        public ref float SpinAngle => ref Projectile.ai[1];

        /// <summary>
        /// How long this sword has existed for, in frames.
        /// </summary>
        public ref float Time => ref Projectile.ai[2];

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.EmpressBlade}";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 9;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2400;

            if (Main.netMode != NetmodeID.Server)
                BloomTexture = LazyAsset<Texture2D>.Request("WoTE/Content/NPCs/EoL/Projectiles/TerraprismaBloom");
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 34;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.timeLeft = 1;
            if (Myself is not null)
            {
                var empress = Myself.As<EmpressOfLight>();
                Projectile.timeLeft = empress.ConvergingTerraprismas_SpinTime + empress.ConvergingTerraprismas_ReelBackTime + ConvergingTerraprismas_AttackTransitionDelay;
            }

            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.WriteVector2(SpinCenter);

        public override void ReceiveExtraAI(BinaryReader reader) => SpinCenter = reader.ReadVector2();

        public override void AI()
        {
            if (Myself is null)
            {
                Projectile.Kill();
                return;
            }

            Time++;
            Projectile.Opacity = Utilities.InverseLerp(0f, ConvergingTerraprismas_TerraprismaFadeInTime, Time);

            var empress = Myself.As<EmpressOfLight>();
            if (Time >= empress.ConvergingTerraprismas_SpinTime + empress.ConvergingTerraprismas_ReelBackTime)
            {
                HandlePostDashEffects();
                return;
            }

            SpinAroundTarget();
            if (Time == empress.ConvergingTerraprismas_SpinTime + empress.ConvergingTerraprismas_ReelBackTime - 1f)
                PerformDash();
        }

        /// <summary>
        /// Makes this sword spin around a designated target.
        /// </summary>
        public void SpinAroundTarget()
        {
            if (Myself is null)
                return;

            var empress = Myself.As<EmpressOfLight>();

            // Lock the spin center on the player at first.
            // This grip is loosened as the terraprismas reel back.
            Vector2 spinCenter = Main.player[Myself.target].Center;
            SpinCenter = Vector2.Lerp(SpinCenter, spinCenter, Utilities.InverseLerp(empress.ConvergingTerraprismas_ReelBackTime, 0f, Time - empress.ConvergingTerraprismas_SpinTime));

            // Make the spin speed go from slow to super fast over the duration of the spin animation.
            float spinSpeed = Utilities.InverseLerpBump(-empress.ConvergingTerraprismas_SpinTime, 0f, 0f, 17f, Time - empress.ConvergingTerraprismas_SpinTime).Squared() * MathHelper.TwoPi / 25f;

            // This ensures that the spin starts out at a moderate speed, rather than at no speed at all. This makes the attack look a bit more interesting at the start.
            spinSpeed += Utilities.InverseLerp(empress.ConvergingTerraprismas_SpinTime * 0.5f, 0f, Time) * MathHelper.TwoPi / 45f;

            SpinAngle += spinSpeed;

            float orbitSquishInterpolant = Utilities.InverseLerp(0f, -ConvergingTerraprismas_OrbitSquishDissipateTime, Time - empress.ConvergingTerraprismas_SpinTime) * 0.3f;
            float reelBackInterpolant = MathHelper.SmoothStep(0f, 1f, Utilities.InverseLerp(0f, empress.ConvergingTerraprismas_ReelBackTime, Time - empress.ConvergingTerraprismas_SpinTime)).Squared();
            float radius = MathHelper.Lerp(ConvergingTerraprismas_InitialRadius, ConvergingTerraprismas_ReelBackRadius, reelBackInterpolant);
            orbitSquishInterpolant += (1f - reelBackInterpolant) * 0.25f;

            Vector2 orbitOffset = SpinAngle.ToRotationVector2() * new Vector2(1f, 1f - orbitSquishInterpolant) * radius;
            Projectile.Center = SpinCenter + orbitOffset;
            Projectile.rotation = Projectile.AngleTo(spinCenter);

            // Use a scaling illusion to give the impression of the swords existing in 3D space.
            // This gets undone as the swords reel back, since it'd be really silly for them to be tiny or big after they've dashed in 2D space.
            Projectile.scale = MathHelper.Lerp(0.5f, 1.5f, Utilities.Sin01(SpinAngle)) * Utilities.InverseLerp(0f, 8f, Time);
            Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, reelBackInterpolant);
        }

        /// <summary>
        /// Handles post-dash effects for this sword, such as enabling the trail and emitting particles.
        /// </summary>
        public void HandlePostDashEffects()
        {
            Color particleColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.7f) * 0.8f;
            Vector2 particleVelocity = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 7f) + Main.rand.NextVector2Circular(2f, 2f);
            Vector2 particleSpawnPosition = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
            BloomCircleParticle particle = new(particleSpawnPosition, particleVelocity, Main.rand.NextFloat(0.02f, 0.05f), Color.Wheat, particleColor, 40, 1.6f, 1.75f);
            particle.Spawn();

            DashVisualsIntensity = 1f;
            Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero) * 5f;
        }

        /// <summary>
        /// Makes this Terraprisma perform its dash. This also prepares it for post-dash effects later, such as by resetting the <see cref="Projectile.oldPos"/> cache.
        /// </summary>
        public void PerformDash()
        {
            ModContent.GetInstance<DistortionMetaball>().CreateParticle(Projectile.Center + Main.rand.NextVector2Circular(50f, 50f), Vector2.Zero, 30f, 0.75f, 0.25f, 0.02f);

            Projectile.oldRot = new float[Projectile.oldRot.Length];
            Projectile.oldPos = new Vector2[Projectile.oldPos.Length];
            Projectile.velocity = Projectile.rotation.ToRotationVector2() * 50f;
            Projectile.netUpdate = true;
        }

        public override Color? GetAlpha(Color lightColor) => lightColor * Projectile.Opacity;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D bloom = BloomTexture.Value;
            Texture2D sword = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color swordColor = Main.hslToRgb(HueInterpolant, 1f, 0.5f, 0);
            Vector2 scale = Vector2.One * Projectile.scale;

            Main.EntitySpriteDraw(bloom, drawPosition, null, Projectile.GetAlpha(swordColor) * Projectile.Opacity.Squared(), Projectile.rotation, bloom.Size() * 0.5f, scale, 0);
            Main.EntitySpriteDraw(bloom, drawPosition, null, Projectile.GetAlpha(swordColor) * Projectile.Opacity.Squared() * 0.5f, Projectile.rotation, bloom.Size() * 0.5f, scale * 1.3f, 0);

            for (int i = 0; i < 4; i++)
            {
                Color backglowColor = Color.Lerp(swordColor, Color.White with { A = 0 }, 0.5f);
                Vector2 drawOffset = (MathHelper.TwoPi * i / 4f).ToRotationVector2() * Projectile.scale * 3f;
                Main.EntitySpriteDraw(sword, drawPosition + drawOffset, null, Projectile.GetAlpha(backglowColor) * Projectile.Opacity.Cubed(), Projectile.rotation, sword.Size() * 0.5f, scale, 0);
            }

            Main.EntitySpriteDraw(sword, drawPosition, null, Projectile.GetAlpha(Color.White with { A = 128 }) * Projectile.Opacity.Squared(), Projectile.rotation, sword.Size() * 0.5f, scale, 0);

            return false;
        }

        public float TrailWidthFunction(float completionRatio)
        {
            float baseWidth = Projectile.width * 0.4f;
            return baseWidth;
        }

        public Color TrailColorFunction(float completionRatio)
        {
            return Projectile.GetAlpha(Main.hslToRgb(HueInterpolant, 1f, 0.5f)) * Utilities.InverseLerp(0f, 0.1f, completionRatio);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            if (DashVisualsIntensity <= 0f)
                return;

            ManagedShader trailShader = ShaderManager.GetShader("WoTE.TerraprismaDashTrailShader");
            trailShader.SetTexture(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion], 1, SamplerState.LinearWrap);
            trailShader.TrySetParameter("localTime", -Main.GlobalTimeWrappedHourly * 1.45f + Projectile.identity * 0.374f);
            trailShader.TrySetParameter("hueOffset", HueInterpolant);
            trailShader.Apply();

            PrimitiveSettings settings = new(TrailWidthFunction, TrailColorFunction, _ => Projectile.Size * 0.5f - Projectile.velocity.SafeNormalize(Vector2.Zero) * 20f, Pixelate: true, Shader: trailShader);
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 30);
        }
    }
}
