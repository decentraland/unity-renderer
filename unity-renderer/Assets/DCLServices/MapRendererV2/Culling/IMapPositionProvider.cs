using UnityEngine;

namespace DCLServices.MapRendererV2.Culling
{
    public interface IMapPositionProvider
    {
        Vector3 CurrentPosition { get; }
    }
}
