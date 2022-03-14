using Cinemachine;
using UnityEngine;

namespace DCL.Camera
{
    public class CameraStateFPS : CameraStateBase
    {
        protected Vector3Variable cameraForward => CommonScriptableObjects.cameraForward;
        protected Vector3NullableVariable characterForward => CommonScriptableObjects.characterForward;

        private CinemachinePOV pov;

        public override void Initialize(UnityEngine.Camera cameraTransform)
        {
            base.Initialize(cameraTransform);

            if (defaultVirtualCamera is CinemachineVirtualCamera vcamera)
                pov = vcamera.GetCinemachineComponent<CinemachinePOV>();
        }

        public override void OnUpdate()
        {
            var xzPlaneForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));
            characterForward.Set(xzPlaneForward);
        }

        public override Vector3 OnGetRotation()
        {
            if (pov != null)
                return new Vector3(pov.m_VerticalAxis.Value, pov.m_HorizontalAxis.Value, 0);

            return Vector3.zero;
        }

        public override void OnSetRotation(CameraController.SetRotationPayload payload)
        {
            var eulerDir = Vector3.zero;

            if (payload.cameraTarget.HasValue)
            {
                var cameraTarget = payload.cameraTarget.GetValueOrDefault();

                var horizontalAxisLookAt = payload.y - cameraTarget.y;
                var verticalAxisLookAt = new Vector3(cameraTarget.x - payload.x, 0, cameraTarget.z - payload.z);

                eulerDir.y = Vector3.SignedAngle(Vector3.forward, verticalAxisLookAt, Vector3.up);
                eulerDir.x = Mathf.Atan2(horizontalAxisLookAt, verticalAxisLookAt.magnitude) * Mathf.Rad2Deg;
            }

            if (pov != null)
            {
                pov.m_HorizontalAxis.Value = eulerDir.y;
                pov.m_VerticalAxis.Value = eulerDir.x;
            }
        }

        public override void OnBlock(bool blocked)
        {
            base.OnBlock(blocked);

            if (pov != null)
            {
                if (blocked)
                {
                    // Before block the virtual camera, we make the main camera point to the same point as the virtual one.
                    defaultVirtualCamera.transform.rotation = Quaternion.Euler(pov.m_VerticalAxis.Value, pov.m_HorizontalAxis.Value, 0f);
                }

                pov.enabled = !blocked;

                // NOTE(Santi): After modify the 'CinemachinePOV.enabled' parameter, the only (and not very elegant) way that Cinemachine
                //              offers to force the update is deactivating and re-activating the virtual camera.
                defaultVirtualCamera.enabled = false;
                defaultVirtualCamera.enabled = true;
            }
        }
    }
}