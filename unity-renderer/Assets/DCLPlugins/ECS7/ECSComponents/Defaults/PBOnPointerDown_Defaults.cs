namespace DCL.ECSComponents
{
    public static class PBOnPointerDown_Defaults
    {
        public static ActionButton GetButton(this PBOnPointerDown self)
        {
            return self.HasButton ? self.Button : ActionButton.Any;
        }
        
        public static string GetHoverText(this PBOnPointerDown self)
        {
            return self.HasHoverText ? self.HoverText : "Interact";
        }

        public static float GetMaxDistance(this PBOnPointerDown self)
        {
            return self.HasMaxDistance ? self.MaxDistance : 10.0f;
        }
        
        public static bool GetShowFeedback(this PBOnPointerDown self)
        {
            return !self.HasShowFeedback || self.ShowFeedback;
        }
    }
}