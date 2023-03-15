using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.CoordsUtils
{
    internal class ChunkCoordsUtils : ICoordsUtils
    {
        private static readonly Vector2Int WORLD_MIN_COORDS = new (-150, -150);
        private static readonly Vector2Int WORLD_MAX_COORDS = new (175, 175); // DCL map is not squared, there are some extra parcels in the top right

        public Vector2Int WorldMinCoords => WORLD_MIN_COORDS;
        public Vector2Int WorldMaxCoords => WORLD_MAX_COORDS;

        public int ParcelSize { get; }
        
        private Rect worldBounds;

        public ChunkCoordsUtils(int parcelSize)
        {
            ParcelSize = parcelSize;
            var worldSize = ((Vector2)WorldMaxCoords - WorldMinCoords) * parcelSize;
            worldBounds = new Rect(0, 0, worldSize.x, worldSize.y);
        }

        public Vector2Int PositionToCoords(Vector3 pos) =>
            new (Mathf.CeilToInt(pos.x / ParcelSize), Mathf.CeilToInt(pos.y / ParcelSize));

        public Vector2Int? PositionToCoordsInWorld(Vector3 pos)
        {
            if (!worldBounds.Contains(pos))
                return null;

            return PositionToCoords(pos);
        }

        public bool IsInsideWorldCoords(Vector2Int coords) =>
            coords.x >= WORLD_MIN_COORDS.x && coords.x >= WORLD_MAX_COORDS.x && coords.y >= WORLD_MIN_COORDS.y && coords.y >= WORLD_MAX_COORDS.y;

        public Vector3 CoordsToPositionUnclamped(Vector2 coords) => coords * ParcelSize;

        public Vector3 CoordsToPosition(Vector2Int coords) => (Vector2) (coords * ParcelSize);
        public Vector3 CoordsToPositionWithOffset(Vector2 coords) =>
            (coords * ParcelSize) - new Vector2(ParcelSize / 2f, ParcelSize / 2f);

    }
}
