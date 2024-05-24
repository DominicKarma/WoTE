using Terraria;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
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
