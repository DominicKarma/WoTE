using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Drawing;
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

        public override bool WingUpdate(int wings, Player player, bool inUse)
        {
            if (!player.HasBuff<GracedWings>() || !inUse)
                return false;

            if (Main.rand.NextBool())
            {
                Dust rainbow = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(26f, 18f), 261);
                rainbow.velocity = Vector2.UnitY * Main.rand.NextFloat(2f, 5f);
                rainbow.color = Main.hslToRgb(Main.rand.NextFloat(0.85f, 1.25f) % 1f, 1f, 0.5f);
                rainbow.color = Color.Lerp(rainbow.color, Color.HotPink, Main.rand.NextFloat(0.6f));
                rainbow.noGravity = true;
            }

            if (Main.rand.NextBool(5))
            {
                ParticleOrchestrator.RequestParticleSpawn(true, Main.rand.NextBool() ? ParticleOrchestraType.PrincessWeapon : ParticleOrchestraType.StardustPunch, new ParticleOrchestraSettings
                {
                    PositionInWorld = player.Center + Main.rand.NextVector2Circular(26f, 18f),
                    MovementVector = Main.rand.NextVector2Circular(2f, 4f) + Vector2.UnitY * 5f
                });
            }

            return false;
        }
    }
}
