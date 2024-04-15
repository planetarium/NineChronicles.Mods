using System;
using UnityEngine;
using NineChronicles.Mods.Athena.Models;

namespace NineChronicles.Mods.Athena.Components
{
    public static class ArenaSimulateSlot
    {
        public static void DrawArenaSlot(AvatarInfo avatarInfo, Action<AvatarInfo> onSlotSelected)
        {
            var slotText = $"AvatarName {avatarInfo.Name}" +
                $"\nCP: {avatarInfo.Cp}";
            if(avatarInfo.WinRate != -1)
            {
                slotText += $"\nWinRate: {Math.Floor(avatarInfo.WinRate * 100)}";
            }
            var slotContent = new GUIContent(slotText);

            if (GUILayout.Button(slotContent, GUILayout.Width(200), GUILayout.Height(80)))
            {
                onSlotSelected?.Invoke(avatarInfo);
            }
        }
    }
}
