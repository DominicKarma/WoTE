using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public class EmpressSkyScene : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override int Music => EmpressOfLight.Myself?.ModNPC?.Music ?? 0;

        public override float GetWeight(Player player) => 0.85f;

        public override bool IsSceneEffectActive(Player player) => NPC.AnyNPCs(ModContent.NPCType<EmpressOfLight>());

        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Filters.Scene[EmpressSky.SkyKey] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(Color.Transparent).UseOpacity(0f), EffectPriority.VeryHigh);
                SkyManager.Instance[EmpressSky.SkyKey] = new EmpressSky();
                SkyManager.Instance[EmpressSky.SkyKey].Load();
            }
        }

        public override void SpecialVisuals(Player player, bool isActive)
        {
            player.ManageSpecialBiomeVisuals(EmpressSky.SkyKey, isActive);
        }
    }
}
