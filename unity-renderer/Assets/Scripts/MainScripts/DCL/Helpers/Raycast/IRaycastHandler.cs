using UnityEngine;
using DCL.Components;

namespace DCL.Helpers
{
    public class HitInfo
    {
        public Vector3 point;
        public Vector3 normal;
        public Collider collider;
        public Rigidbody rigidbody;
        public float distance;
    }

    public class RaycastHitInfo
    {
        public bool isValid = false;
        public HitInfo hit;
        public ColliderInfo collider;
    }
    public class RaycastResultInfo
    {
        public RaycastHitInfo hitInfo;
        public Ray ray;
    }

    public class RaycastResultInfoList
    {
        public RaycastHitInfo[] hitInfo;
        public Ray ray;
    }

    public interface IRaycastHandler
    {
        RaycastResultInfo Raycast(Ray ray, float distance, LayerMask layerMaskTarget, string sceneId);
        RaycastResultInfoList RaycastAll(Ray ray, float distance, LayerMask layerMaskTarget, string sceneId);
    }
}
