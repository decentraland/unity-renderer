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
            skyboxCamera.depth = -5;
            skyboxCamera.cullingMask = (1 << LayerMask.NameToLayer("Skybox"));

            // Attach follow script
            camBehavior = skyboxCameraGO.AddComponent<SkyboxCameraBehavior>();
        }

        public void AssignTargetCamera(Transform mainCam)
        {
            Camera camComponent = mainCam.GetComponent<Camera>();
            var mainCameraData = camComponent.GetUniversalAdditionalCameraData();
            mainCameraData.renderType = CameraRenderType.Overlay;

            var cameraData = skyboxCamera.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(camComponent);


            camBehavior.targetCamera = mainCam.gameObject;
        }

        public void SetCameraEnabledState(bool enabled) { skyboxCamera.enabled = enabled; }
    }
}