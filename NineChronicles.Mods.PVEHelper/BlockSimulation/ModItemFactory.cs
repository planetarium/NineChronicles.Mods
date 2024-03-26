using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Action;
using Nekoyume.Model.State;
using Nekoyume.Module;
using Nekoyume.TableData;
using Nekoyume.Extensions;
using Nekoyume.Model.Item;
using Nekoyume.Model.Skill;
using Nekoyume.Model.Stat;
using NineChronicles.Mods.PVEHelper.Models;
using Libplanet.Action;
using Libplanet.Crypto;


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
            var enhancementCostSheetV2 = sheets.GetSheet<EnhancementCostSheetV2>();
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
            var subRecipe = subRecipeSheetV2[modItem.SubRecipeId.Value];
            var additionalOptionStats = equipment.StatsMap.GetAdditionalStats(false).ToArray();
            foreach (var statMapEx in additionalOptionStats)
            {
                equipment.StatsMap.SetStatAdditionalValue(statMapEx.statType, 0);
            }

            equipment.Skills.Clear();
            equipment.BuffSkills.Clear();

            var options = subRecipe.Options
                .Select(e => optionSheet[e.Id])
                .Where(o => modItem.OptionIdList.Contains(o.Id))
                .ToArray();
            foreach (var option in options)
            {
                if (option.StatType == StatType.NONE)
                {
                    var skillRow = skillSheet[option.SkillId];
                    var skill = SkillFactory.GetV1(
                        skillRow,
                        option.SkillDamageMax,
                        option.SkillChanceMax);
                    equipment.Skills.Add(skill);

                    continue;
                }

                equipment.StatsMap.AddStatAdditionalValue(option.StatType, option.StatMax);
            }

            if (modItem.Level > 0 &&
                ItemEnhancement11.TryGetRow(
                    equipment,
                    enhancementCostSheetV2,
                    out var enhancementCostRow))
            {
                for (var j = 0; j < modItem.Level; j++)
                {
                    equipment.LevelUp(random, enhancementCostRow, true);
                }
            }

            return equipment;
        }
    }
}
