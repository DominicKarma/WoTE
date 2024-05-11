using System;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL.Projectiles;

namespace WoTE.Content.NPCs.EoL
{
    public class DazzlingTornado : ModProjectile, IPixelatedPrimitiveRenderer
    {
        /// <summary>
        /// The vertex buffer used for this tornado.
        /// </summary>
        public static VertexBuffer Vertices
        {
            get;
            private set;
        }

        /// <summary>
        /// The index buffer used for this tornado.
        /// </summary>
        public static IndexBuffer Indices
        {
            get;
            private set;
        }

        public ref float LocalTime => ref Projectile.localAI[0];

        /// <summary>
        /// The lifetime ratio of this tornado as a 0-1 interpolant.
        /// </summary>
        public float LifetimeRatio => 1f - Projectile.timeLeft / (float)Lifetime;

        /// <summary>
        /// How long this tornado should exist for, in frames.
        /// </summary>
        public static int Lifetime => Utilities.SecondsToFrames(3.4f);

        public static readonly Vector2 Size = new(240f, 575f);

        public override string Texture => MiscTexturesRegistry.PixelPath;

        public override void SetStaticDefaults()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            Main.QueueMainThreadAction(GenerateVerticesAndIndices);
        }

        public override void SetDefaults()
        {
            Projectile.width = (int)Size.X;
            Projectile.height = (int)Size.Y;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.timeLeft = Lifetime;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.97f;

            Projectile.Opacity = Utilities.InverseLerp(0f, 48f, Projectile.timeLeft);
            LocalTime += Projectile.velocity.X * 0.0023f;
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            Matrix view = Matrix.CreateTranslation(new Vector3(Projectile.Top.X - Main.screenPosition.X, Projectile.Top.Y - Main.screenPosition.Y, 0f));
            Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -300f, 300f);

            ManagedShader tornadoShader = ShaderManager.GetShader("WoTE.DazzlingTornadoShader");
            tornadoShader.TrySetParameter("uWorldViewProjection", view * projection);
            tornadoShader.TrySetParameter("localTime", LocalTime);
            tornadoShader.TrySetParameter("opacity", Projectile.Opacity);
            tornadoShader.TrySetParameter("horizontalStack", 2.5f);
            tornadoShader.TrySetParameter("swirlDirection", 1f);
            tornadoShader.SetTexture(MiscTexturesRegistry.DendriticNoiseZoomedOut.Value, 1, SamplerState.PointWrap);
            tornadoShader.SetTexture(MiscTexturesRegistry.TurbulentNoise.Value, 2, SamplerState.PointWrap);
            tornadoShader.SetTexture(TextureAssets.Projectile[ModContent.ProjectileType<StarBolt>()], 3, SamplerState.LinearWrap);
            tornadoShader.SetTexture(TextureAssets.Extra[ExtrasID.QueenSlimeGradient], 4, SamplerState.LinearWrap);
            tornadoShader.Apply();

            var gd = Main.instance.GraphicsDevice;
            gd.RasterizerState = RasterizerState.CullNone;
            gd.SetVertexBuffer(Vertices);
            gd.Indices = Indices;

            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Vertices.VertexCount, 0, Vertices.VertexCount / 2);
            gd.SetVertexBuffer(null);
            gd.Indices = null;
        }

        public static void GenerateVerticesAndIndices()
        {
            int precision = 300;
            VertexPosition2DColorTexture[] vertices = new VertexPosition2DColorTexture[precision * 2];
            Vector2 top = -Vector2.UnitY * 180f;
            Vector2 bottom = Vector2.UnitY * (Size.Y + 180f);

            for (int i = 0; i < precision; i++)
            {
                float angle = MathHelper.TwoPi * i / precision * 2f;
                float x = i / (float)precision * 2f;

                Vector2 topTextureCoordinate = new(x, 0f);
                Vector2 bottomTextureCoordinate = new(x, 1f);
                Vector2 topOffset = Vector2.UnitX * MathF.Cos(angle) * Size.X * 0.72f;
                Vector2 bottomOffset = topOffset * 0.3f;

                vertices[i * 2] = new(top + topOffset, Color.White, topTextureCoordinate, MathF.Cos(angle));
                vertices[i * 2 + 1] = new(bottom + bottomOffset, Color.White, bottomTextureCoordinate, MathF.Cos(angle));
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

            Vertices = new(Main.instance.GraphicsDevice, typeof(VertexPosition2DColorTexture), vertices.Length, BufferUsage.None);
            Indices = new(Main.instance.GraphicsDevice, typeof(short), indices.Length, BufferUsage.None);
            Vertices.SetData(vertices);
            Indices.SetData(indices);
        }
    }
}
