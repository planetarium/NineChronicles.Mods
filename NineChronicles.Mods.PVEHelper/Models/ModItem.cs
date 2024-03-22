using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NineChronicles.Mods.PVEHelper.Models
{
    public class ModItem
    {
        public Guid Id {get; set;}

        public int EquipmentId {get; set;}

        public int Level {get; set;}

        public bool ExistsItem {get; set;} = false;

        public int? SubRecipeId {get; set;}

        public ImmutableList<int>? OptionIdList {get; set;}

        public void Enhancement()
        {
            Level += 1;
        }
        
        public void Downgrade()
        {
            if (Level <= 0)
            {
                throw new InvalidOperationException("Level cannot be downgraded further.");
            }

            Level -= 1;
        }
    }
}