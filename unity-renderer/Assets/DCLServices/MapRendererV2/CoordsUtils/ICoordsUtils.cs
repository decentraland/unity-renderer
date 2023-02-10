using UnityEngine;

namespace DCLServices.MapRendererV2.CoordsUtils
{
    internal interface ICoordsUtils
    {
        public int ParcelSize { get; }

        public Vector2Int PositionToCoords(Vector3 pos);

        public Vector2 CoordsToPositionUnclamped(Vector2Int coords);

        public Vector2 CoordsToPositionUnclamped(Vector2Int coords, int parcelSize);

        public Vector2 CoordsToPosition(Vector2Int coords);

        public Vector2 CoordsToPosition(Vector2Int coords, int parcelSize);

        public Vector2 CoordsToPositionWithOffset(Vector3 coords);

        public Vector2 CoordsToPositionWithOffset(Vector3 coords, int parcelSize);
    }
}
