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
        public PlayerAvatarController playerAvatarController { get; set; }
        public CameraController cameraController { get; internal set; }
        public UnityEngine.Camera mainCamera { get; internal set; }
        public GameObject bridgeGameObject { get; internal set; }
        public Light environmentLight { get; internal set; }
        public Volume postProcessVolume { get; internal set; }
        public CinemachineFreeLook thirdPersonCamera { get; internal set; }
        public CinemachineVirtualCamera firstPersonCamera { get; internal set; }
        public void Dispose() {  }
    }
}
