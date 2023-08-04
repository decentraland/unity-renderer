namespace DCLServices.CameraReelService
{
    public readonly struct CameraReelStorageStatus
    {
        public readonly int CurrentScreenshots;
        public readonly int MaxScreenshots;
        public readonly bool HasFreeSpace;

        public CameraReelStorageStatus(int currentScreenshots, int maxScreenshots)
        {
            this.CurrentScreenshots = currentScreenshots;
            MaxScreenshots = maxScreenshots;

            HasFreeSpace = CurrentScreenshots < MaxScreenshots;
        }
    }
}
