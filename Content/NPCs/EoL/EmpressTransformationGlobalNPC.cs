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
            // TODO -- Sidestep this if a different mod attempts to reset the Empress' AI, maybe?
            if (npc.type == NPCID.HallowBoss)
            {
                npc.Transform(ModContent.NPCType<EmpressOfLight>());
                npc.As<EmpressOfLight>().Awaken_IsEnraged = true;
                return false;
            }

            return true;
        }
    }
}
