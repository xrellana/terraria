using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OmniScepter.Items
{
    // Shared plumbing for the mod's two items: both borrow a vanilla sprite,
    // craft from the same cheap recipe, split their behaviour across left and
    // right click and report back through the chat log.
    public abstract class OmniItemBase : ModItem
    {
        // Borrow a vanilla sprite so the mod works without custom art.
        // Replace with a real <ClassName>.png next to this file to use your own.
        public sealed override string Texture => $"Terraria/Images/Item_{BorrowedSprite}";

        protected abstract int BorrowedSprite { get; }

        // Colour this item's chat feedback is printed in.
        protected abstract Color MessageColor { get; }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Item4;
            Item.maxStack = 1;
            Item.noMelee = true;
        }

        // Enables right click as a second use mode.
        public override bool AltFunctionUse(Player player) => true;

        public sealed override bool? UseItem(Player player)
        {
            // Inventory and stat edits must only run for the player actually
            // using the item, never for the copies other clients simulate.
            if (player.whoAmI == Main.myPlayer)
            {
                if (player.altFunctionUse == 2)
                {
                    OnRightClick(player);
                }
                else
                {
                    OnLeftClick(player);
                }
            }
            return true;
        }

        protected abstract void OnLeftClick(Player player);

        protected abstract void OnRightClick(Player player);

        protected void Announce(string key, params object[] args)
        {
            string fullKey = $"Mods.OmniScepter.Messages.{key}";
            string text = args.Length == 0
                ? Language.GetTextValue(fullKey)
                : Language.GetTextValue(fullKey, args);
            Main.NewText(text, MessageColor);
        }

        public override void AddRecipes()
        {
            // Cheap on purpose: this is a quality-of-life / cheat mod.
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
