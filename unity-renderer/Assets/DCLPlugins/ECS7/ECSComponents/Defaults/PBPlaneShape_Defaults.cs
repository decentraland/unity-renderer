namespace DCL.ECSComponents
{
    public static class PBPlaneShape_Defaults
    {
        public static bool GetWithCollisions(this PBPlaneShape self)
        {
            return !self.HasWithCollisions || self.WithCollisions;
        }
        
        public static bool GetIsPointerBlocker(this PBPlaneShape self)
        {
            return !self.HasIsPointerBlocker || self.IsPointerBlocker;
        }
        
        public static bool GetVisible(this PBPlaneShape self)
        {
            return !self.HasVisible || self.Visible;
        }
    }
}