using System;
using System.Collections.Generic;
using System.Linq;
using Lib9c.Renderers;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Game;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Skill;
using Nekoyume.Model.State;
using Nekoyume.Model.Item;
using Nekoyume.State;
using UnityEngine;
using NineChronicles.Mods.PVEHelper.Extensions;

namespace NineChronicles.Mods.PVEHelper.BlockSimulation.Actions
{
    public static class HackAndSlashSimulation
    {
        public static int Simulate(
            List<Equipment> equipments,
            TableSheets tableSheets,
            States states,
            int worldId,
            int stageId,
            long blockIndex = 0,
            int? randomSeed = null)
        {
            randomSeed ??= new RandomImpl(DateTime.Now.Millisecond).Next();
            var signerAddress = states.AgentState.address;
            var avatarState = (AvatarState)states.CurrentAvatarState.Clone();

            // avatarState.inventory;
            var itemSlotState = states.CurrentItemSlotStates[BattleType.Adventure];

            foreach(var equipment in equipments)
            {
                if (!avatarState.inventory.HasNonFungibleItem(equipment.NonFungibleId))
                {
                    avatarState.inventory.AddItem(equipment);
                }
            }

            avatarState.EquipEquipments(equipments.Select(e => e.NonFungibleId).ToList());
            var equippedCount = avatarState.inventory.Equipments.Count(equip => equip.equipped);
            PVEHelperPlugin.Log($"equippedCount: {equippedCount} / equipments.Count: {equipments.Count}");
            var equippedIds = avatarState.inventory.Equipments.Where(equip => equip.equipped).Select(equip => equip.ItemId.ToString()).ToList();
            PVEHelperPlugin.Log(string.Join("\n", equippedIds));

            var skillState = States.Instance.CrystalRandomSkillState;
            var key = string.Format("HackAndSlash.SelectedBonusSkillId.{0}", avatarState.address);
            var skillId = PlayerPrefs.GetInt(key, 0);
            if (skillId == 0 &&
                skillState != null &&
                skillState.SkillIds.Any())
            {
                skillId = skillState.SkillIds
                    .Select(buffId =>
                        TableSheets.Instance.CrystalRandomBuffSheet
                            .TryGetValue(buffId, out var bonusBuffRow)
                            ? bonusBuffRow
                            : null)
                    .Where(x => x != null)
                    .OrderBy(x => x.Rank)
                    .ThenBy(x => x.Id)
                    .First()
                    .Id;
            }

            var action = new HackAndSlash
            {
                Costumes = itemSlotState.Costumes,
                Equipments = itemSlotState.Equipments,
                Foods = new List<Guid>(),
                RuneInfos = States.Instance.CurrentRuneSlotStates[BattleType.Adventure]
                    .GetEquippedRuneSlotInfos(),
                WorldId = worldId,
                StageId = stageId,
                StageBuffId = skillId == 0 ? null : skillId,
                AvatarAddress = avatarState.address,
            };

            PVEHelperPlugin.Log(action.PlainValue.Inspect());

            var eval = new ActionEvaluation<HackAndSlash>
            {
                PreviousState = default,
                OutputState = default,
                BlockIndex = blockIndex,
                RandomSeed = randomSeed.Value,
                Signer = signerAddress,
                Action = action,
            };
            var skillsOnWaveStart = new List<Skill>();
            if (eval.Action.StageBuffId.HasValue)
            {
                var skill = CrystalRandomSkillState.GetSkill(
                    eval.Action.StageBuffId.Value,
                    tableSheets.CrystalRandomBuffSheet,
                    tableSheets.SkillSheet);
                skillsOnWaveStart.Add(skill);
            }
            eval.GetHackAndSlashReward(
                avatarState,
                States.Instance.GetEquippedRuneStates(BattleType.Adventure),
                States.Instance.CollectionState,
                skillsOnWaveStart,
                tableSheets,
                out var simulator,
                out _);
            PVEHelperPlugin.Log($"({nameof(HackAndSlashSimulation)}) Simulate Finish {simulator.Log.clearedWaveNumber}");

            return simulator.Log.clearedWaveNumber;
        }

        public static Dictionary<int, int> Simulate(
            List<Equipment> equipments,
            TableSheets tableSheets,
            States states,
            int worldId,
            int stageId,
            int playCount,
            long blockIndex = 0,
            int? randomSeed = null)
        {
            PVEHelperPlugin.Log(
                $"({nameof(HackAndSlashSimulation)}) Simulate Start\n" +
                $"equipments: {string.Join(',', equipments.Select(e => e.NonFungibleId))}\n" +
                $"worldId: {worldId}\n" +
                $"stageId: {stageId}\n" +
                $"blockIndex: {blockIndex}\n" +
                $"randomSeed: {randomSeed}");

            var result = new Dictionary<int, int>();

            for (var i = 0; i < playCount; i++)
            {
                var clearWave = Simulate(equipments, tableSheets, states, worldId, stageId, blockIndex, randomSeed);

                if (result.TryGetValue(clearWave, out var count))
                {
                    result[clearWave] = count + 1;
                }
                else
                {
                    result[clearWave] = 1;
                }
            }
            var formattedResult = string.Join(", ", result.Select(kv => $"{kv.Key}: {kv.Value}"));
            PVEHelperPlugin.Log($"({nameof(HackAndSlashSimulation)}) Simulate Result: {formattedResult}");

            return result;
        }
    }
}
