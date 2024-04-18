using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using NineChronicles.Mods.Athena.Extensions;
using UnityEngine;

namespace NineChronicles.Mods.Athena.ViewModels
{
    public class ItemSlotsViewModel
    {
        public class Tab
        {
            public readonly int index;
            public readonly BattleType battleType;
            public readonly Content content;

            public Tab(int index, BattleType battleType)
            {
                this.index = index;
                this.battleType = battleType;
                content = new Content();
            }

            public void Clear()
            {
                content.Clear();
            }
        }

        public class Content
        {
            public static readonly ItemSubType[] SupportedEquipmentItemSubTypes = new[]
            {
                ItemSubType.Weapon,
                ItemSubType.Armor,
                ItemSubType.Belt,
                ItemSubType.Necklace,
                ItemSubType.Ring,
                ItemSubType.Aura,
            };
            private static readonly Dictionary<ItemSubType, GUIContent[]> defaultEquipmentGUIContents =
                SupportedEquipmentItemSubTypes.ToDictionary(
                    e => e,
                    e => e == ItemSubType.Ring
                        ? new GUIContent[] { new($"{e}1"), new($"{e}2") }
                        : new GUIContent[] { new(e.ToString()) });

            /// <summary>
            /// ItemSubType.Weapon, ItemSubType.Armor, ItemSubType.Belt, ItemSubType.Necklace, ItemSubType.Ring
            /// ItemSubType.Aura
            /// </summary>
            public readonly Dictionary<ItemSubType, (Equipment equipment, GUIContent guiContent)[]> equipments = new();

            // NOTE: Expanding the feature to support RuneState.
            //public readonly HashSet<RuneState> runeStates = new();

            public IEnumerable<Equipment> Equipments => equipments.Values
                .SelectMany(e => e.Select(e2 => e2.equipment))
                .Where(e => e is not null);

            public Content()
            {
                foreach (var itemSubType in SupportedEquipmentItemSubTypes)
                {
                    if (itemSubType == ItemSubType.Ring)
                    {
                        equipments[itemSubType] = new (Equipment equipment, GUIContent guiContent)[]
                        {
                            (null, defaultEquipmentGUIContents[itemSubType][0]),
                            (null, defaultEquipmentGUIContents[itemSubType][1]),
                        };
                    }
                    else
                    {
                        equipments[itemSubType] = new (Equipment equipment, GUIContent guiContent)[]
                        {
                            (null, defaultEquipmentGUIContents[itemSubType][0])
                        };
                    }
                }
            }

            public void Clear()
            {
                equipments.Clear();
            }

            public void Register(IItem item)
            {
                if (item is Equipment equipment)
                {
                    Register(equipment);
                }
            }

            private void Register(Equipment equipment)
            {
                var index = (int?)null;
                if (equipment.ItemSubType == ItemSubType.Ring)
                {
                    for (var i = 0; i < equipments[ItemSubType.Ring].Length; i++)
                    {
                        var (registeredEquipment, _) = equipments[ItemSubType.Ring][i];
                        if (registeredEquipment is null)
                        {
                            if (!index.HasValue)
                            {
                                index = i;
                            }
                        }
                        else if (registeredEquipment.NonFungibleId == equipment.NonFungibleId)
                        {
                            return;
                        }
                    }

                    index ??= 0;
                }
                else
                {
                    index = 0;
                }

                var guiContent = CreateGUIContent(equipment);
                equipments[equipment.ItemSubType][index.Value] = (equipment, guiContent);
            }

            public void Deregister(IItem item)
            {
                if (item is Equipment equipment)
                {
                    Deregister(equipment);
                }
            }

            public void Deregister(Equipment equipment)
            {
                var index = (int?)null;
                foreach (var itemSubType in SupportedEquipmentItemSubTypes)
                {
                    for (var i = 0; i < equipments[itemSubType].Length; i++)
                    {
                        var (registeredEquipment, _) = equipments[itemSubType][i];
                        if (registeredEquipment?.NonFungibleId == equipment.NonFungibleId)
                        {
                            index = i;
                            break;
                        }
                    }
                }

                if (!index.HasValue)
                {
                    return;
                }

                var guiContent = defaultEquipmentGUIContents[equipment.ItemSubType][index.Value];
                equipments[equipment.ItemSubType][index.Value] = (null, guiContent);
            }

            private GUIContent CreateGUIContent(IItem item)
            {
                if (item is Equipment equipment)
                {
                    var slotText = $"Grade {equipment.Grade}" +
                        $"\n{equipment.ElementalType}" +
                        $"\n{equipment.GetName()}\n" +
                        $"+{equipment.level}";
                    return new GUIContent(slotText);
                }

                return new GUIContent("Not Supported");
            }

            public bool TryGetItem<T>(ItemSubType itemSubType, int index, out T item, out GUIContent guiContent)
                where T : IItem
            {
                var type = typeof(T);
                if (type == typeof(Equipment))
                {
                    if (!equipments.TryGetValue(itemSubType, out var tuples))
                    {
                        item = default;
                        guiContent = null;
                        return false;
                    }

                    var (item2, guiContent2) = tuples[index];
                    item = item2 is null
                        ? default
                        : (T)(IItem)item2;
                    guiContent = guiContent2;
                    return true;
                }

                item = default;
                guiContent = null;
                return false;
            }

            public bool TryGetItem<T>(ItemSubType itemSubType, out T item, out GUIContent guiContent)
                where T : IItem => TryGetItem(itemSubType, 0, out item, out guiContent);
        }

        private readonly List<Tab> _tabs = new List<Tab>();
        private int _currentTabIndex;

        public IEnumerable<BattleType> SupportedBattleTypes => _tabs.Select(e => e.battleType);
        public BattleType CurrentBattleType => _tabs[_currentTabIndex].battleType;
        public IEnumerable<Equipment> CurrentEquipments => _tabs[_currentTabIndex].content.Equipments;

        public void AddTab(BattleType battleType)
        {
            _tabs.Add(new Tab(_tabs.Count, battleType));
        }

        public void Clear()
        {
            foreach (var tab in _tabs)
            {
                tab.Clear();
            }
        }

        public void Register(BattleType battleType, IItem item)
        {
            if (!TryGetTab(battleType, out var tab))
            {
                return;
            }

            if (item is not Equipment equipment)
            {
                return;
            }

            tab.content.Register(equipment);
        }

        public void Register(IItem item) => Register(CurrentBattleType, item);

        public void Deregister(BattleType battleType, IItem item)
        {
            if (!TryGetTab(battleType, out var tab))
            {
                return;
            }

            if (item is not Equipment equipment)
            {
                return;
            }

            tab.content.Deregister(equipment);
        }

        public void Deregister(IItem item) => Deregister(CurrentBattleType, item);

        private bool TryGetTab(BattleType battleType, out Tab tab)
        {
            tab = _tabs.Find(e => e.battleType == battleType);
            return tab != null;
        }

        public bool TryGetItem<T>(
            BattleType battleType,
            ItemSubType itemSubType,
            int index,
            out T item,
            out GUIContent guiContent)
            where T : IItem
        {
            if (!TryGetTab(battleType, out var tab))
            {
                item = default;
                guiContent = null;
                return false;
            }

            return tab.content.TryGetItem(itemSubType, index, out item, out guiContent);
        }

        public bool TryGetItem<T>(ItemSubType itemSubType, int index, out T item, out GUIContent guiContent)
            where T : IItem => TryGetItem(CurrentBattleType, itemSubType, index, out item, out guiContent);

        public bool TryGetItem<T>(ItemSubType itemSubType, out T item, out GUIContent guiContent)
            where T : IItem => TryGetItem(CurrentBattleType, itemSubType, index: 0, out item, out guiContent);

        public IEnumerable<Equipment> GetEquipments(BattleType battleType)
        {
            if (TryGetTab(battleType, out var tab))
            {
                return tab.content.Equipments;
            }

            return Array.Empty<Equipment>();
        }

        public IEnumerable<Equipment> GetEquipments() => GetEquipments(CurrentBattleType);
    }
}
