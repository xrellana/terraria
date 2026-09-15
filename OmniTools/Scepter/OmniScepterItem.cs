using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using OmniTools.Common;

namespace OmniTools.Scepter
{
    public class OmniScepterItem : OmniItemBase
    {
        protected override int BorrowedSprite => ItemID.RainbowRod;

        protected override Color MessageColor => new Color(255, 200, 60);

        // Two hours of real time per click (60 ticks per second).
        private const int BuffDuration = 60 * 60 * 120;

        // A prefix id of 0 is vanilla's "no prefix", which doubles here as
        // "this item cannot be reforged".
        private const int NoPrefix = 0;

        // Vanilla stat caps, expressed the way the crystals build them up.
        private const int BaseLife = 100;
        private const int LifePerCrystal = 20;
        private const int LifePerFruit = 5;
        private const int BaseMana = 20;
        private const int ManaPerCrystal = 20;

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
            base.SetDefaults();
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.rare = ItemRarityID.Red;
            Item.value = Terraria.Item.sellPrice(gold: 10);
        }

        // Left click: buffs + max life/mana + reforge.
        protected override void OnLeftClick(Player player)
        {
            ApplyAllBuffs(player);
            MaxOutLifeAndMana(player);
            EnchantAllEquipment(player);
        }

        // Right click: hand out the endgame gear kit.
        protected override void OnRightClick(Player player)
        {
            int granted = GrantKit(player, EndgameKit) + GrantKit(player, BossSummonKit);
            Announce(granted > 0 ? "GearGranted" : "GearAlreadyOwned", granted);
        }

        private int GrantKit(Player player, (int type, int stack)[] kit)
        {
            int granted = 0;
            foreach ((int type, int stack) in kit)
            {
                // Top up to the stack the kit asks for instead of skipping the
                // entry outright, so owning one Luminite Bullet does not cost
                // you the other 998.
                int missing = stack - CountOwned(player, type, stack);
                if (missing > 0)
                {
                    player.QuickSpawnItem(player.GetSource_ItemUse(Item), type, missing);
                    granted++;
                }
            }
            return granted;
        }

        // Counts equipped pieces as well as carried ones, so wearing the Ankh
        // Shield still counts as owning it. Stops early once the kit's stack is
        // covered, which is all the caller needs to know.
        private static int CountOwned(Player player, int type, int stopCountingAt)
        {
            int count = 0;
            foreach (Item item in player.armor)
            {
                if (item.type == type)
                {
                    count += item.stack;
                    if (count >= stopCountingAt)
                    {
                        return count;
                    }
                }
            }
            return count + player.CountItem(type, stopCountingAt - count);
        }

        // Equivalent to consuming 15 Life Crystals, 20 Life Fruit and
        // 9 Mana Crystals: 500 max life and 200 max mana.
        private void MaxOutLifeAndMana(Player player)
        {
            int targetLife = BaseLife
                + Player.LifeCrystalMax * LifePerCrystal
                + Player.LifeFruitMax * LifePerFruit;
            int targetMana = BaseMana + Player.ManaCrystalMax * ManaPerCrystal;

            bool changed = player.statLifeMax < targetLife
                || player.statManaMax < targetMana
                || player.ConsumedLifeCrystals < Player.LifeCrystalMax
                || player.ConsumedLifeFruit < Player.LifeFruitMax
                || player.ConsumedManaCrystals < Player.ManaCrystalMax;

            // The Consumed* counters only record how many crystals were eaten;
            // the caps themselves live in statLifeMax / statManaMax, so both
            // halves have to be written or the cap never actually moves.
            // Max() leaves a higher cap set by another mod alone.
            player.statLifeMax = Math.Max(player.statLifeMax, targetLife);
            player.statManaMax = Math.Max(player.statManaMax, targetMana);
            player.ConsumedLifeCrystals = Player.LifeCrystalMax;
            player.ConsumedLifeFruit = Player.LifeFruitMax;
            player.ConsumedManaCrystals = Player.ManaCrystalMax;

            // statLifeMax2 is only recomputed on the next frame, so raise it
            // here too; otherwise the refill below clamps to the old cap.
            player.statLifeMax2 = Math.Max(player.statLifeMax2, player.statLifeMax);
            player.statManaMax2 = Math.Max(player.statManaMax2, player.statManaMax);
            player.statLife = player.statLifeMax2;
            player.statMana = player.statManaMax2;

            if (changed)
            {
                Announce("LifeManaMaxed", player.statLifeMax, player.statManaMax);
            }
        }

        private static void ApplyAllBuffs(Player player)
        {
            foreach (int buff in Buffs)
            {
                player.AddBuff(buff, BuffDuration);
            }
        }

        private void EnchantAllEquipment(Player player)
        {
            int reforged = 0;

            // Vanilla prefixes only exist on weapons and accessories;
            // armor pieces (slots 0-2) cannot be reforged, so skip them.
            for (int i = 3; i < 10; i++)
            {
                if (TryApplyPrefix(player.armor[i], PrefixID.Menacing))
                {
                    reforged++;
                }
            }

            for (int i = 0; i < Main.InventorySlotsTotal; i++)
            {
                int prefix = PreferredPrefix(player.inventory[i]);
                if (prefix != NoPrefix && TryApplyPrefix(player.inventory[i], prefix))
                {
                    reforged++;
                }
            }

            Announce(reforged > 0 ? "Reforged" : "NothingToReforge", reforged);
        }

        // Returns NoPrefix for anything that cannot take one.
        private static int PreferredPrefix(Item item)
        {
            if (item.IsAir)
            {
                return NoPrefix;
            }
            if (item.accessory)
            {
                return PrefixID.Menacing;
            }
            if (item.damage > 0 && item.maxStack == 1)
            {
                return BestWeaponPrefix(item);
            }
            return NoPrefix;
        }

        private static int BestWeaponPrefix(Item item)
        {
            if (item.CountsAsClass(DamageClass.Ranged))
            {
                return PrefixID.Unreal;
            }
            // Whips scale with melee speed and take melee prefixes since 1.4.4,
            // so check them before the generic summon class they inherit from.
            if (item.CountsAsClass(DamageClass.SummonMeleeSpeed))
            {
                return PrefixID.Legendary;
            }
            // Minions cannot crit, so raw damage beats Mythical for summon staffs.
            if (item.CountsAsClass(DamageClass.Summon))
            {
                return PrefixID.Ruthless;
            }
            if (item.CountsAsClass(DamageClass.Magic))
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

            // Reforging works like vanilla: reset the item, then roll the prefix.
            // These loader hooks are what let modded weapons carry their own
            // saved data across that reset, exactly as the Goblin Tinkerer does,
            // and let one refuse to be reforged at all.
            if (!ItemLoader.PreReforge(item))
            {
                return false;
            }

            int originalPrefix = item.prefix;
            bool favorited = item.favorited;
            int stack = item.stack;

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

            ItemLoader.PostReforge(item);

            // SetDefaults wiped these; put them back the way vanilla reforging does.
            item.favorited = favorited;
            item.stack = stack;
            return item.prefix != originalPrefix;
        }
    }
}
