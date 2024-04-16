using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Nekoyume.Game;
using Nekoyume.State;
using Nekoyume.UI.Model;
using NineChronicles.Mods.Athena.Components;
using NineChronicles.Mods.Athena.Manager;
using NineChronicles.Mods.Athena.Models;
using NineChronicles.Modules.BlockSimulation.ActionSimulators;
using UnityEngine;

namespace NineChronicles.Mods.Athena.GUIs
{
    public class ArenaGUI : IGUI
    {
        private readonly Rect _arenaLayoutRect;

        private ModInventoryManager _modInventoryManager;
        public GUIContent SlotContent = new GUIContent();

        public List<AvatarInfo> avatarInfos = new List<AvatarInfo>();
        private int currentPage = 0;
        private int itemsPerPage = 20;
        private int totalPages;

        public event Action<AvatarInfo> OnSlotSelected;

        public ArenaGUI(ModInventoryManager modInventoryManager)
        {
            _modInventoryManager = modInventoryManager;

            _arenaLayoutRect = new Rect(
                100,
                100,
                GUIToolbox.ScreenWidthReference - 200,
                GUIToolbox.ScreenHeightReference - 100);
            LoadRank();

            OnSlotSelected += async avatarInfo =>
            {
                try
                {
                    AthenaPlugin.Log($"Simulate Start {avatarInfo.Address}");

                    var result = await UniTask.Run(() => BattleArenaSimulator.ExecuteBulk(
                        TableSheets.Instance,
                        States.Instance,
                        _modInventoryManager.GetEquippedEquipments(),
                        avatarInfo.Address,
                        100,
                        (l) => AthenaPlugin.Log(l)
                    ));
                    var index = avatarInfos.FindIndex((a) => a.Address == avatarInfo.Address);
                    if (index != -1)
                    {
                        avatarInfos[index].WinRate = result;
                        AthenaPlugin.Log($"Simulate Result = {result} updated for {avatarInfos[index].Name}");
                    }

                    AthenaPlugin.Log($"Simulate Result = {result}");
                }
                catch (Exception e)
                {
                    AthenaPlugin.Log($"err {e}");
                }
            };
        }

        private async Task LoadRank()
        {
            var apiClient = Game.instance.ApiClient;

            if (apiClient.IsInitialized)
            {
                var query =
                    $@"query {{
                            abilityRanking(limit: 1000) {{
                                ranking
                                avatarAddress
                                name
                                avatarLevel
                                armorId
                                titleId
                                cp
                            }}
                        }}";

                var response = await apiClient.GetObjectAsync<AbilityRankingResponse>(query);
                if (response is null)
                {
                    AthenaPlugin.Log($"Failed getting response : {nameof(AbilityRankingResponse)}");
                    return;
                }

                foreach (var abilityRanking in response.AbilityRanking)
                {
                    var avatarInfo = new AvatarInfo
                    {
                        Name = abilityRanking.Name,
                        Cp = abilityRanking.Cp,
                        Address = new Libplanet.Crypto.Address(abilityRanking.AvatarAddress),
                    };
                    avatarInfos.Add(avatarInfo);
                }
            }

            totalPages = (int)Math.Ceiling(avatarInfos.Count / (double)itemsPerPage);
        }

        public void OnGUI()
        {
            GUI.matrix = GUIToolbox.GetGUIMatrix();

            using (var areaScope = new GUILayout.AreaScope(_arenaLayoutRect))
            {
                int startIndex = currentPage * itemsPerPage;
                int endIndex = Math.Min(startIndex + itemsPerPage, avatarInfos.Count);
                var currentPageAvatars = avatarInfos.GetRange(startIndex, endIndex - startIndex);

                ArenaSimulateBoard.DrawArenaBoard(currentPageAvatars, OnSlotSelected);

                using (var horizontalScope = new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Prev"))
                    {
                        if (currentPage > 0) currentPage--;
                        AthenaPlugin.Log($"Prev Page: {currentPage}, {startIndex} - {endIndex - startIndex}");
                    }
                    if (GUILayout.Button("Next"))
                    {
                        if (currentPage < totalPages - 1) currentPage++;
                        AthenaPlugin.Log($"Next Page: {currentPage}, {startIndex} - {endIndex - startIndex}");
                    }
                }
            }
        }
    }
}
