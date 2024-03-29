using System;
using System.Collections.Immutable;

namespace NineChronicles.Mods.PVEHelper.Models
{
    public class ModItem
    {
        public Guid Id { get; set; }

        public int EquipmentId { get; set; }

        public int Level { get; set; }

        public bool ExistsItem { get; set; } = false;

        public int? SubRecipeId { get; set; }

        public ImmutableList<int> OptionIdList { get; set; }
        public ImmutableList<float> RatioOfOptionValueRangeList { get; set; }

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