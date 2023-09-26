using TMPro;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.Favorites
{
    internal class FavoriteMarkerObject : MapRendererMarkerBase
    {
        [field: SerializeField]
        internal TextMeshPro title { get; set; }

        [field: SerializeField]
        internal SpriteRenderer[] renderers { get; private set; }
    }
}
