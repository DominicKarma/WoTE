using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace WoTE.Common.MainMenuThemes
{
    public class GlimmerdewBlessedMoonlightMainMenu : ModMenu
    {
        public override string DisplayName => Language.GetTextValue($"Mods.{Mod.Name}.MenuThemes.GlimmerdewBlessedMoonlightMainMenu.DisplayName");

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/EmpressOfLight");

        public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
        {
            Main.time = 16200D;
            Main.dayTime = false;
            return false;
        }
    }
}
