using DCL.Helpers;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.ParcelHighlight
{
    internal class ParcelHighlightMarker : IParcelHighlightMarker
    {
        private readonly ParcelHighlightMarkerObject obj;

        public Vector2 Pivot => obj.pivot;

        public ParcelHighlightMarker(ParcelHighlightMarkerObject obj)
        {
            this.obj = obj;
            Deactivate();
        }

        public void Dispose()
        {
            Utils.SafeDestroy(obj.gameObject);
        }

        public void SetCoordinates(Vector2Int coords, Vector3 position)
        {
            obj.text.text = $"{coords.x}, {coords.y}";
            obj.transform.localPosition = position;
        }

        public void Activate()
        {
            obj.gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            if (obj != null && obj.gameObject != null)
                obj.gameObject.SetActive(false);
        }

        public void SetZoom(float baseZoom, float newZoom)
        {
            obj.SetScale(baseZoom, newZoom);
        }
    }
}
