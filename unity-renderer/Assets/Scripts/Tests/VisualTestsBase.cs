using System.Collections;
using UnityEngine;
using DCL;
using DCL.Helpers;

public class VisualTestsBase : TestsBase
{
    protected override IEnumerator InitScene(bool usesWebServer = false, bool spawnCharController = true, bool spawnTestScene = true, bool spawnUIScene = true, bool debugMode = false, bool reloadUnityScene = true)
    {
        yield return InitUnityScene("MainVisualTest");

        if (debugMode)
            SceneController.i.SetDebug();

        sceneController = TestHelpers.InitializeSceneController(usesWebServer);

        AssetPromiseKeeper_GLTF.i.Cleanup();

        yield return null;

        if (spawnTestScene)
        {
            scene = sceneController.CreateTestScene();
            yield return null;
        }

        if (spawnCharController)
        {
            if (DCLCharacterController.i == null)
            {
                GameObject.Instantiate(Resources.Load("Prefabs/CharacterController"));
            }
        }
    }
}
