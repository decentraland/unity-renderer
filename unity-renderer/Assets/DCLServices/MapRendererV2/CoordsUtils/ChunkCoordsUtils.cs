using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCLServices.MapRendererV2.CoordsUtils
{
    internal class ChunkCoordsUtils : ICoordsUtils
    {
        private static readonly Vector2Int WORLD_MIN_COORDS = new (-150, -150);
        private static readonly Vector2Int WORLD_MAX_COORDS = new (175, 175); // DCL map is not squared, there are some extra parcels in the top right

        private static readonly Vector2Int VISIBLE_WORLD_MIN_COORDS = new (-175, -175);
        private static readonly Vector2Int VISIBLE_WORLD_MAX_COORDS = new (175, 175); // DCL map is not squared, there are some extra parcels in the top right

        private static readonly Rect[] INTERACTABLE_WORLD_BOUNDS =
        {
            Rect.MinMaxRect(-150, -150, 150, 150),
            Rect.MinMaxRect(62, 151, 162, 158),
            Rect.MinMaxRect(151, 59, 163, 150)
        };

        private readonly List<Rect> interactableWorldBoundsInLocalCoordinates;

        public Vector2Int WorldMinCoords => WORLD_MIN_COORDS;
        public Vector2Int WorldMaxCoords => WORLD_MAX_COORDS;

        public int ParcelSize { get; }
        public Rect VisibleWorldBounds { get; }

        public ChunkCoordsUtils(int parcelSize)
        {
            ParcelSize = parcelSize;

            var min = (VISIBLE_WORLD_MIN_COORDS - Vector2Int.one) * parcelSize;
            var max = VISIBLE_WORLD_MAX_COORDS * parcelSize;
            VisibleWorldBounds = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

            interactableWorldBoundsInLocalCoordinates = INTERACTABLE_WORLD_BOUNDS
                                                       .Select(chunk => Rect.MinMaxRect((chunk.xMin - 1) * parcelSize, (chunk.yMin - 1) * parcelSize, chunk.xMax * parcelSize, chunk.yMax * parcelSize))
                                                       .ToList();
        }

        public bool TryGetCoordsWithinInteractableBounds(Vector3 pos, out Vector2Int coords)
        {
            coords = default;

            foreach (Rect rect in interactableWorldBoundsInLocalCoordinates)
            {
                if (rect.Contains(pos))
                {
                    coords = PositionToCoords(pos);
                    return true;
                }
            }

            return false;
        }

        public Vector2Int PositionToCoords(Vector3 pos) =>
            new (Mathf.CeilToInt(pos.x / ParcelSize), Mathf.CeilToInt(pos.y / ParcelSize));

        public Vector2 PositionToCoordsUnclamped(Vector3 pos) =>
            pos / ParcelSize;

        public Vector3 CoordsToPositionUnclamped(Vector2 coords) =>
            coords * ParcelSize;

        public Vector3 CoordsToPosition(Vector2Int coords) =>
            (Vector2)(coords * ParcelSize);

        public Vector3 CoordsToPositionWithOffset(Vector2 coords) =>
            (coords * ParcelSize) - new Vector2(ParcelSize / 2f, ParcelSize / 2f);
    }
}
