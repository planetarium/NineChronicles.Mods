using System;
using System.Collections.Generic;
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
        // Root
        private const int _rootX = 610;
        private const int _rootY = 80;
        private const int _rootWidth = 400;
        private const int _rootHeight = 480;
        // ~Root

        // Slots TabGUI
        private const int _tabWidth = 100;
        private const int _tabHeight = 40;
        private const int _tabCount = 2;

        private static readonly Rect _tabRectPrefab = new(0f, 0f, _tabWidth, _tabHeight);
        private static readonly string[] _tabNames = new[]
        {
            BattleType.Adventure.ToString(),
            BattleType.Arena.ToString(),
        };
        // ~Slots TabGUI

        // Slots parts
        private static readonly Rect _itemSlotsRect = new(
                _rootX,
                _rootY + _tabHeight + 10,
                _rootWidth,
                _rootHeight - _tabHeight - 10);
        // ~Slots parts

        private readonly ItemSlotsViewModel _viewModel;

        // groups
        private static readonly Rect _rootGroupRect = new(_rootX, _rootY, _rootWidth, _rootHeight);
        private static readonly Rect _tabGroupRect = new(0f, 0f, _tabWidth * _tabCount, _tabHeight);

        // pools
        private static readonly Rect _rootBoxRect = new(0f, 0f, _rootWidth, _rootHeight);
        private readonly List<Rect> _tabRectPool = new();

        private readonly InventoryGUI _inventoryGUI;

        public ItemSlotsGUI(InventoryGUI inventoryGUI)
        {
            _inventoryGUI = inventoryGUI;
            _viewModel = new ItemSlotsViewModel();
            InitViewModel(inventoryGUI);

            for (int i = 0; i < _tabCount; i++)
            {
                var rect = new Rect(_tabRectPrefab)
                {
                    x = i * _tabWidth,
                    y = 0f,
                };
                _tabRectPool.Add(rect);
            }
        }

        public void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                _inventoryGUI.OnSlotSelected += OnSlotSelected;
                _inventoryGUI.OnSlotReimportClicked += OnSlotRemoveClicked;
                _inventoryGUI.OnSlotRemoveClicked += OnSlotRemoveClicked;
            }
            else
            {
                _inventoryGUI.OnSlotSelected -= OnSlotSelected;
                _inventoryGUI.OnSlotReimportClicked -= OnSlotRemoveClicked;
                _inventoryGUI.OnSlotRemoveClicked -= OnSlotRemoveClicked;
            }
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
            GUI.BeginGroup(_rootGroupRect);
            GUI.Box(_rootBoxRect, string.Empty);
            DrawTabs();
            GUI.EndGroup();

            // NOTE: DrawItemSlots should be called after GUI.EndGroup().
            //       Move this to front of GUI.EndGroup() when the drawing API is changed.
            DrawItemSlots();
        }

        public void DrawTabs()
        {
            GUI.BeginGroup(_tabGroupRect);
            for (var i = 0; i < _tabCount; i++)
            {
                DrawTab(i);
            }

            GUI.EndGroup();
        }

        private void DrawTab(int index)
        {
            if (index < 0 || index >= _tabCount) return;

            var rect = _tabRectPool[index];
            var tabName = _tabNames[index];
            var isSelected = _viewModel.CurrentTabIndex == index;
            GUI.backgroundColor = isSelected ? Color.yellow : Color.white;
            GUI.skin.button.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
            if (GUI.Button(rect, tabName))
            {
                _viewModel.SelectTab(index);
                AthenaPlugin.Log($"{tabName} Tab({index}) selected.");
            }

            GUI.backgroundColor = Color.white;
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
