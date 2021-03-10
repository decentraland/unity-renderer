using UnityEngine;

namespace Builder.MeshLoadIndicator
{
    public class DCLBuilderMeshLoadIndicator : MonoBehaviour
    {
        [SerializeField] private Camera builderCamera = null;

        public string loadingEntityId { set; get; }

        private const float RELATIVE_SCALE_RATIO = 0.032f;

        private void LateUpdate()
        {
            transform.LookAt(transform.position + builderCamera.transform.rotation * Vector3.forward,
                        builderCamera.transform.rotation * Vector3.up);

            float dist = GetCameraPlaneDistance(builderCamera, transform.position);
            transform.localScale = new Vector3(RELATIVE_SCALE_RATIO * dist, RELATIVE_SCALE_RATIO * dist, RELATIVE_SCALE_RATIO * dist);
        }

        private static float GetCameraPlaneDistance(Camera camera, Vector3 objectPosition)
        {
            Plane plane = new Plane(camera.transform.forward, camera.transform.position);
            return plane.GetDistanceToPoint(objectPosition);
        }

        public void SetCamera(Camera camera)
        {
            builderCamera = camera;
        }
    }
}