using Nekoyume;
using Nekoyume.Model.Item;
using NineChronicles.Mods.PVEHelper.Pools;
using UnityEngine;

namespace NineChronicles.Mods.PVEHelper.Extensions
{
    internal static class ItemExtensions
    {
        public static Texture2D GetIcon(this IItem item)
        {
            if (item is not ItemBase itemBase)
            {
                return ItemTexturePool.Get(0);
            }

            return ItemTexturePool.Get(itemBase.Id);
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
