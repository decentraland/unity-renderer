using System.Data.Common;
using Cinemachine;
using DCL.Camera;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public class SceneReferences
    {
        public static SceneReferences i { get; private set; }
        
        public readonly MouseCatcher mouseCatcher;
        public readonly GameObject groundVisual;
        public readonly GameObject biwCameraParent;
        public readonly InputController inputController;
        public readonly GameObject cursorCanvas;
        public readonly BuilderInWorldBridge builderInWorldBridge;
        public readonly PlayerAvatarController playerAvatarController;
        public readonly CameraController cameraController;
        public readonly UnityEngine.Camera mainCamera;
        public readonly GameObject bridgeGameObject;
        public readonly Light environmentLight;
        public readonly Volume postProcessVolume;
        public readonly CinemachineFreeLook thirdPersonCamera;
        public readonly CinemachineVirtualCamera firstPersonCamera;

        public SceneReferences(MouseCatcher mouseCatcher,
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
            
            i = this;
        }
    }

}