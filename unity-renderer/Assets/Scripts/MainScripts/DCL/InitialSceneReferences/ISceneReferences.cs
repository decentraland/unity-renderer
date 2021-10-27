using DCL.Camera;
using UnityEngine;

namespace DCL
{
    public interface ISceneReferences
    {
        GameObject groundVisual { get; }
        GameObject biwCameraParent { get; }
        GameObject cursorCanvas { get; }
        MouseCatcher mouseCatcher { get; }
        InputController inputController { get; }
        BuilderInWorldBridge builderInWorldBridge { get; }
        PlayerAvatarController playerAvatarController { get; }
        CameraController cameraController { get; }
        UnityEngine.Camera mainCamera { get; }
        GameObject bridgeGameObject { get; }
    }
}