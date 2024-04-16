using System;
using UnityEngine;
using NineChronicles.Mods.Athena.Models;
using System.Collections.Generic;
using Nekoyume.Model.State;
using NineChronicles.Mods.Athena.GUIs;

namespace NineChronicles.Mods.Athena.Components
{
    public static class ArenaSimulateBoard
    {
        public static void DrawArenaBoard(List<ArenaGUI.AvatarInfo> arenaAvatarInfos, Action<ArenaGUI.AvatarInfo> onSlotSelected, int totalPlayCount)
        {
            int itemsPerGroup = 5;

            using (var horizontalScope = new GUILayout.HorizontalScope())
            {
                for(int i = 0; i < arenaAvatarInfos.Count; i += itemsPerGroup)
                {
                    using (var verticalScope = new GUILayout.VerticalScope())
                    {
                        for(int j = i; j < Mathf.Min(i + itemsPerGroup, arenaAvatarInfos.Count); j++)
                        {
                            var avatarInfo = arenaAvatarInfos[j];
                            ArenaSimulateSlot.DrawArenaSlot(avatarInfo, onSlotSelected, totalPlayCount);
                        }
                    }
                }
            }
        }
    }
}
