using System;
using System.Collections.Generic;
using System.Diagnostics;
using Libplanet.Crypto;
using Nekoyume.Action;
using Nekoyume.Arena;
using Nekoyume.Extensions;
using Nekoyume.Model;
using Nekoyume.Model.State;
using Nekoyume;
using Nekoyume.Game;
using Nekoyume.Model.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Lib9c.Renderers;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Game;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using Nekoyume.Model.Skill;
using Nekoyume.Model.State;
using Nekoyume.State;
using UnityEngine;

namespace NineChronicles.Modules.BlockSimulation.ActionSimulators
{
    public static class ModArenaSimulator
    {
        public static float ExecuteArena(
            List<Equipment> equipments,
            TableSheets tableSheets,
            States states,
            Address avatarAddress,
            List<Guid> equipmentIds,
            List<Guid> costumeIds,
            List<RuneState> runeStates,
            Address enemyAvatarAddress,
            List<Guid> enemyEquipmentIds,
            List<Guid> enemyCostumeIds,
            long blockIndex = 0,
            int? randomSeed = null,
            Action<string> onLog = null)
        {
            randomSeed ??= new RandomImpl(DateTime.Now.Millisecond).Next();
            var signerAddress = states.AgentState.address;
            var avatarState = (AvatarState)states.CurrentAvatarState.Clone();
            var avatarState = (AvatarState)states.CurrentAvatarState.Clone();

            var avatarDigest = new ArenaPlayerDigest(
                avatarState,
                equipmentIds,
                costumeIds,
                runeStates);
            var enemyDigest = new ArenaPlayerDigest(
                enemyAvatarState,
                enemyEquipmentIds,
                enemyCostumeIds,
                enemyRuneStates);

            var arenaSimulatorSheets = sheets.GetArenaSimulatorSheets();
            for (var i = 0; i < playCount; i++)
            {
                var simulator = new ArenaSimulator(
                    new RandomImpl(random.Next()),
                    BattleArena.HpIncreasingModifier);

                simulator.Simulate(
                    challenger: avatarDigest,
                    enemy: enemyDigest,
                    arenaSimulatorSheets,
                    setExtraValueBuffBeforeGetBuffs: true);
            }

            return simulator.Log.Result == ArenaResult.Win;
        }
    }
}