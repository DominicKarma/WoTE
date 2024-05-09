using System.ComponentModel;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace WoTE.Core.Configuration
{
    [BackgroundColor(96, 30, 53, 216)]
    public class WoTEConfig : ModConfig
    {
        public static WoTEConfig Instance => ModContent.GetInstance<WoTEConfig>();

        public override ConfigScope Mode => ConfigScope.ClientSide;

        [BackgroundColor(224, 127, 180, 192)]
        [DefaultValue(false)]
        public bool PhotosensitivityMode
        {
            get;
            set;
        }

        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) => false;
    }
}
