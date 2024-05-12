using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public class GracedWings : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
    }

    public class GracedWingsItem : GlobalItem
    {
        public override void HorizontalWingSpeeds(Item item, Player player, ref float horizontalSpeed, ref float horizontalAcceleration)
        {
            if (!player.HasBuff<GracedWings>())
                return;

            horizontalSpeed = MathF.Max(horizontalSpeed, 14.1f);
            horizontalAcceleration = MathF.Max(horizontalAcceleration, 0.7f);
        }

        public override void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            if (!player.HasBuff<GracedWings>())
                return;

            ascentWhenFalling = MathF.Max(ascentWhenFalling, 0.85f);
            ascentWhenRising = MathF.Max(ascentWhenRising, 0.15f);
            maxCanAscendMultiplier = MathF.Max(ascentWhenRising, 1.1f);
            maxAscentMultiplier = MathF.Max(maxAscentMultiplier, 2.7f);
            constantAscend = MathF.Max(constantAscend, 0.135f);
        }
    }
}
