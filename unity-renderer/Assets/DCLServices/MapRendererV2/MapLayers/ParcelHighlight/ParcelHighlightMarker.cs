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
            obj.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            Utils.SafeDestroy(obj.gameObject);
        }
    }
}
