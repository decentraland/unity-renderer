using DCL;
using DCL.Interface;
using UnityEngine;

namespace GotoPanel
{
    public class GotoPanelHUDController : IHUD
    {
        internal IGotoPanelHUDView view { get; private set; }

        internal virtual IGotoPanelHUDView CreateView() => GotoPanelHUDView.CreateView();

        public void Initialize()
        {
            view = CreateView();
            view.OnTeleportPressed += Teleport;
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
            view?.Dispose();
        }

        public void SetVisibility(bool visible)
        {
            view.SetVisible(visible);
        }

        public void Teleport(ParcelCoordinates parcelCoordinates)
        {
            WebInterface.GoTo(parcelCoordinates.x, parcelCoordinates.y);
        }

    }
}
