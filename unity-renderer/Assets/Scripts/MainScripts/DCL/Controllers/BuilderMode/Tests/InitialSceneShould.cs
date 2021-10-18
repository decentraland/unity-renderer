using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class InitialSceneShould
{
    //Note: this test should be deleted when we don't need to get the references from the initial scene anymore
    [UnityTest]
    public IEnumerator HaveReferencesAssgigned()
    {
        yield return SceneManager.LoadSceneAsync("InitialScene");
        DebugConfigComponent component = GameObject.FindObjectOfType<DebugConfigComponent>();
        component.openBrowserWhenStart = false;

        Assert.IsNotNull(InitialSceneReferences.i?.mouseCatcher);
        Assert.IsNotNull(InitialSceneReferences.i.groundVisual);
        Assert.IsNotNull(InitialSceneReferences.i.cameraParent);
        Assert.IsNotNull(InitialSceneReferences.i.inputController);
        Assert.IsNotNull(InitialSceneReferences.i.cursorCanvas);
        Assert.IsNotNull(InitialSceneReferences.i.builderInWorldBridge);
        Assert.IsNotNull(InitialSceneReferences.i.playerAvatarController);
        Assert.IsNotNull(InitialSceneReferences.i.bridgeGameObject);

        yield return SceneManager.UnloadSceneAsync("InitialScene");
    }
}