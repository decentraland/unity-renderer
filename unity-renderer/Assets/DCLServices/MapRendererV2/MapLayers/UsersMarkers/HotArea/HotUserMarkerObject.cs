using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.HotArea
{
    internal class HotUserMarkerObject : MapRendererMarkerBase
    {
        [field: SerializeField]
        internal SpriteRenderer[] spriteRenderers { get; private set; }
    }
}
