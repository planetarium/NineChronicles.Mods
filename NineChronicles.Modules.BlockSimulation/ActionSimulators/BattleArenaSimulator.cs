// using System;
// using System.Collections.Generic;
// using Libplanet.Crypto;
// using Nekoyume.Action;
// using Nekoyume.Blockchain;
// using Nekoyume.Arena;
// using Nekoyume.Model;
// using Nekoyume.Model.State;
// using Nekoyume.Game;
// using Nekoyume.Model.Item;
// using System.Linq;
// using Nekoyume.Model.EnumType;
// using Nekoyume.State;
// using Nekoyume.Module;
// using Libplanet.Action;
// using Bencodex.Types;


// namespace NineChronicles.Modules.BlockSimulation.ActionSimulators
// {
//     public static class BattleArenaSimulator
//     {
//         public static float ExecuteArena(
//             TableSheets tableSheets,
//             States states,
//             List<Equipment> myEquipments,
//             Address enemyAvatarAddress,
//             long blockIndex = 0,
//             int? randomSeed = null,
//             Action<string> onLog = null)
//         {
//             randomSeed ??= new RandomImpl(DateTime.Now.Millisecond).Next();
//             var signerAddress = states.AgentState.address;
//             IRandom random = new RandomFakeImpl(0, 1m);

//             var myAvatarState = (AvatarState)states.CurrentAvatarState.Clone();
//             var myItemSlotState = states.CurrentItemSlotStates[BattleType.Arena];

//             foreach (var equipment in myEquipments)
//             {
//                 if (!myAvatarState.inventory.HasNonFungibleItem(equipment.NonFungibleId))
//                 {
//                     myAvatarState.inventory.AddItem(equipment);
//                 }
//             }

//             myAvatarState.EquipEquipments(myEquipments.Select(e => e.NonFungibleId).ToList());
//             var equippedCount = myAvatarState.inventory.Equipments.Count(equip => equip.equipped);
//             onLog?.Invoke($"equippedCount: {equippedCount} / equipments.Count: {myEquipments.Count}");
//             var myEquippedIds = myAvatarState.inventory.Equipments.Where(equip => equip.equipped).Select(equip => equip.ItemId).ToList();
//             onLog?.Invoke(string.Join("\n", myEquippedIds));

//             var enemyAvatarStateValue = Game.instance.Agent.GetAvatarStatesAsync(new[] { enemyAvatarAddress }).GetAwaiter().GetResult();
//             var enemyAvatarState = enemyAvatarStateValue[enemyAvatarAddress];

//             // ActionRenderHandler 안에 PrepareBattleArena, ResponseBattleArenaAsync에서 하는걸 구현한다고 생각하면 됨
//             // GetArenaPlayerDigest
//             // outputStates를 얻는 방법
//             // ()
//             // R

//             var enemyItemSlotStateAddress = ItemSlotState.DeriveAddress(enemyAvatarAddress, BattleType.Arena);
//             var enemyItemSlotState = states.TryGetLegacyState(enemyItemSlotStateAddress, out List rawEnemyItemSlotState)
//                 ? new ItemSlotState(rawEnemyItemSlotState)
//                 : new ItemSlotState(BattleType.Arena);
//             var enemyRuneSlotStateAddress = RuneSlotState.DeriveAddress(enemyAvatarAddress, BattleType.Arena);
//             var enemyRuneSlotState = states.TryGetLegacyState(enemyRuneSlotStateAddress, out List enemyRawRuneSlotState)
//                 ? new RuneSlotState(enemyRawRuneSlotState)
//                 : new RuneSlotState(BattleType.Arena);

//             var enemyRuneStates = new List<RuneState>();
//             var enemyRuneSlotInfos = enemyRuneSlotState.GetEquippedRuneSlotInfos();
//             foreach (var address in enemyRuneSlotInfos.Select(info => RuneState.DeriveAddress(enemyAvatarAddress, info.RuneId)))
//             {
//                 if (states.TryGetLegacyState(address, out List rawRuneState))
//                 {
//                     enemyRuneStates.Add(new RuneState(rawRuneState));
//                 }
//             }

//             var avatarDigest = new ArenaPlayerDigest(
//                 myAvatarState,
//                 myEquippedIds,
//                 myItemSlotState.Costumes,
//                 states.RuneStates);
//             var enemyDigest = new ArenaPlayerDigest(
//                 enemyAvatarState,
//                 enemyItemSlotState.Equipments,
//                 enemyItemSlotState.Costumes,
//                 enemyRuneStates);


//             var modifiers = new Dictionary<Address, List<StatModifier>>
//             {
//                 [myAvatarAddress] = new(),
//                 [enemyAvatarAddress] = new(),
//             };
//             if (collectionExist)
//             {
//                 var collectionSheet = sheets.GetSheet<CollectionSheet>();
//                 foreach (var (address, state) in collectionStates)
//                 {
//                     var modifier = modifiers[address];
//                     foreach (var collectionId in state.Ids)
//                     {
//                         modifier.AddRange(collectionSheet[collectionId].StatModifiers);
//                     }
//                 }
//             }

//             var arenaSimulatorSheets = tableSheets.GetArenaSimulatorSheets();

//             var gameConfigState = states.GetGameConfigState();
//             var simulator = new ArenaSimulator(
//                 new RandomImpl(random.Next()),
//                 BattleArena.HpIncreasingModifier
//                 gameConfigState.ShatterStrikeMaxDamage);

//             simulator.Simulate(
//                 challenger: avatarDigest,
//                 enemy: enemyDigest,
//                 arenaSimulatorSheets,
//                 modifiers[myAvatarState.address],
//                 modifiers[enemyAvatarAddress],
//                 tableSheets.DeBuffLimitSheet,
//                 true);

//             return simulator.Log.Result == ArenaResult.Win;
//         }
//     }
// }