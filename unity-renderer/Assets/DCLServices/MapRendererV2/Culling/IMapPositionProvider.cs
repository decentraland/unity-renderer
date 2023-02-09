using UnityEngine;

namespace DCLServices.MapRendererV2.Culling
{
    public interface IMapPositionProvider
    {
        Vector2Int CurrentPosition { get; }
    }
}
