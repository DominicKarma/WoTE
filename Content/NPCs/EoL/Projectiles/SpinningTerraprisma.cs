using System.IO;
using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static WoTE.Content.NPCs.EoL.EmpressOfLight;

namespace WoTE.Content.NPCs.EoL
{
    public class SpinningTerraprisma : ModProjectile, IProjOwnedByBoss<EmpressOfLight>
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
        /// How long this sword has existed for, in frames.
        /// </summary>
        public ref float Time => ref Projectile.localAI[0];

        /// <summary>
        /// The hue interpolant of this sword.
        /// </summary>
        public ref float HueInterpolant => ref Projectile.ai[0];

        /// <summary>
        /// The spin angle for this sword.
        /// </summary>
        public ref float SpinAngle => ref Projectile.ai[1];

        /// <summary>
        /// The Z position of this sword.
        /// </summary>
        public ref float ZPosition => ref Projectile.ai[2];

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.EmpressBlade}";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
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
                Projectile.velocity = Projectile.rotation.ToRotationVector2() * 50f;
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
    }
}
