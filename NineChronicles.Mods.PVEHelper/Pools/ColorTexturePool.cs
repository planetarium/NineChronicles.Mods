using System.Collections.Generic;
using UnityEngine;

namespace NineChronicles.Mods.PVEHelper.Pools
{
    internal static class ColorTexturePool
    {
        private readonly struct CachedTexture
        {
            public readonly int Width;
            public readonly int Height;
            public readonly Color Color;
            public readonly Texture2D Texture;

            public CachedTexture(int width, int height, Color color, Texture2D texture)
            {
                Width = width;
                Height = height;
                Color = color;
                Texture = texture;
            }
        }

        private static readonly List<CachedTexture> _cachedTextures = new List<CachedTexture>();

        public static readonly Texture2D Black = Get(Color.black);
        public static readonly Texture2D Dark = Get(new Color(.1f, .1f, .1f));
        public static readonly Texture2D Gray = Get(Color.gray);
        public static readonly Texture2D White = Get(Color.white);
        public static readonly Texture2D Blue = Get(Color.blue);
        public static readonly Texture2D Green = Get(Color.green);

        public static Texture2D Get(Color color) => Get(1, 1, color);

        public static Texture2D Get(int width, int height, Color color)
        {
            foreach (var cachedTexture in _cachedTextures)
            {
                if (cachedTexture.Width == width &&
                    cachedTexture.Height == height &&
                    cachedTexture.Color == color)
                {
                    return cachedTexture.Texture;
                }
            }

            var texture = new Texture2D(width, height);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            _cachedTextures.Add(new CachedTexture(
                width,
                height,
                color,
                texture));
            return texture;
        }
    }
}
