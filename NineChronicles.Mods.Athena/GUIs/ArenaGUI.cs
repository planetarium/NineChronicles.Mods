using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Cysharp.Threading.Tasks;
using Libplanet.Crypto;
using Nekoyume.Game;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using Nekoyume.State;
using Nekoyume.UI;
using Nekoyume.UI.Model;
using NineChronicles.Mods.Athena.Components;
using NineChronicles.Modules.BlockSimulation.ActionSimulators;
using UnityEngine;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace NineChronicles.Mods.Athena.GUIs
{
    public class ArenaGUI : IGUI
    {
        public class AvatarInfo
        {
            public int Cp { get; set; }

            public string Name { get; set; }

            public Address Address { get; set; }

            public double WinRate { get; set; } = -1;

            public double Progress { get; set; } = 0;
        }

        public class BattleModel
        {
            public string avatarAddress { get; set; }

            public string enemyAddress { get; set; }
        }

        public class PercentageModel
        {
            public float winPercentage { get; set; }
        }

        private readonly Rect _arenaLayoutRect;

        private readonly List<AvatarInfo> _avatarInfos = new();
        private int _currentPage = 0;
        private int _itemsPerPage = 28;
        private int _totalPages;
        private int _playCount = 100;
        public event Action<AvatarInfo> OnSlotSelected;

        public ArenaGUI(IEnumerable<Equipment> equippedEquipments, AbilityRankingResponse apiResponse, bool LocalSimulation = true)
        {
            _arenaLayoutRect = new Rect(
                100,
                100,
                GUIToolbox.ScreenWidthReference - 200,
                GUIToolbox.ScreenHeightReference - 100);

            if (apiResponse != null)
            {
                foreach (var abilityRanking in apiResponse.AbilityRanking)
                {
                    var avatarInfo = new AvatarInfo
                    {
                        Name = abilityRanking.Name,
                        Cp = abilityRanking.Cp,
                        Address = new Address(abilityRanking.AvatarAddress),
                    };
                    _avatarInfos.Add(avatarInfo);
                }
            }

            _totalPages = (int)Math.Ceiling(_avatarInfos.Count / (double)_itemsPerPage);

            if (LocalSimulation)
            {
                OnSlotSelected += async avatarInfo =>
                {
                    try
                    {
                        AthenaPlugin.Log($"Simulate Start {avatarInfo.Address}");
                        var index = _avatarInfos.FindIndex((a) => a.Address == avatarInfo.Address);

                        if (index == -1)
                        {
                            return;
                        }

                        _avatarInfos[index].WinRate = -1;
                        _avatarInfos[index].Progress = 0;

                        var (_, equippedCostumes) = States.Instance.GetEquippedItems(BattleType.Arena);
                        var result = await UniTask.RunOnThreadPool(() => BattleArenaSimulator.ExecuteBulkAsync(
                            TableSheets.Instance,
                            States.Instance,
                            equippedEquipments,
                            equippedCostumes,
                            null,
                            avatarInfo.Address,
                            _playCount,
                            p => _avatarInfos[index].Progress = p,
                            AthenaPlugin.Log));
                        _avatarInfos[index].WinRate = result;
                        AthenaPlugin.Log($"Simulate Result = {result} updated for {_avatarInfos[index].Name}");
                    }
                    catch (Exception e)
                    {
                        AthenaPlugin.Log($"err {e}");
                    }
                };
            }
            else
            {
                OnSlotSelected += async avatarInfo =>
                {
                    bool heimdall = false;
                    if(Game.instance.CurrentPlanetId.ToString() == Nekoyume.Multiplanetary.PlanetId.Heimdall.ToString())
                        heimdall = true;

                    try
                    {
                        AthenaPlugin.Log($"Simulate Start {avatarInfo.Address}");
                        var index = _avatarInfos.FindIndex((a) => a.Address == avatarInfo.Address);

                        if (index == -1)
                        {
                            return;
                        }

                        _avatarInfos[index].WinRate = -1;
                        _avatarInfos[index].Progress = 0;

                        var result = await Simulate(avatarInfo.Address.ToString(), _avatarInfos[index].Address.ToString(), heimdall);
                        _avatarInfos[index].WinRate = result/100;
                        _playCount = 100;
                        AthenaPlugin.Log($"9CAPI - Simulate Result = {result} updated for {_avatarInfos[index].Name}");
                    }
                    catch (Exception e)
                    {
                        AthenaPlugin.Log($"err {e}");
                    }
                };
            }
        }

        public void OnGUI()
        {
            GUI.matrix = GUIToolbox.GetGUIMatrix();

            using var areaScope = new GUILayout.AreaScope(_arenaLayoutRect);
            var startIndex = _currentPage * _itemsPerPage;
            var endIndex = Math.Min(startIndex + _itemsPerPage, _avatarInfos.Count);
            var currentPageAvatars = _avatarInfos.GetRange(startIndex, endIndex - startIndex);
            ArenaSimulateBoard.DrawArenaBoard(currentPageAvatars, OnSlotSelected, _playCount);

            using var horizontalScope = new GUILayout.HorizontalScope();
            if (GUILayout.Button("Prev"))
            {
                if (_currentPage > 0) _currentPage--;
                AthenaPlugin.Log($"Prev Page: {_currentPage}, {startIndex} - {endIndex - startIndex}");
            }

            if (GUILayout.Button("Next"))
            {
                if (_currentPage < _totalPages - 1) _currentPage++;
                AthenaPlugin.Log($"Next Page: {_currentPage}, {startIndex} - {endIndex - startIndex}");
            }
        }

        private static async Task<double> Simulate(string avatar, string enemy, bool heimdall)
        {
            string apiUrl = "https://api.9capi.com/arenaSimOdin";
            if (heimdall)
                apiUrl = "https://api.9capi.com/arenaSimHeimdall";
            AthenaPlugin.Log($"9CAPI Preparing Simulation against {apiUrl}");
            var client = new HttpClient();
            var battleModel = new BattleModel();
            battleModel.avatarAddress = avatar;
            battleModel.enemyAddress = enemy;
            string jsonString = JsonSerializer.Serialize(battleModel);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl, content);

            try
            {
                var result = JsonSerializer.Deserialize<PercentageModel>(await response.Content.ReadAsStringAsync());
                return (double)Math.Round(result.winPercentage, 2);
            }
            catch (Exception ex)
            {
                NotificationSystem.Push(
                    Nekoyume.Model.Mail.MailType.System,
                    "9CAPI Simulator is currently unavailable",
                    Nekoyume.UI.Scroller.NotificationCell.NotificationType.Notification);
                return 0;
            }
        }
    }
}
