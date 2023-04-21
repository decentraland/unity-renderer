using UnityEngine;

namespace DCLServices.MapRendererV2.Culling
{
    internal class MapCullingRectVisibilityChecker : IMapCullingVisibilityChecker
    {
        internal readonly float size;

        internal MapCullingRectVisibilityChecker(float size)
        {
            this.size = size;
        }

        public bool IsVisible<T>(T obj, CameraState cameraState) where T: IMapPositionProvider
        {
            var objSize = Vector2.one * size;
            return Intersects(cameraState.Rect, new Rect((Vector2) obj.CurrentPosition - (objSize / 2f), objSize));
        }

        private static bool Intersects(Rect r1, Rect r2) =>
            r1.min.x <= r2.max.x
            && r1.max.x >= r2.min.x
            && r1.min.y <= r2.max.y
            && r1.max.y >= r2.min.y;
    }
}
