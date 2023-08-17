﻿using Cysharp.Threading.Tasks;
using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCLFeatures.CameraReel.Section
{
    public class CameraReelModel
    {
        public CameraReelModel(ICameraReelStorageService storageService)
        {
            storageService.Upload += OnUpload;
        }

        private async void OnUpload(Texture2D image, ScreenshotMetadata metadata, UniTaskCompletionSource<(CameraReelResponse, CameraReelStorageStatus)> completionSource)
        {
            // TODO: add temporary image with the Texture2D + ScreenshotMetadata in the gallery while the upload is in progress. Remove it if upload failed

            (CameraReelResponse screenshot, CameraReelStorageStatus storage) uploadResponse = await completionSource.Task;

            AddScreenshotAsFirst(uploadResponse.screenshot);
            SetStorageStatus(uploadResponse.storage.CurrentScreenshots, uploadResponse.storage.MaxScreenshots);
        }

        public delegate void StorageUpdatedHandler(int totalScreenshots, int maxScreenshots);
        private readonly LinkedList<CameraReelResponse> reels = new ();

        public int LoadedScreenshotCount => reels.Count;
        public int TotalScreenshotsInStorage { get; private set; }
        public int MaxScreenshotsInStorage { get; private set; }

        public event Action<CameraReelResponse> ScreenshotRemoved;
        public event Action<bool, CameraReelResponse> ScreenshotAdded;
        public event StorageUpdatedHandler StorageUpdated;

        public void SetStorageStatus(int totalScreenshots, int maxScreenshots)
        {
            TotalScreenshotsInStorage = totalScreenshots;
            MaxScreenshotsInStorage = maxScreenshots;
            StorageUpdated?.Invoke(totalScreenshots, maxScreenshots);
        }

        private void AddScreenshotAsFirst(CameraReelResponse screenshot)
        {
            CameraReelResponse existingScreenshot = reels.FirstOrDefault(s => s.id == screenshot.id);

            if (existingScreenshot != null)
                RemoveScreenshot(existingScreenshot);

            reels.AddFirst(screenshot);
            ScreenshotAdded?.Invoke(true, screenshot);
        }

        public void AddScreenshotAsLast(CameraReelResponse screenshot)
        {
            CameraReelResponse existingScreenshot = reels.FirstOrDefault(s => s.id == screenshot.id);

            if (existingScreenshot != null)
                RemoveScreenshot(existingScreenshot);

            reels.AddLast(screenshot);
            ScreenshotAdded?.Invoke(false, screenshot);
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
