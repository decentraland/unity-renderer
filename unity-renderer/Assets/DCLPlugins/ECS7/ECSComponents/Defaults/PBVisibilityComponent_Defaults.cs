namespace DCL.ECSComponents
{
    public static class PBVisibilityComponent_Defaults
    {
        public static bool GetVisible(this PBVisibilityComponent self)
        {
            return !self.HasVisible || self.Visible;
        }
    }
}