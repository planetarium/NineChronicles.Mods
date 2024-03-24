using System;
using UnityEngine;

namespace NineChronicles.Mods.PVEHelper.GUIs
{
    public class OverlayGUI : IGUI
    {
        private readonly Action _onClick;

        public OverlayGUI(Action onClick)
        {
            _onClick = onClick;
        }

        public void OnGUI()
        {
            UnityEngine.GUI.matrix = GUIToolbox.GetGUIMatrix();

            using var scope = new GUILayout.AreaScope(new Rect(0, 400, 50, 50));
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Labs", GUILayout.Width(50), GUILayout.Height(50)))
            {
                PVEHelperPlugin.Log(
                    BepInEx.Logging.LogLevel.Info,
                    $"{nameof(OverlayGUI)}.{nameof(OnGUI)}");
                _onClick();
            }

            GUILayout.EndHorizontal();
        }
    }
}
