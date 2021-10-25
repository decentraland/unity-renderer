using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInititalSceneReference
{
    GameObject mouseCatcherReference { get; }
    GameObject groundVisualReference { get; }
    GameObject cameraParentReference { get; }
    GameObject inputControllerReference { get; }
    GameObject cursorCanvasReference { get; }
    GameObject builderInWorldBridgeReference { get; }
    GameObject playerAvatarControllerReference { get; }
    GameObject cameraControllerReference { get; }
    UnityEngine.Camera mainCameraReference { get; }
    GameObject bridgeGameObjectReference { get; }
}