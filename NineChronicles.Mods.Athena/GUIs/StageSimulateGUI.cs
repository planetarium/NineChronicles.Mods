using System;
using System.Linq;
using System.Threading.Tasks;
using Bencodex.Types;
using BepInEx.Logging;
using Cysharp.Threading.Tasks;
using Nekoyume;
using Nekoyume.Game;
using Nekoyume.Model.Item;
using Nekoyume.State;
using Nekoyume.TableData;
using NineChronicles.Mods.Athena.Extensions;
using NineChronicles.Mods.Athena.Manager;
using NineChronicles.Modules.BlockSimulation.ActionSimulators;
using UnityEngine;

namespace NineChronicles.Mods.Athena.GUIs
{
    public class StageSimulateGUI : IGUI
    {
        private bool _isCalculating;
        private int selectedStageId = 0;
        private int simulationStep = 0;
        private (WorldSheet WorldSheet, StageSheet StageSheet, int clearedStageId)? StateData { get; set; } = null;
        private DateTimeOffset? LastSheetsUpdated { get; set; } = null;
        private readonly Rect _selectLayoutRect;
        private readonly Rect _simulateLayoutRect;

        private ModInventoryManager _modInventoryManager;

        private InventoryGUI _inventoryGUI;

        public Equipment SelectedAura { get; set; }
        public Equipment SelectedWeapon { get; set; }
        public Equipment SelectedArmor { get; set; }
        public Equipment SelectedBelt { get; set; }
        public Equipment SelectedNecklace { get; set; }
        public Equipment SelectedRing1 { get; set; }
        public Equipment SelectedRing2 { get; set; }

        public GUIContent SelectedAuraContent = new GUIContent("Aura");
        public GUIContent SelectedWeaponContent = new GUIContent("Weapon");
        public GUIContent SelectedArmorContent = new GUIContent("Armor");
        public GUIContent SelectedBeltContent = new GUIContent("Belt");
        public GUIContent SelectedNecklaceContent = new GUIContent("Necklace");
        public GUIContent SelectedRing1Content = new GUIContent("Ring1");
        public GUIContent SelectedRing2Content = new GUIContent("Ring2");

        private int _wave0ClearCount = 0;
        private int _wave1ClearCount = 0;
        private int _wave2ClearCount = 0;
        private int _wave3ClearCount = 0;
        private int playCount = 100;

        public StageSimulateGUI(ModInventoryManager modInventoryManager, InventoryGUI inventoryGUI)
        {
            _modInventoryManager = modInventoryManager;
            _inventoryGUI = inventoryGUI;

            InitStateData();
            InitEquipments();

            _inventoryGUI.OnSlotSelected += tuple =>
            {
                if (tuple.item is Equipment equipment)
                {
                    switch (equipment.ItemSubType)
                    {
                        case ItemSubType.Weapon:
                            _modInventoryManager.SelectedWeapon = equipment;
                            SelectedWeapon = equipment;
                            SelectedWeaponContent = CreateSlotText(equipment);
                            AthenaPlugin.Log(LogLevel.Info, $"({nameof(StageSimulateGUI)}) Selected weapon {equipment.GetName()} {equipment.ItemId} {equipment.level}");
                            break;
                        case ItemSubType.Armor:
                            _modInventoryManager.SelectedArmor = equipment;
                            SelectedArmor = equipment;
                            SelectedArmorContent = CreateSlotText(equipment);
                            AthenaPlugin.Log(LogLevel.Info, $"({nameof(StageSimulateGUI)}) Selected armor {equipment.GetName()} {equipment.ItemId} {equipment.level}");
                            break;
                        case ItemSubType.Belt:
                            _modInventoryManager.SelectedBelt = equipment;
                            SelectedBelt = equipment;
                            SelectedBeltContent = CreateSlotText(equipment);
                            AthenaPlugin.Log(LogLevel.Info, $"({nameof(StageSimulateGUI)}) Selected belt {equipment.GetName()} {equipment.ItemId} {equipment.level}");
                            break;
                        case ItemSubType.Necklace:
                            _modInventoryManager.SelectedNecklace = equipment;
                            SelectedNecklace = equipment;
                            SelectedNecklaceContent = CreateSlotText(equipment);
                            AthenaPlugin.Log(LogLevel.Info, $"({nameof(StageSimulateGUI)}) Selected necklace {equipment.GetName()} {equipment.ItemId} {equipment.level}");
                            break;
                        case ItemSubType.Ring:
                            if (SelectedRing1 == null)
                            {
                                _modInventoryManager.SelectedRing1 = equipment;
                                SelectedRing1 = equipment;
                                SelectedRing1Content = CreateSlotText(equipment);
                                AthenaPlugin.Log(LogLevel.Info, $"({nameof(StageSimulateGUI)}) Selected ring1 {equipment.GetName()} {equipment.ItemId} {equipment.level}");
                            }
                            else
                            {
                                _modInventoryManager.SelectedRing2 = equipment;
                                SelectedRing2 = equipment;
                                SelectedRing2Content = CreateSlotText(equipment);
                                AthenaPlugin.Log(LogLevel.Info, $"({nameof(StageSimulateGUI)}) Selected ring2 {equipment.GetName()} {equipment.ItemId} {equipment.level}");
                            }
                            break;
                        case ItemSubType.Aura:
                            _modInventoryManager.SelectedAura = equipment;
                            SelectedAura = equipment;
                            SelectedAuraContent = CreateSlotText(equipment);
                            AthenaPlugin.Log(LogLevel.Info, $"({nameof(StageSimulateGUI)}) Selected aura {equipment.GetName()} {equipment.ItemId} {equipment.level}");
                            break;
                    }
                }
            };
            _inventoryGUI.OnSlotRemoveClicked += item =>
            {
                if (item is not Equipment equipment)
                {
                    return;
                }

                switch (equipment.ItemSubType)
                {
                    case ItemSubType.Weapon:
                        if (equipment.NonFungibleId.Equals(SelectedWeapon.Id))
                        {
                            _modInventoryManager.SelectedWeapon = null;
                            SelectedWeapon = null;
                            SelectedWeaponContent = new GUIContent("Weapon");
                        }

                        break;
                    case ItemSubType.Armor:
                        if (equipment.NonFungibleId.Equals(SelectedArmor.Id))
                        {
                            _modInventoryManager.SelectedArmor = null;
                            SelectedArmor = null;
                            SelectedArmorContent = new GUIContent("Armor");
                        }

                        break;
                    case ItemSubType.Belt:
                        if (equipment.NonFungibleId.Equals(SelectedBelt.Id))
                        {
                            _modInventoryManager.SelectedBelt = null;
                            SelectedBelt = null;
                            SelectedBeltContent = new GUIContent("Belt");
                        }

                        break;
                    case ItemSubType.Necklace:
                        if (equipment.NonFungibleId.Equals(SelectedNecklace.Id))
                        {
                            _modInventoryManager.SelectedNecklace = null;
                            SelectedNecklace = null;
                            SelectedNecklaceContent = new GUIContent("Necklace");
                        }

                        break;
                    case ItemSubType.Ring:
                        if (equipment.NonFungibleId.Equals(SelectedRing1.Id))
                        {
                            _modInventoryManager.SelectedRing1 = null;
                            SelectedRing1 = null;
                            SelectedRing1Content = new GUIContent("Ring1");
                        }
                        else if (equipment.NonFungibleId.Equals(SelectedRing2.Id))
                        {
                            _modInventoryManager.SelectedRing2 = null;
                            SelectedRing2 = null;
                            SelectedRing2Content = new GUIContent("Ring2");
                        }

                        break;
                }
            };

            _selectLayoutRect = new Rect(
                GUIToolbox.ScreenWidthReference - 450,
                GUIToolbox.ScreenHeightReference / 2 - 230,
                400,
                500);
            _simulateLayoutRect = new Rect(
                GUIToolbox.ScreenWidthReference - 350,
                GUIToolbox.ScreenHeightReference - 210,
                200,
                400);
        }
        public void OnGUI()
        {
            if (StateData is not { } stateData)
            {
                return;
            }

            GUI.matrix = GUIToolbox.GetGUIMatrix();

            if (selectedStageId == 0)
            {
                selectedStageId = stateData.clearedStageId + 1;
            }

            using (var areaScope = new GUILayout.AreaScope(_selectLayoutRect))
            {
                using (var verticalScope = new GUILayout.VerticalScope())
                {
                    using (var horizontalScope = new GUILayout.HorizontalScope())
                    {
                        DrawEquipmentSlot(SelectedAuraContent, SelectedAura, () =>
                        {
                            _modInventoryManager.SelectedAura = null;
                            SelectedAura = null;
                            SelectedAuraContent = new GUIContent("Aura");
                        });
                        DrawEquipmentSlot(SelectedWeaponContent, SelectedWeapon, () =>
                        {
                            _modInventoryManager.SelectedWeapon = null;
                            SelectedWeapon = null;
                            SelectedWeaponContent = new GUIContent("Weapon");
                        });
                        DrawEquipmentSlot(SelectedArmorContent, SelectedArmor, () =>
                        {
                            _modInventoryManager.SelectedArmor = null;
                            SelectedArmor = null;
                            SelectedArmorContent = new GUIContent("Armor");
                        });
                    }
                    using (var horizontalScope = new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(136);
                        DrawEquipmentSlot(SelectedBeltContent, SelectedBelt, () =>
                        {
                            _modInventoryManager.SelectedBelt = null;
                            SelectedBelt = null;
                            SelectedBeltContent = new GUIContent("Belt");
                        });
                        DrawEquipmentSlot(SelectedNecklaceContent, SelectedNecklace, () =>
                        {
                            _modInventoryManager.SelectedNecklace = null;
                            SelectedNecklace = null;
                            SelectedNecklaceContent = new GUIContent("Necklace");
                        });
                    }
                    using (var horizontalScope = new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(136);
                        DrawEquipmentSlot(SelectedRing1Content, SelectedRing1, () =>
                        {
                            _modInventoryManager.SelectedRing1 = null;
                            SelectedRing1 = null;
                            SelectedRing1Content = new GUIContent("Ring1");
                        });
                        DrawEquipmentSlot(SelectedRing2Content, SelectedRing2, () =>
                        {
                            _modInventoryManager.SelectedRing2 = null;
                            SelectedRing2 = null;
                            SelectedRing2Content = new GUIContent("Ring2");
                        });
                    }
                }
            }

            using (var areaScope = new GUILayout.AreaScope(_simulateLayoutRect))
            {
                using (var horizontalScope = new GUILayout.HorizontalScope())
                {
                    using (var verticalScope = new GUILayout.VerticalScope())
                    {
                        GUILayout.Label("Choose stage to simulate");
                        ControllablePicker(
                            stateData.StageSheet.Keys.Select(x => x.ToString()).ToArray(),
                            (_, index) => selectedStageId = index + 1,
                            selectedStageId - 1);

                        GUILayout.Label("Play Count");
                        DrawPlayCountController();

                        GUI.enabled = !_isCalculating;
                        if (GUILayout.Button("Simulate"))
                        {
                            Simulate();
                            AthenaPlugin.Log($"[StageGUI] Simulate button clicked {selectedStageId})");
                        }

                        GUI.enabled = true;

                        DrawSimulationResultTextArea();
                    }
                }
            }
        }

        private void InitStateData()
        {
            var tableSheets = TableSheets.Instance;
            var stid = States.Instance.CurrentAvatarState.worldInformation.TryGetLastClearedStageId(out int stageId)
                ? stageId
                : 0;
            StateData = (tableSheets.WorldSheet, tableSheets.StageSheet, stid);
        }

        private void InitEquipments()
        {
            SelectedAura = _modInventoryManager.SelectedAura;
            SelectedWeapon = _modInventoryManager.SelectedWeapon;
            SelectedArmor = _modInventoryManager.SelectedArmor;
            SelectedBelt = _modInventoryManager.SelectedBelt;
            SelectedNecklace = _modInventoryManager.SelectedNecklace;
            SelectedRing1 = _modInventoryManager.SelectedRing1;
            SelectedRing2 = _modInventoryManager.SelectedRing2;

            SelectedAuraContent = CreateSlotText(SelectedAura);
            SelectedWeaponContent = CreateSlotText(SelectedWeapon);
            SelectedArmorContent = CreateSlotText(SelectedArmor);
            SelectedBeltContent = CreateSlotText(SelectedBelt);
            SelectedNecklaceContent = CreateSlotText(SelectedNecklace);
            SelectedRing1Content = CreateSlotText(SelectedRing1);
            SelectedRing2Content = CreateSlotText(SelectedRing2);
        }

        private GUIContent CreateSlotText(Equipment equipment)
        {
            var slotText = $"Grade {equipment.Grade}" +
                $"\n{equipment.ElementalType}" +
                $"\n{equipment.GetName()}\n" +
                $"+{equipment.level}";
            return new GUIContent(slotText);
        }

        private void DrawEquipmentSlot(GUIContent content, Equipment? equipment, Action onRemove)
        {
            GUIStyle centeredStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter
            };

            using (var horizontalScope = new GUILayout.HorizontalScope())
            {
                GUILayout.Box(content, centeredStyle, GUILayout.Width(100), GUILayout.Height(100));
                if (equipment != null)
                {
                    if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        onRemove.Invoke();
                        AthenaPlugin.Log(LogLevel.Info, $"({nameof(StageSimulateGUI)}) Removed {content.text.ToLower()}");
                    }
                }
                else
                {
                    GUILayout.Space(20);
                }
            }
        }

        private void DrawSimulationResultTextArea()
        {
            if (_isCalculating)
            {
                GUILayout.Label($"Simulating... {simulationStep}/{playCount}");
            }
            else
            {
                using (var horizontalScope = new GUILayout.HorizontalScope())
                {
                    GUILayout.Label($"Fail: {_wave0ClearCount}");
                    GUILayout.Label($"★: {_wave1ClearCount}");
                    GUILayout.Label($"★★: {_wave2ClearCount}");
                    GUILayout.Label($"★★★: {_wave3ClearCount}");
                }
            }
        }

        private void DrawPlayCountController()
        {
            using (var verticalScope = new GUILayout.HorizontalScope())
            {
                GUI.enabled = !_isCalculating;
                if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(20)))
                {
                    playCount += 100;
                }

                GUI.enabled = true;
                GUILayout.Label(playCount + "", GUILayout.Width(30));

                GUI.enabled = !_isCalculating;
                if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(20)))
                {
                    if (playCount >= 0)
                    {
                        playCount -= 100;
                    }
                }

                GUI.enabled = true;
            }
        }

        private void ControllablePicker(string[] list, Action<string[], int> onChanged, int index = 0)
        {
            void Btn(string text, int change)
            {
                GUI.enabled = !_isCalculating;
                if (GUILayout.Button(text))
                {
                    if (index + change > 0 && index + change < list.Length)
                    {
                        onChanged(list, index + change);
                    }
                }

                GUI.enabled = true;
            }

            using (var horizontalScope = new GUILayout.HorizontalScope())
            {
                Btn("<<", -5);
                Btn("<", -1);
                GUILayout.Label(list[index]);
                Btn(">", 1);
                Btn(">>", 5);
            }
        }

        private async void Simulate()
        {
            _isCalculating = true;
            _wave0ClearCount = -1;
            _wave1ClearCount = -1;
            _wave2ClearCount = -1;
            _wave3ClearCount = -1;

            simulationStep = 0;

            var clearWaveInfo = await UniTask.Run(() => HackAndSlashSimulator.Simulate(
                _modInventoryManager.GetEquipments(),
                TableSheets.Instance,
                States.Instance,
                selectedStageId / 50,
                selectedStageId,
                playCount,
                onProgress: step => simulationStep = step));

            _wave0ClearCount = clearWaveInfo.TryGetValue(0, out var w0) ? w0 : 0;
            _wave1ClearCount = clearWaveInfo.TryGetValue(1, out var w1) ? w1 : 0;
            _wave2ClearCount = clearWaveInfo.TryGetValue(2, out var w2) ? w2 : 0;
            _wave3ClearCount = clearWaveInfo.TryGetValue(3, out var w3) ? w3 : 0;

            AthenaPlugin.Log($"[StageGUI] Simulate {playCount}: w0 ({_wave0ClearCount}) w1({_wave1ClearCount}) w2({_wave2ClearCount}) w3({_wave3ClearCount})");
            _isCalculating = false;
        }

        private async Task UpdateStateData()
        {
            if (LastSheetsUpdated is { } lastSheetsUpdated &&
                DateTimeOffset.UtcNow.Subtract(lastSheetsUpdated).CompareTo(TimeSpan.FromSeconds(30)) <= 0)
            {
                return;
            }

            LastSheetsUpdated = DateTimeOffset.UtcNow;

            var sheets = await Game.instance.Agent.GetSheetsAsync(new[]
            {
                Addresses.GetSheetAddress<WorldSheet>(),
                Addresses.GetSheetAddress<StageSheet>(),
            });

            var worldSheet = new WorldSheet();
            worldSheet.Set((Text)sheets[Addresses.GetSheetAddress<WorldSheet>()]);

            var stageSheet = new StageSheet();
            stageSheet.Set((Text)sheets[Addresses.GetSheetAddress<StageSheet>()]);

            var stid = States.Instance.CurrentAvatarState.worldInformation.TryGetLastClearedStageId(out int stageId)
                ? stageId
                : 0;

            StateData = (worldSheet, stageSheet, stid);
        }
    }
}
