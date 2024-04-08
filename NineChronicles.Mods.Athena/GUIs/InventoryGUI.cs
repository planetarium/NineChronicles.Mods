using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Model.Item;
using NineChronicles.Mods.Athena.Pools;
using NineChronicles.Mods.Athena.ViewModels;
using UnityEngine;

namespace NineChronicles.Mods.Athena.GUIs
{
    public class InventoryGUI : IGUI
    {
        // TabGUI
        private const int _tabWidth = 100;
        private const int _tabHeight = 40;
        private const int _tabCount = 7;

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

        private static readonly Rect _itemIconRectPrefab = new Rect(0f, 0f, _itemIconWidth, _itemIconHeight);
        private static readonly Rect _itemExistsRectPrefab = new Rect(0f, 0f, 8f, 8f);
        private static readonly Rect _itemModdedRectPrefab = new Rect(0f, 0f, 8f, 8f);
        private static readonly Rect _itemCountRectPrefab = new Rect(
            _itemIconWidth - _itemCountWidth,
            _itemIconHeight - _itemCountHeight,
            _itemCountWidth,
            _itemCountHeight);
        private static readonly Rect _itemNameRectPrefab = new Rect(0, _itemIconHeight, _itemIconWidth, _itemNameHeight);

        // ~SlotGUI

        // TageNumberGUI
        private const int _pageNumberWidth = 100;
        private const int _pageNumberHeight = 40;
        private const int _pageNumberCount = 5;

        private static readonly Rect _pageNumberRectPrefab = new Rect(0, 0, _pageNumberWidth, _pageNumberHeight);
        // ~TageNumberGUI

        // Styles
        private GUIStyle _toolTipStyle;
        // ~Styles

        private readonly int _slotCountPerPage;
        private readonly int _slotCountPerRow;

        // view model
        private readonly InventoryViewModel _viewModel;

        // groups
        private readonly Rect _rootGroupRect;
        private readonly Rect _tabGroupRect;
        private readonly Rect _slotGroupRect;
        private readonly Rect _pageNumberGroupRect;

        // pools
        private readonly Rect _rootBoxRect;
        private readonly List<Rect> _tabRectPool = new List<Rect>();
        private readonly List<(
            Rect iconRect,
            Rect existsRect,
            Rect moddedRect,
            Rect countRect,
            Rect nameRect)> _slotRectPool =
            new List<(Rect, Rect, Rect, Rect, Rect)>();
        private readonly List<Rect> _pageNumerRectPool = new List<Rect>();

        public event Action<(IItem item, int count)> OnSlotSelected;
        public event Action OnSlotDeselected;
        public event Action<IItem> OnSlotRemoveClicked;

        public InventoryGUI(
            int positionX,
            int positionY,
            int slotCountPerPage,
            int slotCountPerRow)
        {
            _slotCountPerPage = slotCountPerPage;
            _slotCountPerRow = slotCountPerRow;
            _viewModel = new InventoryViewModel(_tabCount, slotCountPerPage);

            var width = _slotWidth * slotCountPerRow;
            var slotRowCount = slotCountPerPage / slotCountPerRow;
            var height =
                _tabHeight * 2f +
                _slotHeight * slotRowCount +
                _pageNumberHeight;
            _rootGroupRect = new Rect(positionX, positionY, width, height);
            _tabGroupRect = new Rect(0, 0, width, _tabHeight * 2f);
            _slotGroupRect = new Rect(0, _tabGroupRect.height, width, _slotHeight * slotRowCount);
            _pageNumberGroupRect = new Rect(0, _tabGroupRect.height + _slotGroupRect.height, width, _pageNumberHeight);

            _rootBoxRect = new Rect(0f, 0f, width, height);

            for (int i = 0; i < _tabCount; i++)
            {
                var rect = new Rect(_tabRectPrefab)
                {
                    x = i < 5 ? i * _tabWidth : (i - 5) * _tabWidth,
                    y = i < 5 ? 0 : _tabHeight,
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
                var existsRect = new Rect(_itemExistsRectPrefab)
                {
                    x = iconRect.x + 2f,
                    y = iconRect.y + 2f,
                };
                var moddedRect = new Rect(_itemModdedRectPrefab)
                {
                    x = iconRect.x + 2f + existsRect.width + 2f,
                    y = iconRect.y + 2f,
                };
                var countRect = new Rect(_itemCountRectPrefab)
                {
                    x = iconRect.x + iconRect.width - _itemCountWidth,
                    y = iconRect.y + iconRect.height - _itemCountHeight,
                };
                var nameRect = new Rect(_itemNameRectPrefab)
                {
                    x = iconRect.x,
                    y = iconRect.y + iconRect.height,
                };

                _slotRectPool.Add((
                    iconRect,
                    existsRect,
                    moddedRect,
                    countRect,
                    nameRect));
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

        public void OnGUI()
        {
            GUI.matrix = GUIToolbox.GetGUIMatrix();
            GUI.BeginGroup(_rootGroupRect);
            GUI.Box(_rootBoxRect, string.Empty);
            DrawTabs();
            DrawSlots();
            DrawPageNumbers();
            GUI.EndGroup();
            DrawTooltip();
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
            var isSelected = _viewModel.CurrentTabIndex == index;
            GUI.backgroundColor = isSelected ? Color.yellow : Color.white;
            GUI.skin.button.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
            var tabName = _viewModel.TryGetTab(index, out var tab)
                ? tab.name
                : $"Tab {index}";
            if (GUI.Button(rect, tabName))
            {
                _viewModel.SelectTab(index);
                AthenaPlugin.Log($"{tabName} Tab({index}) selected.");
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

        private void DrawSlot(int index, InventoryViewModel.Slot slot)
        {
            var (iconRect, existsRect, moddedRect, countRect, nameRect) = _slotRectPool[index];
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
            var style = new GUIStyle(GUI.skin.button)
            {
                wordWrap = true,
            };
            if (GUI.Button(iconRect, slot.slotGUIContent, style))
            {
                if (isSelected)
                {
                    _viewModel.DeselectSlot();
                    OnSlotDeselected?.Invoke();
                    AthenaPlugin.Log($"Slot({index}) deselected.");
                }
                else
                {
                    _viewModel.SelectSlot(index);
                    OnSlotSelected?.Invoke((item, count));
                    AthenaPlugin.Log($"Slot({index}) selected.");
                }
            }

            if (count > 1)
            {
                GUI.Label(countRect, count.ToString());
            }

            if (slot.isExistsInBlockchain)
            {
                GUI.Box(
                    existsRect,
                    InventoryViewModel.Slot.existsInBlockchainGUIContent,
                    InventoryViewModel.Slot.existsInBlockchainStyle);
            }

            if (slot.isModded)
            {
                GUI.Box(
                    moddedRect,
                    InventoryViewModel.Slot.moddedGUIContent,
                    InventoryViewModel.Slot.moddedStyle);
                if (GUI.Button(nameRect, "Remove"))
                {
                    AthenaPlugin.Log($"Remove button clicked.");
                    RemoveItem(item, count);
                    OnSlotRemoveClicked?.Invoke(item);
                }
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
                AthenaPlugin.Log($"Page #{dataIndex + 1}({dataIndex}) selected.");
            }

            GUI.backgroundColor = Color.white;
        }

        private void DrawTooltip()
        {
            var tooltip = GUI.tooltip;
            if (string.IsNullOrEmpty(tooltip))
            {
                return;
            }

            var mousePosition = Event.current.mousePosition;
            var tooltipRect = new Rect(mousePosition.x, mousePosition.y, 200, 220);
            tooltipRect = GUIToolbox.MoveInsideScreen(tooltipRect, 10, 10);
            _toolTipStyle ??= new GUIStyle(GUI.skin.box)
            {
                normal = { background = ColorTexturePool.Dark },
                wordWrap = true,
            };
            GUI.Box(tooltipRect, tooltip, _toolTipStyle);
        }

        public void Clear()
        {
            _viewModel.Clear();
        }

        public void AddItem(IItem item, int count, bool isExistsInBlockchain, bool isModded)
        {
            _viewModel.AddItem(item, count, isExistsInBlockchain, isModded);
        }

        public void AddOrReplaceItem(INonFungibleItem item, bool isExistsInBlockchain, bool isModded)
        {
            _viewModel.AddOrReplaceItem(item, 1, isExistsInBlockchain, isModded);
        }

        public void RemoveItem(IItem item, int count)
        {
            _viewModel.RemoveItem(item, count);
        }

        public void Sort()
        {
            _viewModel.Sort();
        }

        public bool TryGetSelectedSlot(out InventoryViewModel.Slot slot)
        {
            slot = _viewModel.SelectedSlot;
            return slot is not null;
        }
    }
}
