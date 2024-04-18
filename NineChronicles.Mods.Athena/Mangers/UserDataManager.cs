using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using NineChronicles.Mods.Athena.GUIs;
using UnityEngine;

namespace NineChronicles.Mods.Athena.Mangers
{
    public static class UserDataManager
    {
        private static readonly Dictionary<BattleType, IEnumerable<Equipment>> _itemSlotsCache = new();

        public static IEnumerable<Equipment> GetItemSlotsCache(BattleType battleType)
        {
            return _itemSlotsCache.TryGetValue(battleType, out var equipments)
                ? equipments
                : Enumerable.Empty<Equipment>();
        }

        public static void SaveItemSlots(
            BattleType battleType,
            IEnumerable<Equipment> equipments)
        {
            _itemSlotsCache[battleType] = equipments;
            var value = string.Join(";", equipments.Select(e => e.NonFungibleId.ToString()));
            PlayerPrefs.SetString($"athena_item_slots_{battleType}", value);
        }

        public static List<Equipment> LoadItemSlots(
            BattleType battleType,
            InventoryGUI inventoryGUI)
        {
            var result = new List<Equipment>();
            var value = PlayerPrefs.GetString($"athena_item_slots_{battleType}", string.Empty);
            if (string.IsNullOrEmpty(value))
            {
                _itemSlotsCache[battleType] = result;
                return result;
            }

            var nonFungibleIdStrings = value.Split(';');
            foreach (var nonFungibleIdString in nonFungibleIdStrings)
            {
                if (Guid.TryParse(nonFungibleIdString, out var nonFungibleId) &&
                    inventoryGUI.TryGetItem(nonFungibleId, out Equipment item))
                {
                    result.Add(item);
                }
            }

            _itemSlotsCache[battleType] = result;
            return result;
        }
    }
}
