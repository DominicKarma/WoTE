using System;
using System.Linq;
using Luminance.Assets;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL.Projectiles
{
    public class MagicCircle : ModProjectile
    {
        internal static ManagedRenderTarget UnrotatedCircleTarget;

        public ref float Time => ref Projectile.ai[0];

        public ref float AppearanceInterpolant => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            UnrotatedCircleTarget = new(false, (_, _2) => new(Main.instance.GraphicsDevice, 1024, 1024));
            RenderTargetManager.RenderTargetUpdateLoopEvent += CreateCircleRenderTarget;
        }

        private void CreateCircleRenderTarget()
        {
            var magicCircles = Utilities.AllProjectilesByID(Type);
            if (!magicCircles.Any())
                return;

            Projectile circleToDraw = magicCircles.First();
            GraphicsDevice gd = Main.instance.GraphicsDevice;
            gd.SetRenderTarget(UnrotatedCircleTarget);
            gd.Clear(Color.Transparent);
            circleToDraw.As<MagicCircle>().DrawToTarget();

            gd.SetRenderTarget(null);
        }

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

            Projectile.Center = EmpressOfLight.Myself.Center - Vector2.UnitY * 150f;
            Time++;

            AppearanceInterpolant = Utilities.InverseLerp(0f, 125f, Time);
            Projectile.rotation += MathHelper.TwoPi * Utilities.InverseLerp(0.35f, 0.95f, AppearanceInterpolant).Squared() / 120f;
            Projectile.scale = EasingCurves.Elastic.Evaluate(EasingType.Out, Utilities.InverseLerp(0f, 60f, Time).Squared()) * 0.5f;

            if (Main.mouseRight && Main.mouseRightRelease)
                Time = 0f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Quaternion rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationX(1.05f));
            Vector2 ringDrawOffset = Vector2.Transform(UnrotatedCircleTarget.Size() * new Vector2(-0.5f, 0.5f), rotation);

            DrawRing(Vector2.Zero, rotation, Color.SkyBlue with { A = 0 });
            DrawFromTarget(ringDrawOffset, rotation, Color.White);

            return false;
        }

        public void DrawFromTarget(Vector2 drawOffset, Quaternion rotation, Color circleColor)
        {
            Texture2D drawnCircle = UnrotatedCircleTarget;

            float[] blurWeights = new float[11];
            for (int i = 0; i < blurWeights.Length; i++)
                blurWeights[i] = Utilities.GaussianDistribution(i - (int)(blurWeights.Length * 0.5f), 2f) / 12f;
            ManagedShader underglowShader = ShaderManager.GetShader("WoTE.BlurUnderglowShader");
            underglowShader.TrySetParameter("blurOffset", Projectile.scale * 0.006f);
            underglowShader.TrySetParameter("blurWeights", blurWeights);

            PrimitiveRenderer.RenderQuad(drawnCircle, Projectile.Center + drawOffset, Vector2.One, 0f, circleColor, underglowShader, rotation);
            PrimitiveRenderer.RenderQuad(drawnCircle, Projectile.Center + drawOffset, Vector2.One, 0f, circleColor, null, rotation);
        }

        public void DrawRing(Vector2 drawOffset, Quaternion rotation, Color ringColor)
        {
            int precision = 240;
            float appearanceScaleFactor = 1f;
            VertexPosition2DColorTexture[] vertices = new VertexPosition2DColorTexture[precision * 2];
            Vector2 top = Vector2.UnitY * -4f;
            Vector2 bottom = top + Vector2.UnitY * appearanceScaleFactor.Squared() * 192f;
            Vector2 maxSize = new(524f, 508f);

            for (int i = 0; i < precision; i++)
            {
                float angle = MathHelper.TwoPi * i / precision * 2f;
                float x = i / (float)precision * 2f;

                Vector2 topTextureCoordinate = new(x, 0f);
                Vector2 bottomTextureCoordinate = new(x, 1f);
                Vector2 circularOffset = angle.ToRotationVector2() * maxSize * appearanceScaleFactor;

                vertices[i * 2] = new(top + circularOffset, ringColor, topTextureCoordinate, MathF.Cos(angle));
                vertices[i * 2 + 1] = new(bottom + circularOffset, ringColor, bottomTextureCoordinate, MathF.Cos(angle));
            }

            short indicesIndex = 0;
            short[] indices = new short[(precision - 2) * 6];
            for (short i = 0; i < precision - 2; i++)
            {
                short connectToIndex = (short)(i * 2);
                indices[indicesIndex++] = connectToIndex;
                indices[indicesIndex++] = (short)(connectToIndex + 1);
                indices[indicesIndex++] = (short)(connectToIndex + 2);
                indices[indicesIndex++] = (short)(connectToIndex + 2);
                indices[indicesIndex++] = (short)(connectToIndex + 1);
                indices[indicesIndex++] = (short)(connectToIndex + 3);
            }

            Matrix scale = Matrix.CreateScale(Projectile.scale, Projectile.scale, 1f);
            Matrix view = Matrix.CreateTranslation(new Vector3(Projectile.Center.X + drawOffset.X - Main.screenPosition.X, Projectile.Center.Y + drawOffset.Y - Main.screenPosition.Y, 0f));
            Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height, 0f, -1000f, 1000f);
            Texture2D ring = ModContent.Request<Texture2D>("WoTE/Content/NPCs/EoL/Projectiles/MagicCircleRingStrip").Value;

            ManagedShader ringShader = ShaderManager.GetShader("WoTE.MagicCircleRingShader");
            ringShader.SetTexture(ring, 1, SamplerState.LinearWrap);
            ringShader.TrySetParameter("spinScrollOffset", Time / -240f);
            ringShader.TrySetParameter("uWorldViewProjection", Matrix.CreateFromQuaternion(rotation) * scale * view * Main.GameViewMatrix.TransformationMatrix * projection);
            ringShader.Apply();

            var gd = Main.instance.GraphicsDevice;
            gd.RasterizerState = RasterizerState.CullNone;
            gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, vertices.Length / 2);
            gd.SetVertexBuffer(null);
            gd.Indices = null;
        }

        public void DrawToTarget()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

            float innerAngleOffset = Projectile.rotation;
            float appearInterpolantA = Utilities.InverseLerp(0f, 0.45f, AppearanceInterpolant).Squared();
            float appearInterpolantB = Utilities.InverseLerp(0.45f, 0.8f, AppearanceInterpolant);
            float appearInterpolantC = Utilities.InverseLerp(0.6f, 1f, AppearanceInterpolant);

            DrawMagicCircle(Color.SkyBlue, Vector2.Zero, appearInterpolantA, innerAngleOffset + MathHelper.PiOver2, Projectile.scale, 5, 3);

            for (int i = 0; i < 5; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * (i + 0.5f) / 5f - innerAngleOffset - MathHelper.Pi / 3f).ToRotationVector2() * Projectile.scale * 436f;
                DrawMagicCircle(Color.White, drawOffset, appearInterpolantC, -drawOffset.ToRotation(), Projectile.scale * 0.14f, 3, 6);
            }

            DrawMagicCircle(Color.Aqua, Vector2.Zero, appearInterpolantB, -MathHelper.PiOver2, Projectile.scale * 0.5f, 3);
            DrawMagicCircle(Color.HotPink, Vector2.Zero, appearInterpolantB, innerAngleOffset * -2f, Projectile.scale * 0.5f, 6);
            DrawMagicCircle(Color.LightGoldenrodYellow, Vector2.Zero, appearInterpolantB, innerAngleOffset * 0.5f, Projectile.scale * 0.5f, 5);
            DrawLacewingAtCenterOfRing(appearInterpolantC.Squared());

            Main.spriteBatch.End();
        }

        public void DrawLacewingAtCenterOfRing(float appearInterpolant)
        {
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            Main.instance.LoadNPC(NPCID.EmpressButterfly);
            Texture2D lacewing = TextureAssets.Npc[NPCID.EmpressButterfly].Value;
            Rectangle lacewingFrame = lacewing.Frame(1, 3, 0, (int)Time / 7 % 3);
            Main.spriteBatch.Draw(lacewing, UnrotatedCircleTarget.Size() * 0.5f, lacewingFrame, new Color(1f, 1f, 1f, 0f) * appearInterpolant, 0f, lacewingFrame.Size() * 0.5f, 1.5f, 0, 0f);

            Texture2D glow = MiscTexturesRegistry.BloomCircleSmall.Value;
            Main.spriteBatch.Draw(glow, UnrotatedCircleTarget.Size() * 0.5f, null, new Color(0.6f, 1f, 1f, 0f) * appearInterpolant * 0.3f, 0f, glow.Size() * 0.5f, 1f, 0, 0f);
        }

        public void DrawMagicCircle(Color circleColor, Vector2 drawOffset, float appearanceInterpolant, float angleOffset, float scale, params int[] polygonSides)
        {
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            Texture2D circle = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = drawOffset + UnrotatedCircleTarget.Size() * 0.5f;
            Main.spriteBatch.Draw(circle, drawPosition, null, Projectile.GetAlpha(circleColor) with { A = 0 }, 0f, circle.Size() * 0.5f, circle.Width / 889f * scale, 0, 0f);

            foreach (int polygonSideCount in polygonSides)
            {
                ManagedShader shader = ShaderManager.GetShader("WoTE.FadedPolygonShader");
                shader.TrySetParameter("polygonSides", polygonSideCount);
                shader.TrySetParameter("offsetAngle", angleOffset + MathHelper.Pi / polygonSideCount * (polygonSideCount % 2 == 1).ToInt());
                shader.TrySetParameter("appearanceInterpolant", appearanceInterpolant);
                shader.TrySetParameter("scale", scale);
                shader.TrySetParameter("sectionStartOffsetAngle", -MathHelper.Pi / polygonSideCount + MathHelper.PiOver2);
                shader.Apply();

                Main.spriteBatch.Draw(circle, drawPosition, null, Projectile.GetAlpha(circleColor) with { A = 0 }, 0f, circle.Size() * 0.5f, scale, 0, 0f);
            }
        }
    }
}
