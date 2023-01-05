using Decentraland.Common;

namespace DCL.ECSComponents
{
    public static class PBUiBackground_Defaults
    {
        private static readonly BorderRect DEFAULT_SLICES = new ()
            { Left = 1 / 3f, Bottom = 1 / 3f, Right = 1 / 3f, Top = 1 / 3f };

        public static Color4 GetColor(this PBUiBackground self) =>
            self.Color ?? Color_Defaults.color4White;

        public static BorderRect GetBorder(this PBUiBackground self) =>
            self.TextureSlices ?? DEFAULT_SLICES;
    }
}
