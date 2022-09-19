namespace DCL.ECSComponents
{
    public static class PBNFTShape_Defaults
    {
        public static PBNFTShape.Types.PictureFrameStyle GetStyle(this PBNFTShape self)
        {
            return self.HasStyle ? self.Style : PBNFTShape.Types.PictureFrameStyle.Classic;
        }
    }
}