using DCL;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.PlayerMarker
{
    internal class PlayerMarkerObject : MapRendererMarkerBase
    {
        [SerializeField] private SpriteRenderer compass;
        [SerializeField] private SpriteRenderer circle;

        private void Start()
        {
            if (!DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("map_focus_home_or_user"))
                circle.enabled = false;
        }

        public void SetSortingOrder(int sortingOrder)
        {
            compass.sortingOrder = sortingOrder;
            circle.sortingOrder = sortingOrder - 1;
        }
    }
}
