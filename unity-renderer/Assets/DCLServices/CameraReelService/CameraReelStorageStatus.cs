namespace DCLServices.CameraReelService
{
    public class CameraReelStorageStatus
    {
        public int CurrentScreenshots { get; }
        public int MaxScreenshots { get; }

        public CameraReelStorageStatus(int currentScreenshots, int maxScreenshots)
        {
            CurrentScreenshots = currentScreenshots;
            MaxScreenshots = maxScreenshots;
        }
    }
}
