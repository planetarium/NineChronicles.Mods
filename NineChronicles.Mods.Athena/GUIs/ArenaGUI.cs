using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NineChronicles.Mods.Athena.Components;
using NineChronicles.Modules.BlockSimulation.ActionSimulators;
using NineChronicles.Mods.Athena.Models;
using Nekoyume.UI.Model;
using Nekoyume.Game;
using Nekoyume.State;
using Nekoyume.Model.Item;
using UnityEngine;

namespace NineChronicles.Mods.Athena.GUIs
{
    public class ArenaGUI : IGUI
    {
        private readonly Rect _arenaLayoutRect;

        public GUIContent SlotContent = new GUIContent();

        public List<AvatarInfo> avatarInfos = new List<AvatarInfo>();

        public event Action<AvatarInfo> OnSlotSelected;

        public ArenaGUI()
        {
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
                        new List<Equipment>(),
                        avatarInfo.Address,
                        10,
                        (l) => AthenaPlugin.Log(l)
                    ));
                    var index = avatarInfos.FindIndex((a) => a.Address == avatarInfo.Address);
                    if (index != -1)
                    {
                        avatarInfos[index].WinRate = result; 
                        AthenaPlugin.Log($"Simulate Result = {result} updated for {avatarInfos[index].Name}");
                    }
    
                    AthenaPlugin.Log($"Simulate Result = {result}");
                } catch (Exception e)
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
                            abilityRanking(limit: 20) {{
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
        }

        public void OnGUI()
        {
            GUI.matrix = GUIToolbox.GetGUIMatrix();

            using (var areaScope = new GUILayout.AreaScope(_arenaLayoutRect))
            {
                ArenaSimulateBoard.DrawArenaBoard(avatarInfos, OnSlotSelected);
            }
        }
    }
}
