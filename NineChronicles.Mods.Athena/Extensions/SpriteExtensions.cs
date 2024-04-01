using UnityEngine;

namespace NineChronicles.Mods.Athena.Extensions
{
    public static class SpriteExtensions
    {
        public static Texture2D ToTexture2D(this Sprite sprite)
        {
            if (sprite.rect.width != sprite.texture.width ||
                sprite.rect.height != sprite.texture.height)
            {
                var originalTexture = sprite.texture;
                //var copy = new Texture2D(
                //    originalTexture.width,
                //    originalTexture.height,
                //    format: originalTexture.graphicsFormat,
                //    mipCount: originalTexture.mipmapCount,
                //    flags: UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
                var copy = new Texture2D(
                    originalTexture.width,
                    originalTexture.height,
                    originalTexture.format,
                    false);
                // [Error  : Unity Log] Graphics.CopyTexture called with mismatching mip counts (src 1 dst 12)
                Graphics.CopyTexture(originalTexture, copy);
                // Get the pixels from the original texture and apply them to the new texture
                Color[] newColors = copy.GetPixels(
                    (int)sprite.rect.x,
                    (int)sprite.rect.y,
                    (int)sprite.rect.width,
                    (int)sprite.rect.height);

                // Create a new empty texture with the sprite's size
                Texture2D resultTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                resultTexture.SetPixels(newColors);
                resultTexture.Apply();
                return resultTexture;
            }

            return sprite.texture;
        }
    }

}
