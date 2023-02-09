using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.CoordsUtils
{
    public class ChunkCoordsUtils : ICoordsUtils
    {
        public int ParcelSize { get; }

        public ChunkCoordsUtils(int parcelSize)
        {
            ParcelSize = parcelSize;
        }

        public Vector2Int PositionToCoords(Vector3 pos) =>
            new (Mathf.CeilToInt(pos.x / ParcelSize), Mathf.CeilToInt(pos.y / ParcelSize));

        public Vector2 CoordsToPositionUnclamped(Vector2Int coords) =>
            throw new NotImplementedException();

        public Vector2 CoordsToPositionUnclamped(Vector2Int coords, int parcelSize) =>
            throw new NotImplementedException();

        public Vector2 CoordsToPosition(Vector2Int coords) =>
            throw new NotImplementedException();

        public Vector2 CoordsToPosition(Vector2Int coords, int parcelSize) =>
            throw new NotImplementedException();

        public Vector2 CoordsToPositionWithOffset(Vector3 coords) =>
            throw new NotImplementedException();

        public Vector2 CoordsToPositionWithOffset(Vector3 coords, int parcelSize) =>
            throw new NotImplementedException();

        public Vector2 CoordsToPosition(Vector2 coords) =>
            throw new NotImplementedException();

        public Vector2 CoordsToPosition(Vector2 coords, int parcelSize) =>
            throw new NotImplementedException();
    }
}
