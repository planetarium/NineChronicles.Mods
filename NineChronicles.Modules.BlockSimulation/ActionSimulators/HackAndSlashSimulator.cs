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
using Nekoyume.TableData;
using Nekoyume.TableData.Crystal;
using States = Nekoyume.State.States;

namespace NineChronicles.Modules.BlockSimulation.ActionSimulators
{
    public static class HackAndSlashSimulator
    {
        public static int Simulate(
            AvatarState avatarState,
            IEnumerable<Equipment> equipments,
            IEnumerable<Costume> costumes,
            IEnumerable<Consumable> consumables,
            int worldId,
            int stageId,
            int? stageBuffId,
            TableSheets tableSheets,
            States states,
            long blockIndex = 0,
            int? randomSeed = null,
            Action<string> onLog = null)
        {
            equipments ??= new List<Equipment>();
            costumes ??= new List<Costume>();
            consumables ??= new List<Consumable>();
            avatarState ??= MakeFlyweightAvatarState(
                states.CurrentAvatarState,
                equipments,
                costumes,
                consumables);
            var action = new HackAndSlash
            {
                Costumes = costumes.Select(e => e.NonFungibleId).ToList(),
                Equipments = equipments.Select(e => e.NonFungibleId).ToList(),
                Foods = consumables.Select(e => e.NonFungibleId).ToList(),
                RuneInfos = states.CurrentRuneSlotStates[BattleType.Adventure].GetEquippedRuneSlotInfos(),
                WorldId = worldId,
                StageId = stageId,
                StageBuffId = stageBuffId,
                AvatarAddress = avatarState.address,
            };
            onLog?.Invoke(action.PlainValue.Inspect());

            randomSeed ??= new RandomImpl(DateTime.Now.Millisecond).Next();
            var eval = new ActionEvaluation<HackAndSlash>
            {
                PreviousState = default,
                OutputState = default,
                BlockIndex = blockIndex,
                RandomSeed = randomSeed.Value,
                Signer = states.AgentState.address,
                Action = action,
            };
            var runeStates = states.GetEquippedRuneStates(BattleType.Adventure);
            var collectionState = states.CollectionState;
            var skillsOnWaveStart = GetSkillsOnWaveStart(
                stageBuffId,
                tableSheets.CrystalRandomBuffSheet,
                tableSheets.SkillSheet);
            eval.GetHackAndSlashReward(
                avatarState,
                runeStates,
                collectionState,
                skillsOnWaveStart,
                tableSheets,
                out var simulator,
                out _);
            onLog?.Invoke($"({nameof(HackAndSlashSimulator)}) Simulate Finish {simulator.Log.clearedWaveNumber}");

            return simulator.Log.clearedWaveNumber;
        }

        public static Dictionary<int, int> Simulate(
            IEnumerable<Equipment> equipments,
            IEnumerable<Costume> costumes,
            IEnumerable<Consumable> consumables,
            int worldId,
            int stageId,
            int? stageBuffId,
            TableSheets tableSheets,
            States states,
            int playCount,
            [CanBeNull] Action<int> onProgress = null,
            long blockIndex = 0,
            int? randomSeed = null,
            Action<string> onLog = null)
        {
            equipments ??= Array.Empty<Equipment>();
            costumes ??= Array.Empty<Costume>();
            consumables ??= Array.Empty<Consumable>();

            onLog?.Invoke(
                $"({nameof(HackAndSlashSimulator)}) Simulate Start\n" +
                $"equipments: {string.Join(',', equipments.Select(e => e.NonFungibleId))}\n" +
                $"costumes: {string.Join(',', costumes.Select(e => e.NonFungibleId))}\n" +
                $"consumables: {string.Join(',', consumables.Select(e => e.NonFungibleId))}\n" +
                $"worldId: {worldId}\n" +
                $"stageId: {stageId}\n" +
                $"stageBuffId: {stageBuffId}\n" +
                $"playCount: {playCount}\n" +
                $"blockIndex: {blockIndex}\n" +
                $"randomSeed: {randomSeed}");
            var result = new Dictionary<int, int>
            {
                { 1, 0 },
                { 2, 0 },
                { 3, 0 },
            };
            var avatarState = MakeFlyweightAvatarState(
                states.CurrentAvatarState,
                equipments,
                costumes,
                consumables);
            for (var i = 0; i < playCount; i++)
            {
                var clearWave = Simulate(
                    avatarState,
                    equipments,
                    costumes,
                    consumables,
                    worldId,
                    stageId,
                    stageBuffId,
                    tableSheets,
                    states,
                    blockIndex,
                    randomSeed);
                result[clearWave] += 1;
                onProgress?.Invoke(i + 1);
            }

            var formattedResult = string.Join(", ", result.Select(kv => $"{kv.Key}: {kv.Value}"));
            onLog?.Invoke($"({nameof(HackAndSlashSimulator)}) Simulate Result: {formattedResult}");

            return result;
        }

        public static AvatarState MakeFlyweightAvatarState(
            AvatarState avatarState,
            IEnumerable<Equipment> equipments,
            IEnumerable<Costume> costumes,
            IEnumerable<Consumable> consumables)
        {
            avatarState.inventory = new Inventory();
            var cloned = (AvatarState)avatarState.Clone();
            foreach (var equipment in equipments)
            {
                equipment.Equip();
                cloned.inventory.AddNonFungibleItem(equipment);
            }

            foreach (var costume in costumes)
            {
                costume.Equip();
                cloned.inventory.AddNonFungibleItem(costume);
            }

            foreach (var consumable in consumables)
            {
                cloned.inventory.AddFungibleItem(consumable);
            }

            return cloned;
        }

        public static List<Skill> GetSkillsOnWaveStart(
            int? stageBuffId,
            CrystalRandomBuffSheet crystalRandomBuffSheet,
            SkillSheet skillSheet)
        {
            if (!stageBuffId.HasValue)
            {
                return new List<Skill>();
            }

            var skill = CrystalRandomSkillState.GetSkill(
                    stageBuffId.Value,
                    crystalRandomBuffSheet,
                    skillSheet);
            return new List<Skill> { skill };
        }
    }
}
