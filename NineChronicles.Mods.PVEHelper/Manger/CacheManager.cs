using System;
using System.Collections.Generic;
using System.Linq;
using NineChronicles.Mods.PVEHelper.Models;
using NineChronicles.Mods.PVEHelper.Utils;

namespace NineChronicles.Mods.PVEHelper.Manager
{
    public class CacheManager
    {
        private string filePath;

        public CacheManager(string filePath)
        {
            this.filePath = filePath;
        }

        public void AddEquipmentCache(EquipmentCache EquipmentCache)
        {
            var caches = LoadEquipmentCaches().ToList();
            caches.Add(EquipmentCache);
            SaveEquipmentCaches(caches);
        }

        public List<EquipmentCache> LoadEquipmentCaches()
        {
            return CsvParser<EquipmentCache>.ParseCsv(filePath).ToList();
        }

        public void UpdateEquipmentCache(Guid id, EquipmentCache updatedCache)
        {
            var caches = LoadEquipmentCaches();
            var index = caches.FindIndex(c => c.Id == id);
            if (index != -1)
            {
                caches[index] = updatedCache;
                SaveEquipmentCaches(caches);
            }
        }

        public void DeleteEquipmentCache(Guid id)
        {
            var caches = LoadEquipmentCaches();
            var cacheToRemove = caches.FirstOrDefault(c => c.Id == id);
            if (cacheToRemove != null)
            {
                caches.Remove(cacheToRemove);
                SaveEquipmentCaches(caches);
            }
        }

        private void SaveEquipmentCaches(List<EquipmentCache> caches)
        {
            CsvUtil.SaveToCsv(filePath, caches);
        }
    }
}