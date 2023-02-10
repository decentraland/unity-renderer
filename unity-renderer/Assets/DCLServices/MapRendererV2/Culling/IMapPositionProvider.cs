using UnityEngine;

namespace DCLServices.MapRendererV2.Culling
{
    public interface IMapPositionProvider
    {
        Vector2 CurrentPosition { get; }
    }
}
