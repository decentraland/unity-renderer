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
    }

    [TearDown]
    public void TearDown() { SceneManager.UnloadSceneAsync("InitialScene"); }
}