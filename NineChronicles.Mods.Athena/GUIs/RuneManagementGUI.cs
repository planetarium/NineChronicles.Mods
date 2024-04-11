using NineChronicles.Mods.Athena.ViewModels;
using UnityEngine;

namespace NineChronicles.Mods.Athena.GUIs
{
    public class RuneManagementGUI : IGUI
    {
        private RuneManagementViewModel _viewModel;

        public RuneManagementGUI()
        {
            _viewModel = new RuneManagementViewModel();
        }

        public void OnGUI()
        {
            GUI.matrix = GUIToolbox.GetGUIMatrix();
        }
    }
}
