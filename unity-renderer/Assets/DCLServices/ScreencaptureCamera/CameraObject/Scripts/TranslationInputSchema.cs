using System;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    [Serializable]
    public struct TranslationInputSchema
    {
        public readonly InputAction_Measurable XAxis;
        public readonly InputAction_Measurable YAxis;
        public readonly InputAction_Hold UpAction;
        public readonly InputAction_Hold DownAction;

        public TranslationInputSchema(InputAction_Measurable xAxis, InputAction_Measurable yAxis, InputAction_Hold upAction, InputAction_Hold downAction)
        {
            this.XAxis = xAxis;
            this.YAxis = yAxis;
            this.UpAction = upAction;
            this.DownAction = downAction;
        }
    }
}
