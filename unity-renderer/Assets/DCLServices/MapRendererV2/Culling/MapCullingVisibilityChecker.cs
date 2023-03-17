using UnityEngine;

namespace DCLServices.MapRendererV2.Culling
{
    internal class MapCullingVisibilityChecker : IMapCullingVisibilityChecker
    {
        private readonly float size;

        internal MapCullingVisibilityChecker() : this(100) { } //TODO: This is arbitrary, it needs fine tunning when we have a real scenario to test.

        internal MapCullingVisibilityChecker(float size)
        {
            this.size = size;
        }

        public bool IsVisible<T>(T obj, CameraState cameraState) where T: IMapPositionProvider =>
            GeometryUtility.TestPlanesAABB(cameraState.FrustumPlanes, new Bounds(obj.CurrentPosition, Vector3.one * size));
    }
}
