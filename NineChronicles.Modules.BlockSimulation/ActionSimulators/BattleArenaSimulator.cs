using System;
using System.Collections.Generic;
using System.Linq;
using Bencodex.Types;
using Cysharp.Threading.Tasks;
using Libplanet.Crypto;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Arena;
using Nekoyume.Game;
using Nekoyume.Model;
using Nekoyume.Model.BattleStatus.Arena;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using Nekoyume.State;
using NineChronicles.Modules.BlockSimulation.Extensions;

namespace NineChronicles.Modules.BlockSimulation.ActionSimulators
{
    /// <summary>
    /// <seealso cref="Nekoyume.Action.BattleArena"/>
    /// <seealso cref="Nekoyume.Blockchain.ActionManager.BattleArena"/>
    /// <seealso cref="Nekoyume.Blockchain.ActionRenderHandler.ResponseBattleArenaAsync"/>
    /// <seealso cref="Nekoyume.Blockchain.ActionRenderHandler.GetArenaPlayerDigest"/>
    /// </summary>
    public static class BattleArenaSimulator
    {
        public static async UniTask<double> ExecuteBulkAsync(
            TableSheets tableSheets,
            States states,
            IEnumerable<Equipment> modEquipments,
            IEnumerable<Costume> modCostumes,
            IEnumerable<Consumable> modConsumables,
            Address enemyAvatarAddress,
            int playCount,
            Action<int> onProgress = null,
            Action<string> onLog = null,
            int? randomSeed = null)
        {
            var winCount = 0;
            var myAvatarAddress = states.CurrentAvatarState.address;
            var (myDigest, enemyDigest) = await GetArenaPlayerDigestTuple(
                states,
                modEquipments,
                modCostumes,
                modConsumables,
                enemyAvatarAddress,
                onLog
            );
            var myCollectionState = states.CollectionState;
            var enemyCollectionState = await GetCollectionState(enemyAvatarAddress, onLog);
            var gameConfigState = states.GameConfigState;
            if (onLog is not null)
            {
                var log =
                    $"avatar: {states.CurrentAvatarState.name}({myAvatarAddress})"
                    + $"\nmyLevel: {myDigest.Level}"
                    + $"\nmyEquipments: {(myDigest.Equipments is null ? "null" : string.Join(", ", myDigest.Equipments.Select(e => $"{e.GetLocalizedNonColoredName(false)}(+{e.level})")))}"
                    + $"\nmyCostumes: {(myDigest.Costumes is null ? "null" : string.Join(", ", myDigest.Costumes.Select(e => e.GetLocalizedNonColoredName(false))))}"
                    + $"\nmyRunes: {(myDigest.Runes is null ? "null" : string.Join(", ", myDigest.Runes.Runes.Values.Select(e => $"{e.RuneId}(+{e.Level})")))}"
                    + $"\nmyRuneSlots: {(myDigest.RuneSlotState is null ? "null" : string.Join(", ", myDigest.RuneSlotState.GetEquippedRuneSlotInfos().Select(e => $"slot #{e.SlotIndex}: {e.RuneId}")))}"
                    + $"\nenemyLevel: {enemyDigest.Level}"
                    + $"\nenemyEquipments: {(enemyDigest.Equipments is null ? "null" : string.Join(", ", enemyDigest.Equipments.Select(e => $"{e.GetLocalizedNonColoredName(false)}(+{e.level})")))}"
                    + $"\nenemyCostumes: {(enemyDigest.Costumes is null ? "null" : string.Join(", ", enemyDigest.Costumes.Select(e => e.GetLocalizedNonColoredName(false))))}"
                    + $"\nenemyRunes: {(enemyDigest.Runes is null ? "null" : string.Join(", ", enemyDigest.Runes.Runes.Values.Select(e => $"{e.RuneId}(+{e.Level})")))}"
                    + $"\nenemyRuneSlots: {(enemyDigest.RuneSlotState is null ? "null" : string.Join(", ", enemyDigest.RuneSlotState.GetEquippedRuneSlotInfos().Select(e => $"slot #{e.SlotIndex}: {e.RuneId}")))}";
                onLog.Invoke(log);
            }

            for (var i = 0; i < playCount; i++)
            {
                var simulateResult = Execute(
                    tableSheets,
                    gameConfigState,
                    myDigest,
                    enemyDigest,
                    myCollectionState,
                    enemyCollectionState,
                    onLog,
                    randomSeed
                );

                if (simulateResult)
                {
                    winCount += 1;
                }

                onProgress?.Invoke(i);
            }

            onLog?.Invoke($"{nameof(BattleArenaSimulator)} Bulk Simulation Done, {winCount}/{playCount}");
            return (double)winCount / playCount;
        }

        public static bool Execute(
            TableSheets tableSheets,
            GameConfigState gameConfigState,
            ArenaPlayerDigest myDigest,
            ArenaPlayerDigest enemyDigest,
            CollectionState myCollectionState,
            CollectionState enemyCollectionState,
            Action<string> onLog = null,
            int? randomSeed = null)
        {
            randomSeed ??= new RandomImpl(DateTime.Now.Millisecond).Next();
            var simulator = new ArenaSimulator(
                new RandomImpl(randomSeed.Value),
                BattleArena.HpIncreasingModifier,
                gameConfigState.ShatterStrikeMaxDamage);
            var log = simulator.Simulate(
                myDigest,
                enemyDigest,
                tableSheets.GetArenaSimulatorSheets(),
                myCollectionState.GetEffects(tableSheets.CollectionSheet),
                enemyCollectionState.GetEffects(tableSheets.CollectionSheet),
                tableSheets.DeBuffLimitSheet,
                tableSheets.BuffLinkSheet,
                setExtraValueBuffBeforeGetBuffs: true
            );
            onLog?.Invoke($"{nameof(BattleArenaSimulator)} Done, result: {log.Result == ArenaLog.ArenaResult.Win}");
            return log.Result == ArenaLog.ArenaResult.Win;
        }

        public static async UniTask<CollectionState> GetCollectionState(
            Address avatarAddress,
            Action<string> onLog = null)
        {
            return await Game.instance.Agent.GetStateAsync(Addresses.Collection, avatarAddress)
                is List list
                ? new CollectionState(list)
                : new CollectionState();
        }

        private static async UniTask<(ArenaPlayerDigest myDigest, ArenaPlayerDigest enemyDigest)> GetArenaPlayerDigestTuple(
            States states,
            IEnumerable<Equipment> modEquipments,
            IEnumerable<Costume> modCostumes,
            IEnumerable<Consumable> modConsumables,
            Address enemyAvatarAddress,
            Action<string> onLog = null)
        {
            var myAvatarState = states.CurrentAvatarState.MakeFlyweightAvatarState(
                modEquipments,
                modCostumes,
                modConsumables);
            var myDigest = new ArenaPlayerDigest(
                myAvatarState,
                states.AllRuneState,
                states.CurrentRuneSlotStates[BattleType.Arena]);

            onLog?.Invoke($"{nameof(GetArenaPlayerDigestTuple)} Enemy avatar state {enemyAvatarAddress}");
            var enemyDigest = await Game.instance.Agent.GetArenaPlayerDigestAsync(enemyAvatarAddress);
            return (myDigest, enemyDigest);
        }
    }
}
