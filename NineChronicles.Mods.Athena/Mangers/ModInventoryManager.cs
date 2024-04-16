using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Nekoyume.Model.Item;
using NineChronicles.Mods.Athena.Models;
using NineChronicles.Mods.Athena.Utils;

namespace NineChronicles.Mods.Athena.Managers
{
    public class ModInventoryManager
    {
        private readonly string filePath;
        private ImmutableList<ModItem> items;

        public Equipment SelectedAura { get; set; }
        public Equipment SelectedWeapon { get; set; }
        public Equipment SelectedArmor { get; set; }
        public Equipment SelectedBelt { get; set; }
        public Equipment SelectedNecklace { get; set; }
        public Equipment SelectedRing1 { get; set; }
        public Equipment SelectedRing2 { get; set; }

        public ModInventoryManager(string filePath)
        {
            this.filePath = filePath;
            items = ImmutableList<ModItem>.Empty;
            LoadItemsFromCsv();
        }

        public List<Equipment> GetEquippedEquipments()
        {
            return new List<Equipment>() {
                SelectedAura,
                SelectedWeapon,
                SelectedArmor,
                SelectedBelt,
                SelectedNecklace,
                SelectedRing1,
                SelectedRing2 }.FindAll(e => e != null);
        }

        public void LoadItemsFromCsv()
        {
            try
            {
                var loadedItems = CsvParser<ModItem>.ParseCsv(filePath);
                items = ImmutableList.CreateRange(loadedItems);
            }
            catch (FileNotFoundException)
            {
                AthenaPlugin.Log("No csv file found. Creating new empty list.");
                items = ImmutableList<ModItem>.Empty;
            }
            catch (Exception e)
            {
                AthenaPlugin.LogError(e.Message);
                AthenaPlugin.Log("Failed to load items from csv file. Creating new empty list.");
                items = ImmutableList<ModItem>.Empty;
            }
        }

        public void SaveItemsToCsv()
        {
            CsvUtil.SaveToCsv(filePath, items.ToList());
        }

        public void AddItem(ModItem item)
        {
            items = items.Add(item);
            SaveItemsToCsv();
        }

        public void UpdateItem(Guid id, ModItem updatedItem)
        {
            var index = items.FindIndex(item => item.Id == id);
            if (index != -1)
            {
                items = items.SetItem(index, updatedItem);
                SaveItemsToCsv();
            }
        }

        public void DeleteItem(Guid id)
        {
            var index = items.FindIndex(item => item.Id == id);
            if (index != -1)
            {
                items = items.RemoveAt(index);
                SaveItemsToCsv();
            }
        }

        public ModItem GetItem(Guid id)
        {
            return items.FirstOrDefault(item => item.Id == id);
        }

        public ImmutableList<ModItem> GetAllItems()
        {
            return items;
        }
    }
}
