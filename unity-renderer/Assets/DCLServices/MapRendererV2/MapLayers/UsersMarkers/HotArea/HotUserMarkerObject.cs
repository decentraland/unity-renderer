using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.HotArea
{
    internal class HotUserMarkerObject : MonoBehaviour
    {
        [field: SerializeField]
        internal SpriteRenderer sprite { get; private set; }
    }
}
