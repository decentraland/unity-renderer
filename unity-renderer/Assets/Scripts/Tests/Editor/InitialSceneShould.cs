using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class InitialSceneShould
{
    //Note: this test should be deleted when we don't need to get the references from the initial scene anymore
    [Test]
    public void HaveReferencesAssgigned()
    {
        var scene =  EditorSceneManager.OpenScene("Assets/Scenes/InitialScene.unity");

        DebugConfigComponent component = GameObject.FindObjectOfType<DebugConfigComponent>();
        component.openBrowserWhenStart = false;

        var script = Resources.FindObjectsOfTypeAll<InitialSceneReferences>().FirstOrDefault();

        Assert.IsNotNull(script.mouseCatcherReference);
        Assert.IsNotNull(script.groundVisualReference);
        Assert.IsNotNull(script.cameraParentReference);
        Assert.IsNotNull(script.inputControllerReference);
        Assert.IsNotNull(script.cursorCanvasReference);
        Assert.IsNotNull(script.builderInWorldBridgeReference);
        Assert.IsNotNull(script.playerAvatarControllerReference);
        Assert.IsNotNull(script.bridgeGameObjectReference);

        EditorSceneManager.CloseScene(scene, true);
    }
}