using Cinemachine;
using DCL.Camera;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public struct SceneReferencesData
    {
        public MouseCatcher mouseCatcher;
        public GameObject groundVisual;
        public GameObject biwCameraParent;
        public InputController inputController;
        public GameObject cursorCanvas;
        public BuilderInWorldBridge builderInWorldBridge;
        public PlayerAvatarController playerAvatarController;
        public CameraController cameraController;
        public UnityEngine.Camera mainCamera;
        public GameObject bridgeGameObject;
        public Light environmentLight;
        public Volume postProcessVolume;
        public CinemachineFreeLook thirdPersonCamera;
        public CinemachineVirtualCamera firstPersonCamera;

        public SceneReferencesData(MouseCatcher mouseCatcher,
            GameObject groundVisual,
            GameObject biwCameraParent,
            InputController inputController,
            GameObject cursorCanvas,
            BuilderInWorldBridge builderInWorldBridge,
            PlayerAvatarController playerAvatarController,
            CameraController cameraController,
            UnityEngine.Camera mainCamera,
            GameObject bridgeGameObject,
            Light environmentLight,
            Volume postProcessVolume, 
            CinemachineFreeLook thirdPersonCamera,
            CinemachineVirtualCamera firstPersonCamera)
        {
            this.mouseCatcher = mouseCatcher;
            this.groundVisual = groundVisual;
            this.biwCameraParent = biwCameraParent;
            this.inputController = inputController;
            this.cursorCanvas = cursorCanvas;
            this.builderInWorldBridge = builderInWorldBridge;
            this.playerAvatarController = playerAvatarController;
            this.cameraController = cameraController;
            this.mainCamera = mainCamera;
            this.bridgeGameObject = bridgeGameObject;
            this.environmentLight = environmentLight;
            this.postProcessVolume = postProcessVolume;
            this.thirdPersonCamera = thirdPersonCamera;
            this.firstPersonCamera = firstPersonCamera;
        }
    }
}