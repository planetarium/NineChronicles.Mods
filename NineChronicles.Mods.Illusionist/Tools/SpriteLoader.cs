using BepInEx.Logging;
using UnityEngine;

namespace NineChronicles.Mods.Illusionist.Tools
{
    public static class SpriteLoader
    {
        public static bool TryLoad(string filePath, out Sprite? sprite)
        {
            var log = $"{nameof(SpriteLoader)}.{nameof(TryLoad)}({nameof(filePath)}: {filePath})";
            IllusionistPlugin.Log(LogLevel.Info, log);
            if (TextureLoader.TryLoad(filePath, out var texture))
            {
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture!.width, texture.height),
                    pivot: new Vector2(.2f, .5f),
                    pixelsPerUnit: 100);
                // extrude: ,
                // meshType: ,
                // border: );
                IllusionistPlugin.Log(LogLevel.Info, $"{log} returns true");
                return true;
            }

            IllusionistPlugin.Log(LogLevel.Info, $"{log} returns false");
            sprite = null;
            return false;
        }

        public static bool TryLoad(string filePath, string spriteName, out Sprite? sprite)
        {
            if (TryLoad(filePath, out sprite))
            {
                sprite!.name = spriteName;
                return true;
            }

            return false;
        }
    }
}
