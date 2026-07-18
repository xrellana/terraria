using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using OmniScepter.Buffs;

namespace OmniScepter.Common
{
    // Weapon effects tied to the Omni Globe climates that need combat hooks.
    public class OmniGlobePlayer : ModPlayer
    {
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            ApplyClimateOnHit(target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            ApplyClimateOnHit(target);
        }

        private void ApplyClimateOnHit(NPC target)
        {
            // Personal snowfall coats your weapons in frost.
            if (Player.HasBuff(ModContent.BuffType<SnowGlobeBuff>()))
            {
                target.AddBuff(BuffID.Frostburn, 180);
            }
        }
    }
}
