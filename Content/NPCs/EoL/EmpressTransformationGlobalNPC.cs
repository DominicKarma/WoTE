using Luminance.Common.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public class EmpressTransformationGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override bool PreAI(NPC npc)
        {
            bool calBossRush = ModLoader.TryGetMod("CalamityMod", out Mod calamity) && (bool)calamity.Call("DifficultyActive", "BossRush");
            bool canUseCustomAI = !calBossRush;
            if (npc.type == NPCID.HallowBoss && canUseCustomAI)
            {
                npc.Transform(ModContent.NPCType<EmpressOfLight>());
                npc.As<EmpressOfLight>().Awaken_IsEnraged = true;
                return false;
            }

            return true;
        }
    }
}
