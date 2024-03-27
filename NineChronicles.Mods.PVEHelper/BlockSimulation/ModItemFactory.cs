using System;
using System.Collections.Generic;
using System.Linq;
using Libplanet.Action;
using Libplanet.Crypto;
using Nekoyume.Action;
using Nekoyume.Extensions;
using Nekoyume.Model.Item;
using Nekoyume.Model.Skill;
using Nekoyume.Model.Stat;
using Nekoyume.Model.State;
using Nekoyume.Module;
using Nekoyume.TableData;
using NineChronicles.Mods.PVEHelper.Models;


namespace NineChronicles.Mods.PVEHelper.BlockSimulation
{
    public static class ModItemFactory
    {
        public static Equipment CreateEquipmentWithModItem(
            IRandom random,
            Dictionary<Type, (Address address, ISheet sheet)> sheets,
            ModItem modItem)
        {
            var equipmentItemSheet = sheets.GetSheet<EquipmentItemSheet>();
            var enhancementCostSheetV3 = sheets.GetSheet<EnhancementCostSheetV3>();
            var recipeSheet = sheets.GetSheet<EquipmentItemRecipeSheet>();
            var subRecipeSheetV2 = sheets.GetSheet<EquipmentItemSubRecipeSheetV2>();
            var optionSheet = sheets.GetSheet<EquipmentItemOptionSheet>();
            var skillSheet = sheets.GetSheet<SkillSheet>();

            if (!equipmentItemSheet.TryGetValue(modItem.EquipmentId, out var itemRow, true))
            {
                throw new Exception();
            }

            // NOTE: Do not use `level` argument at here.
            var equipment = (Equipment)ItemFactory.CreateItemUsable(
                itemRow,
                random.GenerateRandomGuid(),
                1);
            if (equipment.Grade == 0)
            {
                return equipment;
            }

            var recipe = recipeSheet.OrderedList!
                .First(e => e.ResultEquipmentId == modItem.EquipmentId);
            var subRecipeRow = subRecipeSheetV2[modItem.SubRecipeId.Value];
            var additionalOptionStats = equipment.StatsMap.GetAdditionalStats(false).ToArray();
            foreach (var statMapEx in additionalOptionStats)
            {
                equipment.StatsMap.SetStatAdditionalValue(statMapEx.statType, 0);
            }

            equipment.Skills.Clear();
            equipment.BuffSkills.Clear();

            var options = modItem.OptionIdList
                .Where(e => optionSheet.ContainsKey(e))
                .Select(e => (
                    optionSheet[e],
                    modItem.RatioOfOptionValueRangeList[modItem.OptionIdList.IndexOf(e)]))
                .ToArray();
            foreach (var (optionRow, ratio) in options)
            {
                if (optionRow.StatType == StatType.NONE)
                {
                    var skillRow = skillSheet[optionRow.SkillId];
                    var skill = SkillFactory.Get(
                        skillRow,
                        (int)(optionRow.SkillDamageMax * ratio),
                        (int)(optionRow.SkillChanceMax * ratio),
                        (int)(optionRow.StatDamageRatioMax * ratio),
                        optionRow.ReferencedStatType);
                    equipment.Skills.Add(skill);

                    continue;
                }

                equipment.StatsMap.AddStatAdditionalValue(optionRow.StatType, optionRow.StatMax);
            }

            if (modItem.Level > 0 &&
                ItemEnhancement.TryGetRow(
                    equipment,
                    enhancementCostSheetV3,
                    out var enhancementCostRow))
            {
                equipment.SetLevel(random, modItem.Level, enhancementCostSheetV3);
            }

            return equipment;
        }
    }
}
