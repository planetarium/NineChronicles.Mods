using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume;
using Nekoyume.Battle;
using Nekoyume.Game;
using Nekoyume.L10n;
using Nekoyume.Model.Item;
using Nekoyume.Model.Skill;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using Nekoyume.TableData.Crystal;
using NineChronicles.Modules.BlockSimulation.Extensions;

namespace NineChronicles.Modules.BlockSimulation.ActionSimulators
{
    /// <summary>
    /// <seealso cref="Nekoyume.Action.HackAndSlash"/>
    /// <seealso cref="Nekoyume.Blockchain.ActionManager.HackAndSlash"/>
    /// <seealso cref="Nekoyume.Blockchain.ActionRenderHandler.ResponseHackAndSlashAsync"/>
    /// <seealso cref="Nekoyume.ActionEvalToViewModelExtensions.GetHackAndSlashReward"/>/
    /// </summary>
    public static class HackAndSlashSimulator
    {
        /// <summary>
        /// This method does not modify the original AvatarState.
        /// It means there is no exp gain, no item gain, and no world information update.
        /// </summary>
        /// <returns>
        /// Dictionary of cleared wave number and its count.
        /// key: 0 ~ 3.
        /// key: 0 means not cleared.
        /// value: cleared count.
        /// </returns>
        public static Dictionary<int, int> Simulate(
            TableSheets tableSheets,
            AvatarState avatarState,
            IEnumerable<Equipment> equipments,
            IEnumerable<Costume> costumes,
            IEnumerable<Consumable> consumables,
            AllRuneState allRuneState,
            RuneSlotState runeSlotState,
            CollectionState collectionState,
            GameConfigState gameConfigState,
            int worldId,
            int stageId,
            int playCount,
            int? stageBuffId,
            Action<int> onProgress = null,
            Action<string> onLog = null)
        {
            if (onLog is not null)
            {
                var log = $"avatar: {avatarState.name}({avatarState.address})" +
                    $"\nequipments: {(equipments is null ? "null" : string.Join(", ", equipments.Select(e => $"{e.GetLocalizedNonColoredName(false)}(+{e.level})")))}" +
                    $"\ncostumes: {(costumes is null ? "null" : string.Join(", ", costumes.Select(e => e.GetLocalizedNonColoredName(false))))}" +
                    $"\nconsumables: {(consumables is null ? "null" : string.Join(", ", consumables.Select(e => e.GetLocalizedNonColoredName(false))))}" +
                    $"\nallRuneStates: {(allRuneState is null ? "null" : string.Join(", ", allRuneState.Runes.Values.Select(e => $"{L10nManager.LocalizeRuneName(e.RuneId)}(+{e.Level})")))}" +
                    $"\nruneSlotState: {(runeSlotState is null ? "null" : string.Join(", ", runeSlotState.GetEquippedRuneSlotInfos().Select(e => e is null ? "null" : $"slot #{e.SlotIndex}: {L10nManager.LocalizeRuneName(e.RuneId)}")))}" +
                    $"\ncollectionState: {(collectionState is null ? "null" : string.Join(", ", collectionState.Ids.Select(L10nManager.LocalizeCollectionName)))}" +
                    $"\ngameConfigState.ShatterStrikeMaxDamage: {(gameConfigState is null ? "null" : gameConfigState.ShatterStrikeMaxDamage)}" +
                    $"\nworldId: {worldId}" +
                    $"\nstageId: {stageId}" +
                    $"\nplayCount: {playCount}" +
                    $"\nstageBuffId: {(stageBuffId.HasValue ? stageBuffId : "null")}";
                onLog.Invoke(log);
            }

            var randomSeedRandom = new RandomImpl(DateTime.Now.Millisecond);
            var flyweightAvatarState = avatarState.MakeFlyweightAvatarState(
                equipments,
                costumes,
                consumables);
            var consumableIds = consumables?.Select(e => e.NonFungibleId).ToList() ?? new List<Guid>();
            var skillsOnWaveStart = GetSkillsOnWaveStart(
                stageBuffId,
                tableSheets.CrystalRandomBuffSheet,
                tableSheets.SkillSheet);
            var stageRow = tableSheets.StageSheet[stageId];
            var stageWaveRow = tableSheets.StageWaveSheet[stageId];
            var isCleared = false;
            // NOTE: Set exp to 0 because we don't need to calculate exp.
            //       And if so we can reuse the avatarState.
            var exp = 0;
            var stageSimulatorSheets = tableSheets.GetStageSimulatorSheets();
            var enemySkillSheet = tableSheets.EnemySkillSheet;
            var costumeStatSheet = tableSheets.CostumeStatSheet;
            var rewards = new List<ItemBase>();
            var collectionEffects = collectionState.GetEffects(tableSheets.CollectionSheet);
            var debuffLimitSheet = tableSheets.DeBuffLimitSheet;
            var logEvent = false;
            var shatterStrikeMaxDamage = gameConfigState.ShatterStrikeMaxDamage;
            var result = new Dictionary<int, int>
            {
                { 0, 0 },
                { 1, 0 },
                { 2, 0 },
                { 3, 0 },
            };
            for (var i = 0; i < playCount; i++)
            {
                var random = new RandomImpl(randomSeedRandom.Next());
                var stageSimulator = new StageSimulator(
                    random,
                    flyweightAvatarState,
                    consumableIds,
                    allRuneState,
                    runeSlotState,
                    skillsOnWaveStart,
                    worldId,
                    stageId,
                    stageRow,
                    stageWaveRow,
                    isCleared,
                    exp,
                    stageSimulatorSheets,
                    enemySkillSheet,
                    costumeStatSheet,
                    rewards,
                    collectionEffects,
                    debuffLimitSheet,
                    logEvent,
                    shatterStrikeMaxDamage);
                stageSimulator.Simulate();
                result[stageSimulator.Log.clearedWaveNumber] += 1;
                onProgress?.Invoke(i + 1);
            }

            var formattedResult = string.Join(", ", result.Select(kv => $"{kv.Key}: {kv.Value}"));
            onLog?.Invoke($"({nameof(HackAndSlashSimulator)}) Simulate Result: {formattedResult}");
            return result;
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
