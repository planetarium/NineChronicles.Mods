using System;
using System.Collections.Generic;
using UnityEngine;

namespace NineChronicles.Mods.PVEHelper.GUIs
{
    public class TabGUI : IGUI
    {
        private readonly List<(string Name, Func<IGUI> UIGenerator)> _uis;

        private int tabIndex;
        private IGUI currentUI;

        public TabGUI(List<(string Name, Func<IGUI> UIGenerator)> uis)
        {
            _uis = uis;
            tabIndex = 0;
        }

        public void OnGUI()
        {
            UnityEngine.GUI.matrix = GUIToolbox.GetGUIMatrix();

            using var scope = new GUILayout.AreaScope(new Rect(
                0,
                0,
                GUIToolbox.ScreenWidthReference,
                GUIToolbox.ScreenHeightReference));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            for (int i = 0; i < _uis.Count; ++i)
            {
                var (name, uiGenerator) = _uis[i];
                if (GUI.Button(new Rect(100 * i, 0, 100, 50), name))
                {
                    PVEHelperPlugin.Log($"Tab Changed {tabIndex} -> {i}");
                    tabIndex = i;
                    currentUI = null;
                }

                if (currentUI is null && tabIndex == i)
                {
                    currentUI = uiGenerator();
                }
            }

            GUILayout.EndHorizontal();
            
            currentUI?.OnGUI();
            GUILayout.EndVertical();
        }
    }
}
