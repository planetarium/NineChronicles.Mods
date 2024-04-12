using System;
using UnityEngine;
using NineChronicles.Mods.Athena.Models;
using System.Collections.Generic;

namespace NineChronicles.Mods.Athena.Components
{
    public static class ArenaSimulateBoard
    {
        public static void DrawArenaBoard(List<AvatarInfo> arenaAvatarInfos, Action<AvatarInfo> onSlotSelected)
        {
            int maxItems = Mathf.Min(arenaAvatarInfos.Count, 20);

            int itemsPerGroup = 5;

            using (var horizontalScope = new GUILayout.HorizontalScope())
            {
                for(int i = 0; i < maxItems; i += itemsPerGroup)
                {
                    using (var verticalScope = new GUILayout.VerticalScope())
                    {
                        for(int j = i; j < Mathf.Min(i + itemsPerGroup, maxItems); j++)
                        {
                            var avatarInfo = arenaAvatarInfos[j];
                            ArenaSimulateSlot.DrawArenaSlot(avatarInfo, onSlotSelected);
                        }
                    }
                }
            }
        }
    }
}
