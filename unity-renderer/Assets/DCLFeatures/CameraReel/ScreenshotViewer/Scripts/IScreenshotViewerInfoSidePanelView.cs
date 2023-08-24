using DCLServices.CameraReelService;
using System;

namespace DCLFeatures.CameraReel.ScreenshotViewer
{
    public interface IScreenshotViewerInfoSidePanelView
    {
        event Action SceneButtonClicked;
        event Action SidePanelButtonClicked;
        event Action OnOpenPictureOwnerProfile;

        void SetSceneInfoText(Scene scene);
        void SetDateText(DateTime dateTime);
        void ShowVisiblePersons(VisiblePerson[] visiblePeople);
        void SetPictureOwner(string userName, string avatarPictureUrl);
    }
}
