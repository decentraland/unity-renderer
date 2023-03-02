namespace DCL.ECSComponents
{
    public static class PBAudioSource_Defaults
    {
        public static float GetVolume(this PBAudioSource self)
        {
            return self.HasVolume ? self.Volume : 1.0f;
        }

        public static float GetPitch(this PBAudioSource self)
        {
            return self.HasPitch ? self.Pitch : 1.0f;
        }
    }
}