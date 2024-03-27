using System;
using System.Collections.Generic;
using System.Linq;
using Libplanet.Action;
using Libplanet.Crypto;
using BepInEx.Logging;
using Nekoyume.Game;
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
            TableSheets tableSheets,
            ModItem modItem)
        {
            var randomSeed = new RandomImpl(DateTime.Now.Millisecond).Next();

            IRandom random = new RandomImpl(randomSeed);

            var equipmentItemSheet = tableSheets.EquipmentItemSheet;
            var enhancementCostSheetV3 = tableSheets.EnhancementCostSheetV3;
            var recipeSheet = tableSheets.EquipmentItemRecipeSheet;
            var subRecipeSheetV2 = tableSheets.EquipmentItemSubRecipeSheetV2;
            var optionSheet = tableSheets.EquipmentItemOptionSheet;
            var skillSheet = tableSheets.SkillSheet;

            if (!equipmentItemSheet.TryGetValue(modItem.EquipmentId, out var itemRow, true))
            {
                throw new Exception();
            }

            // NOTE: Do not use `level` argument at here.
            var equipment = (Equipment)ItemFactory.CreateItemUsable(
                itemRow,
                modItem.Id,
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

            if (modItem.Level > 0)
            {
                equipment.SetLevel(random, modItem.Level, enhancementCostSheetV3);
            }

            return equipment;
        }

        public static Equipment ModifyLevel(
            TableSheets tableSheets,
            Equipment existsItem,
            ModItem modItem)
        {
            var randomSeed = new RandomImpl(DateTime.Now.Millisecond).Next();
            IRandom random = new RandomImpl(randomSeed);
            var enhancementCostSheetV3 = tableSheets.EnhancementCostSheetV3;
            var equipment = new Equipment((Bencodex.Types.Dictionary)existsItem.Serialize());
            
            if (modItem.Level > 0)
            {
                equipment.SetLevel(random, modItem.Level, enhancementCostSheetV3);
            }

            return equipment;
        }
    }
}
