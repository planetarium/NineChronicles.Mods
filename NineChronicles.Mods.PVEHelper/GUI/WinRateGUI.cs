using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Nekoyume.Game;
using Nekoyume.State;
using UnityEngine;

namespace NineChronicles.Mods.PVEHelper.GUI
{
    public class WinRateGUI : IGUI
    {
        private const float AreaWidth = 430;
        private const float AreaHeight = 35;

        private readonly Rect _rect;

        private bool _isCalculating;
        private string _winRate;

        private readonly IWorld _world;
        private readonly Address _agentAddr;
        private readonly int _avatarIndex;
        private readonly int _worldId;
        private readonly int _stageId;

        public WinRateGUI(IWorld world, Address agentAddr, int avatarIndex, int worldId, int stageId)
        {
            // 1136, 640
            // Screen.width, Screen.height
            // x rate, y rate
            // rect size, font size, position
            _rect = new Rect(
                Screen.width - AreaWidth - 5,
                Screen.height - AreaHeight - 5,
                AreaWidth,
                AreaHeight);

            _world = world;
            _agentAddr = agentAddr;
            _avatarIndex = avatarIndex;
            _worldId = worldId;
            _stageId = stageId;

            UpdateWinRate();
        }

        public void OnGUI()
        {
            GUILayout.BeginArea(_rect);
            GUILayout.BeginHorizontal();
            if (_isCalculating)
            {
                UnityEngine.GUI.enabled = false;
            }

            if (GUILayout.Button("Calculate", GUILayout.Width(100), GUILayout.Height(AreaHeight)))
            {
                UpdateWinRate();
            }

            UnityEngine.GUI.enabled = true;

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

            await Task.Delay(1000);
            var winRate = (float)winCount / playCount;
            _winRate = $"Win Rate: {winRate:P1}";
            _isCalculating = false;
        }
    }
}
