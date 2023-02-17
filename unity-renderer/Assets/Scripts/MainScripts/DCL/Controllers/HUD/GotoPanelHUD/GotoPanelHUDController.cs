using Cysharp.Threading.Tasks;
using DCL.Map;
using DCL.Tasks;
using System.Threading;
using UnityEngine;

namespace DCL.GoToPanel
{
    public class GotoPanelHUDController
    {
        private readonly DataStore dataStore;
        private readonly ITeleportController teleportController;
        private readonly IMinimapApiBridge minimapApiBridge;
        private readonly IGotoPanelHUDView view;
        private CancellationTokenSource cancellationToken = new ();

        public GotoPanelHUDController(IGotoPanelHUDView view,
            DataStore dataStore,
            ITeleportController teleportController,
            IMinimapApiBridge minimapApiBridge)
        {
            this.dataStore = dataStore;
            this.teleportController = teleportController;
            this.minimapApiBridge = minimapApiBridge;
            this.view = view;
            view.OnTeleportPressed += Teleport;
            view.OnClosePressed += ClosePanel;
            dataStore.HUDs.gotoPanelVisible.OnChange += ChangeVisibility;
            dataStore.HUDs.gotoPanelCoordinates.OnChange += SetCoordinates;
        }

        public void Dispose()
        {
            view.OnTeleportPressed -= Teleport;
            view.OnClosePressed -= ClosePanel;
            dataStore.HUDs.gotoPanelVisible.OnChange -= ChangeVisibility;
            dataStore.HUDs.gotoPanelCoordinates.OnChange -= SetCoordinates;
            view?.Dispose();
            cancellationToken.SafeCancelAndDispose();
        }

        private void ChangeVisibility(bool current, bool previous) =>
            view.SetVisible(current);

        private void SetCoordinates(ParcelCoordinates current, ParcelCoordinates previous)
        {
            if (current == previous) return;

            async UniTaskVoid SetCoordinatesAsync(ParcelCoordinates coordinates, CancellationToken cancellationToken)
            {
                await minimapApiBridge.GetScenesInformationAroundParcel(new Vector2Int(coordinates.x, coordinates.y),2,
                    cancellationToken);
                view.SetPanelInfo(coordinates);
            }

            cancellationToken = cancellationToken.SafeRestart();
            SetCoordinatesAsync(current, cancellationToken.Token).Forget();
        }

        private void Teleport(ParcelCoordinates parcelCoordinates)
        {
            teleportController.Teleport(parcelCoordinates.x, parcelCoordinates.y);
        }

        private void ClosePanel()
        {
            dataStore.HUDs.gotoPanelVisible.Set(false);
            view.SetVisible(false);
        }
    }
}
