using System;
using UnityEngine;
using NineChronicles.Mods.Athena.Models;
using NineChronicles.Mods.Athena.GUIs;

namespace NineChronicles.Mods.Athena.Components
{
    public static class ArenaSimulateSlot
    {
        public static void DrawArenaSlot(ArenaGUI.AvatarInfo avatarInfo, Action<ArenaGUI.AvatarInfo> onSlotSelected, int totalPlayCount)
        {
            var slotText = $"AvatarName {avatarInfo.Name}" +
                $"\nCP: {avatarInfo.Cp}";
            if(avatarInfo.WinRate != -1)
            {
                slotText += $"\nWinRate: {Math.Floor(avatarInfo.WinRate * 100)}";
            }            
            if(avatarInfo.Progress != totalPlayCount - 1)
            {
                slotText += $"\n{avatarInfo.Progress}/{totalPlayCount}";
            }
            var slotContent = new GUIContent(slotText);

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(2,2,8,8)
            };

            if (GUILayout.Button(slotContent, buttonStyle))
            {
                onSlotSelected?.Invoke(avatarInfo);
            }
        }
    }
}
