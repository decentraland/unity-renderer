namespace DCL.ECSComponents
{
    public static class PBNFTShape_Defaults
    {
        public static bool GetWithCollisions(this PBNFTShape self)
        {
            return !self.HasWithCollisions || self.WithCollisions;
        }
        
        public static bool GetIsPointerBlocker(this PBNFTShape self)
        {
            return !self.HasIsPointerBlocker || self.IsPointerBlocker;
        }
        
        public static bool GetVisible(this PBNFTShape self)
        {
            return !self.HasVisible || self.Visible;
        }
    }
}