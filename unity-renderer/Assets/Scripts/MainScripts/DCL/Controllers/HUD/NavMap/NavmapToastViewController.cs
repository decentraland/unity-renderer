using DCLServices.MapRendererV2.ConsumerUtils;
using System;
using UnityEngine;

namespace DCL
{
    public class NavmapToastViewController : IDisposable
    {
        private readonly MinimapMetadata minimapMetadata;
        private readonly NavmapToastView view;
        private readonly MapRenderImage mapRenderImage;
        private readonly float sqrDistanceToCloseView;

        private Vector2 lastClickPosition;
        private Vector2Int currentParcel;

        public NavmapToastViewController(MinimapMetadata minimapMetadata, NavmapToastView view, MapRenderImage mapRenderImage)
        {
            this.minimapMetadata = minimapMetadata;
            this.view = view;
            this.mapRenderImage = mapRenderImage;
            this.sqrDistanceToCloseView = view.distanceToCloseView * view.distanceToCloseView;
        }

        public void Activate()
        {
            Unsubscribe();

            mapRenderImage.ParcelClicked += OnParcelClicked;
            mapRenderImage.Hovered += OnHovered;
            mapRenderImage.DragStarted += OnDragStarted;
            minimapMetadata.OnSceneInfoUpdated += OnMapMetadataInfoUpdated;
        }

        public void Deactivate()
        {
            view.Close();
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            mapRenderImage.ParcelClicked -= OnParcelClicked;
            mapRenderImage.Hovered -= OnHovered;
            mapRenderImage.DragStarted -= OnDragStarted;
            minimapMetadata.OnSceneInfoUpdated -= OnMapMetadataInfoUpdated;
        }

        private void OnHovered(Vector2 localPosition)
        {
            if (!view.gameObject.activeSelf)
                return;

            if (Vector2.SqrMagnitude(localPosition - lastClickPosition) >= sqrDistanceToCloseView)
                view.Close();
        }

        private void OnDragStarted()
        {
            if (!view.gameObject.activeSelf)
                return;

            view.Close();
        }

        private void OnParcelClicked(MapRenderImage.ParcelClickData parcelClickData)
        {
            lastClickPosition = parcelClickData.WorldPosition;
            currentParcel = parcelClickData.Parcel;

            // transform coordinates from rect coordinates to parent of view coordinates
            //currentPosition = GetViewLocalPosition(lastClickPosition);
            view.Open(currentParcel, lastClickPosition);
        }

        private void OnMapMetadataInfoUpdated(MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            if (!view.gameObject.activeInHierarchy)
                return;

            var updatedCurrentLocationInfo = false;
            foreach (Vector2Int parcel in sceneInfo.parcels)
            {
                if (parcel == currentParcel)
                {
                    updatedCurrentLocationInfo = true;
                    break;
                }
            }

            if (updatedCurrentLocationInfo)
                view.Populate(currentParcel, lastClickPosition, sceneInfo);
        }

        public void Dispose()
        {
            Unsubscribe();
        }
    }
}
