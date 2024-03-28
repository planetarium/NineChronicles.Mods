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

        private static readonly Rect _tabRectPrefab = new Rect(0f, 0f, _tabWidth, _tabHeight);
        // ~TabGUI

        // ItemOptionGUI
        private const int _itemOptionWidth = 300;
        private const int _itemOptionHeight = 80;
        private const int _itemOptionCount = 4;

        private static readonly Rect _itemOptionRectPrefab = new Rect(0f, 0f, _itemOptionWidth, _itemOptionHeight);
        // ~ItemOptionGUI

        private readonly UseItemRecipeViewModel _viewModel;

        // groups
        private readonly Rect _rootGroupRect;
        private readonly Rect _tabGroupRect;

        // pools
        private readonly Rect _rootBoxRect;
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
            _tabGroupRect = new Rect(0f, 0f, _rootGroupRect.width, _tabHeight);

            _rootBoxRect = new Rect(0f, 0f, width, height);

            for (int i = 0; i < _tabCount; i++)
            {
                var rect = new Rect(_tabRectPrefab)
                {
                    x = i * _tabWidth,
                    y = 0f,
                };
                _tabRectPool.Add(rect);
            }

            _mainStatRect = new Rect(10f, _tabHeight + 10f, _rootGroupRect.width - 10f, 20f);

            for (int i = 0; i < _itemOptionCount; i++)
            {
                var rect = new Rect(_itemOptionRectPrefab)
                {
                    x = 0f,
                    y = _tabHeight + _mainStatRect.height + 10f + ((_itemOptionHeight + 2f) * i),
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
            GUI.Box(_rootBoxRect, string.Empty);
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
                    new Rect(10f, 0f, 220f, 20f),
                    $"{itemStatOption.statType} {itemStatOption.minValue}~{itemStatOption.maxValue}");
                GUI.Label(new Rect(240f, 0f, 60f, 20f), $"({itemStatOption.enableChance:P0})");
                itemStatOption.enable = GUI.Toggle(
                    new Rect(10f, 40f, 90f, 20f),
                    itemStatOption.enable,
                    itemStatOption.enable
                        ? "Enabled"
                        : "Disabled");
                if (itemStatOption.enable)
                {
                    GUI.skin.horizontalSlider.alignment = TextAnchor.MiddleLeft;
                    itemStatOption.ratioOfValueRange = GUI.HorizontalSlider(
                        new Rect(100f, 40f + 5f, 130f, 20f - 5f),
                        itemStatOption.ratioOfValueRange,
                        0f,
                        1f);
                    GUI.Label(new Rect(240f, 40f, 60f, 20f), $"({itemStatOption.ratioOfValueRange:P0})");
                    var resultStat = itemStatOption.minValue +
                        (itemStatOption.maxValue - itemStatOption.minValue) * itemStatOption.ratioOfValueRange;
                    GUI.Label(new Rect(10f, 60f, 280f, 20f), $"{itemStatOption.statType} {resultStat}");
                }

                GUI.EndGroup();
            }

            for (var i = 0; i < content.itemSkillOptions.Count; i++)
            {
                GUI.BeginGroup(_itemOptionGroupRectPool[itemStatOptionsCount + i]);
                GUI.Box(_itemOptionRectPrefab, string.Empty);
                var itemSkillOption = content.itemSkillOptions[i];
                GUI.Label(new Rect(10f, 0f, 220f, 20f), $"{itemSkillOption.skillName}");
                GUI.Label(new Rect(240f, 0f, 60f, 20f), $"({itemSkillOption.enableChance:P0})");
                GUI.Label(
                    new Rect(10f, 20f, 290f, 20f),
                    $"{itemSkillOption.skillDamageMin}~{itemSkillOption.skillDamageMax} ({itemSkillOption.skillChanceMin:P0}~{itemSkillOption.skillChanceMax:P0})");
                itemSkillOption.enable = GUI.Toggle(
                    new Rect(10f, 40f, 90f, 20f),
                    itemSkillOption.enable,
                    itemSkillOption.enable
                        ? "Enabled"
                        : "Disabled");
                if (itemSkillOption.enable)
                {
                    GUI.skin.horizontalSlider.alignment = TextAnchor.MiddleLeft;
                    itemSkillOption.ratioOfValueRange = GUI.HorizontalSlider(
                        new Rect(100f, 40f + 5f, 130f, 20f - 5f),
                        itemSkillOption.ratioOfValueRange,
                        0f,
                        1f);
                    GUI.Label(new Rect(240f, 40f, 60f, 20f), $"({itemSkillOption.ratioOfValueRange:P0})");
                    var resultDamage = itemSkillOption.skillDamageMin +
                        (itemSkillOption.skillDamageMax - itemSkillOption.skillDamageMin) * itemSkillOption.ratioOfValueRange;
                    var resultChance = itemSkillOption.skillChanceMin +
                        (itemSkillOption.skillChanceMax - itemSkillOption.skillChanceMin) * itemSkillOption.ratioOfValueRange;
                    GUI.Label(new Rect(10f, 60f, 280f, 20f), $"{resultDamage} ({resultChance:P0})");
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
