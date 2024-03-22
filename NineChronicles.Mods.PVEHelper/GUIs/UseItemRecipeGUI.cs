using System;
using System.Collections.Generic;
using NineChronicles.Mods.PVEHelper.ViewModels;
using UnityEngine;

namespace NineChronicles.Mods.PVEHelper.GUIs
{
    public class UseItemRecipeGUI : IGUI
    {
        // TabGUI
        private const int _tabWidth = 100;
        private const int _tabHeight = 40;
        private const int _tabCount = 3;

        private static readonly Rect _tabRectPrefab = new Rect(0, 0, _tabWidth, _tabHeight);
        // ~TabGUI

        // ItemOptionGUI
        private const int _itemOptionWidth = 300;
        private const int _itemOptionHeight = 80;
        private const int _itemOptionCount = 4;

        private static readonly Rect _itemOptionRectPrefab = new Rect(0, 0, _itemOptionWidth, _itemOptionHeight);
        // ~ItemOptionGUI

        private readonly UseItemRecipeViewModel _viewModel;

        // groups
        private readonly Rect _rootGroupRect;
        private readonly Rect _tabGroupRect;

        // pools
        private readonly List<Rect> _tabRectPool = new List<Rect>();
        private Rect _mainStatRect;
        private readonly List<Rect> _itemOptionGroupRectPool = new List<Rect>();
        private Rect _buttonRect;

        public event Action<UseItemRecipeViewModel.Content> OnClickCreate;

        public UseItemRecipeGUI(
            int positionX,
            int positionY)
        {
            _viewModel = new UseItemRecipeViewModel();

            var width = _tabWidth * _tabCount;
            var height =
                _tabHeight +
                _tabHeight +
                _itemOptionHeight * _itemOptionCount +
                _tabHeight;
            _rootGroupRect = new Rect(positionX, positionY, width, height);
            _tabGroupRect = new Rect(0, 0, _rootGroupRect.width, _tabHeight);

            for (int i = 0; i < _tabCount; i++)
            {
                var rect = new Rect(_tabRectPrefab)
                {
                    x = i * _tabWidth,
                    y = 0f,
                };
                _tabRectPool.Add(rect);
            }

            _mainStatRect = new Rect(0f, _tabHeight + 10f, _rootGroupRect.width, _tabHeight - 10f);
            for (int i = 0; i < _itemOptionCount; i++)
            {
                var rect = new Rect(_itemOptionRectPrefab)
                {
                    x = 0f,
                    y = _tabHeight + _mainStatRect.height + (_itemOptionHeight * i),
                };
                _itemOptionGroupRectPool.Add(rect);
            }

            _buttonRect = new Rect(
                0f,
                _rootGroupRect.height - _tabHeight,
                _rootGroupRect.width,
                _tabHeight);
        }

        public void OnGUI()
        {
            GUI.matrix = GUIToolbox.GetGUIMatrix();
            GUI.BeginGroup(_rootGroupRect);
            GUI.Box(_rootGroupRect, string.Empty);
            DrawTabs();
            DrawContent();
            DrawCreateButton();
            GUI.EndGroup();
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
            var rect = _tabRectPool[index];
            var tabName = index switch
            {
                0 => "1",
                1 => "2",
                2 => "3",
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

        private void DrawContent()
        {
            var content = _viewModel.CurrentContent;
            if (content == null)
            {
                return;
            }

            GUI.Label(_mainStatRect, $"Main Stat: {content.statType} {content.statValue}");

            var itemStatOptionsCount = content.itemStatOptions.Count;
            for (var i = 0; i < itemStatOptionsCount; i++)
            {
                GUI.BeginGroup(_itemOptionGroupRectPool[i]);
                GUI.Box(_itemOptionRectPrefab, string.Empty);
                var itemStatOption = content.itemStatOptions[i];
                GUI.Label(
                    new Rect(10f, 0f, 240f, 30f),
                    $"{itemStatOption.statType} {itemStatOption.minValue}~{itemStatOption.maxValue}");
                GUI.Label(new Rect(250f, 0f, 50f, 30f), $"({itemStatOption.enableChance:P0})");
                itemStatOption.enable = GUI.Toggle(
                    new Rect(10f, 60f, 90f, 20f),
                    itemStatOption.enable,
                    itemStatOption.enable
                        ? "Enabled"
                        : "Disabled");
                if (itemStatOption.enable)
                {
                    itemStatOption.ratioOfValueRange = GUI.HorizontalSlider(
                    new Rect(100f, 60f, 190f, 20f),
                    itemStatOption.ratioOfValueRange,
                    0f,
                    1f);
                }

                GUI.EndGroup();
            }

            for (var i = 0; i < content.itemSkillOptions.Count; i++)
            {
                GUI.BeginGroup(_itemOptionGroupRectPool[itemStatOptionsCount + i]);
                GUI.Box(_itemOptionRectPrefab, string.Empty);
                var itemSkillOption = content.itemSkillOptions[i];
                GUI.Label(new Rect(10f, 0f, 240f, 30f), $"{itemSkillOption.skillId}"); // TODO: Skill name
                GUI.Label(new Rect(250f, 0f, 50f, 30f), $"({itemSkillOption.enableChance:P0})");
                GUI.Label(
                    new Rect(10f, 30f, 290f, 30f),
                    $"{itemSkillOption.skillDamageMin}~{itemSkillOption.skillDamageMax}({itemSkillOption.skillChanceMin:P0}~{itemSkillOption.skillChanceMax:P0})");
                itemSkillOption.enable = GUI.Toggle(
                    new Rect(10f, 60f, 90f, 20f),
                    itemSkillOption.enable,
                    itemSkillOption.enable
                        ? "Enabled"
                        : "Disabled");
                if (itemSkillOption.enable)
                {
                    itemSkillOption.ratioOfValueRange = GUI.HorizontalSlider(
                    new Rect(100f, 60f, 190f, 20f),
                    itemSkillOption.ratioOfValueRange,
                    0f,
                    1f);
                }

                GUI.EndGroup();
            }
        }

        public void DrawCreateButton()
        {
            var content = _viewModel.CurrentContent;
            GUI.enabled = content != null;
            if (GUI.Button(_buttonRect, "Create"))
            {
                OnClickCreate?.Invoke(content);
            }

            GUI.enabled = true;
        }

        public void Clear()
        {
            _viewModel.Clear();
        }

        public void SetItemRecipe(ItemRecipesViewModel.ItemRecipe itemRecipe)
        {
            _viewModel.SetItemRecipe(itemRecipe);
        }
    }
}
