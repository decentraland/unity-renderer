using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public struct ScreenFrameData
    {
        public float ScreenWidth;
        public float ScreenHeight;

        public float FrameWidth;
        public float FrameHeight;

        public int ScreenWidthInt => Mathf.RoundToInt(ScreenWidth);
        public int ScreenHeightInt => Mathf.RoundToInt(ScreenHeight);

        public int FrameWidthInt => Mathf.RoundToInt(FrameWidth);
        public int FrameHeightInt => Mathf.RoundToInt(FrameHeight);

        public float ScreenAspectRatio => ScreenWidth / ScreenHeight;
        public float FrameAspectRatio => FrameWidth / FrameHeight;

        public Vector2Int CalculateFrameCorners() =>
            new ()
            {
                x = Mathf.RoundToInt((ScreenWidth - FrameWidth) / 2f),
                y = Mathf.RoundToInt((ScreenHeight - FrameHeight) / 2f),
            };
    }
}
