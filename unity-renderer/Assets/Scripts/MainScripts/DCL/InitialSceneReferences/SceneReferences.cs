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
        public readonly SceneReferencesData data;

        public GameObject groundVisual => data.groundVisual;
        public GameObject biwCameraParent => data.biwCameraParent;
        public GameObject cursorCanvas => data.cursorCanvas;
        public MouseCatcher mouseCatcher => data.mouseCatcher;
        public InputController inputController => data.inputController;
        public BuilderInWorldBridge builderInWorldBridge => data.builderInWorldBridge;
        public PlayerAvatarController playerAvatarController => data.playerAvatarController;
        public CameraController cameraController => data.cameraController;
        public UnityEngine.Camera mainCamera => data.mainCamera;
        public GameObject bridgeGameObject => data.bridgeGameObject;
        public Light environmentLight => data.environmentLight;
        public Volume postProcessVolume => data.postProcessVolume;
        public CinemachineFreeLook thirdPersonCamera => data.thirdPersonCamera;
        public CinemachineVirtualCamera firstPersonCamera => data.firstPersonCamera;

        public SceneReferences(SceneReferencesData referencesData)
        {
            i ??= this;
            data = referencesData;
        }
    }

}