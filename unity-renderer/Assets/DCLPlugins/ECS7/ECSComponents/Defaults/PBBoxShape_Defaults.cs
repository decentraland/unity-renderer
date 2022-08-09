namespace DCL.ECSComponents
{
    public static class PBBoxShape_Defaults
    {
        public static bool GetWithCollisions(this PBBoxShape self)
        {
            return !self.HasWithCollisions || self.WithCollisions;
        }
        
        public static bool GetIsPointerBlocker(this PBBoxShape self)
        {
            return !self.HasIsPointerBlocker || self.IsPointerBlocker;
        }
        
        public static bool GetVisible(this PBBoxShape self)
        {
            return !self.HasVisible || self.Visible;
        }
    }
}