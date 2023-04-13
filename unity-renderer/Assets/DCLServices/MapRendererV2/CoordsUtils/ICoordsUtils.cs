using UnityEngine;

namespace DCLServices.MapRendererV2.CoordsUtils
{
    internal interface ICoordsUtils
    {
        public Vector2Int WorldMinCoords { get; }
        public Vector2Int WorldMaxCoords { get; }

        /// <summary>
        /// World bounds visible on the map
        /// </summary>
        Rect VisibleWorldBounds { get; }

        int ParcelSize { get; }

        /// <summary>
        /// Clamps position within interactable bounds
        /// </summary>
        bool TryGetCoordsWithinInteractableBounds(Vector3 pos, out Vector2Int coords);

        Vector2Int PositionToCoords(Vector3 pos);

        Vector2 PositionToCoordsUnclamped(Vector3 pos);

        Vector3 CoordsToPositionUnclamped(Vector2 coords);

        Vector3 CoordsToPosition(Vector2Int coords);

        Vector3 CoordsToPositionWithOffset(Vector2 coords);
    }
}
