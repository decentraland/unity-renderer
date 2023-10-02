﻿using DCL;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.PlayerMarker
{
    internal class PlayerMarkerObject : MapRendererMarkerBase
    {
        [SerializeField] private SpriteRenderer compass;
        [SerializeField] private SpriteRenderer circle;
        [SerializeField] private SpriteRenderer friendsArea;

        private float scaleDivider = 20f;

        public void SetAnimatedCircleVisibility(bool visible)
        {
            circle.enabled = visible;
            friendsArea.enabled = visible;
        }

        public void SetSortingOrder(int sortingOrder)
        {
            compass.sortingOrder = sortingOrder;
            circle.sortingOrder = sortingOrder - 1;
            friendsArea.sortingOrder = sortingOrder - 1;
        }

        public void SetZoom(float zoom)
        {
            transform.localScale = new Vector3(zoom/ scaleDivider, zoom/ scaleDivider, 1f);
        }
    }
}
