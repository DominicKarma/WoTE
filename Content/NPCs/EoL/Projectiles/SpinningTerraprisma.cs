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

namespace WoTE.Content.NPCs.EoL
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
                BloomTexture = LazyAsset<Texture2D>.Request("WoTE/Content/NPCs/EoL/Projectiles/SpinningTerraprismaBloom");
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 34;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.timeLeft = ConvergingTerraprismas_SpinTime + ConvergingTerraprismas_ReelBackTime + ConvergingTerraprismas_AttackTransitionDelay;
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

            if (Time >= ConvergingTerraprismas_SpinTime + ConvergingTerraprismas_ReelBackTime)
            {
                Vector2 particleVelocity = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 7f) + Main.rand.NextVector2Circular(2f, 2f);
                Color particleColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.67f) * 0.8f;
                BloomCircleParticle particle = new(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), particleVelocity, Main.rand.NextFloat(0.02f, 0.05f), Color.Wheat, particleColor, 40, 1.6f, 1.75f);
                particle.Spawn();

                DashVisualsIntensity = 1f;
                Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero) * 5f;
                return;
            }

            Vector2 spinCenter = Main.player[Myself.target].Center;
            SpinCenter = Vector2.Lerp(SpinCenter, spinCenter, Utilities.InverseLerp(ConvergingTerraprismas_ReelBackTime, 0f, Time - ConvergingTerraprismas_SpinTime));

            float spinSpeed = Utilities.InverseLerpBump(-ConvergingTerraprismas_SpinTime, 0f, 0f, 17f, Time - ConvergingTerraprismas_SpinTime).Squared() * MathHelper.TwoPi / 25f;
            spinSpeed += Utilities.InverseLerp(ConvergingTerraprismas_SpinTime * 0.5f, 0f, Time) * MathHelper.TwoPi / 45f;

            float squishInterpolant = Utilities.InverseLerp(0f, -ConvergingTerraprismas_SquishDissipateTime, Time - ConvergingTerraprismas_SpinTime) * 0.3f;
            float reelBackInterpolant = MathHelper.SmoothStep(0f, 1f, Utilities.InverseLerp(0f, ConvergingTerraprismas_ReelBackTime, Time - ConvergingTerraprismas_SpinTime)).Squared();
            float radius = 350f + reelBackInterpolant * 500f;
            squishInterpolant += (1f - reelBackInterpolant) * 0.25f;

            SpinAngle += spinSpeed;

            Vector2 orbitOffset = SpinAngle.ToRotationVector2() * new Vector2(1f, 1f - squishInterpolant) * radius;

            Projectile.Center = SpinCenter + orbitOffset;
            Projectile.rotation = Projectile.AngleTo(spinCenter);
            Projectile.scale = MathHelper.Lerp(0.5f, 1.5f, Utilities.Sin01(SpinAngle)) * Utilities.InverseLerp(0f, 8f, Time);
            Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, reelBackInterpolant);
            Projectile.Opacity = Utilities.InverseLerp(0f, 15f, Time);

            if (Time == ConvergingTerraprismas_SpinTime + ConvergingTerraprismas_ReelBackTime - 1f)
            {
                ModContent.GetInstance<DistortionMetaball>().CreateParticle(Projectile.Center + Main.rand.NextVector2Circular(50f, 50f), Vector2.Zero, 30f, 0.75f, 0.25f, 0.02f);

                Projectile.oldRot = new float[Projectile.oldRot.Length];
                Projectile.oldPos = new Vector2[Projectile.oldPos.Length];
                Projectile.velocity = Projectile.rotation.ToRotationVector2() * 50f;
                Projectile.netUpdate = true;
            }
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
