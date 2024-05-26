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
using WoTE.Core.Configuration;

namespace WoTE.Content.NPCs.EoL.Projectiles
{
    public class HomingLacewing : ModProjectile, IProjOwnedByBoss<EmpressOfLight>, IPixelatedPrimitiveRenderer
    {
        /// <summary>
        /// How long this lacewing has existed for, in frames.
        /// </summary>
        public ref float Time => ref Projectile.ai[0];

        /// <summary>
        /// How long this lacewing should exist for, in frames.
        /// </summary>
        public static int Lifetime => Utilities.SecondsToFrames(6f);

        public override string Texture => $"Terraria/Images/NPC_{NPCID.EmpressButterfly}";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 13;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.timeLeft = Lifetime;
            Projectile.scale = 1.5f;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];

            if (Time <= 60)
            {
                float swerveInterpolant = MathF.Cos(Projectile.identity / 7f % 1f + Projectile.Center.X / 320f + Projectile.Center.Y / 160f);
                Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.TwoPi * swerveInterpolant / 240f) * 0.98f;
            }

            if (Time <= 90)
            {
                float homingSharpnessInterpolant = Utils.Remap(Time, 15f, 60f, 0.005f, 0.135f);
                Vector2 idealVelocity = Projectile.SafeDirectionTo(target.Center) * 23.5f;
                Projectile.velocity = Vector2.SmoothStep(Projectile.velocity, idealVelocity, homingSharpnessInterpolant);
            }
            else if (Projectile.velocity.Length() <= 30f)
                Projectile.velocity *= 1.022f;

            Projectile.spriteDirection = Projectile.velocity.X.NonZeroSign();
            Projectile.rotation = MathHelper.Clamp(-0.51f, 0.51f, Projectile.velocity.X * 0.0172f);
            Projectile.frame = (int)Time / 6 % Main.projFrames[Type];
            Projectile.Opacity = Utilities.InverseLerp(0f, 20f, Time) * Utilities.InverseLerp(0f, 45f, Projectile.timeLeft);

            if (Projectile.timeLeft <= 32)
                Projectile.damage = 0;

            Time++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            SpriteEffects direction = (-Projectile.spriteDirection).ToSpriteDirection();

            int totalFrames = Main.projFrames[Type];
            Rectangle frame = texture.Frame(1, totalFrames, 0, Projectile.frame);

            Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
            Main.spriteBatch.Draw(bloom, drawPosition, null, Projectile.GetAlpha(Color.White) with { A = 0 }, 0f, bloom.Size() * 0.5f, 0.32f, 0, 0f);

            DrawRainbowBack(texture, frame, drawPosition, direction);
            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(Color.White), Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, direction);
            return false;
        }

        public void DrawRainbowBack(Texture2D texture, Rectangle frame, Vector2 drawPosition, SpriteEffects direction)
        {
            if (EmpressOfLight.Myself is null)
                return;

            float offset = MathF.Sin(Main.GlobalTimeWrappedHourly * 2.4f + Projectile.identity * 3f) * 12f;
            if (offset < 4f)
                offset = 4f;

            for (int i = 0; i < 6; i++)
            {
                Color color = Projectile.GetAlpha(new(127 - Projectile.alpha, 127 - Projectile.alpha, 127 - Projectile.alpha, 0)) * 0.5f;
                Vector2 drawOffset = (MathHelper.TwoPi * i / 6f + Projectile.rotation).ToRotationVector2() * 2f;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, frame, color, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, direction, 0f);
            }

            for (int i = 0; i < 6; i++)
            {
                Color backglowColor = EmpressOfLight.Myself.As<EmpressOfLight>().Palette.MulticolorLerp(EmpressPaletteType.LacewingTrail, Main.GlobalTimeWrappedHourly + i / 6f);
                Color color = new Color(127 - Projectile.alpha, 127 - Projectile.alpha, 127 - Projectile.alpha, 0).MultiplyRGBA(backglowColor);
                color = Projectile.GetAlpha(color);
                color.A = 0;
                Vector2 drawOffset = (MathHelper.TwoPi * i / 6f + Projectile.rotation).ToRotationVector2() * offset;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, frame, color, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, direction, 0f);
            }
        }

        public float TrailWidthFunction(float completionRatio)
        {
            float baseWidth = 30f;
            float tipCutFactor = Utilities.InverseLerp(0.03f, 0.05f, completionRatio);
            float slownessFactor = Utils.Remap(Projectile.velocity.Length(), 3f, 19f, 0.4f, 1f);
            return baseWidth * tipCutFactor * slownessFactor * (1f - completionRatio);
        }

        public Color TrailColorFunction(float completionRatio)
        {
            return Color.White * Projectile.Opacity * (WoTEConfig.Instance.PhotosensitivityMode ? 0.5f : 1f);
        }

        public float CalculateSinusoidalOffset(float completionRatio)
        {
            return MathF.Sin(MathHelper.TwoPi * completionRatio + Main.GlobalTimeWrappedHourly * -9f + Projectile.identity) * Utilities.InverseLerp(0.01f, 0.9f, completionRatio);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            if (EmpressOfLight.Myself is null)
                return;

            Vector4[] lacewingTrailPalette = EmpressOfLight.Myself.As<EmpressOfLight>().Palette.Get(EmpressPaletteType.LacewingTrail);
            ManagedShader trailShader = ShaderManager.GetShader("WoTE.LacewingTrailShader");
            trailShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly * 1.56f + Projectile.whoAmI * 0.4f);
            trailShader.TrySetParameter("gradient", lacewingTrailPalette);
            trailShader.TrySetParameter("gradientCount", lacewingTrailPalette.Length);
            trailShader.SetTexture(MiscTexturesRegistry.TurbulentNoise.Value, 1, SamplerState.LinearWrap);
            trailShader.SetTexture(TextureAssets.Extra[ExtrasID.MagicMissileTrailShape], 2, SamplerState.LinearWrap);
            trailShader.Apply();

            float perpendicularOffset = Utils.Remap(Projectile.velocity.Length(), 4f, 20f, 12f, 40f) * Utilities.InverseLerp(60f, 15f, Projectile.velocity.Length());
            Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * perpendicularOffset;
            Vector2[] trailPositions = new Vector2[Projectile.oldPos.Length];
            for (int i = 0; i < trailPositions.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float sine = CalculateSinusoidalOffset(i / (float)trailPositions.Length);
                trailPositions[i] = Projectile.oldPos[i] + perpendicular * sine;
            }

            PrimitiveSettings settings = new(TrailWidthFunction, TrailColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true, Shader: trailShader);
            PrimitiveRenderer.RenderTrail(trailPositions, settings, 31);
        }
    }
}
