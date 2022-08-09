namespace DCL.ECSComponents
{
    public static class PBOnPointerUp_Defaults
    {
        public static ActionButton GetButton(this PBOnPointerUp self)
        {
            return self.HasButton ? self.Button : ActionButton.Any;
        }
        
        public static string GetHoverText(this PBOnPointerUp self)
        {
            return self.HasHoverText ? self.HoverText : "Interact";
        }

        public static float GetMaxDistance(this PBOnPointerUp self)
        {
            return self.HasMaxDistance ? self.MaxDistance : 10.0f;
        }
        
        public static bool GetShowFeedback(this PBOnPointerUp self)
        {
            return !self.HasShowFeedback || self.ShowFeedback;
        }
    }
}