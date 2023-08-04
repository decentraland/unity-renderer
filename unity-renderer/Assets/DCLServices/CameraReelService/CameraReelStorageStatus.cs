namespace DCLServices.CameraReelService
{
    public readonly struct CameraReelStorageStatus
    {
        public readonly int CurrentScreenshots;
        public readonly int MaxScreenshots;
        public readonly bool HasSpace;

        public CameraReelStorageStatus(int currentScreenshots, int maxScreenshots)
        {
            this.CurrentScreenshots = currentScreenshots;
            MaxScreenshots = maxScreenshots;

            HasSpace = CurrentScreenshots < MaxScreenshots;
        }
    }
}
