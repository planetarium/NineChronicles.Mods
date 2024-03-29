using System.Collections.Generic;
using NineChronicles.Mods.PVEHelper.Pools;
using UnityEngine;

namespace NineChronicles.Mods.PVEHelper.GUIs
{
    public class NotificationGUI : IGUI
    {
        private static readonly Queue<string> _queue = new Queue<string>();

        // Rect
        private const float _rectWidth = 600f;
        private const float _rectHeight = 75f;
        private Rect _rect = new Rect(
            (GUIToolbox.ScreenWidthReference / 2) - _rectWidth / 2,
            -_rectHeight,
            _rectWidth,
            _rectHeight);
        private GUIStyle _style;

        // Animation
        private const float _animationForwardDuration = 0.3f;
        private const float _animationWaitDuration = 1.5f;
        private const float _animationBackwardDuration = 0.2f;
        private const float _animationTotalDuration = _animationForwardDuration + _animationWaitDuration + _animationBackwardDuration;
        private float _animationStartTime = 0f;

        private string _currentNotification = null;

        public static void Notify(string message)
        {
            _queue.Enqueue(message);
        }

        private static string GetNotification()
        {
            if (_queue.Count == 0)
            {
                return null;
            }

            return _queue.Dequeue();
        }

        public void OnGUI()
        {
            UpdateCurrentNotification();
            if (_currentNotification == null)
            {
                return;
            }

            GUI.matrix = GUIToolbox.GetGUIMatrix();
            DrawNotification();
        }

        private void UpdateCurrentNotification()
        {
            if (_currentNotification == null)
            {
                _currentNotification = GetNotification();
                _animationStartTime = Time.time;
                return;
            }

            if (Time.time - _animationStartTime >= _animationTotalDuration)
            {
                _currentNotification = null;
            }
        }

        private void DrawNotification()
        {
            var deltaTime = Time.time - _animationStartTime;
            if (deltaTime < _animationForwardDuration)
            {
                var progress = deltaTime / _animationForwardDuration;
                var y = -200f * (1f - progress);
                _rect.y = y;
            }
            else if (deltaTime < _animationForwardDuration + _animationWaitDuration)
            {
                _rect.y = 0f;
            }
            else
            {
                var progress = (deltaTime - _animationForwardDuration - _animationWaitDuration) / _animationBackwardDuration;
                var y = -200f * progress;
                _rect.y = y;
            }

            _style ??= new GUIStyle(GUI.skin.box)
            {
                normal = { background = ColorTexturePool.Dark },
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
            };
            GUI.Box(_rect, _currentNotification, _style);
        }
    }
}
