using System;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    [Serializable]
    public struct TranslationInputSchema
    {
        public InputAction_Measurable xAxis;
        public InputAction_Measurable yAxis;
        public InputAction_Hold upAction;
        public InputAction_Hold downAction;
    }
}
