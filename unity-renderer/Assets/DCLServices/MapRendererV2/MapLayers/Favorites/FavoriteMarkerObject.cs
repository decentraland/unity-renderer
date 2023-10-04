using TMPro;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.Favorites
{
    internal class FavoriteMarkerObject : MapRendererMarkerBase
    {
        [field: SerializeField] internal TextMeshPro title { get; set; }
        [field: SerializeField] internal SpriteRenderer[] renderers { get; private set; }

        private float baseTextScale;

        private void Awake()
        {
            baseTextScale = title.transform.localScale.x;
        }

        public void SetScale(float baseScale, float newScale)
        {
            transform.localScale = new Vector3(newScale, newScale, 1f);

            // Apply inverse scaling to the text object
            float textScaleFactor = baseScale / newScale; // Calculate the inverse scale factor
            title.transform.localScale = new Vector3(baseTextScale * textScaleFactor, baseTextScale * textScaleFactor, 1f);

        }
    }
}
