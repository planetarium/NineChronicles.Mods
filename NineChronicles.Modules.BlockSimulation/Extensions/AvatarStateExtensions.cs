using System.Collections.Generic;
using Bencodex.Types;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;

namespace NineChronicles.Modules.BlockSimulation.Extensions
{
    public static class AvatarStateExtensions
    {
        /// <summary>
        /// AvatarStates.Inventory contains all items, so we need to make a flyweight AvatarState
        /// with only equipped and used items.
        /// </summary>
        public static AvatarState MakeFlyweightAvatarState(
            this AvatarState avatarState,
            IEnumerable<Equipment> equipments,
            IEnumerable<Costume> costumes,
            IEnumerable<Consumable> consumables)
        {
            var cloned = new AvatarState((List)avatarState.SerializeList())
            {
                inventory = new Inventory(),
            };
            if (equipments is not null)
            {
                foreach (var equipment in equipments)
                {
                    equipment.Equip();
                    cloned.inventory.AddNonFungibleItem(equipment);
                }
            }

            if (costumes is not null)
            {
                foreach (var costume in costumes)
                {
                    costume.Equip();
                    cloned.inventory.AddNonFungibleItem(costume);
                }
            }

            if (consumables is not null)
            {
                foreach (var consumable in consumables)
                {
                    cloned.inventory.AddFungibleItem(consumable);
                }
            }

            return cloned;
        }
    }
}
