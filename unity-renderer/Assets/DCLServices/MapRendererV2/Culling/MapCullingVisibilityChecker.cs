using UnityEngine;

namespace DCLServices.MapRendererV2.Culling
{
    internal class MapCullingVisibilityChecker : IMapCullingVisibilityChecker
    {
        private const float SIZE = 100; //TODO: This is arbitrary, it needs fine tunning when we have a real scenario to test.

        public bool IsVisible<T>(T obj, CameraState cameraState) where T: IMapPositionProvider =>
            GeometryUtility.TestPlanesAABB(cameraState.FrustrumPlanes, new Bounds(obj.CurrentPosition, Vector3.one * SIZE));
    }
}
