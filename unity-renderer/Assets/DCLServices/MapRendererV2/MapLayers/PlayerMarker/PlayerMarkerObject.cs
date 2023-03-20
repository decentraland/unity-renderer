using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.PlayerMarker
{
    internal class PlayerMarkerObject : MapRendererMarkerBase
    {
        [field: SerializeField]
        internal SpriteRenderer spriteRenderer { get; private set; }
    }
}
