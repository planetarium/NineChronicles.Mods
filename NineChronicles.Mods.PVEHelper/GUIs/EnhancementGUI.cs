using Nekoyume.Model.Item;
using NineChronicles.Mods.PVEHelper.Manager;
using NineChronicles.Mods.PVEHelper.Models;
using UnityEngine;

namespace NineChronicles.Mods.PVEHelper.GUIs
{
    public class EnhancementGUI : IGUI
    {
        private readonly Rect _overlayRect;

        private readonly Rect _upgradeLayoutRect;

        private ModInventoryManager _modInventoryManager;

        private InventoryGUI _inventoryGUI;

        public ModItem SelectedEquipment { get; set; }

        public EnhancementGUI(ModInventoryManager modInventoryManager, InventoryGUI inventoryGUI)
        {
            _modInventoryManager = modInventoryManager;
            _inventoryGUI = inventoryGUI;

            _inventoryGUI.OnSlotSelected += tuple =>
            {
                var modItem = new ModItem();

                if (tuple.item is Equipment equipment)
                {
                    modItem.Id = equipment.NonFungibleId;
                    modItem.EquipmentId = equipment.Id;
                    modItem.Level = equipment.level;
                    modItem.ExistsItem = true;
                }

                SelectedEquipment = modItem;
            };

            _overlayRect = new Rect(
                100,
                100,
                GUIToolbox.ScreenWidthReference - 200,
                GUIToolbox.ScreenHeightReference - 200);
            _upgradeLayoutRect = new Rect(
                150,
                GUIToolbox.ScreenHeightReference / 2 - 100,
                150,
                100);
        }

        public void OnGUI()
        {
            GUI.matrix = GUIToolbox.GetGUIMatrix();
            using (var overlayScope = new GUILayout.AreaScope(_overlayRect))
            {
                GUILayout.Box("Upgrade Menu", GUILayout.Width(_overlayRect.width), GUILayout.Height(_overlayRect.height));

                using (var areaScope = new GUILayout.AreaScope(_upgradeLayoutRect))
                {
                    using (var horizontalScope = new GUILayout.HorizontalScope())
                    {
                        GUILayout.Box("Selected Item", GUILayout.Width(100), GUILayout.Height(100));

                        using (var verticalScope = new GUILayout.VerticalScope())
                        {
                            if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
                            {
                                if (SelectedEquipment != null)
                                {
                                    SelectedEquipment.Enhancement();
                                    if (_modInventoryManager.GetItem(SelectedEquipment.Id) == null)
                                    {
                                        _modInventoryManager.AddItem(SelectedEquipment);
                                    }
                                    _modInventoryManager.UpdateItem(SelectedEquipment.Id, SelectedEquipment);
                                    PVEHelperPlugin.Log("Upgrade button clicked");
                                }
                            }

                            if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
                            {
                                if (SelectedEquipment != null)
                                {
                                    SelectedEquipment.Downgrade();

                                    if (_modInventoryManager.GetItem(SelectedEquipment.Id) == null)
                                    {
                                        _modInventoryManager.AddItem(SelectedEquipment);
                                    }
                                    _modInventoryManager.UpdateItem(SelectedEquipment.Id, SelectedEquipment);
                                    PVEHelperPlugin.Log("Downgrade button clicked");
                                }
                            }
                        }
                    }
                }

            }
        }
    }
}