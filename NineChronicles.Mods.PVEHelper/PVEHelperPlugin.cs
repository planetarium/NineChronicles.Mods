using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nekoyume.State;
using Nekoyume.UI;
using NineChronicles.Mods.PVEHelper.GUIs;
using NineChronicles.Mods.PVEHelper.Manager;
using NineChronicles.Mods.PVEHelper.Models;
using NineChronicles.Mods.PVEHelper.Patches;
using NineChronicles.Mods.PVEHelper.Models;
using System.Collections.Immutable;
using NineChronicles.Mods.PVEHelper.Utils;
using UniRx;
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

        private ModInventoryManager modInventoryManager = new ModInventoryManager("../../mod_inventory.csv");

        private List<IDisposable> _disposables;

        private EventSystem _eventSystem;

        private IGUI _winRateGUI;
        private InventoryGUI _inventoryGUI = new InventoryGUI(
            positionX: 100,
            positionY: 100,
            slotCountPerPage: 15,
            slotCountPerRow: 5);
        private ItemCreationGUI _itemCreationGUI;

        public static void Log(LogLevel logLevel, object data)
        {
            Instance?.Logger.Log(logLevel, data);
        }

        public static void Log(object data) => Log(LogLevel.Info, data);

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
            _inventoryGUI.OnSlotSelected += tuple =>
            {
                Log($"Selected: {tuple.item}, {tuple.count}");
            };

            Logger.LogInfo("Loaded");

            var testItem = new ModItem()
            {
                Id = Guid.NewGuid(),
                EquipmentId = 1,
                SubRecipeId = 1,
                Level = 1,
                OptionIdList = new List<int>{1,2,3}.ToImmutableList()
            };
            modInventoryManager.AddItem(testItem);
            testItem.Enhancement();
            modInventoryManager.UpdateItem(testItem.Id, testItem);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _enhancementGUI = new EnhancementGUI();
                DisableEventSystem();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _enhancementGUI = null;
                EnableEventSystem();
            }
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
            _inventoryGUI?.OnGUI();
            _winRateGUI?.OnGUI();
            _enhancementGUI?.OnGUI();
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

            modInventoryManager.SaveItemsToCsv();

            Logger.LogInfo("Unloaded");
        }

        private void OnWidgetEnable(Widget widget)
        {
            switch (widget)
            {
                case Menu:
                    _inventoryGUI.Clear();
                    var inventory = States.Instance.CurrentAvatarState?.inventory;
                    if (inventory is not null)
                    {
                        foreach (var inventoryItem in inventory.Items)
                        {
                            _inventoryGUI.AddItem(inventoryItem.item, inventoryItem.count);
                        }
                    }

                    break;
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
                    _enhancementGUI = null;
                    break;
            }
        }

        private void BattlePreparationWidgetPatch_OnShow((int worldId, int stageId) tuple)
        {
            Log("BattlePreparationWidgetPatch_OnShow");
            var states = States.Instance;
            _winRateGUI = new WinRateGUI(
                states.CurrentAvatarKey,
                tuple.worldId,
                tuple.stageId);
        }
    }
}
