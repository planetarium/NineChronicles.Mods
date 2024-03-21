using System.Collections.Generic;
using Nekoyume.Helper;
using NineChronicles.Mods.PVEHelper.Extensions;
using UnityEngine;

namespace NineChronicles.Mods.PVEHelper.Pools
{
    internal static class ItemTexturePool
    {
        private static Dictionary<int, Texture2D> _pool = new Dictionary<int, Texture2D>();

        public static Texture2D Get(int itemId)
        {
            if (_pool.TryGetValue(itemId, out var texture))
            {
                return texture;
            }

            texture = SpriteHelper.GetItemIcon(itemId).ToTexture2D();
            _pool[itemId] = texture;
            return texture;
        }
    }
}
