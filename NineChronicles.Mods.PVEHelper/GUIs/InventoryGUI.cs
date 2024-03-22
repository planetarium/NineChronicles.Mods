using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Nekoyume.Model.Item;
using NineChronicles.Mods.PVEHelper.Extensions;
using NineChronicles.Mods.PVEHelper.ViewModels;
using UnityEngine;
using static NineChronicles.Mods.PVEHelper.ViewModels.InventoryViewModel;

namespace NineChronicles.Mods.PVEHelper.GUIs
{
    public class InventoryGUI : IGUI
    {
        // TabGUI
        private const int _tabWidth = 100;
        private const int _tabHeight = 40;
        private const int _tabCount = 2; // temporary.

        private static readonly Rect _tabRectPrefab = new Rect(0, 0, _tabWidth, _tabHeight);
        // ~TabGUI

        // SlotGUI
        private const int _itemIconWidth = 100;
        private const int _itemIconHeight = 100;
        private const int _itemNameHeight = 20;
        private const int _itemCountWidth = 50;
        private const int _itemCountHeight = 20;
        private const int _slotWidth = _itemIconWidth;
        private const int _slotHeight = _itemIconHeight + _itemNameHeight;

        private static readonly Rect _itemIconRectPrefab = new Rect(0, 0, _itemIconWidth, _itemIconHeight);
        private static readonly Rect _itemNameRectPrefab = new Rect(0, _itemIconHeight, _itemIconWidth, _itemNameHeight);
        private static readonly Rect _itemCountRectPrefab = new Rect(
            _itemIconWidth - _itemCountWidth,
            _itemIconHeight - _itemCountHeight,
            _itemCountWidth,
            _itemCountHeight);
        // ~SlotGUI

        // TageNumberGUI
        private const int _pageNumberWidth = 100;
        private const int _pageNumberHeight = 40;
        private const int _pageNumberCount = 5; // temporary.

        private static readonly Rect _pageNumberRectPrefab = new Rect(0, 0, _pageNumberWidth, _pageNumberHeight);
        // ~TageNumberGUI

        private readonly int _slotCountPerPage;
        private readonly int _slotCountPerRow;

        // view model
        private readonly InventoryViewModel _viewModel;

        // groups
        private readonly Rect _inventoryGroupRect;
        private readonly Rect _tabGroupRect;
        private readonly Rect _slotGroupRect;
        private readonly Rect _pageNumberGroupRect;

        // pools
        private readonly List<Rect> _tabRectPool = new List<Rect>();
        private readonly List<(Rect iconRect, Rect textRect, Rect countRect)> _slotRectPool =
            new List<(Rect, Rect, Rect)>();
        private readonly List<Rect> _pageNumerRectPool = new List<Rect>();

        public event Action<(IItem item, int count)> OnSlotSelected;
        public event Action OnSlotUnselected;

        public InventoryGUI(
            int positionX,
            int positionY,
            int slotCountPerPage,
            int slotCountPerRow)
        {
            _slotCountPerPage = slotCountPerPage;
            _slotCountPerRow = slotCountPerRow;
            _viewModel = new InventoryViewModel(slotCountPerPage);

            var inventoryWith = _slotWidth * slotCountPerRow;
            var slotRowCount = slotCountPerPage / slotCountPerRow;
            var inventoryHeight =
                _tabHeight +
                _slotHeight * slotRowCount +
                _pageNumberHeight;
            _inventoryGroupRect = new Rect(positionX, positionY, inventoryWith, inventoryHeight);
            _tabGroupRect = new Rect(0, 0, inventoryWith, _tabHeight);
            _slotGroupRect = new Rect(0, _tabHeight, inventoryWith, _slotHeight * slotRowCount);
            _pageNumberGroupRect = new Rect(0, _tabHeight + _slotHeight * slotRowCount, inventoryWith, _pageNumberHeight);

            for (int i = 0; i < _tabCount; i++)
            {
                var rect = new Rect(_tabRectPrefab)
                {
                    x = i * _tabWidth,
                    y = 0,
                };
                _tabRectPool.Add(rect);
            }

            for (int i = 0; i < _slotCountPerPage; i++)
            {
                var rowIndex = i / _slotCountPerRow;
                var iconRect = new Rect(_itemIconRectPrefab)
                {
                    x = _slotWidth * (i % _slotCountPerRow),
                    y = _slotHeight * rowIndex,
                };
                var nameRect = new Rect(_itemNameRectPrefab)
                {
                    x = iconRect.x,
                    y = iconRect.y + iconRect.height,
                };
                var countRect = new Rect(_itemCountRectPrefab)
                {
                    x = iconRect.x + iconRect.width - _itemCountWidth,
                    y = iconRect.y + iconRect.height - _itemCountHeight,
                };

                _slotRectPool.Add((iconRect, nameRect, countRect));
            }

            for (int i = 0; i < _pageNumberCount; i++)
            {
                var rect = new Rect(_pageNumberRectPrefab)
                {
                    x = i * _pageNumberWidth,
                };
                _pageNumerRectPool.Add(rect);
            }
        }

        // ------ ------
        // | T1 | | T2 |
        // ----------------------------------
        // | I1 | | I2 | | I3 | | I4 | | I5 |
        // ----------------------------------
        // | I5 | | I6 | | I7 | | I8 | | I9 |
        // ----------------------------------
        // | P1 | | P2 | | P3 | | P4 | | P5 |
        // ----------------------------------
        public void OnGUI()
        {
            GUI.matrix = GUIToolbox.GetGUIMatrix();
            GUI.BeginGroup(_inventoryGroupRect);
            DrawTabs();
            DrawSlots();
            DrawPageNumbers();
            GUI.EndGroup();
        }

        private void DrawTabs()
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
            var rect = _tabRectPool[index];
            var tabName = index switch
            {
                0 => "Equipment",
                1 => "The Other",
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
            };
            var isSelected = _viewModel.CurrentTabIndex == index;
            GUI.backgroundColor = isSelected ? Color.yellow : Color.white;
            GUI.skin.button.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
            if (GUI.Button(rect, tabName))
            {
                _viewModel.SelectTab(index);
                PVEHelperPlugin.Instance.Log(LogLevel.Info, $"{tabName} Tab({index}) selected.");
            }

            GUI.backgroundColor = Color.white;
        }

        private void DrawSlots()
        {
            GUI.BeginGroup(_slotGroupRect);
            var slots = _viewModel.CurrentPageSlots.ToArray();
            for (var i = 0; i < slots.Length; i++)
            {
                DrawSlot(i, slots[i]);
            }

            GUI.EndGroup();
        }

        private void DrawSlot(int index, Slot slot)
        {
            var (iconRect, nameRect, countRect) = _slotRectPool[index];
            var item = slot.item;
            var count = slot.count;

            if (item is null)
            {
                GUI.enabled = false;
                GUI.Button(iconRect, "Empty");
                GUI.enabled = true;
                return;
            }

            var isSelected = _viewModel.SelectedSlotIndex == index;
            GUI.backgroundColor = isSelected ? Color.yellow : Color.white;
            if (GUI.Button(iconRect, item.GetIcon()))
            {
                if (isSelected)
                {
                    _viewModel.UnselectSlot();
                    OnSlotUnselected?.Invoke();
                    PVEHelperPlugin.Instance.Log(LogLevel.Info, $"Slot({index}) unselected.");
                }
                else
                {
                    _viewModel.SelectSlot(index);
                    OnSlotSelected?.Invoke((item, count));
                    PVEHelperPlugin.Instance.Log(LogLevel.Info, $"Slot({index}) selected.");
                }
            }

            GUI.backgroundColor = Color.white;
            GUI.skin.label.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
            GUI.Label(nameRect, item.GetName());

            if (count > 1)
            {
                GUI.Label(countRect, count.ToString());
            }
        }

        private void DrawPageNumbers()
        {
            // FIXME: CurrentPageIndex 4 페이지 누르면 에러.
            var middleIndex = _pageNumberCount / 2;
            var startIndex = _viewModel.CurrentPageIndex > middleIndex
                ? _viewModel.CurrentPageIndex - middleIndex
                : 0;

            GUI.BeginGroup(_pageNumberGroupRect);
            for (int i = 0; i < _pageNumberCount; i++)
            {
                var dataIndex = startIndex + i;
                if (_viewModel.IsEmptyPage(dataIndex))
                {
                    continue;
                }

                DrawPageNumber(i, dataIndex);
            }

            GUI.EndGroup();
        }

        private void DrawPageNumber(int viewIndex, int dataIndex)
        {
            var rect = _pageNumerRectPool[viewIndex];
            var isSelected = _viewModel.CurrentPageIndex == dataIndex;
            GUI.backgroundColor = isSelected ? Color.yellow : Color.white;
            GUI.skin.button.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
            if (GUI.Button(rect, $"{dataIndex + 1}"))
            {
                _viewModel.SelectPage(dataIndex);
                PVEHelperPlugin.Instance.Log(LogLevel.Info, $"Page #{dataIndex + 1}({dataIndex}) selected.");
            }

            GUI.backgroundColor = Color.white;
        }

        public void Clear()
        {
            _viewModel.Clear();
        }

        public void AddItem(IItem item, int count)
        {
            _viewModel.AddItem(item, count);
        }

        public void RemoveItem(IItem item, int count)
        {
            _viewModel.RemoveItem(item, count);
        }
    }
}
