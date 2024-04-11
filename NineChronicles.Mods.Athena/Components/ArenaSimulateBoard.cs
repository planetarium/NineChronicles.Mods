using System.Collections.Generic;
using Nekoyume.Game;
using Nekoyume.Model.Item;
using NineChronicles.Mods.Athena.Extensions;
using NineChronicles.Mods.Athena.Factories;
using NineChronicles.Mods.Athena.Manager;
using NineChronicles.Mods.Athena.Models;
using UnityEngine;

namespace NineChronicles.Mods.Athena.Components
{
    public static class ArenaSimulateBoard
    {
        public static void DrawArenaBoard(List<AvatarInfo> arenaAvatarInfos)
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
                            ArenaSimulateSlot.DrawArenaCell(avatarInfo.Name, avatarInfo.Cp);
                        }
                    }
                }
            }
        }
    }
}
