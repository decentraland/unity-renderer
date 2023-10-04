using DCL.Helpers;
using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.HomePoint
{
    internal class HomePointMarker : IHomePointMarker
    {
        private readonly HomePointMarkerObject markerObject;

        private readonly float baseScale;

        public HomePointMarker(HomePointMarkerObject markerObject)
        {
            this.markerObject = markerObject;
            baseScale = markerObject.transform.localScale.x;
        }

        public void Dispose()
        {
            Utils.SafeDestroy(markerObject);
        }

        public void SetPosition(Vector3 position)
        {
            markerObject.transform.localPosition = position;
        }

        public void SetActive(bool active)
        {
            markerObject.gameObject.SetActive(active);
        }

        public void SetZoom(float baseZoom, float zoom)
        {
            float newScale = Math.Max(zoom / baseZoom * baseScale, baseScale);
            markerObject.transform.localScale = new Vector3(newScale, newScale, 1f);
        }

        public void ResetToBaseScale()
        {
            markerObject.transform.localScale = new Vector3(baseScale, baseScale, 1f);
        }
    }
}
