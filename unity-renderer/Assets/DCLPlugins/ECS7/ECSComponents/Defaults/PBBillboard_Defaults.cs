namespace DCL.ECSComponents
{
    public static class PBBillboard_Defaults
    {
        public static BillboardMode GetBillboardMode(this PBBillboard self)
        {
            return self.HasBillboardMode ? self.BillboardMode : BillboardMode.BmAll;
        }
    }
}
