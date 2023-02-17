using System;

namespace DCL.GoToPanel
{
    public interface IGotoPanelHUDView
    {
        event Action<ParcelCoordinates> OnTeleportPressed;
        event Action OnClosePressed;
        void SetVisible(bool isVisible);
        void SetPanelInfo(ParcelCoordinates coordinates, MinimapMetadata.MinimapSceneInfo sceneInfo);
        void Dispose();
        void ShowLoading();
        void HideLoading();
    }
}
