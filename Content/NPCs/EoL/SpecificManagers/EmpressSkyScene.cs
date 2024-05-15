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
        /// <summary>
        /// Whether the game music is paused because the game is paused during the Empress' battle.
        /// </summary>
        public static bool MusicIsPaused
        {
            get;
            set;
        }

        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override int Music => EmpressOfLight.Myself?.ModNPC?.Music ?? 0;

        public override float GetWeight(Player player) => 0.85f;

        public override bool IsSceneEffectActive(Player player) => EmpressOfLight.Myself is not null;

        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Filters.Scene[EmpressSky.SkyKey] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(Color.Transparent).UseOpacity(0f), EffectPriority.VeryHigh);
                SkyManager.Instance[EmpressSky.SkyKey] = new EmpressSky();
                SkyManager.Instance[EmpressSky.SkyKey].Load();
            }

            On_Main.UpdateAudio += CheckPauseState;
        }

        private void CheckPauseState(On_Main.orig_UpdateAudio orig, Main self)
        {
            bool musicShouldPause = EmpressOfLight.Myself is not null && Main.gamePaused;
            if (MusicIsPaused != musicShouldPause)
            {
                if (musicShouldPause)
                    Main.audioSystem.PauseAll();
                else
                    Main.audioSystem.ResumeAll();
                MusicIsPaused = musicShouldPause;
            }

            if (!MusicIsPaused)
                orig(self);
        }

        public override void SpecialVisuals(Player player, bool isActive)
        {
            player.ManageSpecialBiomeVisuals(EmpressSky.SkyKey, isActive);
        }
    }
}
