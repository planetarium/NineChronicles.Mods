using System;
using System.Linq;
using Libplanet.Action;
using Nekoyume.Game;
using Nekoyume.Model.Item;
using Nekoyume.Model.Skill;
using Nekoyume.Model.Stat;
using Nekoyume.Model.State;
using NineChronicles.Mods.Athena.BlockSimulation;
using NineChronicles.Mods.Athena.Models;


namespace NineChronicles.Mods.Athena.Factories
{
    public static class ModItemFactory
    {
        public static Equipment CreateEquipmentWithModItem(
            TableSheets tableSheets,
            ModItem modItem)
        {
            if (tableSheets is null)
            {
                AthenaPlugin.Log("TableSheets is null.");
                return null;
            }

            if (modItem is null)
            {
                AthenaPlugin.Log("ModItem is null.");
                return null;
            }

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
                requiredBlockIndex: 0);

            // NOTE: This condition about `Grade` can be changed.
            if (equipment.Grade == 0)
            {
                return equipment;
            }

            var recipe = recipeSheet.OrderedList!
                .First(e => e.ResultEquipmentId == modItem.EquipmentId);
            var subRecipeRow = subRecipeSheetV2[modItem.SubRecipeId.Value];
            var additionalOptionStats = equipment.StatsMap.GetAdditionalStats(false).ToArray();

            foreach (var (statType, _) in additionalOptionStats)
            {
                equipment.StatsMap.SetStatAdditionalValue(statType, 0);
            }

            equipment.Skills.Clear();
            equipment.BuffSkills.Clear();

            if (modItem.OptionIdList != null)
            {
                var options = modItem.OptionIdList
                .Where(e => optionSheet.ContainsKey(e))
                .Select(e => optionSheet[e])
                .ToArray();
                foreach (var (optionId, ratio) in modItem.GetOptionTuples())
                {
                    if (!optionSheet.TryGetValue(optionId, out var optionRow))
                    {
                        AthenaPlugin.LogWarning($"Option not found: {optionId}");
                        continue;
                    }

                    var optionIndex = modItem.OptionIdList.IndexOf(optionRow.Id);
                    if (optionRow.StatType == StatType.NONE)
                    {
                        var skillRow = skillSheet[optionRow.SkillId];
                        var skill = SkillFactory.Get(
                            skillRow,
                            (long)(optionRow.SkillDamageMin + ((optionRow.SkillDamageMax - optionRow.SkillDamageMin) * ratio)),
                            (int)(optionRow.SkillChanceMin + ((optionRow.SkillChanceMax - optionRow.SkillChanceMin) * ratio)),
                            (int)(optionRow.StatDamageRatioMin + ((optionRow.StatDamageRatioMax - optionRow.StatDamageRatioMin) * ratio)),
                            optionRow.ReferencedStatType);
                        equipment.Skills.Add(skill);
                        continue;
                    }

                    equipment.StatsMap.AddStatAdditionalValue(
                        optionRow.StatType,
                        (decimal)(optionRow.StatMin + ((optionRow.StatMax - optionRow.StatMin) * ratio)));
                }
            }

            if (modItem.Level > 0)
            {
                // NOTE: Set fake ratio to 1 for getting the maximum value.
                IRandom random = new RandomFakeImpl(0, 1m);
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
