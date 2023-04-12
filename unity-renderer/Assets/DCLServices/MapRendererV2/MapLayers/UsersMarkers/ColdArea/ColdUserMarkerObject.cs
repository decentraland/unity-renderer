using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.ColdArea
{
    internal class ColdUserMarkerObject : MapRendererMarkerBase
    {
        [field: SerializeField]
        internal SpriteRenderer outerCircle;

        [field: SerializeField]
        internal SpriteRenderer innerCircle;

        [field: SerializeField]
        internal Color sameRealmColor;

        [field: SerializeField]
        internal Color otherRealmColor;
    }
}
