using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OmniTools.Globe
{
    // Base class for the Omni Globe personal climates. Each climate is a buff so
    // that vanilla handles multiplayer sync, saving and the on-screen icon for
    // free; right-clicking the icon dispels the climate like any other buff.
    public abstract class OmniGlobeBuff : ModBuff
    {
        // How long a freshly shaken climate lasts. Update alone keeps topping it
        // back up, so the only thing this really controls is how long the climate
        // survives if Update ever stops running (loading screens, death, reload).
        // The item applies it and Update refreshes it, so both must agree.
        public const int ClimateDuration = 18000;

        // Borrow a fitting vanilla buff icon so the mod works without custom art.
        public override string Texture => $"Terraria/Images/Buff_{VanillaIconBuff}";

        protected abstract int VanillaIconBuff { get; }

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public sealed override void Update(Player player, ref int buffIndex)
        {
            // Keep the climate active until it is dispelled manually.
            player.buffTime[buffIndex] = ClimateDuration;
            UpdateClimate(player);
        }

        protected abstract void UpdateClimate(Player player);

        // Purely cosmetic weather particles around the player.
        protected static void AmbientDust(Player player, int dustType, Vector2 velocity, int amount = 2)
        {
            if (Main.dedServ)
            {
                return;
            }
            for (int i = 0; i < amount; i++)
            {
                Vector2 position = player.Center + new Vector2(
                    Main.rand.NextFloat(-220f, 220f),
                    Main.rand.NextFloat(-180f, -40f));
                Dust dust = Dust.NewDustPerfect(position, dustType, velocity);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.3f);
            }
        }
    }

    // Snow: counts as snow biome, attacks inflict Frostburn (see OmniGlobePlayer).
    public class SnowGlobeBuff : OmniGlobeBuff
    {
        protected override int VanillaIconBuff => BuffID.Frostburn;

        protected override void UpdateClimate(Player player)
        {
            player.ZoneSnow = true;
            AmbientDust(player, DustID.Snow, new Vector2(0f, 2f));
        }
    }

    // Sandstorm: counts as desert + sandstorm, wind speeds up your attacks.
    public class SandstormGlobeBuff : OmniGlobeBuff
    {
        protected override int VanillaIconBuff => BuffID.Swiftness;

        protected override void UpdateClimate(Player player)
        {
            player.ZoneDesert = true;
            player.ZoneSandstorm = true;
            player.GetAttackSpeed(DamageClass.Generic) += 0.15f;
            AmbientDust(player, DustID.Sand, new Vector2(5f, 0.6f), 3);
        }
    }

    // Blood moon: raw damage up; blood creatures hunt you (see OmniGlobeGlobalNPC).
    public class BloodMoonGlobeBuff : OmniGlobeBuff
    {
        protected override int VanillaIconBuff => BuffID.Wrath;

        protected override void UpdateClimate(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.15f;
            AmbientDust(player, DustID.Blood, new Vector2(0f, 1.5f), 1);
        }
    }

    // Glowing mushroom: counts as glowshroom biome, strong regeneration and light.
    public class GlowshroomGlobeBuff : OmniGlobeBuff
    {
        protected override int VanillaIconBuff => BuffID.Shine;

        protected override void UpdateClimate(Player player)
        {
            player.ZoneGlowshroom = true;
            player.lifeRegen += 6;
            Lighting.AddLight(player.Center, 0.3f, 0.6f, 1f);
            AmbientDust(player, DustID.BlueTorch, Vector2.Zero, 1);
        }
    }

    // Hallow: counts as hallow biome, blessed precision raises crit chance.
    public class HallowGlobeBuff : OmniGlobeBuff
    {
        protected override int VanillaIconBuff => BuffID.Rage;

        protected override void UpdateClimate(Player player)
        {
            player.ZoneHallow = true;
            player.GetCritChance(DamageClass.Generic) += 10f;
            AmbientDust(player, DustID.PinkTorch, Vector2.Zero, 1);
        }
    }
}
