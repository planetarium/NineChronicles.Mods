using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Libplanet.Action.State;
using Nekoyume.Game;
using Nekoyume.State;
using Nekoyume.UI;
using NineChronicles.Mods.PVEHelper.GUIs;
using NineChronicles.Mods.PVEHelper.Patches;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NineChronicles.Mods.PVEHelper
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class PVEHelperPlugin : BaseUnityPlugin
    {
        private const string PluginGUID = "org.ninechronicles.mods.pvehelper";
        private const string PluginName = "PVE Helper";
        private const string PluginVersion = "0.1.0";

        internal static PVEHelperPlugin Instance { get; private set; }

        private Harmony _harmony;

        private List<IDisposable> _disposables;

        private EventSystem _eventSystem;

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
            _harmony.PatchAll(typeof(BattlePreparationWidgetPatch));

            _eventSystem = FindObjectOfType<EventSystem>();

            _disposables = new List<IDisposable>
            {
                Widget.OnEnableStaticObservable.Subscribe(OnWidgetEnable),
                Widget.OnDisableStaticObservable.Subscribe(OnWidgetDisable)
            };
            BattlePreparationWidgetPatch.OnShow += BattlePreparationWidgetPatch_OnShow;

            Logger.LogInfo("Loaded");
        }

        private void DisableEventSystem()
        {
            if (_eventSystem != null)
            {
                _eventSystem.enabled = false;
            }
        }

        private void EnableEventSystem()
        {
            if (_eventSystem == null)
            {
                _eventSystem = FindObjectOfType<EventSystem>();
            }

            if (_eventSystem != null)
            {
                _eventSystem.enabled = true;
            }
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

            Logger.LogInfo("Unloaded");
        }
        
        public void Log(LogLevel logLevel, object data)
        {
            Logger.Log(logLevel, data);
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
            Debug.Log("BattlePreparationWidgetPatch_OnShow");
            var states = States.Instance;
            _winRateGUI = new WinRateGUI(
                states.CurrentAvatarKey,
                tuple.worldId,
                tuple.stageId);
        }
    }
}
