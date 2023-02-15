using UnityEngine;

namespace DCLServices.MapRendererV2.CoordsUtils
{
    internal interface ICoordsUtils
    {
        public Vector2Int WorldMinCoords { get; }
        public Vector2Int WorldMaxCoords { get; }

        public int ParcelSize { get; }

        public Vector2Int PositionToCoords(Vector3 pos);

        public Vector2Int? PositionToCoordsInWorld(Vector3 pos);

        public bool IsInsideWorldCoords(Vector2Int coords);

        public Vector2 CoordsToPositionUnclamped(Vector2Int coords);

        public Vector2 CoordsToPositionUnclamped(Vector2Int coords, int parcelSize);

        public Vector2 CoordsToPosition(Vector2Int coords);

        public Vector2 CoordsToPosition(Vector2Int coords, int parcelSize);

        public Vector2 CoordsToPositionWithOffset(Vector3 coords);

        public Vector2 CoordsToPositionWithOffset(Vector3 coords, int parcelSize);
    }
}
