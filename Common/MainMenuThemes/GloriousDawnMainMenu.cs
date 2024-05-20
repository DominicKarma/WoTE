using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace WoTE.Common.MainMenuThemes
{
    public class GloriousDawnMainMenu : ModMenu
    {
        public override bool IsAvailable => false;

        public override string DisplayName => Language.GetTextValue($"Mods.{Mod.Name}.MenuThemes.GloriousDawnMainMenu.DisplayName");

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/EmpressOfLight");

        public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
        {
            Main.time = 27000D;
            Main.dayTime = true;
            return false;
        }
    }
}
