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

        // Endgame gear granted by right click. Covers all four damage classes
        // plus the strongest general-purpose armor and accessories.
        private static readonly (int type, int stack)[] EndgameKit =
        {
            (ItemID.Zenith, 1),                 // melee sword
            (ItemID.SDMG, 1),                   // ranged gun
            (ItemID.LastPrism, 1),              // magic
            (ItemID.EmpressBlade, 1),           // summon (Terraprisma)
            (ItemID.RainbowWhip, 1),            // whip (Kaleidoscope)
            (ItemID.SolarFlareHelmet, 1),
            (ItemID.SolarFlareBreastplate, 1),
            (ItemID.SolarFlareLeggings, 1),
            (ItemID.StardustHelmet, 1),         // Stardust set for summoners
            (ItemID.StardustBreastplate, 1),
            (ItemID.StardustLeggings, 1),
            (ItemID.LongRainbowTrailWings, 1),  // Celestial Starboard wings
            (ItemID.AnkhShield, 1),
            (ItemID.TerrasparkBoots, 1),
            (ItemID.CelestialShell, 1),
            (ItemID.MasterNinjaGear, 1),
            (ItemID.DestroyerEmblem, 1),
            (ItemID.MoonlordBullet, 999),       // ammo for the SDMG
        };

        // One summon item for every boss that can be summoned with an item.
        // Skeletron has no summon item (talk to the Old Man); Plantera and the
        // Lunatic Cultist are triggered in the world, so they are not listed.
        private static readonly (int type, int stack)[] BossSummonKit =
        {
            (ItemID.SlimeCrown, 3),             // King Slime
            (ItemID.SuspiciousLookingEye, 3),   // Eye of Cthulhu
            (ItemID.WormFood, 3),               // Eater of Worlds
            (ItemID.BloodySpine, 3),            // Brain of Cthulhu
            (ItemID.Abeemination, 3),           // Queen Bee
            (ItemID.DeerThing, 3),              // Deerclops
            (ItemID.GuideVoodooDoll, 3),        // Wall of Flesh (drop into lava)
            (ItemID.MechanicalEye, 3),          // The Twins
            (ItemID.MechanicalWorm, 3),         // The Destroyer
            (ItemID.MechanicalSkull, 3),        // Skeletron Prime
            (ItemID.QueenSlimeCrystal, 3),      // Queen Slime
            (ItemID.LihzahrdPowerCell, 3),      // Golem
            (ItemID.TruffleWorm, 3),            // Duke Fishron (fishing bait)
            (ItemID.EmpressButterfly, 3),       // Empress of Light (kill it at night)
            (ItemID.CelestialSigil, 3),         // Moon Lord
            (ItemID.ClothierVoodooDoll, 1),     // Skeletron re-fight via the Clothier
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

        // Enables right click as a second use mode.
        public override bool AltFunctionUse(Player player) => true;

        public override bool? UseItem(Player player)
        {
            // Inventory edits must only run for the player actually using the item.
            if (player.whoAmI == Main.myPlayer)
            {
                if (player.altFunctionUse == 2)
                {
                    // Right click: hand out the endgame gear kit.
                    GrantEndgameGear(player);
                }
                else
                {
                    // Left click: buffs + max life/mana + reforge.
                    ApplyAllBuffs(player);
                    MaxOutLifeAndMana(player);
                    EnchantAllEquipment(player);
                }
            }
            return true;
        }

        private void GrantEndgameGear(Player player)
        {
            int granted = GrantKit(player, EndgameKit) + GrantKit(player, BossSummonKit);

            string key = granted > 0
                ? "Mods.OmniScepter.Messages.GearGranted"
                : "Mods.OmniScepter.Messages.GearAlreadyOwned";
            Main.NewText(Language.GetTextValue(key, granted), 255, 200, 60);
        }

        private int GrantKit(Player player, (int type, int stack)[] kit)
        {
            int granted = 0;
            foreach ((int type, int stack) in kit)
            {
                if (!PlayerOwns(player, type))
                {
                    player.QuickSpawnItem(player.GetSource_ItemUse(Item), type, stack);
                    granted++;
                }
            }
            return granted;
        }

        // Equivalent to consuming 15 Life Crystals, 20 Life Fruit and
        // 9 Mana Crystals: 500 max life and 200 max mana.
        private static void MaxOutLifeAndMana(Player player)
        {
            bool changed = player.ConsumedLifeCrystals < Player.LifeCrystalMax
                || player.ConsumedLifeFruit < Player.LifeFruitMax
                || player.ConsumedManaCrystals < Player.ManaCrystalMax;

            player.ConsumedLifeCrystals = Player.LifeCrystalMax;
            player.ConsumedLifeFruit = Player.LifeFruitMax;
            player.ConsumedManaCrystals = Player.ManaCrystalMax;

            // Also refill so the new capacity is immediately usable.
            player.statLife = player.statLifeMax2;
            player.statMana = player.statManaMax2;

            if (changed)
            {
                Main.NewText(
                    Language.GetTextValue("Mods.OmniScepter.Messages.LifeManaMaxed",
                        player.statLifeMax, player.statManaMax),
                    255, 200, 60);
            }
        }

        private static bool PlayerOwns(Player player, int type)
        {
            // Check equipped armor and accessories, then the inventory.
            foreach (Item item in player.armor)
            {
                if (item.type == type)
                {
                    return true;
                }
            }
            return player.HasItem(type);
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
