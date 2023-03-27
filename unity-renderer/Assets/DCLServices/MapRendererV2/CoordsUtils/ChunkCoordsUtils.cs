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
        public Rect WorldBounds { get; }

        public ChunkCoordsUtils(int parcelSize)
        {
            ParcelSize = parcelSize;
            //var worldSize = ((Vector2)WorldMaxCoords - WorldMinCoords) * parcelSize;
            var min = WORLD_MIN_COORDS * parcelSize;
            var max = WORLD_MAX_COORDS * parcelSize;
            WorldBounds = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        public Vector2Int PositionToCoords(Vector3 pos) =>
            new (Mathf.CeilToInt(pos.x / ParcelSize), Mathf.CeilToInt(pos.y / ParcelSize));

        public Vector2 PositionToCoordsUnclamped(Vector3 pos) =>
            pos / ParcelSize;

        public bool IsInsideWorldCoords(Vector2Int coords) =>
            coords.x >= WORLD_MIN_COORDS.x && coords.x >= WORLD_MAX_COORDS.x && coords.y >= WORLD_MIN_COORDS.y && coords.y >= WORLD_MAX_COORDS.y;

        public Vector3 CoordsToPositionUnclamped(Vector2 coords) => coords * ParcelSize;

        public Vector3 CoordsToPosition(Vector2Int coords) => (Vector2) (coords * ParcelSize);

        public Vector3 CoordsToPositionWithOffset(Vector2 coords) =>
            (coords * ParcelSize) - new Vector2(ParcelSize / 2f, ParcelSize / 2f);

    }
}
