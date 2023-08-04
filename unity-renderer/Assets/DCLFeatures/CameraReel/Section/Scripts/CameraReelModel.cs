using DCL;
using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCLFeatures.CameraReel.Section
{
    public class CameraReelModel : Singleton<CameraReelModel>
    {
        public delegate void StorageUpdatedHandler(int totalScreenshots, int maxScreenshots);
        private readonly LinkedList<CameraReelResponse> reels = new ();

        public int LoadedScreenshotCount => reels.Count;
        public int TotalScreenshotsInStorage { get; private set; }
        public int MaxScreenshotsInStorage { get; private set; }

        public event Action<CameraReelResponse> ScreenshotRemoved;
        public event Action<CameraReelResponse> ScreenshotAdded;
        public event StorageUpdatedHandler StorageUpdated;

        public void SetStorageStatus(int totalScreenshots, int maxScreenshots)
        {
            TotalScreenshotsInStorage = totalScreenshots;
            MaxScreenshotsInStorage = maxScreenshots;
            StorageUpdated?.Invoke(totalScreenshots, maxScreenshots);
        }

        public void AddScreenshotAsFirst(CameraReelResponse screenshot)
        {
            CameraReelResponse existingScreenshot = reels.FirstOrDefault(s => s.id == screenshot.id);

            if (existingScreenshot != null)
                RemoveScreenshot(existingScreenshot);

            reels.AddFirst(screenshot);
            ScreenshotAdded?.Invoke(screenshot);
        }

        public void AddScreenshotAsLast(CameraReelResponse screenshot)
        {
            CameraReelResponse existingScreenshot = reels.FirstOrDefault(s => s.id == screenshot.id);

            if (existingScreenshot != null)
                RemoveScreenshot(existingScreenshot);

            reels.AddLast(screenshot);
            ScreenshotAdded?.Invoke(screenshot);
        }

        public void RemoveScreenshot(CameraReelResponse current)
        {
            LinkedListNode<CameraReelResponse> nodeToRemove = reels.Find(current);

            if (nodeToRemove != null)
                reels.Remove(nodeToRemove);

            ScreenshotRemoved?.Invoke(current);
        }

        public CameraReelResponse GetNextScreenshot(CameraReelResponse current) =>
            reels.Find(current)?.Next?.Value;

        public CameraReelResponse GetPreviousScreenshot(CameraReelResponse current) =>
            reels.Find(current)?.Previous?.Value;
    }
}
