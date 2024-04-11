using Nekoyume.Game;
using Nekoyume.Model.Item;
using NineChronicles.Mods.Athena.Extensions;
using NineChronicles.Mods.Athena.Factories;
using NineChronicles.Mods.Athena.Manager;
using NineChronicles.Mods.Athena.Models;
using UnityEngine;

namespace NineChronicles.Mods.Athena.Components
{
    public static class ArenaSimulateSlot
    {
        public static void DrawArenaCell(string avatarName, int cp)
        {
            var slotText = $"AvatarName {avatarName}" +
                $"\nCP: {cp}";
            var slotContent = new GUIContent(slotText);

            GUILayout.Box(slotContent, GUILayout.Width(200), GUILayout.Height(80));
        }
    }
}
