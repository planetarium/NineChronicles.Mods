using System;
using System.Collections.Generic;
using System.Linq;
using NineChronicles.Mods.PVEHelper.Extensions;
using NineChronicles.Mods.PVEHelper.ViewModels;
using UnityEngine;
using static NineChronicles.Mods.PVEHelper.ViewModels.ItemRecipesViewModel;

namespace NineChronicles.Mods.PVEHelper.GUIs
{
    public class ItemRecipesGUI : IGUI
    {
        // TabGUI
        private const int _tabWidth = 100;
        private const int _tabHeight = 40;
        private const int _tabCount = 6;

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
        private const int _pageNumberCount = 5;

        private static readonly Rect _pageNumberRectPrefab = new Rect(0, 0, _pageNumberWidth, _pageNumberHeight);
        // ~TageNumberGUI

        private readonly int _slotCountPerPage;
        private readonly int _slotCountPerRow;

        // view model
        private readonly ItemRecipesViewModel _viewModel;

        // groups
        private readonly Rect _rootGroupRect;
        private readonly Rect _tabGroupRect;
        private readonly Rect _slotGroupRect;
        private readonly Rect _pageNumberGroupRect;

        // pools
        private readonly Rect _rootBoxRect;
        private readonly List<Rect> _tabRectPool = new List<Rect>();
        private readonly List<(Rect iconRect, Rect textRect, Rect countRect)> _slotRectPool =
            new List<(Rect, Rect, Rect)>();
        private readonly List<Rect> _pageNumerRectPool = new List<Rect>();

        public event Action<ItemRecipe> OnSlotSelected;
        public event Action OnSlotDeselected;

        public ItemRecipesGUI(
            int positionX,
            int positionY,
            int slotCountPerPage,
            int slotCountPerRow)
        {
            _slotCountPerPage = slotCountPerPage;
            _slotCountPerRow = slotCountPerRow;
            _viewModel = new ItemRecipesViewModel(_tabCount, slotCountPerPage);

            var width = _slotWidth * slotCountPerRow;
            var slotRowCount = slotCountPerPage / slotCountPerRow;
            var height =
                _tabHeight * 2 +
                _slotHeight * slotRowCount +
                _pageNumberHeight;
            _rootGroupRect = new Rect(positionX, positionY, width, height);
            _tabGroupRect = new Rect(0f, 0f, width, _tabHeight * 2f);
            _slotGroupRect = new Rect(0f, _tabGroupRect.height, width, _slotHeight * slotRowCount);
            _pageNumberGroupRect = new Rect(0f, _tabGroupRect.height + _slotGroupRect.height, width, _pageNumberHeight);

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

        public void OnGUI()
        {
            GUI.matrix = GUIToolbox.GetGUIMatrix();
            GUI.BeginGroup(_rootGroupRect);
            GUI.Box(_rootBoxRect, string.Empty);
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
                0 => "Weapon",
                1 => "Armor",
                2 => "Belt",
                3 => "Necklace",
                4 => "Ring",
                5 => "Aura",
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
            };
            var isSelected = _viewModel.CurrentTabIndex == index;
            GUI.backgroundColor = isSelected ? Color.yellow : Color.white;
            GUI.skin.button.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
            if (GUI.Button(rect, tabName))
            {
                _viewModel.SelectTab(index);
                PVEHelperPlugin.Log($"{tabName} Tab({index}) selected.");
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
            var itemRecipe = slot.itemRecipe;
            if (itemRecipe is null)
            {
                GUI.enabled = false;
                GUI.Button(iconRect, "Empty");
                GUI.enabled = true;
                return;
            }

            var isSelected = _viewModel.SelectedSlotIndex == index;
            var equipmentRow = itemRecipe.equipmentRow;
            GUI.backgroundColor = isSelected ? Color.yellow : Color.white;
            var style = new GUIStyle(GUI.skin.button)
            {
                wordWrap = true,
            };
            if (GUI.Button(iconRect, itemRecipe.slotText, style))
            {
                if (isSelected)
                {
                    _viewModel.DeselectSlot();
                    OnSlotDeselected?.Invoke();
                    PVEHelperPlugin.Log($"Slot({index}) deselected.");
                }
                else
                {
                    _viewModel.SelectSlot(index);
                    OnSlotSelected?.Invoke(itemRecipe);
                    PVEHelperPlugin.Log($"Slot({index}) selected.");
                }
            }

            GUI.backgroundColor = Color.white;
            GUI.skin.label.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
            GUI.Label(nameRect, equipmentRow.GetName());
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
                PVEHelperPlugin.Log($"Page #{dataIndex + 1}({dataIndex}) selected.");
            }

            GUI.backgroundColor = Color.white;
        }

        public void Clear()
        {
            _viewModel.Clear();
        }

        public void AddItem(ItemRecipesViewModel.ItemRecipe itemRecipe)
        {
            _viewModel.AddItem(itemRecipe);
        }

        public void RemoveItem(ItemRecipesViewModel.ItemRecipe itemRecipe)
        {
            _viewModel.RemoveItem(itemRecipe);
        }

        public bool TryGetSelectedSlot(out Slot slot)
        {
            if (_viewModel.SelectedSlotIndex == -1)
            {
                slot = null;
                return false;
            }

            slot = _viewModel.CurrentPageSlots.ElementAt(_viewModel.SelectedSlotIndex);
            return true;
        }
    }
}
