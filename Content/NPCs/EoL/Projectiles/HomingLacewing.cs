using System;
using Luminance.Common.DataStructures;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL.Projectiles
{
    public class HomingLacewing : ModProjectile, IProjOwnedByBoss<EmpressOfLight>
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

        public override void SetStaticDefaults() => Main.projFrames[Type] = 3;

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
                float homingSharpnessInterpolant = Utils.Remap(Time, 15f, 85f, 0.005f, 0.1f);
                Vector2 idealVelocity = Projectile.SafeDirectionTo(target.Center) * 21f;
                Projectile.velocity = Vector2.SmoothStep(Projectile.velocity, idealVelocity, homingSharpnessInterpolant);
            }
            else if (Projectile.velocity.Length() <= 30f)
                Projectile.velocity *= 1.022f;

            Projectile.spriteDirection = Projectile.velocity.X.NonZeroSign();
            Projectile.rotation = MathHelper.Clamp(-0.51f, 0.51f, Projectile.velocity.X * 0.0172f);
            Projectile.frame = (int)Time / 6 % Main.projFrames[Type];
            Projectile.Opacity = Utilities.InverseLerp(0f, 20f, Time) * Utilities.InverseLerp(0f, 45f, Projectile.timeLeft);
            Time++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            SpriteEffects direction = (-Projectile.spriteDirection).ToSpriteDirection();

            int totalFrames = Main.projFrames[Type];
            Rectangle frame = texture.Frame(1, totalFrames, 0, Projectile.frame);

            DrawRainbowBack(texture, frame, drawPosition, direction);
            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(Color.White), Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, direction);
            return false;
        }

        public void DrawRainbowBack(Texture2D texture, Rectangle frame, Vector2 drawPosition, SpriteEffects direction)
        {
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
                Color color = new Color(127 - Projectile.alpha, 127 - Projectile.alpha, 127 - Projectile.alpha, 0).MultiplyRGBA(Main.hslToRgb((Main.GlobalTimeWrappedHourly + i / 6f) % 1f, 1f, 0.5f));
                color = Projectile.GetAlpha(color);
                color.A = 0;
                Vector2 drawOffset = (MathHelper.TwoPi * i / 6f + Projectile.rotation).ToRotationVector2() * offset;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, frame, color, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, direction, 0f);
            }
        }
    }
}
