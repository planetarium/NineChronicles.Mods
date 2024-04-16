using System;
using System.Collections.Generic;
using Libplanet.Crypto;
using Nekoyume.Action;
using Nekoyume.Arena;
using Nekoyume.Model;
using Nekoyume.Model.State;
using Nekoyume.Game;
using Nekoyume.Model.Item;
using System.Linq;
using Nekoyume.Model.EnumType;
using Nekoyume.State;
using Bencodex.Types;
using Cysharp.Threading.Tasks;
using Libplanet.Action.State;
using Nekoyume.Model.BattleStatus.Arena;
using Nekoyume;
using NineChronicles.Modules.BlockSimulation.Extensions;


namespace NineChronicles.Modules.BlockSimulation.ActionSimulators
{
    public static class BattleArenaSimulator
    {
        public static double ExecuteBulk(
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
            int winCount = 0;
            var myAvatarAddress = states.CurrentAvatarState.address;

            if (onLog is not null)
            {
                var log = $"avatar: {states.CurrentAvatarState.name}({myAvatarAddress})" +
                    $"\nequipments: {(modEquipments is null ? "null" : string.Join(", ", modEquipments.Select(e => $"{e.GetLocalizedNonColoredName(false)}(+{e.level})")))}" +
                    $"\ncostumes: {(modCostumes is null ? "null" : string.Join(", ", modCostumes.Select(e => e.GetLocalizedNonColoredName(false))))}" +
                    $"\nconsumables: {(modConsumables is null ? "null" : string.Join(", ", modConsumables.Select(e => e.GetLocalizedNonColoredName(false))))}" +
                    $"\nplayCount: {playCount}";
                onLog.Invoke(log);
            }

            var (myDigest, enemyDigest) = GetArenaPlayerDigest(
                    states,
                    modEquipments,
                    modCostumes,
                    modConsumables,
                    enemyAvatarAddress,
                    onLog);
            var myCollectionState = states.CollectionState;
            var enemyCollectionState = GetCollectionState(enemyAvatarAddress, onLog);

            var gameConfigState = states.GameConfigState;

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
                    randomSeed);

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

            var arenaSimulatorSheets = tableSheets.GetArenaSimulatorSheets();
                
            var simulator = new ArenaSimulator(
                new RandomImpl(randomSeed.Value),
                BattleArena.HpIncreasingModifier,
                gameConfigState.ShatterStrikeMaxDamage);
            var log = simulator.Simulate(
                myDigest,
                enemyDigest,
                arenaSimulatorSheets,
                myCollectionState.GetEffects(tableSheets.CollectionSheet),
                enemyCollectionState.GetEffects(tableSheets.CollectionSheet),
                tableSheets.DeBuffLimitSheet,
                true);
            onLog?.Invoke($"{nameof(BattleArenaSimulator)} Done, result: {log.Result == ArenaLog.ArenaResult.Win}");

            return log.Result == ArenaLog.ArenaResult.Win;
        }

        public static CollectionState  GetCollectionState(
            Address avatarAddress,
            Action<string> onLog = null)
        {
            var rawCollectionState = Game.instance.Agent.GetStateAsync(
                Addresses.Collection,
                avatarAddress).Result;
            var collectionState = rawCollectionState is List
                ? new CollectionState((List)rawCollectionState)
                : new CollectionState();
            
            return collectionState;
        }

        private static (ArenaPlayerDigest myDigest, ArenaPlayerDigest enemyDigest) GetArenaPlayerDigest(
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

            var equippedRuneStates = states.GetEquippedRuneStates(BattleType.Arena);
            var myDigest = new ArenaPlayerDigest(
                myAvatarState,
                equippedRuneStates);

            // Enemy
            var enemyAvatarState =
                Game.instance.Agent.GetAvatarStatesAsync(new[] { enemyAvatarAddress }).Result[enemyAvatarAddress];
            onLog.Invoke($"{nameof(GetArenaPlayerDigest)} Enemy avatar state {enemyAvatarState.address}");

            var enemyRuneSlotStateAddress = RuneSlotState.DeriveAddress(enemyAvatarAddress, BattleType.Arena);
            
            var rawEnemyRuneSlotState = Game.instance.Agent.GetStateAsync(
                ReservedAddresses.LegacyAccount,
                enemyRuneSlotStateAddress).Result;
            var enemyRuneSlotState = rawEnemyRuneSlotState is List
                ? new RuneSlotState((List)rawEnemyRuneSlotState)
                : new RuneSlotState(BattleType.Arena);

            var enemyRuneStates = new List<RuneState>();
            var enemyRuneSlotInfos = enemyRuneSlotState.GetEquippedRuneSlotInfos();
            var runeAddresses = enemyRuneSlotInfos.Select(info =>
                RuneState.DeriveAddress(enemyAvatarAddress, info.RuneId));
            foreach (var address in runeAddresses)
            {
                var rawRuneState = Game.instance.Agent.GetStateAsync(
                    ReservedAddresses.LegacyAccount,
                    address).Result;

                if (rawRuneState is List)
                {
                    enemyRuneStates.Add(new RuneState((List)rawRuneState));
                }
            }
            var enemyDigest = new ArenaPlayerDigest(enemyAvatarState,
                enemyRuneStates);

            return (myDigest, enemyDigest);
        }
    }
}