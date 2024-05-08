using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Nekoyume.Game;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using Nekoyume.State;
using Nekoyume.TableData;
using NineChronicles.Modules.BlockSimulation.ActionSimulators;
using NineChronicles.Modules.BlockSimulation.Extensions;
using UnityEngine;

namespace NineChronicles.Mods.Athena.GUIs
{
    public class AdventureGUI : IGUI
    {
        private bool _isCalculating;
        private int _selectedStageId;
        private int _simulationStep;
        private (WorldSheet WorldSheet, StageSheet StageSheet, int clearedStageId)? StateData { get; set; }
        private readonly Rect _simulateLayoutRect;

        private int _wave0ClearCount;
        private int _wave1ClearCount;
        private int _wave2ClearCount;
        private int _wave3ClearCount;
        private int _playCount = 100;

        private readonly GUIStyle _fs16 = new(GUI.skin.label)
        {
            fontSize = 16,
        };

        private readonly GUIStyle _fs16Center = new(GUI.skin.label)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };
        private readonly GUIStyle _leftMargin = new(GUI.skin.button)
        {
            margin = new RectOffset(50, 2, 30, 2)
        };

        private readonly IEnumerable<Equipment> _equippedEquipments;

        public AdventureGUI(IEnumerable<Equipment> equippedEquipments)
        {
            _equippedEquipments = equippedEquipments;
            InitStateData();

            _simulateLayoutRect = new Rect(
                100,
                100,
                GUIToolbox.ScreenWidthReference - 200,
                GUIToolbox.ScreenHeightReference - 100);
        }
        public void OnGUI()
        {
            if (StateData is not { } stateData)
            {
                return;
            }

            GUI.matrix = GUIToolbox.GetGUIMatrix();

            if (_selectedStageId == 0)
            {
                _selectedStageId = stateData.clearedStageId + 1;
            }

            using var areaScope = new GUILayout.AreaScope(_simulateLayoutRect);
            using var horizontalScope = new GUILayout.HorizontalScope();
            using var verticalScope = new GUILayout.VerticalScope(GUILayout.Width(400));
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("Choose stage to simulate", _fs16);
                    ControllablePicker(
                        stateData.StageSheet.Keys.Select(x => x.ToString()).ToArray(),
                        (_, index) => _selectedStageId = index + 1,
                        _selectedStageId - 1);

                    GUILayout.Label("Play Count", _fs16);
                    DrawPlayCountController();
                }

                GUI.enabled = !_isCalculating;
                if (GUILayout.Button("Simulate", _leftMargin, GUILayout.Width(80), GUILayout.Height(80)))
                {
                    SimulateLocal();
                    AthenaPlugin.Log($"[StageGUI] Simulate button clicked {_selectedStageId})");
                }
            }

            // NOTE: Use for testing with remote state.
            //if (GUILayout.Button("Simulate(Remote)"))
            //{
            //    SimulateRemote();
            //    AthenaPlugin.Log($"[StageGUI] Simulate button clicked {selectedStageId})");
            //}

            GUI.enabled = true;

            DrawSimulationResultTextArea();
        }

        private void InitStateData()
        {
            var tableSheets = TableSheets.Instance;
            var stId = States.Instance.CurrentAvatarState.worldInformation.TryGetLastClearedStageId(out var stageId)
                ? stageId
                : 0;
            StateData = (tableSheets.WorldSheet, tableSheets.StageSheet, stId);
        }

        private void DrawSimulationResultTextArea()
        {
            if (_isCalculating)
            {
                GUILayout.Label($"Simulating... {_simulationStep}/{_playCount}", _fs16);
            }
            else
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label($"Fail: {_wave0ClearCount}", _fs16, GUILayout.Width(60));
                    GUILayout.Label($"★: {_wave1ClearCount}", _fs16, GUILayout.Width(60));
                    GUILayout.Label($"★★: {_wave2ClearCount}", _fs16, GUILayout.Width(60));
                    GUILayout.Label($"★★★: {_wave3ClearCount}", _fs16, GUILayout.Width(60));
                }

                using (new GUILayout.HorizontalScope())
                {
                    var totalStars = _wave1ClearCount + _wave2ClearCount * 2 + _wave3ClearCount * 3;
                    GUILayout.Label($"Total Stars: {totalStars}", _fs16);
                }

                using (new GUILayout.HorizontalScope())
                {
                    var winRate = (float)_wave3ClearCount / _playCount;
                    GUILayout.Label($"Win Rate: {winRate:P2}", _fs16);
                }
            }
        }
        private void DrawPlayCountController()
        {
            using var verticalScope = new GUILayout.HorizontalScope();
            GUI.enabled = !_isCalculating;
            if (GUILayout.Button("+", GUILayout.Width(35), GUILayout.Height(35)))
            {
                _playCount += 100;
            }

            GUI.enabled = true;
            GUILayout.Label(_playCount + "", _fs16Center, GUILayout.Height(35));

            GUI.enabled = !_isCalculating;
            if (GUILayout.Button("-", GUILayout.Width(35), GUILayout.Height(35)))
            {
                if (_playCount >= 0)
                {
                    _playCount -= 100;
                }
            }

            GUI.enabled = true;
        }

        private void ControllablePicker(string[] list, Action<string[], int> onChanged, int index = 0)
        {
            using var horizontalScope = new GUILayout.HorizontalScope();
            Btn("<<", -5);
            Btn("<", -1);
            GUILayout.Label(list[index], _fs16Center, GUILayout.Height(35));
            Btn(">", 1);
            Btn(">>", 5);
            return;

            void Btn(string text, int change)
            {
                GUI.enabled = !_isCalculating;
                if (GUILayout.Button(text, GUILayout.Width(35), GUILayout.Height(35)))
                {
                    if (index + change > 0 && index + change < list.Length)
                    {
                        onChanged(list, index + change);
                    }
                }

                GUI.enabled = true;
            }
        }

        private async void SimulateLocal()
        {
            _isCalculating = true;
            _wave0ClearCount = -1;
            _wave1ClearCount = -1;
            _wave2ClearCount = -1;
            _wave3ClearCount = -1;
            _simulationStep = 0;

            var states = States.Instance;
            var (_, equippedCostumes) = states.GetEquippedItems(BattleType.Adventure);
            var clearWaveInfo = await UniTask.Run(() => HackAndSlashSimulator.Simulate(
                TableSheets.Instance,
                states.CurrentAvatarState,
                equipments: _equippedEquipments,
                costumes: equippedCostumes,
                consumables: null,
                allRuneState: states.AllRuneState,
                runeSlotState: states.CurrentRuneSlotStates[BattleType.Adventure],
                collectionState: states.CollectionState,
                gameConfigState: states.GameConfigState,
                worldId: _selectedStageId / 50,
                _selectedStageId,
                _playCount,
                stageBuffId: null,
                onProgress: step => _simulationStep = step,
                onLog: AthenaPlugin.Log));
            _wave0ClearCount = clearWaveInfo.GetValueOrDefault(0, 0);
            _wave1ClearCount = clearWaveInfo.GetValueOrDefault(1, 0);
            _wave2ClearCount = clearWaveInfo.GetValueOrDefault(2, 0);
            _wave3ClearCount = clearWaveInfo.GetValueOrDefault(3, 0);

            AthenaPlugin.Log($"[StageGUI] Simulate {_playCount}: w0 ({_wave0ClearCount}) w1({_wave1ClearCount}) w2({_wave2ClearCount}) w3({_wave3ClearCount})");
            _isCalculating = false;
        }

        private async void SimulateRemote()
        {
            _isCalculating = true;
            _wave0ClearCount = -1;
            _wave1ClearCount = -1;
            _wave2ClearCount = -1;
            _wave3ClearCount = -1;
            _simulationStep = 0;

            var states = States.Instance;
            var avatarAddress = states.CurrentAvatarState.address;
            var agent = Game.instance.Agent;
            var avatarDict = await agent.GetAvatarStatesAsync(new[] { avatarAddress });
            var avatarState = avatarDict[avatarAddress];
            var inventory = avatarState.inventory;
            var (equippedEquipments, equippedCostumes) = await agent.GetEquippedItemsAsync(
                avatarAddress,
                BattleType.Adventure,
                inventory: inventory);
            var collectionState = await agent.GetCollectionStateAsync(avatarAddress);
            var clearWaveInfo = await UniTask.Run(() => HackAndSlashSimulator.Simulate(
                TableSheets.Instance,
                avatarState,
                equipments: equippedEquipments,
                costumes: equippedCostumes,
                consumables: null,
                allRuneState: states.AllRuneState,
                runeSlotState: states.CurrentRuneSlotStates[BattleType.Adventure],
                collectionState: collectionState,
                gameConfigState: states.GameConfigState,
                worldId: _selectedStageId / 50,
                _selectedStageId,
                _playCount,
                stageBuffId: null,
                onProgress: step => _simulationStep = step,
                onLog: AthenaPlugin.Log));
            _wave0ClearCount = clearWaveInfo.GetValueOrDefault(0, 0);
            _wave1ClearCount = clearWaveInfo.GetValueOrDefault(1, 0);
            _wave2ClearCount = clearWaveInfo.GetValueOrDefault(2, 0);
            _wave3ClearCount = clearWaveInfo.GetValueOrDefault(3, 0);

            AthenaPlugin.Log($"[StageGUI] Simulate {_playCount}: w0 ({_wave0ClearCount}) w1({_wave1ClearCount}) w2({_wave2ClearCount}) w3({_wave3ClearCount})");
            _isCalculating = false;
        }
    }
}
