using System;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace WoTE.Content.Particles
{
    public class BloomPixelParticle : Particle
    {
        /// <summary>
        /// The color of bloom behind the pixel.
        /// </summary>
        public Color BloomColor;

        /// <summary>
        /// The base scale of this pixel
        /// </summary>
        public Vector2 BaseScale;

        /// <summary>
        /// The scale factor of the back-bloom.
        /// </summary>
        public Vector2 BloomScaleFactor;

        /// <summary>
        /// The homer-in speed factor. Only used if <see cref="HomeInDestination"/> is used.
        /// </summary>
        public float HomeInSpeedFactor;

        /// <summary>
        /// The optional position function that this pixel should try to home towards.
        /// </summary>
        public Func<Vector2>? HomeInDestination;

        /// <summary>
        /// The bloom texture.
        /// </summary>
        public static AtlasTexture BloomTexture
        {
            get;
            private set;
        }

        public override string AtlasTextureName => "WoTE.Pixel.png";

        public override BlendState BlendState => BlendState.Additive;

        public BloomPixelParticle(Vector2 position, Vector2 velocity, Color color, Color bloomColor, int lifetime, Vector2 scale, Func<Vector2>? homeInDestination = null, Vector2? bloomScaleFactor = null, float homeInSpeedFactor = 1f)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            BloomColor = bloomColor;
            Scale = scale;
            BaseScale = scale;
            Lifetime = lifetime;
            Opacity = 1f;
            Rotation = 0f;
            HomeInDestination = homeInDestination;
            HomeInSpeedFactor = homeInSpeedFactor;
            BloomScaleFactor = bloomScaleFactor ?? Vector2.One * 0.04f;
        }

        public override void Update()
        {
            if (Time >= Lifetime * 0.65f)
            {
                Opacity *= 0.91f;
                Scale *= 0.96f;
                Velocity *= 0.94f;

                if (Time >= Lifetime - 5)
                {
                    Opacity *= 0.75f;
                    Scale *= 0.75f;
                }
            }
            else
                Scale = Vector2.Lerp(BaseScale, Scale, Utilities.InverseLerp(0f, 24f, Time).Squared());

            if (HomeInDestination is null)
                Velocity *= 0.96f;
            else
            {
                Vector2 homeInDestination = HomeInDestination?.Invoke() ?? Position;
                float flySpeedInterpolant = Utilities.InverseLerp(0f, 120f, Time);
                float currentDirection = Velocity.ToRotation();
                float idealDirection = (homeInDestination - Position).ToRotation();
                Velocity = currentDirection.AngleLerp(idealDirection, flySpeedInterpolant * 0.014f).ToRotationVector2() * Velocity.Length();
                Velocity = Vector2.Lerp(Velocity, idealDirection.ToRotationVector2() * (Time * 0.05f + HomeInSpeedFactor * 25f), flySpeedInterpolant * 0.0137f);
                if (Position.WithinRange(homeInDestination, 300f))
                    Velocity *= 0.97f;

                if (Position.WithinRange(homeInDestination, 18f))
                    Kill();
            }

            Rotation += Velocity.X * 0.07f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            BloomTexture ??= AtlasManager.GetTexture("WoTE.BloomCircle.png");
            spriteBatch.Draw(BloomTexture, Position - Main.screenPosition, null, BloomColor * Opacity, Rotation, null, Scale * BloomScaleFactor, 0);
            base.Draw(spriteBatch);
        }
    }
}
