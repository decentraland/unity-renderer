using Cysharp.Threading.Tasks;
using DCL;
using DCL.Browser;
using DCL.Tasks;
using DCLFeatures.CameraReel.Section;
using DCLServices.CameraReelService;
using System;
using System.Threading;
using UnityEngine;

namespace DCLFeatures.CameraReel.Gallery
{
    public class ThumbnailContextMenuController
    {
        private const string DELETE_ERROR_MESSAGE = "There was an unexpected error when deleting the picture. Try again later.";

        private readonly ThumbnailContextMenuView view;
        private CameraReelResponse picture;
        private CancellationTokenSource deleteScreenshotCancellationToken;

        public ThumbnailContextMenuController(ThumbnailContextMenuView view,
            IClipboard clipboard,
            CameraReelModel cameraReelModel,
            IBrowserBridge browser,
            ICameraReelStorageService storageService,
            DataStore dataStore)
        {
            this.view = view;

            view.OnSetup += p => picture = p;

            view.OnCopyPictureLinkRequested += () =>
            {
                var url = $"https://dcl.gg/reels?image={picture.id}";
                clipboard.WriteText(url);
            };

            view.OnDownloadRequested += () => browser.OpenUrl(picture.url);

            view.OnShareToTwitterRequested += () =>
            {
                var description = "Check out what I'm doing in Decentraland right now and join me!";
                var url = $"https://dcl.gg/reels?image={picture.id}";
                var twitterUrl = $"https://twitter.com/intent/tweet?text={description}&hashtags=DCLCamera&url={url}";
                clipboard.WriteText(twitterUrl);
                browser.OpenUrl(twitterUrl);
            };

            view.OnDeletePictureRequested += () =>
            {
                async UniTaskVoid DeleteScreenshotAsync(CameraReelResponse screenshot, CancellationToken cancellationToken)
                {
                    try
                    {
                        CameraReelStorageStatus storage = await storageService.DeleteScreenshot(screenshot.id, cancellationToken);
                        cameraReelModel.RemoveScreenshot(screenshot);
                        cameraReelModel.SetStorageStatus(storage.CurrentScreenshots, storage.MaxScreenshots);
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception e)
                    {
                        dataStore.notifications.DefaultErrorNotification.Set(DELETE_ERROR_MESSAGE, true);
                        Debug.LogException(e);
                    }
                }

                dataStore.notifications.GenericConfirmation.Set(new GenericConfirmationNotificationData(
                    "Are you sure you want to delete this picture?",
                    "This picture will be removed and you will no longer be able to access it.",
                    "NO",
                    "YES",
                    () => { },
                    () =>
                    {
                        deleteScreenshotCancellationToken = deleteScreenshotCancellationToken.SafeRestart();
                        DeleteScreenshotAsync(picture, deleteScreenshotCancellationToken.Token).Forget();
                    }), true);
            };
        }

        public void Dispose()
        {
            view.Dispose();
        }
    }
}
