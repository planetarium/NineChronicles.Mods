using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Nekoyume.Model.Item;
using NineChronicles.Mods.PVEHelper.Models;
using NineChronicles.Mods.PVEHelper.Utils;
using Nekoyume.Model.Item;
using System.Collections.Generic;

namespace NineChronicles.Mods.PVEHelper.Manager
{
    public class ModInventoryManager
    {
        private readonly string filePath;
        private ImmutableList<ModItem> items;

        public Equipment SelectedAura { get; set; }
        public Equipment SelectedWeapon { get; set; }
        public Equipment SelectedArmor { get; set; }
        public Equipment SelectedBelt { get; set; }
        public Equipment SelectedRing1 { get; set; }
        public Equipment SelectedRing2 { get; set; }

        public ModInventoryManager(string filePath)
        {
            this.filePath = filePath;
            items = ImmutableList<ModItem>.Empty;
            LoadItemsFromCsv();
        }

        public List<Equipment> GetEquipments()
        {
            return new List<Equipment>() { SelectedAura, SelectedWeapon, SelectedArmor, SelectedBelt, SelectedRing1, SelectedRing2 }.FindAll(e => e != null);
        }

        private void LoadItemsFromCsv()
        {
            try
            {
                var loadedItems = CsvParser<ModItem>.ParseCsv(filePath);
                items = ImmutableList.CreateRange(loadedItems);
            }
            catch (FileNotFoundException)
            {
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
