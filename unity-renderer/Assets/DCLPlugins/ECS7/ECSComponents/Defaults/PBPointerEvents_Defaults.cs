namespace DCL.ECSComponents
{
    public static class PBPointerEvents_Extensions
    {
        public static InputAction GetButton(this PBPointerEvents.Types.Info self)
        {
            return self.HasButton ? self.Button : InputAction.IaAny;
        }

        public static string GetHoverText(this PBPointerEvents.Types.Info self)
        {
            return self.HasHoverText ? self.HoverText : "Interact";
        }

        public static float GetMaxDistance(this PBPointerEvents.Types.Info self)
        {
            return self.HasMaxDistance ? self.MaxDistance : 10;
        }

        public static bool GetShowFeedback(this PBPointerEvents.Types.Info self)
        {
            return !self.HasShowFeedback || self.ShowFeedback;
        }
    }
}