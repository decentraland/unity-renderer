namespace DCL.ECSComponents
{
    public static class PBPointerEvent_Defaults
    {
        public static ActionButton GetButton(this PBPointerEvent self)
        {
            return self.HasButton ? self.Button : ActionButton.Any;
        }
        
        public static string GetHoverText(this PBPointerEvent self)
        {
            return self.HasHoverText ? self.HoverText : "Interact";
        }

        public static float GetMaxDistance(this PBPointerEvent self)
        {
            return self.HasMaxDistance ? self.MaxDistance : 10.0f;
        }
        
        public static bool GetShowFeedback(this PBPointerEvent self)
        {
            return !self.HasShowFeedback || self.ShowFeedback;
        }
    }
}