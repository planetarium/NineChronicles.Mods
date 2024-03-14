using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nekoyume.UI;
using NineChronicles.Mods.PVEHelper.GUI;

namespace NineChronicles.Mods.PVEHelper
{
    using Libplanet.Action.State;
    using Nekoyume.State;
    using NineChronicles.Mods.PVEHelper.BlockSimulation;
    using NineChronicles.Mods.PVEHelper.Patches;
    using UniRx;

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class PVEHelperPlugin : BaseUnityPlugin
    {
        private const string PluginGUID = "org.ninechronicles.mods.pvehelper";
        private const string PluginName = "PVE Helper";
        private const string PluginVersion = "0.1.0";

        internal static PVEHelperPlugin Instance { get; private set; }

        private Harmony _harmony;
        private ManualLogSource _logger;

        private List<IDisposable> _disposables;

        private IWorld _world;
        private IGUI _winRateGUI;

        private void Awake()
        {
            if (Instance is not null)
            {
                throw new InvalidOperationException("PVEHelperPlugin must be only one instance.");
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _harmony = new Harmony(PluginGUID);
            _harmony.PatchAll(typeof(PVEHelperPlugin));
            _logger = BepInEx.Logging.Logger.CreateLogSource(PluginName);

            _disposables = new List<IDisposable>();
            _disposables.Add(Widget.OnEnableStaticObservable.Subscribe(OnWidgetEnable));
            _disposables.Add(Widget.OnDisableStaticObservable.Subscribe(OnWidgetDisable));
            BattlePreparationWidgetPatch.OnShow += BattlePreparationWidgetPatch_OnShow;

            _logger.LogInfo("Loaded");
        }

        private void OnGUI()
        {
            _winRateGUI?.OnGUI();
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;

            _harmony.UnpatchSelf();
            _harmony = null;

            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            BattlePreparationWidgetPatch.OnShow -= BattlePreparationWidgetPatch_OnShow;

            _logger.LogInfo("Unloaded");
            _logger.Dispose();
            _logger = null;
        }

        private void OnWidgetEnable(Widget widget)
        {
            switch (widget)
            {
                case BattlePreparation:
                    // do nothing: show BattlePreparationWidgetPatch_OnShow((int, int))
                    break;
            }
        }

        private void OnWidgetDisable(Widget widget)
        {
            switch (widget)
            {
                case BattlePreparation:
                    _winRateGUI = null;
                    break;
            }
        }

        private void BattlePreparationWidgetPatch_OnShow((int worldId, int stageId) tuple)
        {
            var states = States.Instance;
            _winRateGUI = new WinRateGUI(
                GetOrCreateWorld(),
                states.AgentState.address,
                states.CurrentAvatarKey,
                tuple.worldId,
                tuple.stageId);
        }

        private IWorld GetOrCreateWorld()
        {
            if (_world is not null)
            {
                return _world;
            }

            _world = WorldFactory.CreateWorld();
            return _world;
        }
    }
}
