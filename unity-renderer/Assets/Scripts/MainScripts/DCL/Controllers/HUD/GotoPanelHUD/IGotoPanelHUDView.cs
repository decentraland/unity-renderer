using System;

public interface IGotoPanelHUDView
{
    event Action<ParcelCoordinates> OnTeleportPressed;
    void SetVisible(bool isVisible);
    void SetPanelInfo(ParcelCoordinates parcelCoordinates);
    void Dispose();
}