namespace DCL.ECSComponents
{
    public static class PBNFTShape_Defaults
    {
        private static readonly Color3 defaultColor = new Color3()
        {
            R = 0.6404918f,
            G = 0.611472f,
            B = 0.8584906f
        };

        public static PBNFTShape.Types.PictureFrameStyle GetStyle(this PBNFTShape self)
        {
            return self.HasStyle ? self.Style : PBNFTShape.Types.PictureFrameStyle.Classic;
        }

        public static Color3 GetColor(this PBNFTShape self)
        {
            return self.Color ?? defaultColor;
        }
    }
}