using System;
using System.IO;
using BepInEx.Logging;
using UnityEngine;

namespace NineChronicles.Mods.Illusionist.Tools
{
    public static class TextureLoader
    {
        public static bool TryLoad(string filePath, out Texture2D? texture)
        {
            var log = $"{nameof(TextureLoader)}.{nameof(TryLoad)}({nameof(filePath)}: {filePath})";
            IllusionistPlugin.Log(LogLevel.Info, log);
            try
            {
                if (File.Exists(filePath))
                {
                    var byteData = File.ReadAllBytes(filePath);
                    texture = Deserialize(byteData);
                    IllusionistPlugin.Log(LogLevel.Info, $"{log} returns true");
                    return true;
                }
            }
            catch (Exception e)
            {
                IllusionistPlugin.Log(LogLevel.Error, e);
                texture = null;
                return false;
            }

            IllusionistPlugin.Log(LogLevel.Info, $"{log} returns false");
            texture = null;
            return false;
        }

        private static Texture2D Deserialize(byte[] data)
        {
            var tex = new Texture2D(2, 2);
            tex.LoadImage(data);
            return tex;
        }
    }
}
