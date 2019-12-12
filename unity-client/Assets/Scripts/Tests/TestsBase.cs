using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestsBase
{
    protected SceneController sceneController;
    protected ParcelScene scene;

    [UnityTearDown]
    public virtual IEnumerator TearDown()
    {
        yield return null;

        AssetPromiseKeeper_GLTF.i?.Cleanup();
        AssetPromiseKeeper_AB_GameObject.i?.Cleanup();
        AssetPromiseKeeper_AB.i?.Cleanup();

        MemoryManager.i?.CleanupPoolsIfNeeded(true);
        PoolManager.i?.Cleanup();
        PointerEventsController.i?.Cleanup();
        MessagingControllersManager.i?.Stop();

        Caching.ClearCache();
        Resources.UnloadUnusedAssets();

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
        yield return InitUnityScene("MainTest");

        if (debugMode)
            SceneController.i.SetDebug();

        sceneController = TestHelpers.InitializeSceneController(usesWebServer);

        yield return new WaitForSeconds(0.01f);

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

            yield return null;
            Assert.IsTrue(DCLCharacterController.i != null);
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
