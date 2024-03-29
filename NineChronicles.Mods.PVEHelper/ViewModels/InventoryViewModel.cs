﻿using System.Collections.Generic;
using System.Linq;
using Nekoyume;
using Nekoyume.Helper;
using Nekoyume.Model.Item;
using NineChronicles.Mods.PVEHelper.Extensions;
using UnityEngine;

namespace NineChronicles.Mods.PVEHelper.ViewModels
{
    public class InventoryViewModel
    {
        private class Tab
        {
            public readonly int index;
            public readonly List<Page> pages;

            public Tab(int index, int slotCount)
            {
                this.index = index;
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

        private class Page
        {
            public readonly int index;
            public readonly List<Slot> slots;

            public Page(int index, int slotCount)
            {
                this.index = index;
                slots = new List<Slot>();
                for (var i = 0; i < slotCount; i++)
                {
                    slots.Add(new Slot(null, 0));
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
            public IItem item;
            public int count;

            public string slotText;
            public string tooltip;
            public GUIContent slotGUIContent;

            public Slot(IItem item, int count)
            {
                Set(item, count);
            }

            public void Clear() => Set(null, 0);

            public void Set(IItem item, int count)
            {
                this.item = item;
                this.count = count;
                UpdateSlotTextAndTooltip();
            }

            public void AddCount(int count)
            {
                this.count += count;
            }

            public void RemoveCount(int count)
            {
                this.count -= count;
            }

            private void UpdateSlotTextAndTooltip()
            {
                if (item is null)
                {
                    slotText = string.Empty;
                    tooltip = string.Empty;
                    slotGUIContent = GUIContent.none;
                    return;
                }

                if (item is Equipment equipment)
                {
                    slotText = $"Grade {equipment.Grade}" +
                        $"\n{equipment.ElementalType}" +
                        $"\n{equipment.GetName()}";
                    tooltip = $"+{equipment.level} {equipment.GetName()}" +
                        $"\n{equipment.GetGradeText()} | {equipment.ElementalType.GetLocalizedString()} | {equipment.GetSubTypeText()}" +
                        $"\n{equipment.GetCPText()}";

                    var optionInfo = new ItemOptionInfo(equipment);
                    var (mainStatType, _, mainStatTotalValue) = optionInfo.MainStat;
                    tooltip += $"\n$ {mainStatType} {mainStatTotalValue}";
                    foreach (var (type, value, count) in optionInfo.StatOptions)
                    {
                        tooltip += $"\n{string.Concat(Enumerable.Range(0, count).Select(_ => "#"))} {type} +{value}";
                    }

                    foreach (var (skillRow, power, chance, statPowerRatio, refStatType) in optionInfo.SkillOptions)
                    {
                        tooltip += $"\n@ {skillRow.GetLocalizedName()}" +
                            $"\nPower: {power}" +
                            $"\nChance: {chance}";
                        if (refStatType != Nekoyume.Model.Stat.StatType.NONE)
                        {
                            tooltip += $"\nStatPowerRatio: {statPowerRatio}" +
                                $"\nRefStatType: {refStatType}";
                        }
                    }
                }
                else
                {
                    slotText = $"Not Implemented: {item.GetType().Name}" +
                        $"\n{item.GetName()}";
                }

                if (count > 1)
                {
                    slotText += $"\nx{count}";
                }

                slotGUIContent = new GUIContent(slotText, tooltip);
            }
        }

        private readonly int _slotCountPerEachPage;
        private readonly List<Tab> _tabs;

        public int CurrentTabIndex { get; private set; } = 0;
        public int CurrentPageIndex { get; private set; } = 0;

        /// <summary>
        /// -1: No slot selected
        /// 0~: Selected slot index
        /// </summary>
        public int SelectedSlotIndex { get; private set; } = -1;
        public Slot SelectedSlot => SelectedSlotIndex < 0
            ? null
            : _tabs[CurrentTabIndex].pages[CurrentPageIndex].slots[SelectedSlotIndex];
        public int TabCount => _tabs.Count;
        public int PageCount => _tabs[CurrentTabIndex].pages.Count;
        public IEnumerable<Slot> CurrentPageSlots => _tabs[CurrentTabIndex].pages[CurrentPageIndex].slots;

        public InventoryViewModel(int itemCountPerEachPage)
        {
            _slotCountPerEachPage = itemCountPerEachPage;
            _tabs = new List<Tab>
            {
                new Tab(0, itemCountPerEachPage),
                new Tab(1, itemCountPerEachPage),
                new Tab(2, itemCountPerEachPage),
            };
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

        public void AddItem(IItem item, int count)
        {
            var tab = GetTab(item);
            AddItem(tab, item, count);
        }

        private void AddItem(Tab tab, IItem item, int count)
        {
            var slot = GetSlotToAdd(tab, item);
            var addableCount = int.MaxValue - slot.count;
            if (addableCount >= count)
            {
                slot.Set(item, slot.count + count);
                return;
            }

            slot.Set(item, int.MaxValue);
            AddItem(tab, item, count - addableCount);
        }

        private Tab GetTab(IItem item)
        {
            var tabIndex = item switch
            {
                Equipment equipment => equipment.ItemSubType != ItemSubType.Aura
                    ? 0
                    : 1,
                _ => 2, // TODO: Add more tabs
            };
            return _tabs[tabIndex];
        }

        private Page GetOrCreatePageHasEmptySlot(Tab tab)
        {
            foreach (var page in tab.pages)
            {
                if (page.slots.Any(slot => slot.item is null))
                {
                    return page;
                }
            }

            return tab.AddPage(_slotCountPerEachPage);
        }

        private Slot GetEmptySlot(Page page)
        {
            return page.slots.First(slot => slot.item is null);
        }

        private Slot GetSlotToAdd(Tab tab, IItem item)
        {
            return item switch
            {
                IFungibleItem fungibleItem => GetSlotToAdd(tab, fungibleItem),
                INonFungibleItem nonFungibleItem => GetSlotToAdd(tab, nonFungibleItem),
                _ => throw new System.ArgumentOutOfRangeException(nameof(item)),
            };
        }

        private Slot GetSlotToAdd(Tab tab, IFungibleItem item)
        {
            foreach (var page in tab.pages)
            {
                foreach (var slot in page.slots)
                {
                    if (slot.item is IFungibleItem fungibleItem &&
                        fungibleItem.FungibleId.Equals(item.FungibleId) &&
                        slot.count < int.MaxValue)
                    {
                        return slot;
                    }
                }
            }

            var pageHasEmptySlot = GetOrCreatePageHasEmptySlot(tab);
            return GetEmptySlot(pageHasEmptySlot);
        }

        private Slot GetSlotToAdd(Tab tab, INonFungibleItem item)
        {
            foreach (var page in tab.pages)
            {
                foreach (var slot in page.slots)
                {
                    if (slot.item is INonFungibleItem nonFungibleItem &&
                        nonFungibleItem.NonFungibleId.Equals(item.NonFungibleId) &&
                        slot.count < int.MaxValue)
                    {
                        return slot;
                    }
                }
            }

            var pageHasEmptySlot = GetOrCreatePageHasEmptySlot(tab);
            return GetEmptySlot(pageHasEmptySlot);
        }

        public void RemoveItem(IItem item, int count)
        {
            var tab = GetTab(item);
            RemoveItem(tab, item, count);
        }

        private void RemoveItem(Tab tab, IItem item, int count)
        {
            if (!TryGetSlotToRemove(tab, item, out var slot))
            {
                return;
            }

            var removeableCount = slot.count;
            if (removeableCount >= count)
            {
                slot.RemoveCount(count);
                return;
            }

            slot.Clear();
            RemoveItem(tab, item, count - removeableCount);
        }

        private bool TryGetSlotToRemove(Tab tab, IItem item, out Slot slot)
        {
            return item switch
            {
                IFungibleItem fungibleItem => TryGetSlotToRemove(tab, fungibleItem, out slot),
                INonFungibleItem nonFungibleItem => TryGetSlotToRemove(tab, nonFungibleItem, out slot),
                _ => throw new System.ArgumentOutOfRangeException(nameof(item)),
            };
        }

        private bool TryGetSlotToRemove(Tab tab, IFungibleItem item, out Slot slot)
        {
            var targetSlots = new List<Slot>();
            foreach (var page in tab.pages)
            {
                foreach (var slot2 in page.slots)
                {
                    if (slot2.item is IFungibleItem fungibleItem &&
                        fungibleItem.FungibleId.Equals(item.FungibleId))
                    {
                        targetSlots.Add(slot2);
                    }
                }
            }

            if (targetSlots.Count == 0)
            {
                slot = default;
                return false;
            }

            slot = targetSlots.OrderBy(slot => slot.count).First();
            return true;
        }

        private bool TryGetSlotToRemove(Tab tab, INonFungibleItem item, out Slot slot)
        {
            var targetSlots = new List<Slot>();
            foreach (var page in tab.pages)
            {
                foreach (var slot2 in page.slots)
                {
                    if (slot2.item is INonFungibleItem nonFungibleItem &&
                        nonFungibleItem.NonFungibleId.Equals(item.NonFungibleId))
                    {
                        targetSlots.Add(slot2);
                    }
                }
            }

            if (targetSlots.Count == 0)
            {
                slot = default;
                return false;
            }

            slot = targetSlots.First();
            return true;
        }
    }
}
