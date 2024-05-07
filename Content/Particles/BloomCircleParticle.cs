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
        public float StartingScale
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

        public BloomCircleParticle(Vector2 position, Vector2 velocity, float scale, Color color, Color bloomColor, int lifetime, float bloomScaleFactor, float opacity = 1f)
        {
            Position = position;
            Velocity = velocity;
            Scale = Vector2.One * scale;
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
            Velocity *= 0.98f;
            Scale = Vector2.One * EasingCurves.Quadratic.Evaluate(EasingType.Out, StartingScale, 0f, LifetimeRatio);

            float fadeOutInterpolant = Utilities.InverseLerp(0f, 0.54f, LifetimeRatio);
            Opacity = EasingCurves.Quadratic.Evaluate(EasingType.In, StartingOpacity, 0f, fadeOutInterpolant);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 drawPosition = Position - Main.screenPosition;
            spriteBatch.Draw(Texture, drawPosition, null, BloomColor * Opacity, 0f, null, Scale * BloomScaleFactor, 0);
            spriteBatch.Draw(Texture, drawPosition, null, DrawColor * Opacity, 0f, null, Scale, 0);
        }
    }
}
