using System;
using BepInEx.Logging;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using NineChronicles.Mods.Athena.Mangers;
using NineChronicles.Mods.Athena.ViewModels;
using UnityEngine;

namespace NineChronicles.Mods.Athena.GUIs
{
    public class ItemSlotsGUI : IGUI
    {
        private readonly ItemSlotsViewModel _viewModel;
        private readonly Rect _itemSlotsRect;
        private readonly InventoryGUI _inventoryGUI;

        public ItemSlotsGUI(InventoryGUI inventoryGUI)
        {
            _viewModel = new ItemSlotsViewModel();
            InitViewModel(inventoryGUI);
            _itemSlotsRect = new Rect(
                GUIToolbox.ScreenWidthReference - 450,
                GUIToolbox.ScreenHeightReference / 2 - 230,
                400,
                500);
            _inventoryGUI = inventoryGUI;
            _inventoryGUI.OnSlotSelected += OnSlotSelected;
            _inventoryGUI.OnSlotReimportClicked += OnSlotRemoveClicked;
            _inventoryGUI.OnSlotRemoveClicked += OnSlotRemoveClicked;
        }

        private void InitViewModel(InventoryGUI inventoryGUI)
        {
            var battleTypes = new BattleType[]
            {
                BattleType.Adventure,
                BattleType.Arena,
            };
            foreach (var battleType in battleTypes)
            {
                _viewModel.AddTab(battleType);
                var equipments = UserDataManager.LoadItemSlots(
                    battleType,
                    inventoryGUI);
                foreach (var equipment in equipments)
                {
                    _viewModel.Register(battleType, equipment);
                }
            }
        }

        private void OnSlotSelected((IItem item, int count) tuple)
        {
            if (tuple.item is not Equipment equipment)
            {
                return;
            }

            _viewModel.Register(equipment);
            UserDataManager.SaveItemSlots(_viewModel.CurrentBattleType, _viewModel.CurrentEquipments);
        }

        private void OnSlotRemoveClicked(IItem item)
        {
            if (item is not Equipment equipment)
            {
                return;
            }

            var battleTypes = _viewModel.SupportedBattleTypes;
            foreach (var battleType in battleTypes)
            {
                _viewModel.Deregister(battleType, equipment);
                UserDataManager.SaveItemSlots(battleType, _viewModel.GetEquipments(battleType));
            }
        }

        public void OnGUI()
        {
            GUI.matrix = GUIToolbox.GetGUIMatrix();
            _inventoryGUI.OnGUI();
            DrawItemSlots();
        }

        private void DrawItemSlots()
        {
            using var areaScope = new GUILayout.AreaScope(_itemSlotsRect);
            using var verticalScope = new GUILayout.VerticalScope();
            using (var horizontalScope = new GUILayout.HorizontalScope())
            {
                DrawEquipmentSlot(ItemSubType.Aura);
                DrawEquipmentSlot(ItemSubType.Weapon);
                DrawEquipmentSlot(ItemSubType.Armor);
            }
            using (var horizontalScope = new GUILayout.HorizontalScope())
            {
                GUILayout.Space(136);
                DrawEquipmentSlot(ItemSubType.Belt);
                DrawEquipmentSlot(ItemSubType.Necklace);
            }
            using (var horizontalScope = new GUILayout.HorizontalScope())
            {
                GUILayout.Space(136);
                DrawEquipmentSlot(ItemSubType.Ring, index: 0);
                DrawEquipmentSlot(ItemSubType.Ring, index: 1);
            }
        }

        private void DrawEquipmentSlot(ItemSubType itemSubType, int index = 0)
        {
            Action onRemove;
            if (_viewModel.TryGetItem<Equipment>(itemSubType, index, out var equipment, out var guiContent))
            {
                if (equipment is null)
                {
                    onRemove = null;
                }
                else
                {
                    onRemove = () =>
                    {
                        _viewModel.Deregister(equipment);
                        UserDataManager.SaveItemSlots(_viewModel.CurrentBattleType, _viewModel.GetEquipments());
                    };
                }
            }
            else
            {
                guiContent = new GUIContent(itemSubType.ToString());
                onRemove = null;
            }

            GUIStyle centeredStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter
            };

            using var horizontalScope = new GUILayout.HorizontalScope();
            GUILayout.Box(guiContent, centeredStyle, GUILayout.Width(100), GUILayout.Height(100));
            if (onRemove != null)
            {
                if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    onRemove.Invoke();
                    AthenaPlugin.Log(LogLevel.Info, $"({nameof(AdventureGUI)}) Removed {guiContent.text.ToLower()}");
                }
            }
            else
            {
                GUILayout.Space(20);
            }
        }
    }
}
