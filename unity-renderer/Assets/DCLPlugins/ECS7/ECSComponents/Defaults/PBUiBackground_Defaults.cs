namespace DCL.ECSComponents
{
    public static class PBUiBackground_Defaults
    {
        public static Color4 GetColor(this PBUiBackground self) =>
            self.Color ?? Color_Defaults.color4White;
    }
}
