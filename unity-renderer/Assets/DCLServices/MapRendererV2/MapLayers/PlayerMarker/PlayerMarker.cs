using DCL.Helpers;
using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.PlayerMarker
{
    internal class PlayerMarker : IPlayerMarker
    {
        private readonly PlayerMarkerObject markerObject;
        private readonly float baseScale;

        public Vector2 Pivot => markerObject.pivot;

        public PlayerMarker(PlayerMarkerObject markerObject)
        {
            this.markerObject = markerObject;
            baseScale = markerObject.transform.localScale.x;
            SetActive(false);
        }

        public void Dispose()
        {
            if (markerObject)
                Utils.SafeDestroy(markerObject.gameObject);
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
