using DCL.Helpers;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.PlayerMarker
{
    internal class PlayerMarker : IPlayerMarker
    {
        private readonly PlayerMarkerObject markerObject;

        public Vector2 Pivot => markerObject.pivot;

        public PlayerMarker(PlayerMarkerObject markerObject)
        {
            this.markerObject = markerObject;
            SetActive(false);
        }

        public void SetPosition(Vector3 position)
        {
            markerObject.transform.localPosition = position;
        }

        public void SetActive(bool active)
        {
            markerObject.gameObject.SetActive(active);
        }

        public void SetRotation(Quaternion rot)
        {
            markerObject.transform.localRotation = rot;
        }

        public void Dispose()
        {
            if (markerObject)
                Utils.SafeDestroy(markerObject.gameObject);
        }
    }
}
