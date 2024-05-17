using System;
using System.IO;
using System.Linq;
using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.Particles;

namespace WoTE.Content.NPCs.EoL.Projectiles
{
    public class MagicCircle : ModProjectile, IProjOwnedByBoss<EmpressOfLight>
    {
        internal static ManagedRenderTarget UnrotatedCircleTarget;

        /// <summary>
        /// The 3D rotation of this magic circle.
        /// </summary>
        public Quaternion Rotation
        {
            get;
            set;
        }

        /// <summary>
        /// How long this magic circle has existed for, in frames.
        /// </summary>
        public ref float Time => ref Projectile.ai[0];

        /// <summary>
        /// The general appearance interpolant of this magic circle.
        /// </summary>
        public ref float AppearanceInterpolant => ref Projectile.ai[1];

        /// <summary>
        /// The current aim direction of the circle.
        /// </summary>
        public ref float AimDirection => ref Projectile.ai[2];

        /// <summary>
        /// The ring height of this circle.
        /// </summary>
        public ref float RingHeight => ref Projectile.localAI[0];

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
            Projectile.timeLeft = EmpressOfLight.PrismaticOverload_ShootDelay + EmpressOfLight.PrismaticOverload_ShootTime;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Rotation.X);
            writer.Write(Rotation.Y);
            writer.Write(Rotation.Z);
            writer.Write(Rotation.W);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            float w = reader.ReadSingle();
            Rotation = new(x, y, z, w);
        }

        public override void AI()
        {
            if (EmpressOfLight.Myself is null)
            {
                Projectile.Kill();
                return;
            }

            float aimUpwardInterpolant = MathHelper.SmoothStep(0f, 1f, Utilities.InverseLerp(0f, 27f, Time - EmpressOfLight.PrismaticOverload_RotateUpwardDelay));
            DecideRotation(aimUpwardInterpolant);

            AppearanceInterpolant = Utilities.InverseLerp(0f, EmpressOfLight.PrismaticOverload_MagicCircleAppearTime, Time);
            RingHeight = aimUpwardInterpolant * 280f;

            Projectile.scale = EasingCurves.Elastic.Evaluate(EasingType.Out, Utilities.InverseLerp(0f, EmpressOfLight.PrismaticOverload_ScaleIntoExistenceTime, Time).Squared()) * 0.425f;
            Projectile.scale += EasingCurves.Elastic.Evaluate(EasingType.InOut, Utilities.InverseLerp(-60f, 0f, Time - EmpressOfLight.PrismaticOverload_ShootDelay)) * 0.25f;
            Projectile.scale *= Utilities.InverseLerp(0f, 10f, Projectile.timeLeft);

            Projectile.Center = EmpressOfLight.Myself.Center + (AimDirection - MathHelper.PiOver2).ToRotationVector2() * Projectile.scale * 380f;
            Projectile.rotation = EmpressOfLight.Myself.As<EmpressOfLight>().PrismaticOverload_MagicCircleSpinAngle;

            for (int i = 0; i < 2; i++)
                ReleaseCircleParticleForward();

            if (Time >= EmpressOfLight.PrismaticOverload_ShootDelay && Projectile.timeLeft >= 10)
            {
                for (int i = 0; i < 7; i++)
                    ReleaseLanceForward();

                if (Time % 90 == 89)
                {
                    for (int i = 0; i < 7; i++)
                        ReleaseLacewing((MathHelper.TwoPi * i / 7f).ToRotationVector2());
                }
            }

            Time++;
        }

        /// <summary>
        /// Decides the 3D rotation of this magic circle.
        /// </summary>
        /// <param name="aimUpwardInterpolant">How much, as a 0-1 interpolant, this circle should aim upward.</param>
        public void DecideRotation(float aimUpwardInterpolant)
        {
            if (EmpressOfLight.Myself is null)
                return;

            Player target = Main.player[EmpressOfLight.Myself.target];
            float idealDirection = EmpressOfLight.Myself.AngleTo(target.Center) + MathHelper.PiOver2;
            float aimAtPlayerInterpolant = MathHelper.SmoothStep(0f, 1f, Utilities.InverseLerp(0f, EmpressOfLight.PrismaticOverload_AimTowardsTargetTime, Time - EmpressOfLight.PrismaticOverload_AimTowardsTargetDelay));
            idealDirection = idealDirection.AngleLerp(0f, 1f - aimAtPlayerInterpolant);
            AimDirection = AimDirection.AngleTowards(idealDirection, 0.0071f).AngleLerp(idealDirection, 0.0189f);

            Quaternion forwardPerspective = Quaternion.Identity;
            Quaternion upwardPerspective = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationX(1.14f));
            Quaternion towardTargetPerspective = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationX(1.14f) * Matrix.CreateRotationZ(AimDirection));
            Rotation = Quaternion.Slerp(forwardPerspective, upwardPerspective, aimUpwardInterpolant * (1f - aimAtPlayerInterpolant));
            Rotation = Quaternion.Slerp(Rotation, towardTargetPerspective, aimAtPlayerInterpolant);
        }

        /// <summary>
        /// Releases a single bloom streak particle forward at the edge of the magic circle.
        /// </summary>
        public void ReleaseCircleParticleForward()
        {
            float particleSpeedInterpolant = Main.rand.NextFloat();
            int particleLifetime = (int)MathHelper.Lerp(36f, 10f, particleSpeedInterpolant);
            Vector2 edgeParticleSpawnPosition = Projectile.Center + Vector2.Transform(Main.rand.NextVector2CircularEdge(250f, 250f), Rotation) * Projectile.scale * 2f;
            Vector2 edgeParticleVelocity = (AimDirection - MathHelper.PiOver2).ToRotationVector2() * MathHelper.Lerp(2f, 14f, particleSpeedInterpolant);
            BloomCircleParticle edgeParticle = new(edgeParticleSpawnPosition, edgeParticleVelocity, new(0.2f, 0.06f), Color.Wheat, Color.DeepSkyBlue * 0.5f, particleLifetime, 1.5f);
            edgeParticle.Spawn();
        }

        /// <summary>
        /// Releases a single lance forward at the edge of the magic circle.
        /// </summary>
        public void ReleaseLanceForward()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            float lanceHue = (Time / 30f + Main.rand.NextFloat(0.19f)) % 1f;
            Vector2 lanceSpawnPosition = Projectile.Center + Vector2.Transform(Main.rand.NextVector2CircularEdge(250f, 250f), Rotation) * Projectile.scale * 2f;
            Vector2 lanceVelocity = (AimDirection - MathHelper.PiOver2 + Main.rand.NextFloatDirection() * 0.08f).ToRotationVector2() * 80f;

            Utilities.NewProjectileBetter(Projectile.GetSource_FromThis(), lanceSpawnPosition, lanceVelocity, ModContent.ProjectileType<LightLance>(), EmpressOfLight.LightLanceDamage, 0f, -1, 0f, lanceHue, 1f);
        }

        /// <summary>
        /// Releases a single lacewing forward at the center of the magic circle.
        /// </summary>
        public void ReleaseLacewing(Vector2 lacewingDirection)
        {
            Vector2 lacewingVelocity = lacewingDirection * 24f;
            Utilities.NewProjectileBetter(Projectile.GetSource_FromThis(), Projectile.Center, lacewingVelocity, ModContent.ProjectileType<HomingLacewing>(), EmpressOfLight.MagicRingLacewingDamage, 0f);
        }

        /// <summary>
        /// Draws the weakly glowing cylinder for the magic circle.
        /// </summary>
        /// <param name="drawOffset">The draw offset of the cylinder.</param>
        /// <param name="rotation">The cylinder's rotation.</param>
        /// <param name="colorolor">The color of the cylinder.</param>
        public void DrawBackglowCylinder(Vector2 drawOffset, Quaternion rotation, Color colorolor)
        {
            float appearanceScaleFactor = 1f;
            float verticalOffset = appearanceScaleFactor.Squared() * RingHeight * 3f;
            Vector2 top = -Vector2.UnitY * verticalOffset;
            Vector2 bottom = top + Vector2.UnitY * verticalOffset * 1.4f;
            GenerateCylinderUVs(new(524f, 508f), top, bottom, colorolor, out short[] indices, out VertexPosition2DColorTexture[] vertices);

            Matrix scale = Matrix.CreateScale(Projectile.scale, Projectile.scale, 1f);
            Matrix view = Matrix.CreateTranslation(new Vector3(Projectile.Center.X + drawOffset.X - Main.screenPosition.X, Projectile.Center.Y + drawOffset.Y - Main.screenPosition.Y, 0f));
            Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height, 0f, -2000f, 2000f);

            ManagedShader ringShader = ShaderManager.GetShader("WoTE.MagicCircleGlowRingShader");
            ringShader.TrySetParameter("uWorldViewProjection", Matrix.CreateFromQuaternion(rotation) * scale * view * Main.GameViewMatrix.TransformationMatrix * projection);
            ringShader.SetTexture(MiscTexturesRegistry.DendriticNoiseZoomedOut.Value, 1, SamplerState.LinearWrap);
            ringShader.Apply();

            var gd = Main.instance.GraphicsDevice;
            gd.RasterizerState = RasterizerState.CullNone;
            gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, vertices.Length / 2);
        }

        /// <summary>
        /// Draws the magic circle from the <see cref="UnrotatedCircleTarget"/> with 3D rotation and backglow.
        /// </summary>
        /// <param name="drawOffset">The draw offset of the ring.</param>
        /// <param name="rotation">The circle's rotation.</param>
        /// <param name="circleColor">The color of the circle.</param>
        public void DrawFromTarget(Vector2 drawOffset, Quaternion rotation, Color circleColor)
        {
            Texture2D drawnCircle = UnrotatedCircleTarget;

            float[] blurWeights = new float[11];
            for (int i = 0; i < blurWeights.Length; i++)
                blurWeights[i] = Utilities.GaussianDistribution(i - (int)(blurWeights.Length * 0.5f), 2f) / 13f;
            ManagedShader underglowShader = ShaderManager.GetShader("WoTE.BlurUnderglowShader");
            underglowShader.TrySetParameter("blurOffset", Projectile.scale * 0.004f);
            underglowShader.TrySetParameter("blurWeights", blurWeights);

            PrimitiveRenderer.RenderQuad(drawnCircle, Projectile.Center + drawOffset, Vector2.One, 0f, circleColor, underglowShader, rotation);
            PrimitiveRenderer.RenderQuad(drawnCircle, Projectile.Center + drawOffset, Vector2.One, 0f, circleColor, null, rotation);
        }

        /// <summary>
        /// Draws the depthed right with the symbol text.
        /// </summary>
        /// <param name="drawOffset">The draw offset of the ring.</param>
        /// <param name="rotation">The ring's rotation.</param>
        /// <param name="ringColor">The color of the ring.</param>
        public void DrawRing(Vector2 drawOffset, Quaternion rotation, Color ringColor)
        {
            float appearanceScaleFactor = 1f;
            Vector2 top = Vector2.UnitY * -4f;
            Vector2 bottom = top + Vector2.UnitY * appearanceScaleFactor.Squared() * RingHeight;
            GenerateCylinderUVs(new(524f, 508f), top, bottom, ringColor, out short[] indices, out VertexPosition2DColorTexture[] vertices);

            Matrix scale = Matrix.CreateScale(Projectile.scale, Projectile.scale, 1f);
            Matrix view = Matrix.CreateTranslation(new Vector3(Projectile.Center.X + drawOffset.X - Main.screenPosition.X, Projectile.Center.Y + drawOffset.Y - Main.screenPosition.Y, 0f));
            Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height, 0f, -1000f, 1000f);
            Texture2D ring = ModContent.Request<Texture2D>("WoTE/Content/NPCs/EoL/Projectiles/MagicCircleRingStrip").Value;

            ManagedShader ringShader = ShaderManager.GetShader("WoTE.MagicCircleRingShader");
            ringShader.SetTexture(ring, 1, SamplerState.LinearWrap);
            ringShader.TrySetParameter("spinScrollOffset", Projectile.rotation * -0.75f);
            ringShader.TrySetParameter("uWorldViewProjection", Matrix.CreateFromQuaternion(rotation) * scale * view * Main.GameViewMatrix.TransformationMatrix * projection);
            ringShader.Apply();

            var gd = Main.instance.GraphicsDevice;
            gd.RasterizerState = RasterizerState.CullNone;
            gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, vertices.Length / 2);
        }

        /// <summary>
        /// Generates vertices and indices for use with a 3D cylinder shape.
        /// </summary>
        /// <param name="maxSize">The max size of the overall ring. The X axis corresponds to the max radial width, and the Y axis corresponds to the max radial height.</param>
        /// <param name="top">The top of the cylinder, centered relative to the origin of (0, 0).</param>
        /// <param name="bottom">The bottom of the cylinder, centered relative to the origin of (0, 0).</param>
        /// <param name="cylinderColor">The color of the cylinder.</param>
        /// <param name="indices">The resulting indices.</param>
        /// <param name="vertices">The resulting vertices.</param>
        public static void GenerateCylinderUVs(Vector2 maxSize, Vector2 top, Vector2 bottom, Color cylinderColor, out short[] indices, out VertexPosition2DColorTexture[] vertices)
        {
            int precision = 240;
            float appearanceScaleFactor = 1f;
            vertices = new VertexPosition2DColorTexture[precision * 2];
            for (int i = 0; i < precision; i++)
            {
                float angle = MathHelper.TwoPi * i / precision * 2f;
                float x = i / (float)precision * 2f;

                Vector2 topTextureCoordinate = new(x, 0f);
                Vector2 bottomTextureCoordinate = new(x, 1f);
                Vector2 circularOffset = angle.ToRotationVector2() * maxSize * appearanceScaleFactor;

                vertices[i * 2] = new(top + circularOffset, cylinderColor, topTextureCoordinate, MathF.Cos(angle));
                vertices[i * 2 + 1] = new(bottom + circularOffset, cylinderColor, bottomTextureCoordinate, MathF.Cos(angle));
            }

            short indicesIndex = 0;
            indices = new short[(precision - 2) * 6];
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
        }

        /// <summary>
        /// Renders the base magic circle to the <see cref="UnrotatedCircleTarget"/>.
        /// </summary>
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

            DrawMagicCircle(Color.HotPink, Vector2.Zero, appearInterpolantB, innerAngleOffset * -2f, Projectile.scale * 0.5f, 6);
            DrawMagicCircle(Color.LightGoldenrodYellow, Vector2.Zero, appearInterpolantB, innerAngleOffset * 0.5f, Projectile.scale * 0.5f, 5);
            DrawMagicCircle(Color.Pink, Vector2.Zero, appearInterpolantB, -MathHelper.PiOver2, Projectile.scale * 0.5f, 3);
            DrawLacewingAtCenterOfRing(appearInterpolantC.Squared());

            Main.spriteBatch.End();
        }

        /// <summary>
        /// Draws a lacewing at the center of the magic circle.
        /// </summary>
        /// <param name="appearInterpolant">The 0-1 interpolant that represents how much the lacewing has appeared.</param>
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

        /// <summary>
        /// Draws a magic circle configuration.
        /// </summary>
        /// <param name="circleColor">The color of the magic circle.</param>
        /// <param name="drawOffset">The draw offset of the circle.</param>
        /// <param name="appearanceInterpolant">The 0-1 interpolant that represents how much the magic circle has appeared.</param>
        /// <param name="angleOffset">The angular offset of the inscribed shapes.</param>
        /// <param name="scale">The overall scale of the circle.</param>
        /// <param name="polygonSides">The set of all polygon side counts to render. A value of 3 in the set would render a triangle, 4 a quadrilateral, etc.</param>
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

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 ringDrawOffset = Vector2.Transform(UnrotatedCircleTarget.Size() * new Vector2(-0.5f, 0.5f), Rotation);

            DrawBackglowCylinder(Vector2.Zero, Rotation, Color.SkyBlue with { A = 0 } * Utilities.InverseLerp(0.25f, 0.8f, AppearanceInterpolant));
            DrawFromTarget(ringDrawOffset, Rotation, Color.White);
            DrawRing(Vector2.Zero, Rotation, Color.SkyBlue with { A = 0 });

            return false;
        }
    }
}
