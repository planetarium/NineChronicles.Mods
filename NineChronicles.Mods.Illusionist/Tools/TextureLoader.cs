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
                IllusionistPlugin.Log(LogLevel.Error, $"{log} thrown exception: {e.Message}");
                IllusionistPlugin.Log(LogLevel.Error, e);
                IllusionistPlugin.Log(LogLevel.Info, $"{log} returns false");
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

        private static Texture2D Deserialize2(byte[] data)
        {
            var tex = new Texture2D(2, 2);
            tex.LoadImage(data);
            var pixels = tex.GetPixels(); // get pixel colors
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i].a = pixels[i].grayscale; // set the alpha of each pixel to the grayscale value
            }

            tex.SetPixels(pixels); // set changed pixel alphas
            tex.Apply(); // upload texture to GPU
            return tex;
        }

        private static Texture2D DuplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
    }
}
