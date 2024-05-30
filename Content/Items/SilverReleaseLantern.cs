using Luminance.Common.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL;

namespace WoTE.Content.Items
{
    public class SilverReleaseLantern : ModItem
    {
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 24;
            Item.DefaultToThrownWeapon(ModContent.ProjectileType<SilverReleaseLanternProj>(), 25, 4f);
            Item.value = 0;
            Item.damage = 0;
            Item.DamageType = DamageClass.Default;
            Item.rare = ItemRarityID.Purple;
            Item.maxStack = 1;
            Item.consumable = false;
            Item.noUseGraphic = true;
        }

        public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<EmpressOfLight>()) && !Utilities.AnyProjectiles(ModContent.ProjectileType<SilverReleaseLanternProj>());

        public override void AddRecipes()
        {
            CreateRecipe(1).
                AddTile(TileID.LunarCraftingStation).
                AddIngredient(ItemID.ReleaseLantern).
                AddIngredient(ItemID.LunarOre, 10).
                AddIngredient(ItemID.EmpressButterfly).
                Register();
        }
    }
}
