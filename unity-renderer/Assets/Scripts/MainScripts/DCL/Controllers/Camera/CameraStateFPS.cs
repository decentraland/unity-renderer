using Cinemachine;
using UnityEngine;

namespace DCL.Camera
{
    public class CameraStateFPS : CameraStateBase
    {
        protected Vector3Variable cameraForward => CommonScriptableObjects.cameraForward;
        protected Vector3NullableVariable characterForward => CommonScriptableObjects.characterForward;

        private CinemachinePOV pov;

        public override void Init(UnityEngine.Camera cameraTransform)
        {
            base.Init(cameraTransform);

            if (defaultVirtualCamera is CinemachineVirtualCamera vcamera)
                pov = vcamera.GetCinemachineComponent<CinemachinePOV>();
        }

        public override void OnSelect()
        {
            base.OnSelect();
            CommonScriptableObjects.playerIsOnMovingPlatform.OnChange += OnMovingPlatformStart;
            if (CommonScriptableObjects.playerIsOnMovingPlatform.Get())
                OnMovingPlatformStart(true, true);
        }

        public override void OnUnselect()
        {
            base.OnUnselect();
            CommonScriptableObjects.playerIsOnMovingPlatform.OnChange -= OnMovingPlatformStart;
            CommonScriptableObjects.movingPlatformRotationDelta.OnChange -= OnMovingPlatformRotate;
        }
        private void OnMovingPlatformStart(bool current, bool previous)
        {
            if (current)
            {
                CommonScriptableObjects.movingPlatformRotationDelta.OnChange += OnMovingPlatformRotate;
            }
            else
            {
                CommonScriptableObjects.movingPlatformRotationDelta.OnChange -= OnMovingPlatformRotate;
            }
        }
        private void OnMovingPlatformRotate(Quaternion current, Quaternion previous)
        {
            // Disabled camera rotation on moving platforms because this was causing issues for some scenes
            return;
            Vector3 diff = current.eulerAngles;
            if (pov != null)
            {
                pov.m_HorizontalAxis.Value += diff.y;
                pov.m_VerticalAxis.Value += diff.x;
            }
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
                var newPos = new Vector3(payload.x, payload.y, payload.z);
                var cameraTarget = payload.cameraTarget.GetValueOrDefault();
                var dirToLook = (cameraTarget - newPos);
                eulerDir = Quaternion.LookRotation(dirToLook).eulerAngles;
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