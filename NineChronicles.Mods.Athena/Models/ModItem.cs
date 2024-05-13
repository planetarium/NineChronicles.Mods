using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Nekoyume.Game;
using Nekoyume.Model.Item;

namespace NineChronicles.Mods.Athena.Models
{
    public class ModItem
    {
        [NonSerialized]
        private (int optionId, float ratioOfOptionValue)[] _optionTuples;

        public Guid Id { get; set; }

        public int EquipmentId { get; set; }

        public int Level { get; set; }

        public bool ExistsItem { get; set; } = false;

        public int? SubRecipeId { get; set; }

        public ImmutableList<int> OptionIdList { get; set; }
        public ImmutableList<float> RatioOfOptionValueRangeList { get; set; }

        public IReadOnlyCollection<(int optionId, float ratioOfOptionValue)> GetOptionTuples()
        {
            if (_optionTuples == null)
            {
                if (RatioOfOptionValueRangeList.Count < OptionIdList.Count)
                {
                    RatioOfOptionValueRangeList = RatioOfOptionValueRangeList
                        .Concat(Enumerable.Repeat(1f, OptionIdList.Count - RatioOfOptionValueRangeList.Count))
                        .ToImmutableList();
                }

                _optionTuples = OptionIdList.Zip(
                    RatioOfOptionValueRangeList,
                    (optionId, ratioOfOptionValue) => (optionId, ratioOfOptionValue)).ToArray();
            }

            return _optionTuples;
        }

        public void Enhancement(TableSheets sheets)
        {
            var enhancementSheet = sheets.EnhancementCostSheetV3;
            var equipmentItemSheet = sheets.EquipmentItemSheet;
            var equipment = equipmentItemSheet[EquipmentId];

            var maxLevel = enhancementSheet.OrderedList.Where(x => x.Grade == equipment.Grade).Max(x => x.Level);
            if (Level <= maxLevel)
            {
                Level += 1;
            }
        }

        public void Downgrade()
        {
            if (Level > 0)
            {
                Level -= 1;
            }
        }
    }
}