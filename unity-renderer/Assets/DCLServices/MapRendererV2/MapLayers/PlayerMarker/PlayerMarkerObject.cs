using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.PlayerMarker
{
    public class PlayerMarkerObject : MonoBehaviour
    {
        [field: SerializeField]
        internal SpriteRenderer spriteRenderer { get; private set; }
    }
}
