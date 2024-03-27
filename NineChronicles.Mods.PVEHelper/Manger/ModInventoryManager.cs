using System;
using System.Linq;
using System.IO;
using System.Collections.Immutable;
using NineChronicles.Mods.PVEHelper.Models;
using NineChronicles.Mods.PVEHelper.Utils;
using Nekoyume.Model.Item;

namespace NineChronicles.Mods.PVEHelper.Manager
{
    public class ModInventoryManager
    {
        private ImmutableList<ModItem> items;
        private string filePath;

        public Equipment? SelectedAura { get; set; }
        public Equipment? SelectedWeapon { get; set; }
        public Equipment? SelectedArmor { get; set; }
        public Equipment? SelectedBelt { get; set; }
        public Equipment? SelectedRing1 { get; set; }
        public Equipment? SelectedRing2 { get; set; }

        public ModInventoryManager(string filePath)
        {
            this.filePath = filePath;
            items = ImmutableList<ModItem>.Empty;
            LoadItemsFromCsv();
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
