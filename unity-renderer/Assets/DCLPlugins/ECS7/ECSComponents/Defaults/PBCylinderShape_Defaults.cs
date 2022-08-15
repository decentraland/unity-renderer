namespace DCL.ECSComponents
{
    public static class PBCylinderShape_Defaults
    {
        public static bool GetWithCollisions(this PBCylinderShape self)
        {
            return !self.HasWithCollisions || self.WithCollisions;
        }
        
        public static bool GetIsPointerBlocker(this PBCylinderShape self)
        {
            return !self.HasIsPointerBlocker || self.IsPointerBlocker;
        }
        
        public static bool GetVisible(this PBCylinderShape self)
        {
            return !self.HasVisible || self.Visible;
        }
        
        public static float GetRadiusTop(this PBCylinderShape self)
        {
            return self.HasRadiusTop ? self.RadiusTop : 1.0f;
        }
        
        public static float GetRadiusBottom(this PBCylinderShape self)
        {
            return self.HasRadiusBottom ? self.RadiusBottom : 1.0f;
        }
    }
}