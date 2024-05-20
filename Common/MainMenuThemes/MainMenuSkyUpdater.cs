using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL;

namespace WoTE.Common.MainMenuThemes
{
    public sealed class MainMenuSkyUpdater : ModSystem
    {
        private bool readyToRender;

        public override void Load()
        {
            On_Main.DoUpdate += UpdateSky;
        }

        public override void PostSetupContent()
        {
            readyToRender = true;
        }

        private void UpdateSky(On_Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
        {
            orig(self, ref gameTime);

            // Don't bother if not in the game menu or not fully initialized.
            if (!Main.gameMenu || !readyToRender)
                return;

            if (SkyManager.Instance[EmpressSky.SkyKey] is null || !ShaderManager.HasFinishedLoading)
                return;

            bool active = ModContent.GetInstance<GloriousDawnMainMenu>().IsSelected || ModContent.GetInstance<GlimmerdewBlessedMoonlightMainMenu>().IsSelected;
            if (active)
                SkyManager.Instance.Activate(EmpressSky.SkyKey);
            else
                SkyManager.Instance.Deactivate(EmpressSky.SkyKey);

            SkyManager.Instance[EmpressSky.SkyKey].Update(gameTime);
        }
    }
}
