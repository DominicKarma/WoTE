using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL.Projectiles
{
    public class MagicCircle : ModProjectile
    {
        public ref float Time => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.timeLeft = 9999999;
        }

        public override void AI()
        {
            if (EmpressOfLight.Myself is null)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = EmpressOfLight.Myself.Center;
            Time++;

            if (Main.mouseRight && Main.mouseRightRelease)
                Time = 0f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float innerAngleOffset = Main.GlobalTimeWrappedHourly * 0.9f;
            float appearanceInterpolant = Utilities.InverseLerp(0f, 74f, Time);

            DrawMagicCircle(Color.Pink, Vector2.Zero, appearanceInterpolant, innerAngleOffset + MathHelper.PiOver2, 0.5f, 5, 3);

            for (int i = 0; i < 6; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * (i + 0.5f) / 6f - innerAngleOffset).ToRotationVector2() * ScaleByAppearanceInterpolant(appearanceInterpolant) * 218f;
                DrawMagicCircle(Color.LightCyan, drawOffset, appearanceInterpolant.Cubed(), -drawOffset.ToRotation(), 0.07f, 3, 6);
            }
            DrawMagicCircle(Color.CadetBlue, Vector2.Zero, appearanceInterpolant, innerAngleOffset * -3f, 0.25f, 3, 6);
            DrawMagicCircle(Color.LightCyan, Vector2.Zero, appearanceInterpolant, innerAngleOffset * 0.5f, 0.25f, 8);

            return false;
        }

        public float ScaleByAppearanceInterpolant(float appearanceInterpolant) => EasingCurves.Exp.Evaluate(EasingType.Out, appearanceInterpolant);

        public void DrawMagicCircle(Color circleColor, Vector2 drawOffset, float appearanceInterpolant, float angleOffset, float scale, params int[] polygonSides)
        {
            scale *= ScaleByAppearanceInterpolant(appearanceInterpolant);

            Texture2D circle = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = drawOffset + Projectile.Center - Main.screenPosition;
            Main.spriteBatch.Draw(circle, drawPosition, null, Projectile.GetAlpha(circleColor) with { A = 0 }, 0f, circle.Size() * 0.5f, circle.Width / 889f * scale, 0, 0f);

            Main.spriteBatch.PrepareForShaders();
            foreach (int polygonSideCount in polygonSides)
            {
                ManagedShader shader = ShaderManager.GetShader("WoTE.FadedPolygonShader");
                shader.TrySetParameter("polygonSides", polygonSideCount);
                shader.TrySetParameter("offsetAngle", angleOffset + MathHelper.Pi / polygonSideCount * (polygonSideCount % 2 == 1).ToInt());
                shader.TrySetParameter("appearanceInterpolant", appearanceInterpolant);
                shader.TrySetParameter("scale", scale);
                shader.Apply();

                Main.spriteBatch.Draw(circle, drawPosition, null, Projectile.GetAlpha(circleColor) with { A = 0 }, 0f, circle.Size() * 0.5f, scale, 0, 0f);
            }

            Main.spriteBatch.ResetToDefault();
        }
    }
}
