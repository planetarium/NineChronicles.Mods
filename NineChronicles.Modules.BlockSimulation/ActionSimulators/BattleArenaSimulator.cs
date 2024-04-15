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


namespace NineChronicles.Modules.BlockSimulation.ActionSimulators
{
    public static class BattleArenaSimulator
    {
        public static double ExecuteBulk(
            TableSheets tableSheets,
            States states,
            List<Equipment> myEquipments,
            Address enemyAvatarAddress,
            int playCount,
            Action<string> onLog = null,
            int? randomSeed = null)
        {
            int winCount = 0;

            var (gameConfigState, myDigest, enemyDigest, myCollectionState, enemyCollectionState) = PrepareStates(states, myEquipments, enemyAvatarAddress, onLog);

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
            }

            onLog?.Invoke($"{nameof(BattleArenaSimulator)} Bulk Simulation Done, {winCount}/{playCount}");

            return (double)winCount / playCount;
        }

        public static (GameConfigState, ArenaPlayerDigest myDigest, ArenaPlayerDigest enemyDigest, CollectionState myCollectionState, CollectionState enemyCollectionState) PrepareStates(
            States states,
            List<Equipment> myEquipments,
            Address enemyAvatarAddress,
            Action<string> onLog = null)
        {
            var myAvatarAddress = states.CurrentAvatarState.address;
            var (myDigest, enemyDigest) = GetArenaPlayerDigest(
                    states,
                    myAvatarAddress,
                    enemyAvatarAddress,
                    onLog);

            var rawMyCollectionState = Game.instance.Agent.GetStateAsync(
                Addresses.Collection,
                myAvatarAddress).Result;
            var myCollectionState = rawMyCollectionState is List
                ? new CollectionState((List)rawMyCollectionState)
                : new CollectionState();

            var rawEnemyCollectionState = Game.instance.Agent.GetStateAsync(
                Addresses.Collection,
                enemyAvatarAddress).Result;
            var enemyCollectionState = rawEnemyCollectionState is List
                ? new CollectionState((List)rawEnemyCollectionState)
                : new CollectionState();

            IValue state = Game.instance.Agent.GetStateAsync(ReservedAddresses.LegacyAccount, GameConfigState.Address).Result;
            if (state == null || state is Null)
            {
                onLog?.Invoke($"No game config state ({GameConfigState.Address.ToHex()})");
            }
            var gameConfigState = new GameConfigState((Dictionary)state);
            
            return (gameConfigState, myDigest, enemyDigest, myCollectionState, enemyCollectionState);
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

        private static (ArenaPlayerDigest myDigest, ArenaPlayerDigest enemyDigest) GetArenaPlayerDigest(
            States states,
            Address myAvatarAddress,
            Address enemyAvatarAddress,
            Action<string> onLog = null)
        {
            var myAvatarState = states.CurrentAvatarState;
            var enemyAvatarState =
                Game.instance.Agent.GetAvatarStatesAsync(new[] { enemyAvatarAddress }).Result[enemyAvatarAddress];
            onLog.Invoke($"{nameof(GetArenaPlayerDigest)} Enemy avatar state {enemyAvatarState.address}");

            var myItemSlotStateAddress = ItemSlotState.DeriveAddress(myAvatarAddress, BattleType.Arena);

            var rawItemSlotState = Game.instance.Agent.GetStateAsync(
                ReservedAddresses.LegacyAccount,
                myItemSlotStateAddress).Result;
            var myItemSlotState = rawItemSlotState is List
                ? new ItemSlotState((List)rawItemSlotState)
                : new ItemSlotState(BattleType.Arena);

            var myRuneSlotState = states.CurrentRuneSlotStates[BattleType.Arena];
            var myRuneStates = new List<RuneState>();
            var myRuneSlotInfos = myRuneSlotState.GetEquippedRuneSlotInfos();

            foreach (var runeId in myRuneSlotInfos.Select(r => r.RuneId))
            {
                if (states.TryGetRuneState(runeId, out var runeState))
                {
                    myRuneStates.Add(runeState);
                }
            }
            var myDigest = new ArenaPlayerDigest(myAvatarState,
                myItemSlotState.Equipments,
                myItemSlotState.Costumes,
                myRuneStates);
            var enemyItemSlotStateAddress = ItemSlotState.DeriveAddress(enemyAvatarAddress, BattleType.Arena);

            var rawEnemyItemSlotState = Game.instance.Agent.GetStateAsync(
                ReservedAddresses.LegacyAccount,
                enemyItemSlotStateAddress).Result;
            var enemyItemSlotState = rawEnemyItemSlotState is List
                ? new ItemSlotState((List)rawEnemyItemSlotState)
                : new ItemSlotState(BattleType.Arena);

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
                enemyItemSlotState.Equipments,
                enemyItemSlotState.Costumes,
                enemyRuneStates);

            return (myDigest, enemyDigest);
        }
    }
}