using Terraria;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL;

namespace WoTE
{
    public class LowGravityRemovingPlayer : ModPlayer
    {
        public override void UpdateEquips()
        {
            if (NPC.AnyNPCs(ModContent.NPCType<EmpressOfLight>()))
                Player.gravity = Player.defaultGravity;
        }
    }
}
