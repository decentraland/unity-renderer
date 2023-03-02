using System.Data.Common;
using Cinemachine;
using DCL.Camera;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public class SceneReferences : Singleton<SceneReferences>, ISceneReferences
    {
        public MouseCatcher mouseCatcher { get; internal set; }
        public GameObject groundVisual { get; internal set; }
        public InputController inputController { get; internal set; }
        public GameObject cursorCanvas { get; internal set; }
        public PlayerAvatarController playerAvatarController { get; internal set; }
        public CameraController cameraController { get; internal set; }
        public UnityEngine.Camera mainCamera { get; internal set; }
        public GameObject bridgeGameObject { get; internal set; }
        public Light environmentLight { get; internal set; }
        public Volume postProcessVolume { get; internal set; }
        public CinemachineFreeLook thirdPersonCamera { get; internal set; }
        public CinemachineVirtualCamera firstPersonCamera { get; internal set; }
        public void Dispose() {  }

        public void Initialize(MouseCatcher mouseCatcher,
            GameObject groundVisual,
            InputController inputController,
            GameObject cursorCanvas,
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
            this.inputController = inputController;
            this.cursorCanvas = cursorCanvas;
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