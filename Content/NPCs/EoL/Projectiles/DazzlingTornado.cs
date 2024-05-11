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
using WoTE.Content.Particles;

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

        /// <summary>
        /// Whether this tornado should decelerate or not.
        /// </summary>
        public bool ShouldDecelerate
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value.ToInt();
        }

        /// <summary>
        /// How long this tornado has existed, in frames.
        /// </summary>
        public ref float Time => ref Projectile.ai[1];

        /// <summary>
        /// The visuals timer used in the tornado shader that increases based on the speed of the tornado.
        /// </summary>
        public ref float SpeedTime => ref Projectile.localAI[0];

        /// <summary>
        /// The visuals timer used in the tornado shader that increases based on the opacity and scale of the tornado.
        /// </summary>
        public ref float VisualsTime => ref Projectile.localAI[1];

        /// <summary>
        /// How long this tornado should exist for, in frames.
        /// </summary>
        public static int Lifetime => Utilities.SecondsToFrames(3.4f);

        /// <summary>
        /// The standard hitbox size of this tornado.
        /// </summary>
        public static Vector2 Size => new(115f, 214f);

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
            Projectile.Opacity = Utilities.InverseLerp(0f, 9f, Time) * Utilities.InverseLerp(0f, 90f, Projectile.timeLeft);
            Projectile.scale = Utilities.InverseLerp(0f, 6f, Time);

            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            Projectile.ai[2] = Projectile.ai[2].AngleLerp(MathHelper.WrapAngle(Projectile.AngleTo(target.Center) - Projectile.velocity.ToRotation()) * 0.05f, 0.02f);
            Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[2]);

            for (int i = 0; i < Projectile.scale.Squared(); i++)
            {
                Color pixelColor = Color.Lerp(Color.DeepSkyBlue, Color.HotPink, Main.rand.NextFloat()) * Projectile.Opacity;
                Vector2 pixelSpawnCore = Vector2.Lerp(Projectile.Top, Projectile.Bottom, Main.rand.NextFloat(0.75f));
                Vector2 pixelSpawnPosition = pixelSpawnCore + Vector2.UnitX * Main.rand.NextFloatDirection() * Projectile.width * Projectile.scale * 0.4f;
                Vector2 pixelVelocity = pixelSpawnPosition.SafeDirectionTo(pixelSpawnCore) * Projectile.Opacity * Projectile.scale * Main.rand.NextFloat(5f, 9.3f);
                pixelVelocity.Y -= Main.rand.NextFloat(1f, 7f) * (pixelSpawnPosition.X < pixelSpawnCore.X).ToDirectionInt();
                if (Projectile.velocity.X <= 0f)
                    pixelVelocity = Vector2.Reflect(pixelVelocity, Vector2.UnitX);
                pixelVelocity -= Projectile.velocity.RotatedByRandom(MathHelper.Pi / 3f) * 0.7f;

                BloomPixelParticle pixel = new(pixelSpawnPosition, pixelVelocity, Color.White * Projectile.Opacity * 0.75f, pixelColor * 0.4f, (int)(Projectile.Opacity * 38f), Vector2.One * Main.rand.NextFloat(1.7f, 3.3f));
                pixel.Spawn();
            }

            Time++;

            float visualTimePower = Projectile.scale * Projectile.Opacity;
            VisualsTime += visualTimePower / 120f;
            SpeedTime += Projectile.velocity.X * visualTimePower / 1550f;
        }

        public override bool? CanDamage() => Projectile.scale >= 0.9f && Projectile.Opacity >= 0.7f;

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            GenerateVerticesAndIndices();
            Matrix scale = Matrix.CreateTranslation(0f, Projectile.height * -0.5f, 0f) * Matrix.CreateScale(Projectile.scale / Projectile.Opacity, Projectile.scale, 1f) * Matrix.CreateTranslation(0f, Projectile.height * 0.5f, 0f);
            Matrix view = Matrix.CreateTranslation(new Vector3(Projectile.Top.X - Main.screenPosition.X, Projectile.Top.Y - Main.screenPosition.Y, 0f));
            Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -300f, 300f);

            ManagedShader tornadoShader = ShaderManager.GetShader("WoTE.DazzlingTornadoShader");
            tornadoShader.TrySetParameter("uWorldViewProjection", scale * view * projection);
            tornadoShader.TrySetParameter("speedTime", SpeedTime + Projectile.identity * 3.189f);
            tornadoShader.TrySetParameter("localTime", VisualsTime + Projectile.identity * 7.3817f);
            tornadoShader.TrySetParameter("opacity", Projectile.Opacity);
            tornadoShader.TrySetParameter("horizontalStack", 2.5f);
            tornadoShader.TrySetParameter("swirlDirection", Projectile.velocity.X.NonZeroSign());
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
