using TMPro;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.PointsOfInterest
{
    internal class SceneOfInterestMarkerObject : MapRendererMarkerBase
    {
        [field: SerializeField]
        internal TextMeshPro title { get; set; }

        [field: SerializeField]
        internal SpriteRenderer[] renderers { get; private set; }
    }
}
