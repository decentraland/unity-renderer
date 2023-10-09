using System;
using TMPro;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.ParcelHighlight
{
    internal class ParcelHighlightMarkerObject : MapRendererMarkerBase
    {
        [field: SerializeField]
        internal SpriteRenderer spriteRenderer { get; private set; }

        [field: SerializeField]
        internal TextMeshPro text { get; private set; }

        private float textBaseScale;

        private void Awake()
        {
            textBaseScale = text.transform.localScale.x;
        }

        public void SetScale(float baseScale, float newScale)
        {
            text.transform.localScale = new Vector3(textBaseScale * newScale/baseScale, textBaseScale * newScale/baseScale, 1f);
        }
    }
}
