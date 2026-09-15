using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using OmniScepter.Buffs;

namespace OmniScepter.Items
{
    public class OmniGlobeItem : OmniItemBase
    {
        protected override int BorrowedSprite => ItemID.SnowGlobe;

        protected override Color MessageColor => new Color(150, 220, 255);

        // The personal climates the globe can be shaken into. Built once, after
        // every buff has been registered, rather than on each click.
        private static ModBuff[] climates;

        public override void SetStaticDefaults()
        {
            climates = new ModBuff[]
            {
                ModContent.GetInstance<SnowGlobeBuff>(),
                ModContent.GetInstance<SandstormGlobeBuff>(),
                ModContent.GetInstance<BloodMoonGlobeBuff>(),
                ModContent.GetInstance<GlowshroomGlobeBuff>(),
                ModContent.GetInstance<HallowGlobeBuff>(),
            };
        }

        public override void Unload()
        {
            // tModLoader reloads mods in-process, so static references to modded
            // content have to be dropped or the old assembly leaks.
            climates = null;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 28;
            Item.height = 28;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.rare = ItemRarityID.Blue;
            Item.value = Terraria.Item.sellPrice(silver: 10);
        }

        protected override void OnLeftClick(Player player)
        {
            ShakeClimate(player);
        }

        // Right click: calm the climate.
        protected override void OnRightClick(Player player)
        {
            ClearClimates(player, announce: true);
        }

        private void ShakeClimate(Player player)
        {
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
            player.AddBuff(climates[next].Type, OmniGlobeBuff.ClimateDuration);
            Announce("GlobeMode", climates[next].DisplayName.Value);
        }

        private void ClearClimates(Player player, bool announce)
        {
            foreach (ModBuff climate in climates)
            {
                player.ClearBuff(climate.Type);
            }
            if (announce)
            {
                Announce("GlobeOff");
            }
        }
    }
}
