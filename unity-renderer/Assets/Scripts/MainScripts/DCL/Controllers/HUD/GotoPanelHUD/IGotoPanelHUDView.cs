using System;

public interface IGotoPanelHUDView
{
    event Action<ParcelCoordinates> OnTeleportPressed;
    event Action OnClosePressed;
    void SetVisible(bool isVisible);
    void SetPanelInfo(ParcelCoordinates parcelCoordinates);
    void Dispose();
}