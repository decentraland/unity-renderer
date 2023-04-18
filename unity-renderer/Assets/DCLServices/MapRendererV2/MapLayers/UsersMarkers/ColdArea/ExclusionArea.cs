using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.ColdArea
{
    internal struct ExclusionArea
    {
        public Vector2Int Position;
        public int Radius;

        public readonly bool Contains(in Vector2Int coords) =>
            (coords - Position).sqrMagnitude <= Radius * Radius;
    }
}
