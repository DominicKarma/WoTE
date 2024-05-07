using System;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace WoTE.Content.Particles.Metaballs
{
    public class DistortionMetaball : MetaballType
    {
        public override string MetaballAtlasTextureToUse => "WoTE.AngularBloomRing.png";

        public override Color EdgeColor => Color.Transparent;

        public override bool ShouldRender => ActiveParticleCount >= 1;

        public override bool DrawnManually => true;

        public override Func<Texture2D>[] LayerTextures => [() => MiscTexturesRegistry.Pixel.Value];

        public override bool LayerIsFixedToScreen(int layerIndex) => true;

        public override bool PerformCustomSpritebatchBegin(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
            return true;
        }

        public override void UpdateParticle(MetaballInstance particle)
        {
            particle.Velocity.X *= 0.97f;
            particle.Size *= 1f + particle.ExtraInfo[1];
            particle.ExtraInfo[0] = Utilities.Saturate(particle.ExtraInfo[0] - particle.ExtraInfo[2]);
        }

        public override void DrawInstances()
        {
            var texture = AtlasManager.GetTexture(MetaballAtlasTextureToUse);

            foreach (var particle in Particles)
                Main.spriteBatch.Draw(texture, particle.Center - Main.screenPosition, null, Color.White * particle.ExtraInfo[0], 0f, null, new Vector2(particle.Size) / texture.Frame.Size(), SpriteEffects.None);

            ExtraDrawing();
        }

        public override bool ShouldKillParticle(MetaballInstance particle) => particle.ExtraInfo[0] <= 0.01f;
    }
}
