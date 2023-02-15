using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.ColdArea
{
    internal class ColdUserMarkerObject : MonoBehaviour
    {
        [field: SerializeField]
        internal SpriteRenderer sprite;

        [field: SerializeField]
        internal Color sameRealmColor;

        [field: SerializeField]
        internal Color otherRealmColor;
    }
}
