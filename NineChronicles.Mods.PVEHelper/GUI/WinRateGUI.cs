using BepInEx.Logging;
using Cysharp.Threading.Tasks;
using Nekoyume.Game;
using Nekoyume.State;
using UnityEngine;

namespace NineChronicles.Mods.PVEHelper.GUI
{
    public class WinRateGUI : IGUI
    {
        private const float AreaWidth = 430;
        private const float AreaHeight = 35;

        private readonly int _screenWidthReference = 1136;
        private readonly int _screenHeightReference = 640;
        private int _screenWidthCache;
        private int _screenHeightCache;
        private float _screenWidthRatio;
        private float _screenHeightRatio;
        private Rect _rect;

        private bool _isCalculating;
        private string _winRate;

        private readonly int _avatarIndex;
        private readonly int _worldId;
        private readonly int _stageId;

        public WinRateGUI(int avatarIndex, int worldId, int stageId)
        {
            _screenWidthCache = Screen.width;
            _screenHeightCache = Screen.height;
            _screenWidthRatio = (float)Screen.width / _screenWidthReference;
            _screenHeightRatio = (float)Screen.height / _screenHeightReference;
            _rect = new Rect(
                (_screenWidthReference - AreaWidth - 5) * _screenWidthRatio,
                (_screenHeightReference - AreaHeight - 5) * _screenHeightRatio,
                AreaWidth * _screenWidthRatio,
                AreaHeight * _screenHeightRatio);

            _avatarIndex = avatarIndex;
            _worldId = worldId;
            _stageId = stageId;

            UpdateWinRate();
        }

        public void OnGUI()
        {
            if (_screenWidthCache != Screen.width || _screenHeightCache != Screen.height)
            {
                _screenWidthCache = Screen.width;
                _screenHeightCache = Screen.height;
                _screenWidthRatio = (float)Screen.width / _screenWidthReference;
                _screenHeightRatio = (float)Screen.height / _screenHeightReference;
                PVEHelperPlugin.Instance.Log(
                    LogLevel.Info,
                    $"Screen size changed: {Screen.width}x{Screen.height}" +
                    $"\nRatio: {_screenWidthRatio:F2}x{_screenHeightRatio:F2}");
                _rect = new Rect(
                    (_screenWidthReference - AreaWidth - 5) * _screenWidthRatio,
                    (_screenHeightReference - AreaHeight - 5) * _screenHeightRatio,
                    AreaWidth * _screenWidthRatio,
                    AreaHeight * _screenHeightRatio);
            }

            GUILayout.BeginArea(_rect);
            GUILayout.BeginHorizontal();
            if (_isCalculating)
            {
                UnityEngine.GUI.enabled = false;
            }

            if (GUILayout.Button(
                "Calculate",
                new GUIStyle(UnityEngine.GUI.skin.button)
                {
                    fontSize = (int)(15 * _screenWidthRatio),
                },
                GUILayout.Width(100 * _screenWidthRatio),
                GUILayout.Height(AreaHeight * _screenHeightRatio)))
            {
                UpdateWinRate();
            }

            UnityEngine.GUI.enabled = true;

            GUILayout.Label(
                _winRate,
                new GUIStyle(UnityEngine.GUI.skin.label)
                {
                    fontSize = (int)(15 * _screenWidthRatio),
                },
                GUILayout.Height(AreaHeight * _screenHeightRatio));
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
            PVEHelperPlugin.Instance.Log(LogLevel.Info, $"Play Count: {playCount}, Win Count: {winCount}, Win Rate: {winRate:P1}");
            _winRate = $"Win Rate: {winRate:P1}";
            _isCalculating = false;
        }
    }
}
