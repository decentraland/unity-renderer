using DCL;
using DCL.Controllers;
using DCL.Models;
using DCL.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class TestsBase
{
    protected SceneController sceneController;
    protected ParcelScene scene;

    [UnityTearDown]
    public virtual IEnumerator TearDown()
    {
        AssetPromiseKeeper_GLTF.i.Cleanup();
        yield return null;
    }

    protected IEnumerator InitUnityScene(string sceneName = null)
    {
        yield return TestHelpers.UnloadAllUnityScenes();

        Scene? newScene;

        if (string.IsNullOrEmpty(sceneName))
        {
            newScene = SceneManager.CreateScene(TestHelpers.testingSceneName + (TestHelpers.testSceneIteration++));
            if (newScene.HasValue)
            {
                SceneManager.SetActiveScene(newScene.Value);
            }
        }
        else
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }
    }

    protected virtual IEnumerator InitScene(bool usesWebServer = false, bool spawnCharController = true, bool spawnTestScene = true, bool spawnUIScene = true, bool debugMode = false, bool reloadUnityScene = true)
    {
        if (SceneController.i == null || reloadUnityScene)
        {
            yield return InitUnityScene("MainTest");
        }

        if (debugMode)
            SceneController.i.SetDebug();

        yield return MemoryManager.i.CleanupPoolsIfNeeded(true);

        sceneController = TestHelpers.InitializeSceneController(usesWebServer);

        yield return new WaitForSeconds(0.01f);

        if (spawnTestScene)
        {
            scene = sceneController.CreateTestScene();

            yield return new WaitForSeconds(0.01f);
        }

        if (spawnCharController)
        {
            if (DCLCharacterController.i == null)
            {
                GameObject.Instantiate(Resources.Load("Prefabs/CharacterController"));
            }

            yield return new WaitForSeconds(0.01f);
        }

        var newPos = new Vector3(10, 0, 10);
        DCLCharacterController.i.SetPosition(newPos);
        yield return null;

        if (spawnUIScene)
        {
            string globalSceneId = "global-scene";

            sceneController.CreateUIScene(
                JsonConvert.SerializeObject(
                    new CreateUISceneMessage
                    {
                        id = globalSceneId,
                        baseUrl = "",
                    })
            );
        }

        DCL.PointerEventsController.i.Initialize(isTesting: true);

        yield return new WaitForAllMessagesProcessed();
    }

    protected IEnumerator WaitForUICanvasUpdate()
    {
        yield break;
    }

    public static T Reflection_GetStaticField<T>(Type baseType, string fieldName)
    {
        return (T)baseType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
    }

    public static T Reflection_GetField<T>(object instance, string fieldName)
    {
        return (T)instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
    }

}
