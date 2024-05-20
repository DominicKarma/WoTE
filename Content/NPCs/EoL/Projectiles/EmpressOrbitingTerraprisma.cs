using System.IO;
using Luminance.Common.DataStructures;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static WoTE.Content.NPCs.EoL.EmpressOfLight;

namespace WoTE.Content.NPCs.EoL.Projectiles
{
    public class EmpressOrbitingTerraprisma : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<EmpressOfLight>
    {
        /// <summary>
        /// The direction angle that this sword should follow when dashing.
        /// </summary>
        public float DashDirection
        {
            get;
            set;
        }

        /// <summary>
        /// The hue interpolant of this sword.
        /// </summary>
        public ref float HueInterpolant => ref Projectile.ai[0];

        /// <summary>
        /// The spin angle for this sword.
        /// </summary>
        public ref float SpinAngle => ref Projectile.ai[1];

        /// <summary>
        /// How long, in frames, this sword should delay its dash.
        /// </summary>
        public ref float DashDelay => ref Projectile.ai[2];

        /// <summary>
        /// The Z position of this sword.
        /// </summary>
        public ref float ZPosition => ref Projectile.localAI[0];

        /// <summary>
        /// The intensity of dash visuals.
        /// </summary>
        public ref float DashVisualsIntensity => ref Projectile.localAI[1];

        /// <summary>
        /// How long this sword has existed for, in frames.
        /// </summary>
        public ref float Time => ref Projectile.localAI[2];

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.EmpressBlade}";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 9;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2400;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.timeLeft = 99999;
            Projectile.MaxUpdates = 3;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ZPosition);
            writer.Write(Time);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ZPosition = reader.ReadSingle();
            Time = reader.ReadSingle();
        }

        public override void AI()
        {
            if (Myself is null)
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.IsFinalExtraUpdate())
                Time++;

            Projectile.Opacity = Utilities.InverseLerp(0f, 20f, Time);

            Player target = Main.player[Myself.target];
            if (Time <= OrbitReleasedTerraprismas_TerraprismaSpinTime + DashDelay)
                SpinAroundOwner();
            else
                HandlePostDashBehaviors();

            // Choose the dash direction when ready.
            if (Time == OrbitReleasedTerraprismas_TerraprismaSpinTime)
            {
                DashDirection = Myself.AngleTo(target.Center);
                Projectile.netUpdate = true;
            }

            if (Time == OrbitReleasedTerraprismas_TerraprismaSpinTime + DashDelay + 1f)
                PerformDash(target);

            // Use a scaling illusion to give the impression of the swords existing in 3D space.
            Projectile.scale = Utilities.InverseLerp(0f, 8f, Time) / (ZPosition + 1f);
        }

        public override bool? CanDamage() => Time >= OrbitReleasedTerraprismas_TerraprismaSpinTime;

        /// <summary>
        /// Makes this sword spin around the Empress.
        /// </summary>
        public void SpinAroundOwner()
        {
            if (Myself is null)
                return;

            // Make the spin speed go from slow to super fast over the duration of the spin animation.
            float spinCompletion = Time / OrbitReleasedTerraprismas_TerraprismaSpinTime;
            float spinSpeed = Utilities.InverseLerp(0f, Main.dayTime ? 30f : 60f, Time).Squared() * Utilities.InverseLerp(0.85f, 0.7f, spinCompletion) * MathHelper.TwoPi / 56f;

            // This ensures that the spin starts out at a moderate speed, rather than at no speed at all. This makes the attack look a bit more interesting at the start.
            spinSpeed += Utilities.InverseLerp(OrbitReleasedTerraprismas_TerraprismaSpinTime * 0.5f, 0f, Time) * MathHelper.TwoPi / 45f;

            SpinAngle += spinSpeed / Projectile.MaxUpdates;

            float radius = 245f;
            Vector2 orbitOffset = SpinAngle.ToRotationVector2() * new Vector2(1f, 0.45f) * radius;
            Projectile.Center = Myself.Center + orbitOffset;
            Projectile.rotation = Projectile.AngleFrom(Myself.Center);

            ZPosition = MathHelper.Lerp(1.2f, -0.25f, Utilities.Sin01(SpinAngle));
        }

        /// <summary>
        /// Handles post-dash behaviors for this sword, making it redirect and then accelerate.
        /// </summary>
        public void HandlePostDashBehaviors()
        {
            // Redirect after the dash.
            if (Time <= OrbitReleasedTerraprismas_TerraprismaSpinTime + DashDelay + 7f)
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, DashDirection.ToRotationVector2() / Projectile.MaxUpdates * 26f, 0.26f);

            // Accelerate after redirecting.
            else
                Projectile.velocity *= 1.024f;

            ZPosition = MathHelper.Lerp(ZPosition, 0f, 0.08f);
            DashVisualsIntensity = MathHelper.Lerp(DashVisualsIntensity, 1f, 0.1f);

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        /// <summary>
        /// Performs this sword's initial dash.
        /// </summary>
        /// <param name="target">The player to dash towards</param>
        public void PerformDash(Player target)
        {
            Vector2 fireDirection = SpinAngle.ToRotationVector2();
            float targetAngleDeviation = fireDirection.AngleBetween(Projectile.SafeDirectionTo(target.Center));
            Projectile.velocity = fireDirection * Utils.Remap(targetAngleDeviation, 0.3f, 0.95f, 160f, 180f) / Projectile.MaxUpdates;
            Projectile.netUpdate = true;

            ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, 3f);
        }

        public override Color? GetAlpha(Color lightColor) => lightColor * Projectile.Opacity;

        public override bool PreDraw(ref Color lightColor)
        {
            if (Myself is null)
                return false;

            Texture2D bloom = SpinningTerraprisma.BloomTexture.Value;
            Texture2D sword = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color swordColor = Myself.As<EmpressOfLight>().Palette.MulticolorLerp(EmpressPaletteType.RainbowArrow, HueInterpolant) with { A = 0 };
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
            Color baseColor = Color.White;
            if (Myself is not null)
                baseColor = Myself.As<EmpressOfLight>().Palette.MulticolorLerp(EmpressPaletteType.RainbowArrow, HueInterpolant);

            return Projectile.GetAlpha(baseColor) * Utilities.InverseLerp(0f, 0.1f, completionRatio);
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
