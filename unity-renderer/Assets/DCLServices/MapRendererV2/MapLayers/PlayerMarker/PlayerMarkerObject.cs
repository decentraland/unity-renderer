using DCL;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.PlayerMarker
{
    internal class PlayerMarkerObject : MapRendererMarkerBase
    {
        [SerializeField] private SpriteRenderer compass;
        [SerializeField] private SpriteRenderer circle;

        public void SetAnimatedCircleVisibility(bool visible)
        {
            circle.enabled = visible;
        }

        public void SetSortingOrder(int sortingOrder)
        {
            compass.sortingOrder = sortingOrder;
            circle.sortingOrder = sortingOrder - 1;
        }
    }
}
