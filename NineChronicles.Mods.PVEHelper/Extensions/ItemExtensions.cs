using Nekoyume;
using Nekoyume.Helper;
using Nekoyume.Model.Item;
using UnityEngine;

namespace NineChronicles.Mods.PVEHelper.Extensions
{
    internal static class ItemExtensions
    {
        public static Sprite GetIcon(this IItem item)
        {
            if (item is not ItemBase itemBase)
            {
                return SpriteHelper.GetItemIcon(0);
            }

            return SpriteHelper.GetItemIcon(itemBase.Id);
        }

        public static string GetName(this IItem item)
        {
            if (item is not ItemBase itemBase)
            {
                return "Unknown";
            }

            return itemBase.GetLocalizedNonColoredName(useElementalIcon: false);
        }
    }
}
