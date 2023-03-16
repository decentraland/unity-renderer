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

        public Vector3 CoordsToPositionUnclamped(Vector2 coords);

        public Vector3 CoordsToPosition(Vector2Int coords);

        public Vector3 CoordsToPositionWithOffset(Vector2 coords);

        /// <summary>
        /// Sets map object scale accordingly to the `ParcelSize`
        /// </summary>
        internal void SetObjectScale(MonoBehaviour obj)
        {
            var parcelSize = ParcelSize;
            obj.transform.localScale = new Vector3(parcelSize, parcelSize, 1);
        }
    }
}
