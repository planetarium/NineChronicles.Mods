using System.Collections.Generic;
using Nekoyume.L10n;
using Nekoyume.Model.Stat;
using Nekoyume.TableData;
using static NineChronicles.Mods.Athena.ViewModels.ItemRecipesViewModel;

namespace NineChronicles.Mods.Athena.ViewModels
{
    public class UseItemRecipeViewModel
    {
        private class Tab
        {
            public readonly int index;
            public Content content;

            public Tab(int index)
            {
                this.index = index;
            }

            public void Clear()
            {
                content = null;
            }

            public void SetContent(ItemRecipe itemRecipe)
            {
                if (itemRecipe is null)
                {
                    content = null;
                    return;
                }

                if (itemRecipe.subRecipes.Length <= index)
                {
                    content = null;
                    return;
                }

                var subRecipe = itemRecipe.subRecipes[index];
                content = new Content(
                    itemRecipe.equipmentRow,
                    subRecipe.subRecipeRow,
                    subRecipe.itemOptionRows);
            }
        }

        public class Content
        {
            public readonly int equipmentId;
            public readonly StatType statType;
            public readonly decimal statValue;
            public readonly int subRecipeId;
            public readonly List<ItemStatOpsion> itemStatOptions;
            public readonly List<ItemSkillOpsion> itemSkillOptions;

            public Content(
                EquipmentItemSheet.Row equipmentRow,
                EquipmentItemSubRecipeSheetV2.Row subRecipeRow,
                EquipmentItemOptionSheet.Row[] itemOptionRows)
            {
                equipmentId = equipmentRow.Id;
                if (equipmentRow.Stat is null)
                {
                    AthenaPlugin.Log("equipmentRow.Stat is null");
                }
                else
                {
                    statType = equipmentRow.Stat.StatType;
                    statValue = equipmentRow.Stat.TotalValue;
                }

                subRecipeId = subRecipeRow.Id;
                itemStatOptions = new List<ItemStatOpsion>();
                itemSkillOptions = new List<ItemSkillOpsion>();
                for (var i = 0; i < subRecipeRow.Options.Count; i++)
                {
                    var optionInfo = subRecipeRow.Options[i];
                    var itemOptionRow = itemOptionRows[i];
                    if (itemOptionRow is null)
                    {
                        AthenaPlugin.Log($"itemOptionRows[{i}] is null");
                        continue;
                    }

                    if (itemOptionRow.StatType == StatType.NONE &&
                        itemOptionRow.SkillId != 0)
                    {
                        var itemSkillOption = new ItemSkillOpsion(
                        itemOptionRow,
                        optionInfo.Ratio / 10_000f);
                        itemSkillOptions.Add(itemSkillOption);
                    }
                    else
                    {
                        var itemStatOption = new ItemStatOpsion(
                        itemOptionRow,
                        optionInfo.Ratio / 10_000f);
                        itemStatOptions.Add(itemStatOption);
                    }
                }
            }
        }

        public class ItemStatOpsion
        {
            public readonly int itemOptionId;
            public readonly StatType statType;
            public readonly int minValue;
            public readonly int maxValue;
            public readonly float enableChance;
            public bool enable;
            public float ratioOfValueRange;

            public ItemStatOpsion(
                EquipmentItemOptionSheet.Row row,
                float enableChance)
            {
                itemOptionId = row.Id;
                statType = row.StatType;
                minValue = row.StatMin;
                maxValue = row.StatMax;
                this.enableChance = enableChance;
                enable = true;
                ratioOfValueRange = 1f;
            }
        }

        public class ItemSkillOpsion
        {
            public readonly int itemOptionId;
            public readonly int skillId;
            public readonly string skillName;
            public readonly int skillDamageMin;
            public readonly int skillDamageMax;
            public readonly float skillChanceMin;
            public readonly float skillChanceMax;
            public readonly int statDamageRatioMin;
            public readonly int statDamageRatioMax;
            public readonly StatType referencedStatType;
            public float enableChance;
            public bool enable;
            public float ratioOfValueRange;

            public ItemSkillOpsion(
                EquipmentItemOptionSheet.Row row,
                float enableChance)
            {
                itemOptionId = row.Id;
                skillId = row.SkillId;
                skillName = L10nManager.Localize($"SKILL_NAME_{skillId}");
                skillDamageMin = row.SkillDamageMin;
                skillDamageMax = row.SkillDamageMax;
                skillChanceMin = row.SkillChanceMin / 100f;
                skillChanceMax = row.SkillChanceMax / 100f;
                statDamageRatioMin = row.StatDamageRatioMin;
                statDamageRatioMax = row.StatDamageRatioMax;
                referencedStatType = row.ReferencedStatType;
                this.enableChance = enableChance;
                enable = true;
                ratioOfValueRange = 1f;
            }
        }

        private readonly List<Tab> _tabs;

        public int CurrentTabIndex { get; private set; } = 0;

        public Content CurrentContent => _tabs[CurrentTabIndex].content;

        public UseItemRecipeViewModel()
        {
            _tabs = new List<Tab>
            {
                new Tab(0),
                new Tab(1),
                new Tab(2),
            };
        }

        public void Clear()
        {
            foreach (var tab in _tabs)
            {
                tab.Clear();
            }
        }

        public void SetItemRecipe(ItemRecipe itemRecipe)
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                var tab = _tabs[i];
                tab.SetContent(itemRecipe);
            }
        }

        public void SelectTab(int index)
        {
            CurrentTabIndex = index;
        }
    }
}
