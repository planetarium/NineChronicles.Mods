using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using Nekoyume;
using Nekoyume.Battle;
using Nekoyume.Game;
using Nekoyume.Model.Item;
using Nekoyume.State;
using Nekoyume.UI;
using NineChronicles.Mods.PVEHelper.BlockSimulation;
using NineChronicles.Mods.PVEHelper.GUIs;
using NineChronicles.Mods.PVEHelper.Manager;
using NineChronicles.Mods.PVEHelper.Patches;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NineChronicles.Mods.PVEHelper
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class PVEHelperPlugin : BaseUnityPlugin
    {
        private const string PluginGUID = "org.ninechronicles.mods.athena";
        private const string PluginName = "Athena";
        private const string PluginVersion = "0.1.0";

        private const string PluginLastDayOfUseKey = PluginName + "_Last_Day_Of_Use";
        private const string PluginDailyOpenKey = PluginName + "_Daily_Open";

        internal static PVEHelperPlugin Instance { get; private set; }

        private Harmony _harmony;

        private ModInventoryManager modInventoryManager = new ModInventoryManager("../../mod_inventory.csv");

        private List<IDisposable> _disposables;

        private Camera _mainCamera;
        private Color _mainCameraBackgroundColor;
        private int _mainCameraCullingMask;
        private EventSystem _eventSystem;

        // NOTE: Please add your GUIs here as alphabetical order.
        private EnhancementGUI _enhancementGUI;
        // private EquipGUI _equipGUI;
        private InventoryGUI _inventoryGUI;
        private ItemCreationGUI _itemCreationGUI;
        private NotificationGUI _notificationGUI;
        private IGUI _tabGUI;
        private StageSimulateGUI _stageSimulateGUI;

        public static void Log(LogLevel logLevel, object data)
        {
            Instance?.Logger.Log(logLevel, data);
        }

        public static void Log(object data) => Log(LogLevel.Info, data);

        private void Awake()
        {
            if (Instance is not null)
            {
                throw new InvalidOperationException($"{nameof(PVEHelperPlugin)} must be only one instance.");
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

            Logger.LogInfo("Loaded");
        }

        private async void TrackOnce()
        {
            if (Analyzer.Instance is null)
            {
                await UniTask.WaitUntil(() => Analyzer.Instance is not null);
            }

            var days = (DateTime.Now - new DateTime(2019, 3, 11)).Days;
            if (days > PlayerPrefs.GetInt(PluginLastDayOfUseKey, 0))
            {
                Analyzer.Instance.Track(PluginDailyOpenKey);
            }
        }

        private void DisableModeGUI()
        {
            _enhancementGUI = null;
            // _equipGUI = null;
            _inventoryGUI = null;
            _itemCreationGUI = null;
            _notificationGUI = null;
            _tabGUI = null;
            _stageSimulateGUI = null;
            EnableEventSystem();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Log("Escape key pressed.");
                DisableModeGUI();
            }

            if (_tabGUI is not null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Log("space key pressed.");

                // var itemSlotState = States.Instance.CurrentItemSlotStates[BattleType.Adventure];

                // foreach (var equipmentId in itemSlotState.Equipments)
                // {
                //     var inventory = States.Instance.CurrentAvatarState?.inventory;

                //     if (inventory.TryGetNonFungibleItem<Equipment>(equipmentId, out var equipment))
                //     {
                //         switch (equipment.ItemSubType)
                //         {
                //             case ItemSubType.Weapon:
                //                 modInventoryManager.SelectedWeapon = equipment;
                //                 break;
                //             case ItemSubType.Armor:
                //                 modInventoryManager.SelectedArmor = equipment;
                //                 break;
                //             case ItemSubType.Belt:
                //                 modInventoryManager.SelectedBelt = equipment;
                //                 break;
                //             case ItemSubType.Necklace:
                //                 modInventoryManager.SelectedNecklace = equipment;
                //                 break;
                //             case ItemSubType.Ring:
                //                 if (modInventoryManager.SelectedRing1 == null)
                //                 {
                //                     modInventoryManager.SelectedRing1 = equipment;
                //                 }
                //                 else
                //                 {
                //                     modInventoryManager.SelectedRing2 = equipment;
                //                 }
                //                 break;
                //             case ItemSubType.Aura:
                //                 modInventoryManager.SelectedAura = equipment;
                //                 break;
                //         }
                //     }
                // }

                _tabGUI = new TabGUI(new List<(string Name, Func<IGUI> UI)>
                {
                    // ("Simulate", CreateSimulateGUI),
                    ("Create", CreateItemCreationGUI),
                    ("Enhancement", CreateEnhancementGUI),
                    ("Simulate", CreateStageSimulateGUI),
                }, DisableModeGUI);
                _notificationGUI = new NotificationGUI();

                TrackOnce();
                DisableEventSystem();
            }
        }

        private void DisableEventSystem()
        {
            if (_mainCamera is null)
            {
                _mainCamera = Camera.main;
                _mainCameraBackgroundColor = _mainCamera.backgroundColor;
                _mainCameraCullingMask = _mainCamera.cullingMask;
                _mainCamera.backgroundColor = Color.gray;
                _mainCamera.cullingMask = 0;
            }

            if (_eventSystem != null)
            {
                _eventSystem.enabled = false;
            }
        }

        private void EnableEventSystem()
        {
            if (_mainCamera)
            {
                _mainCamera.backgroundColor = _mainCameraBackgroundColor;
                _mainCamera.cullingMask = _mainCameraCullingMask;
                _mainCamera = null;
            }

            if (_eventSystem == null)
            {
                _eventSystem = FindObjectOfType<EventSystem>();
            }

            if (_eventSystem != null)
            {
                _eventSystem.enabled = true;
            }
        }

        // private IGUI CreateSimulateGUI()
        // {
        //     RemoveInventory();
        //     return new StageSimulateGUI(modInventoryManager);
        // }

        private IGUI CreateItemCreationGUI()
        {
            RemoveInventory();

            var tableSheets = TableSheets.Instance;
            var ui = new ItemCreationGUI(modInventoryManager);
            ui.SetItemRecipes(
                tableSheets.EquipmentItemSheet,
                tableSheets.EquipmentItemRecipeSheet,
                tableSheets.EquipmentItemSubRecipeSheetV2,
                tableSheets.EquipmentItemOptionSheet);

            return ui;
        }

        private IGUI CreateEnhancementGUI()
        {
            CreateInventoryGUI();
            return new EnhancementGUI(modInventoryManager, _inventoryGUI);
        }

        private IGUI CreateStageSimulateGUI()
        {
            CreateInventoryGUI();
            return new StageSimulateGUI(modInventoryManager, _inventoryGUI);
        }

        private void CreateInventoryGUI()
        {
            var inventoryGUI = new InventoryGUI(
                positionX: 100,
                positionY: 100,
                slotCountPerPage: 15,
                slotCountPerRow: 5);
            inventoryGUI.Clear();

            var inventory = States.Instance.CurrentAvatarState?.inventory;
            List<Equipment> equipments = new List<Equipment>();
            if (inventory is not null)
            {
                foreach (var inventoryItem in inventory.Items)
                {
                    if (inventoryItem.item is Equipment inventoryEquipment)
                    {
                        equipments.Add(inventoryEquipment);
                    }
                    else
                    {
                        inventoryGUI.AddItem(inventoryItem.item, inventoryItem.count);
                    }
                }
                foreach (var modItem in modInventoryManager.GetAllItems())
                {
                    if (modItem.ExistsItem)
                    {
                        if (inventory.TryGetNonFungibleItem<Equipment>(modItem.Id, out var existsItem))
                        {
                            equipments.Remove(existsItem);
                            var createdEquipment = ModItemFactory.ModifyLevel(TableSheets.Instance, existsItem, modItem);
                            equipments.Add(createdEquipment);
                        }
                        else
                        {
                            Log(LogLevel.Info, $"Error {modItem.Id}");
                        }
                    }
                    else
                    {
                        var createdEquipment = ModItemFactory.CreateEquipmentWithModItem(TableSheets.Instance, modItem);
                        equipments.Add(createdEquipment);
                    }
                }
            }
            equipments.Sort((e1, e2) => CPHelper.GetCP(e2).CompareTo(CPHelper.GetCP(e1)));

            foreach (var equipment in equipments)
            {
                inventoryGUI.AddItem(equipment);
            }

            _inventoryGUI = inventoryGUI;
        }

        private void RemoveInventory()
        {
            _inventoryGUI = null;
        }

        private void OnGUI()
        {
            _enhancementGUI?.OnGUI();
            // _equipGUI?.OnGUI();
            _inventoryGUI?.OnGUI();
            _itemCreationGUI?.OnGUI();
            _tabGUI?.OnGUI();
            _stageSimulateGUI?.OnGUI();
            _notificationGUI?.OnGUI();

        }

        //private void OnDestroy()
        //{
        //    if (Instance != this)
        //    {
        //        return;
        //    }

        //    Instance = null;

        //    _harmony.UnpatchSelf();
        //    _harmony = null;

        //    foreach (var disposable in _disposables)
        //    {
        //        disposable.Dispose();
        //    }

        //    modInventoryManager.SaveItemsToCsv();

        //    Logger.LogInfo("Unloaded");
        //}

        private void OnWidgetEnable(Widget widget)
        {
            switch (widget)
            {
                case Menu:
                    break;
                case BattlePreparation:
                    // do nothing: show BattlePreparationWidgetPatch_OnShow((worldId, stageId))
                    break;
            }
        }

        private void OnWidgetDisable(Widget widget)
        {
            switch (widget)
            {
                case BattlePreparation:
                    _enhancementGUI = null;
                    break;
            }
        }
    }
}
