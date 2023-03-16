using TMPro;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.ParcelHighlight
{
    internal class ParcelHighlightMarkerObject : MonoBehaviour
    {
        [field: SerializeField]
        internal SpriteRenderer spriteRenderer { get; private set; }

        [field: SerializeField]
        internal TextMeshPro text { get; private set; }
    }
}
