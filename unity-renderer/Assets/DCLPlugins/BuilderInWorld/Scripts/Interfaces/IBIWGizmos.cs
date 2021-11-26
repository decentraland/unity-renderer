using UnityEngine;

namespace DCL.Builder
{
    public interface IBIWGizmos
    {
        abstract void SetSnapFactor(SnapInfo snapInfo);

        bool initialized { get; }
        GameObject currentGameObject { get; }
        void Initialize(UnityEngine.Camera camera, Transform cameraHolderTransform);
        Vector3 GetActiveAxisVector();
        void OnEndDrag();
        void ForceRelativeScaleRatio();
        string GetGizmoType();
        void SetTargetTransform(Transform entityTransform);
        void OnBeginDrag(IBIWGizmosAxis axis, Transform entityTransform);
        float OnDrag(Vector3 hitPoint, Vector2 mousePosition);
        bool RaycastHit(Ray ray, out Vector3 hitPoint);
    }

    public class SnapInfo
    {
        public float position = 0;
        public float rotation = 0;
        public float scale = 0;
    }
}