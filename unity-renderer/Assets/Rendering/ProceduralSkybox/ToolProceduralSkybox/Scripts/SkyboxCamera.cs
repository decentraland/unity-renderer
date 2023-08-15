using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DCL.Skybox
{
    public class SkyboxCamera
    {
        private readonly GameObject skyboxCameraGO;
        private readonly Camera skyboxCamera;
        private readonly SkyboxCameraBehaviour camBehavior;

        private List<Camera> skyboxCameraStack;

        public Camera CurrentCamera { get; private set; }

        public SkyboxCamera()
        {
            // Make a new camera
            skyboxCameraGO = new GameObject("Skybox Camera");
            skyboxCameraGO.transform.position = Vector3.zero;
            skyboxCameraGO.transform.rotation = Quaternion.identity;

            // Attach camera component
            skyboxCamera = skyboxCameraGO.AddComponent<Camera>();

            UniversalAdditionalCameraData cameraData = skyboxCamera.GetUniversalAdditionalCameraData();
            cameraData.renderShadows = false;

            // This index is defined in UniversalRenderPipelineAsset
            // We are using a custom ForwardRenderer with less features that increase the performance and lowers the passes
            cameraData.SetRenderer(1);

            skyboxCamera.useOcclusionCulling = false;
            skyboxCamera.cullingMask = 1 << LayerMask.NameToLayer("Skybox");
            skyboxCamera.farClipPlane = 5000;

            // Attach follow script
            camBehavior = skyboxCameraGO.AddComponent<SkyboxCameraBehaviour>();
        }

        public void AssignTargetCamera(Transform mainCam)
        {
            if (mainCam == null)
                return;

            CurrentCamera = mainCam.GetComponent<Camera>();
            UniversalAdditionalCameraData mainCameraData = CurrentCamera.GetUniversalAdditionalCameraData();
            List<Camera> cameraStack = mainCameraData.cameraStack;

            mainCameraData.renderType = CameraRenderType.Overlay;

            if (skyboxCameraStack == null)
            {
                UniversalAdditionalCameraData cameraData = skyboxCamera.GetUniversalAdditionalCameraData();
                skyboxCameraStack = cameraData.cameraStack;
                skyboxCameraStack.Add(CurrentCamera);

                foreach (Camera camera in cameraStack)
                    skyboxCameraStack.Add(camera);
            }
            else
                skyboxCameraStack[0] = CurrentCamera;

            camBehavior.AssignCamera(CurrentCamera, skyboxCamera);
        }

        public void SetCameraEnabledState(bool enabled)
        {
            skyboxCamera.enabled = enabled;
        }
    }
}
