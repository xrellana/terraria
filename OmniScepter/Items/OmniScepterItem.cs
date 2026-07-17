using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OmniScepter.Items
{
    public class OmniScepterItem : ModItem
    {
        // Borrow the Rainbow Rod sprite so the mod works without custom art.
        // Replace with a real OmniScepterItem.png next to this file to use your own sprite.
        public override string Texture => $"Terraria/Images/Item_{ItemID.RainbowRod}";

        // Two in-game hours per click (60 ticks per second).
        private const int BuffDuration = 60 * 60 * 120;

        // Curated list of beneficial buffs. Deliberately excludes ones that
        // change movement in annoying ways (Gravitation, Featherfall).
        private static readonly int[] Buffs =
        {
            BuffID.ObsidianSkin,
            BuffID.Regeneration,
            BuffID.Swiftness,
            BuffID.Ironskin,
            BuffID.ManaRegeneration,
            BuffID.MagicPower,
            BuffID.Spelunker,
            BuffID.Hunter,
            BuffID.Thorns,
            BuffID.WaterWalking,
            BuffID.Archery,
            BuffID.NightOwl,
            BuffID.Shine,
            BuffID.Mining,
            BuffID.Heartreach,
            BuffID.Dangersense,
            BuffID.AmmoReservation,
            BuffID.Lifeforce,
            BuffID.Endurance,
            BuffID.Inferno,
            BuffID.Rage,
            BuffID.Wrath,
            BuffID.Summoning,
            BuffID.Builder,
            BuffID.WellFed3,
            BuffID.Sharpened,
            BuffID.AmmoBox,
            BuffID.Bewitched,
            BuffID.Clairvoyance,
            BuffID.SugarRush,
        };

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item4;
            Item.rare = ItemRarityID.Red;
            Item.value = Terraria.Item.sellPrice(gold: 10);
            Item.maxStack = 1;
            Item.noMelee = true;
        }

        public override bool? UseItem(Player player)
        {
            // Inventory edits must only run for the player actually using the item.
            if (player.whoAmI == Main.myPlayer)
            {
                ApplyAllBuffs(player);
                EnchantAllEquipment(player);
            }
            return true;
        }

        private static void ApplyAllBuffs(Player player)
        {
            foreach (int buff in Buffs)
            {
                player.AddBuff(buff, BuffDuration);
            }
        }

        private static void EnchantAllEquipment(Player player)
        {
            int reforged = 0;

            // Vanilla prefixes only exist on weapons and accessories;
            // armor pieces (slots 0-2) cannot be reforged, so skip them.
            for (int i = 3; i < 10; i++)
            {
                if (TryApplyPrefix(player.armor[i], PrefixID.Warding))
                {
                    reforged++;
                }
            }

            for (int i = 0; i < Main.InventorySlotsTotal; i++)
            {
                Item item = player.inventory[i];
                if (!item.IsAir && item.damage > 0 && item.maxStack == 1 && !item.accessory)
                {
                    if (TryApplyPrefix(item, BestWeaponPrefix(item)))
                    {
                        reforged++;
                    }
                }
                else if (!item.IsAir && item.accessory)
                {
                    if (TryApplyPrefix(item, PrefixID.Warding))
                    {
                        reforged++;
                    }
                }
            }

            string key = reforged > 0
                ? "Mods.OmniScepter.Messages.Reforged"
                : "Mods.OmniScepter.Messages.NothingToReforge";
            Main.NewText(Language.GetTextValue(key, reforged), 255, 200, 60);
        }

        private static int BestWeaponPrefix(Item item)
        {
            if (item.CountsAsClass(DamageClass.Ranged))
            {
                return PrefixID.Unreal;
            }
            if (item.CountsAsClass(DamageClass.Magic) || item.CountsAsClass(DamageClass.Summon))
            {
                return PrefixID.Mythical;
            }
            // Melee and anything unclassified.
            return PrefixID.Legendary;
        }

        private static bool TryApplyPrefix(Item item, int preferred)
        {
            if (item.IsAir || item.prefix == preferred)
            {
                return false;
            }

            int originalPrefix = item.prefix;
            bool favorited = item.favorited;

            // Reforging works like vanilla: reset the item, then roll the prefix.
            item.SetDefaults(item.type);

            // Not every prefix fits every item (e.g. spears can't be Legendary),
            // so fall back through weaker universal prefixes.
            if (!item.Prefix(preferred) &&
                !item.Prefix(PrefixID.Godly) &&
                !item.Prefix(PrefixID.Demonic) &&
                originalPrefix > 0)
            {
                // Everything failed: restore whatever the item had before.
                item.Prefix(originalPrefix);
            }

            item.favorited = favorited;
            return item.prefix != originalPrefix;
        }

        public override void AddRecipes()
        {
            // Cheap on purpose: this is a quality-of-life / cheat item.
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
