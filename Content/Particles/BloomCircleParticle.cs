using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace WoTE.Content.Particles
{
    public class BloomCircleParticle : Particle
    {
        /// <summary>
        /// The starting scale of this particle.
        /// </summary>
        public Vector2 StartingScale
        {
            get;
            set;
        }

        /// <summary>
        /// The starting opacity of this particle.
        /// </summary>
        public float StartingOpacity
        {
            get;
            set;
        }

        /// <summary>
        /// The scale factor of bloom for this particle.
        /// </summary>
        public float BloomScaleFactor
        {
            get;
            set;
        }

        /// <summary>
        /// The color of bloom for this particle.
        /// </summary>
        public Color BloomColor
        {
            get;
            set;
        }

        public override BlendState BlendState => BlendState.Additive;

        public override string AtlasTextureName => "WoTE.BloomCircle.png";

        public BloomCircleParticle(Vector2 position, Vector2 velocity, Vector2 scale, Color color, Color bloomColor, int lifetime, float bloomScaleFactor, float opacity = 1f)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            StartingScale = scale;
            DrawColor = color;
            BloomColor = bloomColor;
            Opacity = opacity;
            StartingOpacity = opacity;
            BloomScaleFactor = bloomScaleFactor;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Velocity *= 0.96f;
            Scale = StartingScale * EasingCurves.Quadratic.Evaluate(EasingType.Out, 1f, 0f, LifetimeRatio);

            float fadeOutInterpolant = Utilities.InverseLerp(0f, 0.54f, LifetimeRatio);
            Opacity = EasingCurves.Quadratic.Evaluate(EasingType.In, StartingOpacity, 0f, fadeOutInterpolant);

            Rotation = Velocity.ToRotation();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 drawPosition = Position - Main.screenPosition;
            spriteBatch.Draw(Texture, drawPosition, null, BloomColor * Opacity, Rotation, null, Scale * BloomScaleFactor, 0);
            spriteBatch.Draw(Texture, drawPosition, null, DrawColor * Opacity, Rotation, null, Scale, 0);
        }
    }
}
