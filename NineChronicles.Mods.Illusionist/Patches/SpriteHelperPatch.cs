using System;
using System.IO;
using BepInEx.Logging;
using HarmonyLib;
using Nekoyume.Helper;
using NineChronicles.Mods.Illusionist.Tools;
using UnityEngine;

namespace NineChronicles.Mods.Illusionist.Patches
{
    [HarmonyPatch(typeof(SpriteHelper))]
    internal class SpriteHelperPatch
    {
        [HarmonyPatch(nameof(SpriteHelper.GetPlayerSpineTextureWeapon))]
        [HarmonyPrefix]
        public static bool GetPlayerSpineTextureWeaponPrefix(ref Sprite __result, int equipmentId)
        {
            var log =
                $"{nameof(SpriteHelperPatch)}.{nameof(GetPlayerSpineTextureWeaponPrefix)}({nameof(equipmentId)}: {equipmentId})";
            IllusionistPlugin.Log(LogLevel.Info, log);
            var filePath = Path.Combine(
                BepInEx.Paths.PluginPath,
                $"{Manifest.PluginName}/CharacterTextures/Weapons/{equipmentId}.png");
            try
            {
                if (SpriteLoader.TryLoad(filePath, equipmentId.ToString(), out var sprite))
                {
                    __result = sprite!;
                    IllusionistPlugin.Log(LogLevel.Info, $"{log} returns false");
                    return false;
                }
            }
            catch (Exception e)
            {
                IllusionistPlugin.Log(LogLevel.Error, e);
            }

            IllusionistPlugin.Log(LogLevel.Info, $"{log} returns true");
            return true;
        }
    }
}
