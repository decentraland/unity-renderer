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
        private SkyboxCameraBehavior camBehavior;

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
            camBehavior = skyboxCameraGO.AddComponent<SkyboxCameraBehavior>();
        }

        public void AssignTargetCamera(Transform mainCam)
        {
            Camera mainCamComponent = mainCam.GetComponent<Camera>();
            var mainCameraData = mainCamComponent.GetUniversalAdditionalCameraData();
            mainCameraData.renderType = CameraRenderType.Overlay;

            var cameraData = skyboxCamera.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(mainCamComponent);


            camBehavior.AssignCamera(mainCamComponent, skyboxCamera);
        }

        public void SetCameraEnabledState(bool enabled) { skyboxCamera.enabled = enabled; }
    }
}