using Cysharp.Threading.Tasks;
using Nekoyume.Game;
using Nekoyume.State;
using UnityEngine;

namespace NineChronicles.Mods.PVEHelper.GUIs
{
    public class WinRateGUI : IGUI
    {
        private const float AreaWidth = 430;
        private const float AreaHeight = 35;

        private Rect _rect;

        private bool _isCalculating;
        private string _winRate;

        private readonly int _avatarIndex;
        private readonly int _worldId;
        private readonly int _stageId;

        public WinRateGUI(int avatarIndex, int worldId, int stageId)
        {
            _rect = new Rect(
                GUIToolbox.ScreenWidthReference - AreaWidth - 5,
                GUIToolbox.ScreenHeightReference - AreaHeight - 5,
                AreaWidth,
                AreaHeight);

            _avatarIndex = avatarIndex;
            _worldId = worldId;
            _stageId = stageId;

            UpdateWinRate();
        }

        public void OnGUI()
        {
            GUI.matrix = GUIToolbox.GetGUIMatrix();
            GUILayout.BeginArea(_rect);
            GUILayout.BeginHorizontal();
            if (_isCalculating)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Calculate", GUILayout.Width(100), GUILayout.Height(AreaHeight)))
            {
                UpdateWinRate();
            }

            GUI.enabled = true;

            GUILayout.Label(_winRate, GUILayout.Height(AreaHeight));
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private async void UpdateWinRate()
        {
            _isCalculating = true;
            _winRate = "Win Rate: Now calculating...";
            const int playCount = 700;
            var winCount = await UniTask.Run(() => BlockSimulation.Actions.HackAndSlashSimulation.Simulate(
                TableSheets.Instance,
                States.Instance,
                _avatarIndex,
                _worldId,
                _stageId,
                playCount));
            var winRate = (float)winCount / playCount;
            PVEHelperPlugin.Log($"Play Count: {playCount}, Win Count: {winCount}, Win Rate: {winRate:P1}");
            _winRate = $"Win Rate: {winRate:P1}";
            _isCalculating = false;
        }
    }
}
