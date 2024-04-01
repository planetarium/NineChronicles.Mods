using UnityEngine;
using UnityEngine.EventSystems;

namespace NineChronicles.Mods.Shared.Utils
{
    internal class CameraAndEventUtils
    {
        private static CameraAndEventUtils _instance;

        private Camera _mainCamera;
        private Color _mainCameraBackgroundColor;
        private int _mainCameraCullingMask;
        private EventSystem _eventSystem;

        public static CameraAndEventUtils GetInstance()
        {
            if (_instance is null)
            {
                _instance = new CameraAndEventUtils();
            }

            return _instance;
        }

        public void TurnOff()
        {
            if (_mainCamera is null)
            {
                _mainCamera = Camera.main;
                _mainCameraBackgroundColor = _mainCamera.backgroundColor;
                _mainCameraCullingMask = _mainCamera.cullingMask;
                _mainCamera.backgroundColor = Color.gray;
                _mainCamera.cullingMask = 0;
            }

            if (_eventSystem is null)
            {
                _eventSystem = Object.FindObjectOfType<EventSystem>();
            }
            if (_eventSystem != null)
            {
                _eventSystem.enabled = false;
            }
        }


        public void TurnOn()
        {
            if (_mainCamera)
            {
                _mainCamera.backgroundColor = _mainCameraBackgroundColor;
                _mainCamera.cullingMask = _mainCameraCullingMask;
                _mainCamera = null;
            }

            if (_eventSystem == null)
            {
                _eventSystem = Object.FindObjectOfType<EventSystem>();
            }

            if (_eventSystem != null)
            {
                _eventSystem.enabled = true;
            }
        }
    }
}
