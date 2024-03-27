using BepInEx.Logging;
using Nekoyume.Model.Item;
using UnityEngine;
using NineChronicles.Mods.PVEHelper.Manager;
using NineChronicles.Mods.PVEHelper.Models;
using NineChronicles.Mods.PVEHelper.Extensions;

namespace NineChronicles.Mods.PVEHelper.GUIs
{
    public class EquipGUI : IGUI
    {
        private readonly Rect _overlayRect;

        private readonly Rect _selectLayoutRect;

        private ModInventoryManager _modInventoryManager;

        private InventoryGUI _inventoryGUI;

        public Equipment? SelectedAura { get; set; }
        public Equipment? SelectedWeapon { get; set; }
        public Equipment? SelectedArmor { get; set; }
        public Equipment? SelectedBelt { get; set; }
        public Equipment? SelectedRing1 { get; set; }
        public Equipment? SelectedRing2 { get; set; }

        public EquipGUI(ModInventoryManager modInventoryManager, InventoryGUI inventoryGUI)
        {
            _modInventoryManager = modInventoryManager;
            _inventoryGUI = inventoryGUI;

            _inventoryGUI.OnSlotSelected += tuple =>
            {
                if (tuple.item is Equipment equipment)
                {
                    switch (equipment.ItemSubType)
                    {
                        case ItemSubType.Weapon:
                            SelectedWeapon = equipment;
                            _modInventoryManager.SelectedWeapon = equipment;
                            PVEHelperPlugin.Log(LogLevel.Info, $"[EquipGUI] Selected weapon {equipment.GetName()} {equipment.ItemId} {equipment.level}");
                            break;
                        case ItemSubType.Armor:
                            SelectedArmor = equipment;
                            _modInventoryManager.SelectedArmor = equipment;
                            PVEHelperPlugin.Log(LogLevel.Info, $"[EquipGUI] Selected armor {equipment.GetName()} {equipment.ItemId} {equipment.level}");
                            break;
                        case ItemSubType.Belt:
                            SelectedBelt = equipment;
                            _modInventoryManager.SelectedBelt = equipment;
                            PVEHelperPlugin.Log(LogLevel.Info, $"[EquipGUI] Selected belt {equipment.GetName()} {equipment.ItemId} {equipment.level}");
                            break;
                        case ItemSubType.Ring:
                            if (SelectedRing1 == null)
                            {
                                SelectedRing1 = equipment;
                                _modInventoryManager.SelectedRing1 = equipment;
                                PVEHelperPlugin.Log(LogLevel.Info, $"[EquipGUI] Selected ring1 {equipment.GetName()} {equipment.ItemId} {equipment.level}");
                            } else {
                                SelectedRing2 = equipment;
                                _modInventoryManager.SelectedRing2 = equipment;
                                PVEHelperPlugin.Log(LogLevel.Info, $"[EquipGUI] Selected ring2 {equipment.GetName()} {equipment.ItemId} {equipment.level}");
                            }
                            break;
                        case ItemSubType.Aura:
                            SelectedAura = equipment;
                            _modInventoryManager.SelectedAura = equipment;
                            PVEHelperPlugin.Log(LogLevel.Info, $"[EquipGUI] Selected aura {equipment.GetName()} {equipment.ItemId} {equipment.level}");
                            break;
                    }
                }
            };

            _overlayRect = new Rect(
                100,
                100,
                GUIToolbox.ScreenWidthReference - 200,
                GUIToolbox.ScreenHeightReference - 200);
            _selectLayoutRect = new Rect(
                150,
                GUIToolbox.ScreenHeightReference / 2 - 320,
                400,
                800);
        }

        public void OnGUI()
        {
            GUI.matrix = GUIToolbox.GetGUIMatrix();
            using (var overlayScope = new GUILayout.AreaScope(_overlayRect))
            {
                GUILayout.Box("Select Menu", GUILayout.Width(_overlayRect.width), GUILayout.Height(_overlayRect.height));

                using (var areaScope = new GUILayout.AreaScope(_selectLayoutRect))
                {
                    using (var verticalScope = new GUILayout.VerticalScope())
                    {
                        using (var horizontalScope = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Box("Aura", GUILayout.Width(100), GUILayout.Height(100));
                        }
                        using (var horizontalScope = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Box("Sword", GUILayout.Width(100), GUILayout.Height(100));
                            GUILayout.Box("Armor", GUILayout.Width(100), GUILayout.Height(100));
                        }
                        using (var horizontalScope = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Box("Neck", GUILayout.Width(100), GUILayout.Height(100));
                            GUILayout.Box("Belt", GUILayout.Width(100), GUILayout.Height(100));
                        }
                        using (var horizontalScope = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Box("Ring1", GUILayout.Width(100), GUILayout.Height(100));
                            GUILayout.Box("Ring2", GUILayout.Width(100), GUILayout.Height(100));
                        }
                    }
                }

            }
        }
    }
}
