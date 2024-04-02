using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

        public void Enhancement()
        {
            if (Level < 21)
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