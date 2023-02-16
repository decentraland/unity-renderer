using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.ColdArea
{
    internal class ColdUserMarker : IColdUserMarker
    {
        private readonly ColdUserMarkerObject instance;

        private string markerRealm;

        private bool active;
        private bool culled;

        private ColdUserMarker(ColdUserMarkerObject instance)
        {
            this.instance = instance;
        }

        public static IColdUserMarker Create(ColdUserMarkerObject prefab, Transform parent, int drawOrder)
        {
            var instance = Object.Instantiate(prefab, parent);
            instance.sprite.sortingOrder = drawOrder;
            return new ColdUserMarker(instance);
        }

        public Vector3 CurrentPosition => instance.transform.localPosition;

        public Vector2Int Coords { get; private set; }

        public void Dispose()
        {
            Object.Destroy(instance.gameObject);
        }

        public void OnRealmChanged(string realm)
        {
            instance.sprite.color =
                string.Equals(realm, markerRealm, StringComparison.OrdinalIgnoreCase) ? instance.sameRealmColor : instance.otherRealmColor;
        }

        public void SetActive(bool isActive)
        {
            active = isActive;
            ResolveVisibility();
        }

        public void SetCulled(bool culled)
        {
            this.culled = culled;
            ResolveVisibility();
        }

        private void ResolveVisibility()
        {
            instance.gameObject.SetActive(!culled && active);
        }

        public void SetData(string realm, string userRealm, Vector2Int coords, Vector3 position)
        {
            markerRealm = realm;
            Coords = coords;
            instance.transform.localPosition = position;

#if UNITY_EDITOR
            instance.name = $"UsersPositionMarker({coords.x},{coords.y})";
#endif

            OnRealmChanged(userRealm);
        }
    }
}
