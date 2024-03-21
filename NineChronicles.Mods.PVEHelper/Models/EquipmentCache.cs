using System;
using System.Collections.Immutable;

namespace NineChronicles.Mods.PVEHelper.Models
{
    public class EquipmentCache
    {
        public Guid Id {get; set;}

        public int EquipmentId {get; set;}

        public int Level {get; set;}

        public int SubRecipeId {get; set;}

        public ImmutableList<int> OptionIdList {get; set;}
    }
}