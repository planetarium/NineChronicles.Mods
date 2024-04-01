using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Nekoyume.TableData;
using NineChronicles.Mods.Athena.Extensions;
using NineChronicles.Mods.Athena.Manager;
using NineChronicles.Mods.Athena.ViewModels;
using UnityEngine;

namespace NineChronicles.Mods.Athena.GUIs
{
    public class ItemCreationGUI : IGUI
    {
        private readonly ModInventoryManager _modInventoryManager;
        private readonly ItemRecipesGUI _itemRecipesGUI;
        private readonly UseItemRecipeGUI _useItemRecipeGUI;

        public ItemCreationGUI(ModInventoryManager modInventoryManager)
        {
            _modInventoryManager = modInventoryManager;
            _itemRecipesGUI = new ItemRecipesGUI(
                positionX: 100,
                positionY: 80,
                slotCountPerPage: 15,
                slotCountPerRow: 5);
            _useItemRecipeGUI = new UseItemRecipeGUI(
                positionX: 610,
                positionY: 80);

            _itemRecipesGUI.OnSlotSelected += (itemRecipe) => _useItemRecipeGUI.SetItemRecipe(itemRecipe);
            _itemRecipesGUI.OnSlotDeselected += () => _useItemRecipeGUI.Clear();
            _useItemRecipeGUI.OnClickCreate += OnClickCreate;
        }

        public void OnGUI()
        {
            GUI.matrix = GUIToolbox.GetGUIMatrix();
            _itemRecipesGUI.OnGUI();
            _useItemRecipeGUI.OnGUI();
        }

        public void SetItemRecipes(
            EquipmentItemSheet itemSheet,
            EquipmentItemRecipeSheet recipeSheet,
            EquipmentItemSubRecipeSheetV2 subRecipeSheet,
            EquipmentItemOptionSheet itemOptionSheet)
        {
            foreach (var recipeRow in recipeSheet.OrderedList.Reverse())
            {
                if (!itemSheet.TryGetValue(recipeRow.ResultEquipmentId, out var itemRow))
                {
                    continue;
                }

                var subRecipes = new EquipmentItemSubRecipeSheetV2.Row[recipeRow.SubRecipeIds.Count];
                var itemOptions = new List<EquipmentItemOptionSheet.Row>[recipeRow.SubRecipeIds.Count];
                for (var i = 0; i < subRecipes.Length; i++)
                {
                    var subRecipeId = recipeRow.SubRecipeIds[i];
                    if (!subRecipeSheet.TryGetValue(subRecipeId, out var subRecipeRow))
                    {
                        continue;
                    }

                    subRecipes[i] = subRecipeRow;
                    var itemOptionRows = new List<EquipmentItemOptionSheet.Row>();
                    for (int j = 0; j < subRecipeRow.Options.Count; j++)
                    {
                        var optionInfo = subRecipeRow.Options[j];
                        if (!itemOptionSheet.TryGetValue(optionInfo.Id, out var itemOptionRow))
                        {
                            continue;
                        }

                        itemOptionRows.Add(itemOptionRow);
                    }

                    itemOptions[i] = itemOptionRows;
                }

                var itemRecipe = new ItemRecipesViewModel.ItemRecipe(
                    itemRow,
                    recipeRow,
                    subRecipes,
                    itemOptions);
                _itemRecipesGUI.AddItem(itemRecipe);
            }
        }

        private void OnClickCreate(UseItemRecipeViewModel.Content content)
        {
            var item = new Models.ModItem
            {
                Id = System.Guid.NewGuid(),
                EquipmentId = content.equipmentId,
                Level = 0,
                ExistsItem = false,
                SubRecipeId = content.subRecipeId,
                OptionIdList = content.itemStatOptions
                    .Where(e => e.enable)
                    .Select(e => e.itemOptionId)
                    .Concat(content.itemSkillOptions.Select(e => e.itemOptionId))
                    .ToImmutableList(),
                RatioOfOptionValueRangeList = content.itemStatOptions
                    .Where(e => e.enable)
                    .Select(e => e.ratioOfValueRange)
                    .Concat(content.itemSkillOptions.Select(e => e.ratioOfValueRange))
                    .ToImmutableList()
            };

            _modInventoryManager.AddItem(item);
            var itemName = _itemRecipesGUI.TryGetSelectedSlot(out var selectedSlot)
                ? selectedSlot.itemRecipe.equipmentRow.GetName()
                : content.equipmentId.ToString();
            NotificationGUI.Notify($"Item Created\n{itemName}");
        }
    }
}
