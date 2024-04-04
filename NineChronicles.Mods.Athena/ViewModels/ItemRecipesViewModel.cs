using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Model.Item;
using Nekoyume.TableData;
using NineChronicles.Mods.Athena.Extensions;

namespace NineChronicles.Mods.Athena.ViewModels
{
    public class ItemRecipesViewModel
    {
        public class Tab
        {
            public readonly int index;
            public readonly string name;
            public readonly List<Page> pages;

            public Tab(int index, int slotCount)
            {
                this.index = index;
                name = index switch
                {
                    0 => "Weapon",
                    1 => "Armor",
                    2 => "Belt",
                    3 => "Necklace",
                    4 => "Ring",
                    5 => "Aura",
                    _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
                };
                pages = new List<Page>();
                AddPage(slotCount);
            }

            public void Clear()
            {
                pages.RemoveRange(1, pages.Count - 1);
                pages[0].Clear();
            }

            public Page AddPage(int slotCount)
            {
                var page = new Page(pages.Count, slotCount);
                pages.Add(page);
                return page;
            }
        }

        public class Page
        {
            public readonly int index;
            public readonly List<Slot> slots;

            public Page(int index, int slotCount)
            {
                this.index = index;
                slots = new List<Slot>();
                for (var i = 0; i < slotCount; i++)
                {
                    slots.Add(new Slot(null));
                }
            }

            public void Clear()
            {
                foreach (var slot in slots)
                {
                    slot.Clear();
                }
            }
        }

        public class Slot
        {
            public ItemRecipe itemRecipe;

            public Slot(ItemRecipe itemRecipe)
            {
                Set(itemRecipe);
            }

            public void Clear() => Set(null);

            public void Set(ItemRecipe itemRecipe)
            {
                this.itemRecipe = itemRecipe;
            }
        }

        public class ItemRecipe
        {
            public readonly EquipmentItemSheet.Row equipmentRow;
            public readonly EquipmentItemRecipeSheet.Row recipeRow;
            public readonly ItemSubRecipe[] subRecipes;

            public readonly string slotText;

            public ItemRecipe(
                EquipmentItemSheet.Row equipmentRow,
                EquipmentItemRecipeSheet.Row recipeRow,
                EquipmentItemSubRecipeSheetV2.Row[] subRecipeRows,
                List<EquipmentItemOptionSheet.Row>[] itemOptionRows)
            {
                this.equipmentRow = equipmentRow;
                this.recipeRow = recipeRow;

                subRecipes = new ItemSubRecipe[subRecipeRows.Length];
                for (var i = 0; i < subRecipeRows.Length; i++)
                {
                    subRecipes[i] = new ItemSubRecipe(
                        subRecipeRows[i],
                        itemOptionRows[i].ToArray());
                }

                slotText = $"Grade {equipmentRow.Grade}\n{equipmentRow.ElementalType}\n{equipmentRow.GetName()}";
            }
        }

        public class ItemSubRecipe
        {
            public readonly EquipmentItemSubRecipeSheetV2.Row subRecipeRow;
            public readonly EquipmentItemOptionSheet.Row[] itemOptionRows;

            public ItemSubRecipe(
                EquipmentItemSubRecipeSheetV2.Row subRecipeRow,
                EquipmentItemOptionSheet.Row[] itemOptionRows)
            {
                this.subRecipeRow = subRecipeRow;
                this.itemOptionRows = itemOptionRows;
            }
        }

        private readonly int _slotCountPerEachPage;
        private readonly List<Tab> _tabs;

        public int CurrentTabIndex { get; private set; } = 0;
        public Tab CurrentTab => _tabs[CurrentTabIndex];
        public int CurrentPageIndex { get; private set; } = 0;
        public Page CurrentPage => _tabs[CurrentTabIndex].pages[CurrentPageIndex];

        /// <summary>
        /// -1: No slot selected
        /// 0~: Selected slot index
        /// </summary>
        public int SelectedSlotIndex { get; private set; } = -1;
        public int TabCount => _tabs.Count;
        public int PageCount => _tabs[CurrentTabIndex].pages.Count;
        public IEnumerable<Slot> CurrentPageSlots => _tabs[CurrentTabIndex].pages[CurrentPageIndex].slots;

        public ItemRecipesViewModel(int tabCount, int itemCountPerEachPage)
        {
            if (tabCount <= 0)
            {
                AthenaPlugin.Log("[ItemRecipesViewModel] tabCount < 1");
                tabCount = 1;
            }

            _slotCountPerEachPage = itemCountPerEachPage;
            _tabs = new List<Tab>();
            for (int i = 0; i < tabCount; i++)
            {
                _tabs.Add(new Tab(i, _slotCountPerEachPage));
            }
        }

        public bool IsEmptyPage(int pageIndex)
        {
            var tab = _tabs[CurrentTabIndex];
            return tab.pages.Count <= pageIndex;
        }

        public void SelectTab(int index)
        {
            CurrentTabIndex = index;
            SelectPage(0);
        }

        public void SelectPage(int index)
        {
            CurrentPageIndex = index;
            SelectSlot(-1);
        }

        public void SelectSlot(int index)
        {
            SelectedSlotIndex = index;
        }

        public void DeselectSlot()
        {
            SelectedSlotIndex = -1;
        }

        public void Clear()
        {
            foreach (var tab in _tabs)
            {
                tab.Clear();
            }
        }

        public void AddItem(ItemRecipe itemRecipe)
        {
            if (!TryGetTab(itemRecipe, out var tab))
            {
                return;
            }

            AddItem(tab, itemRecipe);
        }

        private void AddItem(Tab tab, ItemRecipe itemRecipe)
        {
            var slot = GetSlotToAdd(tab);
            slot.Set(itemRecipe);
        }

        private bool TryGetTab(ItemRecipe itemRecipe, out Tab tab)
        {
            var tabIndex = itemRecipe.recipeRow.ItemSubType switch
            {
                ItemSubType.Weapon => 0,
                ItemSubType.Armor => 1,
                ItemSubType.Belt => 2,
                ItemSubType.Necklace => 3,
                ItemSubType.Ring => 4,
                ItemSubType.Aura => 5,
                _ => -1,
            };
            if (tabIndex == -1)
            {
                tab = null;
                return false;
            }

            tab = _tabs[tabIndex];
            return true;
        }

        private Page GetOrCreatePageHasEmptySlot(Tab tab)
        {
            foreach (var page in tab.pages)
            {
                if (page.slots.Any(slot => slot.itemRecipe is null))
                {
                    return page;
                }
            }

            return tab.AddPage(_slotCountPerEachPage);
        }

        private Slot GetEmptySlot(Page page)
        {
            return page.slots.First(slot => slot.itemRecipe is null);
        }

        private Slot GetSlotToAdd(Tab tab)
        {
            foreach (var page in tab.pages)
            {
                foreach (var slot in page.slots)
                {
                    if (slot.itemRecipe is null)
                    {
                        return slot;
                    }
                }
            }

            var pageHasEmptySlot = GetOrCreatePageHasEmptySlot(tab);
            return GetEmptySlot(pageHasEmptySlot);
        }

        public void RemoveItem(ItemRecipe itemRecipe)
        {
            if (!TryGetTab(itemRecipe, out var tab))
            {
                return;
            }

            RemoveItem(tab, itemRecipe);
        }

        private void RemoveItem(Tab tab, ItemRecipe itemRecipe)
        {
            if (!TryGetSlotToRemove(tab, itemRecipe, out var slot))
            {
                return;
            }

            slot.Clear();
        }

        private bool TryGetSlotToRemove(Tab tab, ItemRecipe itemRecipe, out Slot slot)
        {
            foreach (var page in tab.pages)
            {
                foreach (var slot2 in page.slots)
                {
                    if (slot2.itemRecipe.recipeRow.Id.Equals(itemRecipe.recipeRow.Id))
                    {
                        slot = slot2;
                        return true;
                    }
                }
            }

            slot = default;
            return false;
        }
    }
}
