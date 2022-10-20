using DCL;
using DCL.Interface;
using UnityEngine;

namespace GotoPanel
{
    public class GotoPanelHUDController
    {
        internal IGotoPanelHUDView view { get; private set; }

        internal virtual IGotoPanelHUDView CreateView() => GotoPanelHUDView.CreateView();

        public void Initialize()
        {
            view = CreateView();
            view.OnTeleportPressed += Teleport;
            view.OnClosePressed += ClosePanel;
            DataStore.i.HUDs.gotoPanelVisible.OnChange += ChangeVisibility;
            DataStore.i.HUDs.gotoPanelCoordinates.OnChange += SetCoordinates;
        }

        private void ChangeVisibility(bool current, bool previous)
        {
            SetVisibility(current);
        }

        private void SetCoordinates(ParcelCoordinates current, ParcelCoordinates previous)
        {
            if (current == previous)
                return;

            view.SetPanelInfo(current);
        }

        public void Dispose()
        {
            view.OnTeleportPressed -= Teleport;
            view.OnClosePressed -= ClosePanel;
            DataStore.i.HUDs.gotoPanelVisible.OnChange -= ChangeVisibility;
            DataStore.i.HUDs.gotoPanelCoordinates.OnChange -= SetCoordinates;
            view?.Dispose();
        }

        public void SetVisibility(bool visible)
        {
            view.SetVisible(visible);
        }

        public void Teleport(ParcelCoordinates parcelCoordinates)
        {
            Environment.i.world.teleportController.Teleport(parcelCoordinates.x, parcelCoordinates.y);
        }

        public void ClosePanel()
        {
            DataStore.i.HUDs.gotoPanelVisible.Set(false);
            AudioScriptableObjects.dialogClose.Play(true);
        }

    }
}
