using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    [Autoload(Side = ModSide.Client)]
    public class EmpressOfLightTargetManager : ModSystem
    {
        /// <summary>
        /// The render target the encompasses the Empress of Light.
        /// </summary>
        internal static ManagedRenderTarget EmpressTarget;

        public override void OnModLoad()
        {
            EmpressTarget = new(false, (_, _2) =>
            {
                return new(Main.instance.GraphicsDevice, 672, 672);
            });
            RenderTargetManager.RenderTargetUpdateLoopEvent += DrawToEmpressTarget;
        }

        /// <summary>
        /// Draws the Empress to her render target.
        /// </summary>
        private void DrawToEmpressTarget()
        {
            if (EmpressOfLight.Myself is null)
                return;

            var gd = Main.instance.GraphicsDevice;

            gd.SetRenderTarget(EmpressTarget);
            gd.Clear(Color.Transparent);

            BeginSpriteBatch(SpriteSortMode.Deferred);
            EmpressOfLight.Myself.As<EmpressOfLight>().DrawToTarget(EmpressTarget.Size() * 0.5f);
            Main.spriteBatch.End();

            gd.SetRenderTarget(null);
        }

        /// <summary>
        /// Starts the sprite batch for the Empress' rendering.
        /// </summary>
        /// <param name="sortMode">The sprite sort mode to use when rendering.</param>
        internal static void BeginSpriteBatch(SpriteSortMode sortMode)
        {
            Main.spriteBatch.Begin(sortMode, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        }
    }
}
