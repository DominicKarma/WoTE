using Luminance.Common.Utilities;
using Terraria;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL;

namespace WoTE
{
    public class LowGravityRemovingPlayer : ModPlayer
    {
        public override void PostUpdate()
        {
            float x = (Main.maxTilesX / 4200f).Squared();
            float spaceGravityMult = (float)((Player.position.Y / 16f - (60f + 10f * x)) / (Main.worldSurface / 6f));
            bool inSpace = spaceGravityMult < 1f; ;

            if (EmpressOfLight.Myself is not null && inSpace)
                Player.gravity = Player.defaultGravity;
        }
    }
}
