using System;
using System.Collections.Generic;
using UnityEngine;

namespace NineChronicles.Mods.Athena.GUIs
{
    public class TabGUI : IGUI
    {
        private readonly List<(string Name, Func<IGUI> UIGenerator)> _uis;

        private int tabIndex = -1;
        private IGUI currentUI;

        private readonly Action _onClose;

        public event Action<(IGUI from, IGUI to)> OnTabChanged;

        public TabGUI(List<(string Name, Func<IGUI> UIGenerator)> uis, Action onClose)
        {
            _uis = uis;
            _onClose = onClose;
        }

        private void SetTabIndex(int index)
        {
            var previousUI = currentUI;
            tabIndex = index;
            currentUI = _uis[tabIndex].UIGenerator();
            OnTabChanged?.Invoke((previousUI, currentUI));
            AthenaPlugin.Log($"Tab Changed {tabIndex} -> {index}");
        }

        public void OnGUI()
        {
            // NOTE: Set tab index in OnGUI to avoid UnityEngine.GUI error.
            if (tabIndex < 0)
            {
                SetTabIndex(0);
            }

            GUI.matrix = GUIToolbox.GetGUIMatrix();

            using var scope = new GUILayout.AreaScope(new Rect(
                0,
                0,
                GUIToolbox.ScreenWidthReference,
                GUIToolbox.ScreenHeightReference));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            for (int i = 0; i < _uis.Count; ++i)
            {
                var (name, _) = _uis[i];
                if (GUI.Button(new Rect(100 * i, 0, 100, 50), name))
                {
                    SetTabIndex(i);
                }
            }

            CloseButton();

            GUILayout.EndHorizontal();

            currentUI?.OnGUI();
            GUILayout.EndVertical();
        }

        private void CloseButton()
        {
            var style = new GUIStyle
            {
                margin =
                {
                    left = 960,
                    top = 30,
                    right = 20,
                },
                fixedWidth = 40,
                fixedHeight = 40,
                normal =
                {
                    textColor = Color.black,
                },
                fontSize = 40,
                fontStyle = FontStyle.Bold
            };

            if (GUI.Button(new Rect(GUIToolbox.ScreenWidthReference - 40, 0, 40, 40), "X", style))
            {
                AthenaPlugin.Log("Close TabGUI mode");
                _onClose();
            }
        }
    }
}
