using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInitialSceneReferences
{
    GameObject mouseCatcher { get; }
    GameObject groundVisual { get; }
    GameObject cameraParent { get; }
    GameObject inputController { get; }
    GameObject cursorCanvas { get; }
    GameObject builderInWorldBridge { get; }
    GameObject playerAvatarController { get; }
    GameObject cameraController { get; }
    UnityEngine.Camera mainCamera { get; }
    GameObject bridgeGameObject { get; }
}