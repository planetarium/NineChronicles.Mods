using UnityEngine;

namespace NineChronicles.Mods.Athena.GUIs
{
    internal static class GUIToolbox
    {
        public static int ScreenWidthReference { get; private set; } = 1136;
        public static int ScreenHeightReference { get; private set; } = 640;
        private static int _screenWidthCache;
        private static int _screenHeightCache;
        private static Matrix4x4 _guiMatrixCache;

        public static void SetReferenceSize(int width, int height)
        {
            ScreenWidthReference = width;
            ScreenHeightReference = height;
        }

        public static Matrix4x4 GetGUIMatrix()
        {
            if (_screenWidthCache != Screen.width || _screenHeightCache != Screen.height)
            {
                _screenWidthCache = Screen.width;
                _screenHeightCache = Screen.height;
                UpdateGUIMatrix();
            }

            return _guiMatrixCache;
        }

        private static void UpdateGUIMatrix()
        {
            var scale = new Vector3(
                _screenWidthCache / (float)ScreenWidthReference,
                _screenHeightCache / (float)ScreenHeightReference,
                1f);
            _guiMatrixCache = Matrix4x4.TRS(
                Vector3.zero,
                Quaternion.identity,
                scale);

            AthenaPlugin.Log(
                $"Screen size changed: {_screenWidthCache}x{_screenHeightCache}" +
                $"\nScale: {scale}" +
                $"\nMatrix: {GUI.matrix}");
        }

        public static Rect MoveInsideScreen(Rect rect, int marginX = 0, int marginY = 0)
        {
            if (rect.x + rect.width + marginX > ScreenWidthReference)
            {
                rect.x = ScreenWidthReference - rect.width - marginX;
            }

            if (rect.y + rect.height + marginY > ScreenHeightReference)
            {
                rect.y = ScreenHeightReference - rect.height - marginY;
            }

            if (rect.x - marginX < 0f)
            {
                rect.x = marginX;
            }

            if (rect.y - marginY < 0f)
            {
                rect.y = marginY;
            }

            return rect;
        }
    }
}
