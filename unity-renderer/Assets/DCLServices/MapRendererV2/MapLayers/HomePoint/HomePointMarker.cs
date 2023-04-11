using DCL.Helpers;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.HomePoint
{
    internal class HomePointMarker : IHomePointMarker
    {
        private readonly HomePointMarkerObject markerObject;

        public HomePointMarker(HomePointMarkerObject markerObject)
        {
            this.markerObject = markerObject;
        }

        public void SetPosition(Vector3 position)
        {
            markerObject.transform.localPosition = position;
        }

        public void SetActive(bool active)
        {
            markerObject.gameObject.SetActive(active);
        }

        public void Dispose()
        {
            Utils.SafeDestroy(markerObject);
        }
    }
}
