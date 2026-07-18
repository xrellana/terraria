using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using OmniScepter.Buffs;

namespace OmniScepter.Items
{
    public class OmniGlobeItem : ModItem
    {
        // Borrow the Snow Globe sprite so the mod works without custom art.
        // Replace with a real OmniGlobeItem.png next to this file to use your own sprite.
        public override string Texture => $"Terraria/Images/Item_{ItemID.SnowGlobe}";

        // The personal climates the globe can be shaken into.
        private static ModBuff[] Climates => new ModBuff[]
        {
            ModContent.GetInstance<SnowGlobeBuff>(),
            ModContent.GetInstance<SandstormGlobeBuff>(),
            ModContent.GetInstance<BloodMoonGlobeBuff>(),
            ModContent.GetInstance<GlowshroomGlobeBuff>(),
            ModContent.GetInstance<HallowGlobeBuff>(),
        };

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.UseSound = SoundID.Item4;
            Item.rare = ItemRarityID.Blue;
            Item.value = Terraria.Item.sellPrice(silver: 10);
            Item.maxStack = 1;
            Item.noMelee = true;
        }

        // Enables right click as a second use mode.
        public override bool AltFunctionUse(Player player) => true;

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (player.altFunctionUse == 2)
                {
                    // Right click: calm the climate.
                    ClearClimates(player, announce: true);
                }
                else
                {
                    ShakeClimate(player);
                }
            }
            return true;
        }

        private static void ShakeClimate(Player player)
        {
            ModBuff[] climates = Climates;
            int current = -1;
            for (int i = 0; i < climates.Length; i++)
            {
                if (player.HasBuff(climates[i].Type))
                {
                    current = i;
                    break;
                }
            }

            // Roll a random climate, excluding the active one so every shake
            // visibly changes the weather.
            int next = Main.rand.Next(current >= 0 ? climates.Length - 1 : climates.Length);
            if (current >= 0 && next >= current)
            {
                next++;
            }

            ClearClimates(player, announce: false);
            player.AddBuff(climates[next].Type, 18000);
            Main.NewText(
                Language.GetTextValue("Mods.OmniScepter.Messages.GlobeMode",
                    climates[next].DisplayName.Value),
                150, 220, 255);
        }

        private static void ClearClimates(Player player, bool announce)
        {
            foreach (ModBuff climate in Climates)
            {
                player.ClearBuff(climate.Type);
            }
            if (announce)
            {
                Main.NewText(Language.GetTextValue("Mods.OmniScepter.Messages.GlobeOff"),
                    150, 220, 255);
            }
        }

        public override void AddRecipes()
        {
            // Cheap on purpose: meant to be available from the first tree you chop.
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
