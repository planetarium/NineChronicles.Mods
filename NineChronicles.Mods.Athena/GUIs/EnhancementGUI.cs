using Nekoyume.Game;
using Nekoyume.Model.Item;
using NineChronicles.Mods.Athena.Extensions;
using NineChronicles.Mods.Athena.Factories;
using NineChronicles.Mods.Athena.Managers;
using NineChronicles.Mods.Athena.Models;
using UnityEngine;

namespace NineChronicles.Mods.Athena.GUIs
{
    public class EnhancementGUI : IGUI
    {
        private readonly Rect _upgradeLayoutRect;

        private ModInventoryManager _modInventoryManager;

        private InventoryGUI _inventoryGUI;

        public ModItem SelectedEquipment { get; set; }
        public GUIContent SlotContent = new GUIContent();

        public EnhancementGUI(ModInventoryManager modInventoryManager, InventoryGUI inventoryGUI)
        {
            _modInventoryManager = modInventoryManager;
            _inventoryGUI = inventoryGUI;

            _inventoryGUI.OnSlotSelected += tuple =>
            {
                if (tuple.item is Equipment equipment)
                {
                    var modItem = _modInventoryManager.GetItem(equipment.NonFungibleId);

                    if (modItem is null)
                    {
                        modItem = new ModItem()
                        {
                            Id = equipment.NonFungibleId,
                            EquipmentId = equipment.Id,
                            Level = equipment.level,
                            ExistsItem = true,
                        };
                    }
                    SelectedEquipment = modItem;

                    var slotText = $"Grade {equipment.Grade}" +
                        $"\n{equipment.ElementalType}" +
                        $"\n{equipment.GetName()}\n" +
                        $"+{equipment.level}";
                    SlotContent = new GUIContent(slotText);
                }
            };
            _inventoryGUI.OnSlotDeselected += () =>
            {
                SelectedEquipment = null;
                SlotContent = new GUIContent();
            };
            _inventoryGUI.OnSlotReimportClicked += item =>
            {
                if (SelectedEquipment is not null &&
                    item is INonFungibleItem nonFungibleItem &&
                    nonFungibleItem.NonFungibleId.Equals(SelectedEquipment.Id))
                {
                    SelectedEquipment = null;
                    SlotContent = new GUIContent();
                }
            };
            _inventoryGUI.OnSlotRemoveClicked += item =>
            {
                if (SelectedEquipment is not null &&
                    item is INonFungibleItem nonFungibleItem &&
                    nonFungibleItem.NonFungibleId.Equals(SelectedEquipment.Id))
                {
                    SelectedEquipment = null;
                    SlotContent = new GUIContent();
                }
            };

            _upgradeLayoutRect = new Rect(
                GUIToolbox.ScreenWidthReference - 350,
                GUIToolbox.ScreenHeightReference / 2 - 100,
                150,
                100);
        }

        public void OnGUI()
        {
            GUI.matrix = GUIToolbox.GetGUIMatrix();
            _inventoryGUI.OnGUI();

            GUIStyle centeredStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter
            };

            using (var areaScope = new GUILayout.AreaScope(_upgradeLayoutRect))
            {
                using (var horizontalScope = new GUILayout.HorizontalScope())
                {
                    GUILayout.Box(SlotContent, centeredStyle, GUILayout.Width(100), GUILayout.Height(100));

                    using (var verticalScope = new GUILayout.VerticalScope())
                    {
                        if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            if (SelectedEquipment != null)
                            {
                                AthenaPlugin.Log($"[EnhancementGUI] Upgrade button clicked, Selected equipment: {SelectedEquipment.Id}");
                                SelectedEquipment.Enhancement();
                                SlotContent = new GUIContent(SlotContent.text.Replace($"+{SelectedEquipment.Level - 1}", $"+{SelectedEquipment.Level}"));

                                if (_modInventoryManager.GetItem(SelectedEquipment.Id) == null)
                                {
                                    AthenaPlugin.Log($"[EnhancementGUI] NotFound {SelectedEquipment.Id} in csv, add item");
                                    _modInventoryManager.AddItem(SelectedEquipment);
                                }
                                else
                                {
                                    _modInventoryManager.UpdateItem(SelectedEquipment.Id, SelectedEquipment);
                                }

                                UpdateInventoryItem();
                            }
                        }

                        if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            if (SelectedEquipment != null)
                            {
                                AthenaPlugin.Log($"[EnhancementGUI] Downgrade button clicked, Selected equipment: {SelectedEquipment.Id}");
                                SelectedEquipment.Downgrade();
                                SlotContent = new GUIContent(SlotContent.text.Replace($"+{SelectedEquipment.Level + 1}", $"+{SelectedEquipment.Level}"));

                                if (_modInventoryManager.GetItem(SelectedEquipment.Id) == null)
                                {
                                    AthenaPlugin.Log($"[EnhancementGUI] NotFound {SelectedEquipment.Id} in csv, add item");
                                    _modInventoryManager.AddItem(SelectedEquipment);
                                }
                                else
                                {
                                    _modInventoryManager.UpdateItem(SelectedEquipment.Id, SelectedEquipment);
                                }

                                UpdateInventoryItem();
                            }
                        }
                    }
                }
            }
        }

        private void UpdateInventoryItem()
        {
            if (SelectedEquipment is null ||
                !_inventoryGUI.TryGetSelectedSlot(out var slot) ||
                slot.item is not Equipment equipment)
            {
                return;
            }

            equipment = ModItemFactory.ModifyLevel(
                TableSheets.Instance,
                equipment,
                SelectedEquipment);
            slot.Set(equipment, slot.count, slot.isExistsInBlockchain, isModded: true);
        }
    }
}
