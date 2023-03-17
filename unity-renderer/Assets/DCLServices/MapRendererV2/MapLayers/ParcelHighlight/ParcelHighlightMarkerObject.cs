using TMPro;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.ParcelHighlight
{
    internal class ParcelHighlightMarkerObject : MonoBehaviour
    {
        [field: SerializeField]
        internal Vector2 pivot { get; private set; } = new (0.5f, 0.5f);

        [field: SerializeField]
        internal SpriteRenderer spriteRenderer { get; private set; }

        [field: SerializeField]
        internal TextMeshPro text { get; private set; }
    }
}
