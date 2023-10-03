using DCL.Helpers;
using System;
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

        private float scaleMultiplier = 20f;
        private bool isInit;
        float baseZoom;
        float baseScale;

        public void SetZoom(float zoom)
        {
            if (!isInit)
            {
                baseZoom = zoom;
                baseScale = markerObject.transform.localScale.x;
                isInit = true;

                Debug.Log($" base Scale {baseScale}");
            }
            else
            {
                float newScale = Math.Max((zoom/baseZoom) * baseScale, baseScale);
                Debug.Log($" new Scale {newScale}");

                markerObject.transform.localScale = new Vector3(newScale, newScale, 1f);
            }
        }

        public void ResetToBaseScale()
        {
            markerObject.transform.localScale = new Vector3(baseScale, baseScale, 1f);
        }

        public void Dispose()
        {
            if (markerObject)
                Utils.SafeDestroy(markerObject.gameObject);
        }
    }
}
