using System;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace WoTE.Content.Particles
{
    public class PrismaticLacewingParticle : Particle
    {
        /// <summary>
        /// The current frame of this Lacewing.
        /// </summary>
        public int CurrentFrame;

        /// <summary>
        /// The optional position function that this pixel should try to home towards.
        /// </summary>
        public Func<Vector2>? HomeInDestination;

        public override string AtlasTextureName => "WoTE.Lacewing.png";

        public override BlendState BlendState => BlendState.Additive;

        public PrismaticLacewingParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, Vector2 scale, Func<Vector2>? homeInDestination = null)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            Opacity = 1f;
            Rotation = 0f;
            CurrentFrame = Main.rand.Next(3);
            HomeInDestination = homeInDestination;
        }

        public override void Update()
        {
            Opacity = Utilities.InverseLerpBump(0f, 0.03f, 0.5f, 1f, LifetimeRatio);

            if (HomeInDestination is null)
            {
                if (Velocity.Length() >= 50f)
                    Velocity *= 0.65f;
                else if (Velocity.Length() >= 15f)
                    Velocity *= 0.85f;
                else
                    Velocity *= 0.93f;
            }
            else
            {
                Vector2 homeInDestination = HomeInDestination?.Invoke() ?? Position;
                Velocity = Vector2.Lerp(Velocity, Position.SafeDirectionTo(homeInDestination), 0.015f);
                if (Position.WithinRange(homeInDestination, 300f))
                    Velocity *= 0.979f;

                if (Position.WithinRange(homeInDestination, 16f))
                    Kill();
            }

            float idealRotation = MathHelper.Clamp(Velocity.X * 0.02f, -0.46f, 0.46f);
            Rotation = MathHelper.Lerp(Rotation, idealRotation, 0.18f);

            if (Math.Abs(Velocity.X) >= 0.4f)
                Direction = -Velocity.X.NonZeroSign();

            if (Time % 5 == 4)
                CurrentFrame = (CurrentFrame + 1) % 3;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle frame = Texture.Frame;
            frame.Height /= 3;
            frame.Y += frame.Height * CurrentFrame;

            for (int i = 0; i < 4; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 4f).ToRotationVector2() * 2f;
                spriteBatch.Draw(Texture, Position + drawOffset - Main.screenPosition, frame, DrawColor * Opacity.Cubed(), Rotation, null, Scale, Direction.ToSpriteDirection());
            }

            spriteBatch.Draw(Texture, Position - Main.screenPosition, frame, Color.White * Opacity, Rotation, null, Scale, Direction.ToSpriteDirection());
        }
    }
}
