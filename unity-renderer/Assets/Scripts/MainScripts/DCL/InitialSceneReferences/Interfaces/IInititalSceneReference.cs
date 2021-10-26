using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInitialSceneReferences
{
    GameObject mouseCatcherReference2 { get; }
    GameObject groundVisualReference2 { get; }
    GameObject cameraParentReference2 { get; }
    GameObject inputControllerReference2 { get; }
    GameObject cursorCanvasReference2 { get; }
    GameObject builderInWorldBridgeReference2 { get; }
    GameObject playerAvatarControllerReference2 { get; }
    GameObject cameraControllerReference2 { get; }
    UnityEngine.Camera mainCameraReference2 { get; }
    GameObject bridgeGameObjectReference2 { get; }
}