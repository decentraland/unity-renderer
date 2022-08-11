using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DCL.Skybox
{

    public class SkyboxCamera
    {
        private GameObject skyboxCameraGO;
        private Camera skyboxCamera;
        private SkyboxCameraBehaviour camBehavior;

        public SkyboxCamera()
        {
            // Make a new camera
            skyboxCameraGO = new GameObject("Skybox Camera");
            skyboxCameraGO.transform.position = Vector3.zero;
            skyboxCameraGO.transform.rotation = Quaternion.identity;

            // Attach camera component
            skyboxCamera = skyboxCameraGO.AddComponent<Camera>();

            var cameraData = skyboxCamera.GetUniversalAdditionalCameraData();
            cameraData.renderShadows = false;
            skyboxCamera.useOcclusionCulling = false;
            skyboxCamera.cullingMask = (1 << LayerMask.NameToLayer("Skybox"));

            // Attach follow script
            camBehavior = skyboxCameraGO.AddComponent<SkyboxCameraBehaviour>();
        }

        public void AssignTargetCamera(Transform mainCam)
        {
            if (mainCam == null)
                return;

            Camera mainCamComponent = mainCam.GetComponent<Camera>();
            var mainCameraData = mainCamComponent.GetUniversalAdditionalCameraData();
            var cameraStack = mainCameraData.cameraStack;

            mainCameraData.renderType = CameraRenderType.Overlay;

            var cameraData = skyboxCamera.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(mainCamComponent);
            foreach (Camera camera in cameraStack)
            {
                cameraData.cameraStack.Add(camera);
            }

            camBehavior.AssignCamera(mainCamComponent, skyboxCamera);
        }
    }
}