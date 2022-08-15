namespace DCL.ECSComponents
{
    public static class PBSphereShape_Defaults
    {
        public static bool GetWithCollisions(this PBSphereShape self)
        {
            return !self.HasWithCollisions || self.WithCollisions;
        }
        
        public static bool GetIsPointerBlocker(this PBSphereShape self)
        {
            return !self.HasIsPointerBlocker || self.IsPointerBlocker;
        }
        
        public static bool GetVisible(this PBSphereShape self)
        {
            return !self.HasVisible || self.Visible;
        }
    }
}