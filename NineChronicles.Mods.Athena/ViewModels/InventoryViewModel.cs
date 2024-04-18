using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume;
using Nekoyume.Battle;
using Nekoyume.Helper;
using Nekoyume.Model.Item;
using NineChronicles.Mods.Athena.Extensions;
using NineChronicles.Mods.Athena.Pools;
using UnityEngine;

namespace NineChronicles.Mods.Athena.ViewModels
{
    public class InventoryViewModel
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
                    6 => "The Other",
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
                    slots.Add(new Slot(null, 0, false, false));
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
            public static readonly GUIContent existsInBlockchainGUIContent =
                new(string.Empty, "Exists in blockchain");
            public static readonly GUIStyle existsInBlockchainStyle = new(GUI.skin.box)
            {
                normal = { background = ColorTexturePool.Blue },
                hover = { background = ColorTexturePool.Blue },
                active = { background = ColorTexturePool.Blue },
            };
            public static readonly GUIContent moddedGUIContent =
                new(string.Empty, "Modded");
            public static readonly GUIStyle moddedStyle = new(GUI.skin.box)
            {
                normal = { background = ColorTexturePool.Green },
                hover = { background = ColorTexturePool.Green },
                active = { background = ColorTexturePool.Green },
            };

            public IItem item;
            public int count;
            public bool isExistsInBlockchain;
            public bool isModded;

            public string slotText;
            public string tooltip;
            public GUIContent slotGUIContent;

            public bool IsEmpty => item is null;

            public Slot(IItem item, int count, bool isExistsInBlockchain, bool isModded)
            {
                Set(item, count, isExistsInBlockchain, isModded);
            }

            public void Clear() => Set(null, 0, false, false);

            public void Set(IItem item, int count, bool isExistsInBlockchain, bool isModded)
            {
                this.item = item;
                this.count = count;
                this.isExistsInBlockchain = isExistsInBlockchain;
                this.isModded = isModded;
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
                        $"\n{equipment.GetName()}\n" +
                        $"+{equipment.level}";
                    tooltip = $"+{equipment.level} {equipment.GetName()}" +
                        $"\n{equipment.GetGradeText()} | {equipment.ElementalType.GetLocalizedString()} | {equipment.GetSubTypeText()}" +
                        $"\n{equipment.GetCPText()}";

                    var optionInfo = new ItemOptionInfo(equipment);
                    var (mainStatType, _, mainStatTotalValue) = optionInfo.MainStat;
                    tooltip += $"\n\n$ {mainStatType} {mainStatTotalValue}";
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
        public Tab CurrentTab => _tabs[CurrentTabIndex];
        public int CurrentPageIndex { get; private set; } = 0;
        public Page CurrentPage => _tabs[CurrentTabIndex].pages[CurrentPageIndex];

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

        public InventoryViewModel(int tabCount, int itemCountPerEachPage)
        {
            if (tabCount < 1)
            {
                AthenaPlugin.Log("[InventoryViewModel] tabCount < 1");
                tabCount = 1;
            }

            _slotCountPerEachPage = itemCountPerEachPage;
            _tabs = new List<Tab>();
            for (var i = 0; i < tabCount; i++)
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

        public void AddOrReplaceItem(
            IItem item,
            int count,
            bool isExistsInBlockchain,
            bool isModded,
            bool sort = true)
        {
            if (item is null)
            {
                AthenaPlugin.Log("[InventoryViewModel] AddItem item is null");
                return;
            }

            var tab = GetTab(item);
            AddOrReplaceItem(tab, item, count, isExistsInBlockchain, isModded, sort);
        }

        private void AddOrReplaceItem(
            Tab tab,
            IItem item,
            int count,
            bool isExistsInBlockchain,
            bool isModded,
            bool sort)
        {
            var slot = GetSlotToAddOrReplace(tab, item, out var isEmptySlot);
            if (item is INonFungibleItem)
            {
                // NOTE: Replace non-fungible item.
                slot.Set(item, count, isExistsInBlockchain, isModded);
                if (sort && isEmptySlot)
                {
                    Sort(tab);
                }

                return;
            }

            var addableCount = int.MaxValue - slot.count;
            if (addableCount >= count)
            {
                slot.Set(item, slot.count + count, isExistsInBlockchain, isModded);
                if (sort && isEmptySlot)
                {
                    Sort(tab);
                }

                return;
            }

            slot.Set(item, int.MaxValue, isExistsInBlockchain, isModded);
            AddOrReplaceItem(tab, item, count - addableCount, isExistsInBlockchain, isModded, sort);
        }

        private Tab GetTab(IItem item) => GetTab(item.ItemSubType);

        private Tab GetTab(ItemSubType itemSubType)
        {
            var tabIndex = itemSubType switch
            {
                ItemSubType.Weapon => 0,
                ItemSubType.Armor => 1,
                ItemSubType.Belt => 2,
                ItemSubType.Necklace => 3,
                ItemSubType.Ring => 4,
                ItemSubType.Aura => 5,
                _ => _tabs.Count, // The Other
            };
            return _tabs[tabIndex];
        }

        public bool TryGetTab(int index, out Tab tab)
        {
            if (index < 0 || index >= _tabs.Count)
            {
                tab = default;
                return false;
            }

            tab = _tabs[index];
            return true;
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

        private Slot GetSlotToAddOrReplace(Tab tab, IItem item, out bool isEmptySlot)
        {
            return item switch
            {
                IFungibleItem fungibleItem => GetSlotToAddOrReplace(tab, fungibleItem, out isEmptySlot),
                INonFungibleItem nonFungibleItem => GetSlotToAddOrReplace(tab, nonFungibleItem, out isEmptySlot),
                _ => throw new System.ArgumentOutOfRangeException(nameof(item)),
            };
        }

        private Slot GetSlotToAddOrReplace(Tab tab, IFungibleItem item, out bool isEmptySlot)
        {
            foreach (var page in tab.pages)
            {
                foreach (var slot in page.slots)
                {
                    if (slot.item is IFungibleItem fungibleItem &&
                        fungibleItem.FungibleId.Equals(item.FungibleId) &&
                        slot.count < int.MaxValue)
                    {
                        isEmptySlot = false;
                        return slot;
                    }
                }
            }

            var pageHasEmptySlot = GetOrCreatePageHasEmptySlot(tab);
            isEmptySlot = true;
            return GetEmptySlot(pageHasEmptySlot);
        }

        private Slot GetSlotToAddOrReplace(Tab tab, INonFungibleItem item, out bool isEmptySlot)
        {
            var alreadyExistsSlot = tab.pages
                .SelectMany(page => page.slots)
                .FirstOrDefault(slot =>
                    slot.item is INonFungibleItem nonFungibleItem &&
                    nonFungibleItem.NonFungibleId.Equals(item.NonFungibleId));
            if (alreadyExistsSlot is not null)
            {
                isEmptySlot = false;
                return alreadyExistsSlot;
            }

            var pageHasEmptySlot = GetOrCreatePageHasEmptySlot(tab);
            isEmptySlot = true;
            return GetEmptySlot(pageHasEmptySlot);
        }

        public void RemoveItem(IItem item, int count, bool sort = true)
        {
            if (item is null)
            {
                AthenaPlugin.Log("[InventoryViewModel] RemoveItem item is null");
                return;
            }

            var tab = GetTab(item);
            RemoveItem(tab, item, count, sort);
        }

        private void RemoveItem(Tab tab, IItem item, int count, bool sort)
        {
            if (!TryGetSlotToRemove(tab, item, out var slot))
            {
                AthenaPlugin.Log("[InventoryViewModel] RemoveItem !TryGetSlotToRemove");
                return;
            }

            var removeableCount = slot.count;
            if (removeableCount > count)
            {
                slot.RemoveCount(count);
                return;
            }
            else if (removeableCount == count)
            {
                slot.Clear();
                if (sort)
                {
                    Sort(tab);
                }
                return;
            }

            slot.Clear();
            RemoveItem(tab, item, count - removeableCount, sort);
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
                AthenaPlugin.Log("[InventoryViewModel] TryGetSlotToRemove targetSlots.Count == 0");
                slot = default;
                return false;
            }

            slot = targetSlots.First();
            return slot is not null;
        }

        public void SortAllTabs()
        {
            foreach (var tab in _tabs)
            {
                Sort(tab);
            }
        }

        private void Sort(Tab tab)
        {
            var slotSources = tab.pages
                .SelectMany(page => page.slots)
                .Where(slot => slot.item is not null)
                .Select(slot => (slot.item, slot.count, slot.isExistsInBlockchain, slot.isModded))
                .ToList();
            slotSources.Sort((s1, s2) =>
            {
                if (s1.item is not Equipment e1)
                {
                    return 1;
                }

                if (s2.item is not Equipment e2)
                {
                    return -1;
                }

                return CPHelper.GetCP(e2).CompareTo(CPHelper.GetCP(e1));
            });
            tab.Clear();
            foreach (var (item, count, isExistsInBlockchain, isModded) in slotSources)
            {
                AddOrReplaceItem(item, count, isExistsInBlockchain, isModded, sort: false);
            }
        }

        public bool TryGetItem<T>(Guid nonFungibleId, out T item) where T : INonFungibleItem
        {
            if (typeof(T) != typeof(Equipment))
            {
                item = default;
                return false;
            }

            foreach (var tab in _tabs)
            {
                foreach (var page in tab.pages)
                {
                    foreach (var slot in page.slots)
                    {
                        if (slot.item is T t && t.NonFungibleId.Equals(nonFungibleId))
                        {
                            item = t;
                            return true;
                        }
                    }
                }
            }

            item = default;
            return false;
        }
    }
}
