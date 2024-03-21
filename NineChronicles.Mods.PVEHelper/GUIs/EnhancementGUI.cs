using BepInEx.Logging;
using Nekoyume.Model.Item;
using UnityEngine;
using NineChronicles.Mods.PVEHelper.Manager;
using NineChronicles.Mods.PVEHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NineChronicles.Mods.PVEHelper.GUIs
{
    public class EnhancementGUI : IGUI
    {
        private readonly Rect _overlayRect;

        private readonly Rect _upgradeLayoutRect;

        private CacheManager _cacheManager;

        public EquipmentCache SelectedEquipment {get; set;}

        public EnhancementGUI(CacheManager cacheManager)
        {
            _cacheManager = cacheManager;

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
                                    _cacheManager.UpdateEquipmentCache(SelectedEquipment.Id, SelectedEquipment);
                                    PVEHelperPlugin.Instance.Log(LogLevel.Info, "Upgrade button clicked");
                                }
                            }

                            if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
                            {
                                if (SelectedEquipment != null)
                                {
                                    SelectedEquipment.Downgrade();
                                    _cacheManager.UpdateEquipmentCache(SelectedEquipment.Id, SelectedEquipment);
                                    PVEHelperPlugin.Instance.Log(LogLevel.Info, "Downgrade button clicked");
                                }
                            }
                        }
                    }
                }

            }
        }
    }
}