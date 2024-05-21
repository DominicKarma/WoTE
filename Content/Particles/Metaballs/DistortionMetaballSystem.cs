using Luminance.Core.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using WoTE.Content.Particles.Metaballs;

namespace WoTE.Core.Graphics.EffectManagers
{
    [Autoload(Side = ModSide.Client)]
    public class DistortionMetaballSystem : ModSystem
    {
        public override void PostUpdateEverything()
        {
            ManagedScreenFilter distortionShader = ShaderManager.GetFilter("WoTE.MetaballDistortionFilter");

            bool shouldBeActive = ModContent.GetInstance<DistortionMetaball>().ShouldRender;
            if (!distortionShader.IsActive && shouldBeActive)
                ApplyDistortionParameters(distortionShader);
        }

        private static void ApplyDistortionParameters(ManagedScreenFilter distortionShader)
        {
            distortionShader.TrySetParameter("screenZoom", Main.GameViewMatrix.Zoom);
            distortionShader.SetTexture(ModContent.GetInstance<DistortionMetaball>().LayerTargets[0], 2, SamplerState.LinearClamp);
            distortionShader.Activate();
        }
    }
}
