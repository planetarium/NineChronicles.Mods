using UnityEngine;

namespace NineChronicles.Mods.PVEHelper.GUIs
{
    public class OverlayGUI : IGUI
    {
        public OverlayGUI()
        {
        }

        public void OnGUI()
        {
            UnityEngine.GUI.matrix = GUIToolbox.GetGUIMatrix();

            using var scope = new GUILayout.AreaScope(new Rect(0, 400, 50, 50));
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Labs", GUILayout.Width(50), GUILayout.Height(50)))
            {
                PVEHelperPlugin.Instance.Log(
                    BepInEx.Logging.LogLevel.Info,
                    $"{nameof(OverlayGUI)}.{nameof(OnGUI)}");
            }

            GUILayout.EndHorizontal();
        }
    }
}
