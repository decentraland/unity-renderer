using Decentraland.Common;

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

        public static DCL.ECSComponents.NftFrameType GetStyle(this PBNftShape self)
        {
            return self.HasStyle ? self.Style : DCL.ECSComponents.NftFrameType.NftClassic;
        }

        public static Color3 GetColor(this PBNftShape self)
        {
            return self.Color ?? defaultColor;
        }
    }
}
