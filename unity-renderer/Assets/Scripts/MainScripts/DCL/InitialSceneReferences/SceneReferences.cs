using System.Data.Common;
using Cinemachine;
using DCL.Camera;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public class SceneReferences : ISceneReferences
    {
        public static SceneReferences i { get; private set; }

        public MouseCatcher mouseCatcher { get; private set; }
        public GameObject groundVisual { get; private set; }
        public GameObject biwCameraParent { get; private set; }
        public InputController inputController { get; private set; }
        public GameObject cursorCanvas { get; private set; }
        public GameObject bridges { get; private set; }
        public PlayerAvatarController playerAvatarController { get; private set; }
        public CameraController cameraController { get; private set; }
        public UnityEngine.Camera mainCamera { get; private set; }
        public GameObject bridgeGameObject { get; private set; }
        public Light environmentLight { get; private set; }
        public Volume postProcessVolume { get; private set; }
        public CinemachineFreeLook thirdPersonCamera { get; private set; }
        public CinemachineVirtualCamera firstPersonCamera { get; private set; }
        public void Dispose() {  }

        public SceneReferences(MouseCatcher mouseCatcher,
            GameObject groundVisual,
            GameObject biwCameraParent,
            InputController inputController,
            GameObject cursorCanvas,
            GameObject bridges,
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
            this.bridges = bridges;
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