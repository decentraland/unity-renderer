namespace DCL.ECSComponents
{
    public static class PBGLTFShape_Defaults
    {
        public static bool GetWithCollisions(this PBGLTFShape self)
        {
            return !self.HasWithCollisions || self.WithCollisions;
        }
        
        public static bool GetIsPointerBlocker(this PBGLTFShape self)
        {
            return !self.HasIsPointerBlocker || self.IsPointerBlocker;
        }
        
        public static bool GetVisible(this PBGLTFShape self)
        {
            return !self.HasVisible || self.Visible;
        }
    }
}