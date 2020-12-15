using System.Collections;
using UnityEngine;
using DCL;
using DCL.Helpers;

public class VisualTestsBase : TestsBase
{
    protected override bool enableSceneIntegrityChecker => false;

    protected override IEnumerator SetUp()
    {
        yield break;
    }

    public IEnumerator InitVisualTestsScene(string testName)
    {
        yield return InitScene();
        yield return null;

        //TODO(Brian): This is to wait for SceneController.Awake(). We should remove this
        //             When the entry point is refactored.
        RenderProfileManifest.i.Initialize(RenderProfileManifest.i.testProfile);

        Environment.i.sceneBoundsChecker.Stop();

        base.SetUp_Renderer();

        VisualTestHelpers.currentTestName = testName;
        VisualTestHelpers.snapshotIndex = 0;

        DCLCharacterController.i.PauseGravity();
        DCLCharacterController.i.enabled = false;

        // Position character inside parcel (0,0)
        TestHelpers.SetCharacterPosition(new Vector3(0, 2f, 0f));

        yield return null;
    }

    protected override IEnumerator InitScene(bool usesWebServer = false, bool spawnCharController = true, bool spawnTestScene = true, bool spawnUIScene = true, bool debugMode = false, bool reloadUnityScene = true)
    {
        yield return InitUnityScene("MainVisualTest");

        if (debugMode)
            Environment.i.debugController.SetDebug();

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