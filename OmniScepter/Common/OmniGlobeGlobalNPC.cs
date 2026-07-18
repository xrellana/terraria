using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using OmniScepter.Buffs;

namespace OmniScepter.Common
{
    // The personal blood moon cannot flip the global Main.bloodMoon flag without
    // affecting everyone, so instead it injects blood moon enemies into the spawn
    // pool of players carrying the climate.
    public class OmniGlobeGlobalNPC : GlobalNPC
    {
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            Player player = spawnInfo.Player;
            if (!player.HasBuff(ModContent.BuffType<BloodMoonGlobeBuff>()))
            {
                return;
            }
            // Blood moon creatures only roam the surface, like the real event.
            if (spawnInfo.SpawnTileY > Main.worldSurface)
            {
                return;
            }
            pool[NPCID.BloodZombie] = 0.6f;
            pool[NPCID.Drippler] = 0.4f;
        }
    }
}
