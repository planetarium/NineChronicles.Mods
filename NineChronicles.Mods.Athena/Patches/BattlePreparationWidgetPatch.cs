﻿using System;
using HarmonyLib;
using Nekoyume.UI;

namespace NineChronicles.Mods.Athena.Patches
{
    [HarmonyPatch(typeof(BattlePreparation))]
    internal class BattlePreparationWidgetPatch
    {
        public static event Action<(int worldId, int stageId)> OnShow;

        [HarmonyPostfix]
        [HarmonyPatch("Show")]
        public static void ShowPostfix(ref int ____worldId, ref int ____stageId)
        {
            AthenaPlugin.Log($"BattlePreparationWidgetPatch.ShowPostfix({____worldId}, {____stageId})");
            OnShow?.Invoke((____worldId, ____stageId));
        }
    }
}
