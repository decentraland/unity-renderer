namespace DCL.ECSComponents
{
    public static class PBAudioStream_Defaults
    {
        public static float GetVolume(this PBAudioStream self)
        {
            return self.HasVolume ? self.Volume : 1.0f;
        }
    }
}