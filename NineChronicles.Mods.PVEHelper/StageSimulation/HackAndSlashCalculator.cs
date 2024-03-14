using System;
using System.Collections.Generic;
using Lib9c.DevExtensions;
using Libplanet.Crypto;
using Nekoyume.Model.State;
using Nekoyume.TableData;

namespace NineChronicles.Mods.PVEHelper.StageSimulation
{
    public static class HackAndSlashCalculator
    {
        public struct InputData
        {
            public int WorldId;
            public int StageId;
            public int AvatarLevel;
            public (int equipmentId, int level)[] Equipments;
            public int[] CostumeIds;
            public (int consumableId, int count)[] Foods;
            public (int runeSlotIndex, int runeId, int level)[] Runes;
            public int CrystalRandomBuffId;
        }

        public struct OutputData
        {
            /// <summary>
            /// Key: Wave number(1..), Value: Count.
            /// </summary>
            public IReadOnlyDictionary<int, int> ClearedWaves;

            /// <summary>
            /// Key: Item ID, Value: Count.
            /// </summary>
            public IReadOnlyDictionary<int, int> TotalRewards;

            public int TotalExp;
        }

        public static async void CalculateAsync(
            Dictionary<Type, (Address address, ISheet sheet)> sheets,
            AvatarState avatarState,
            InputData inputData,
            int playCount)
        {
            var randomSeed = new RandomImpl(DateTime.Now.Millisecond).Next(0, int.MaxValue);
        }
    }
}
