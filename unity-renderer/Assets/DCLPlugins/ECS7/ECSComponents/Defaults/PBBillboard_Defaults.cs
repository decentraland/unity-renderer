namespace DCL.ECSComponents
{
    public static class PBBillboard_Defaults
    {
        public static bool GetX(this PBBillboard self)
        {
            return !self.HasX || self.X;
        }
        
        public static bool GetY(this PBBillboard self)
        {
            return !self.HasY || self.Y;
        }
        
        public static bool GetZ(this PBBillboard self)
        {
            return !self.HasZ || self.Z;
        }
    }
}