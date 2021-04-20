using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;
using DCL.Tutorial;
using NSubstitute;
using UnityEditor;

public class IntegrationTestSuite_Legacy
{
    protected virtual string TEST_SCENE_NAME => "MainTest";


    protected bool sceneInitialized = false;
    protected ISceneController sceneController;
    protected ParcelScene scene;
    protected CameraController cameraController;

    /// <summary>
    /// Use this as a parent for your dynamically created gameobjects in tests
    /// so they are cleaned up automatically in the teardown
    /// </summary>
    private GameObject runtimeGameObjectsRoot;

    protected virtual bool justSceneSetUp => false;
    protected virtual bool enableSceneIntegrityChecker => true;

    protected TestSceneIntegrityChecker testSceneIntegrityChecker;

    [UnitySetUp]
    protected virtual IEnumerator SetUp()
    {
        DCL.Configuration.EnvironmentSettings.RUNNING_TESTS = true;

        if (!sceneInitialized)
        {
            yield return InitUnityScene(TEST_SCENE_NAME);
            sceneInitialized = true;
        }

        runtimeGameObjectsRoot = new GameObject("_RuntimeGameObjectsRoot");

        testSceneIntegrityChecker = new TestSceneIntegrityChecker();

        //NOTE(Brian): integrity checker is disabled in batch mode to make it run faster in CI
        if (Application.isBatchMode || !enableSceneIntegrityChecker)
        {
            testSceneIntegrityChecker.enabled = false;
        }

        if (justSceneSetUp)
        {
            RenderProfileManifest.i.Initialize();
            Environment.SetupWithBuilders();
            SetUp_SceneController();

            SetUp_TestScene();
            SetUp_Renderer();
            yield return testSceneIntegrityChecker.SaveSceneSnapshot();
            yield return null;
            //TODO(Brian): Remove when the init layer is ready
            Environment.i.platform.cullingController.Stop();
            yield break;
        }

        RenderProfileManifest.i.Initialize();
        Environment.SetupWithBuilders();

        SetUp_SceneController();
        SetUp_TestScene();

        SetUp_Camera();
        yield return SetUp_CharacterController();
        SetUp_Renderer();
        yield return testSceneIntegrityChecker.SaveSceneSnapshot();
        yield return null;
        //TODO(Brian): Remove when the init layer is ready
        Environment.i.platform.cullingController.Stop();
    }


    [UnityTearDown]
    protected virtual IEnumerator TearDown()
    {
        yield return null;

        if (runtimeGameObjectsRoot != null)
            Object.Destroy(runtimeGameObjectsRoot.gameObject);

        if (DCLCharacterController.i != null)
        {
            DCLCharacterController.i.ResumeGravity();
            DCLCharacterController.i.enabled = true;

            if (DCLCharacterController.i.characterController != null)
                DCLCharacterController.i.characterController.enabled = true;
        }

        Environment.Dispose();

        yield return TearDown_Memory();

        if (MapRenderer.i != null)
            MapRenderer.i.Cleanup();

        yield return testSceneIntegrityChecker?.TestSceneSnapshot();
    }

    protected void TearDown_PromiseKeepers()
    {
        AssetPromiseKeeper_GLTF.i?.Cleanup();
        AssetPromiseKeeper_AB_GameObject.i?.Cleanup();
        AssetPromiseKeeper_AB.i?.Cleanup();
    }

    protected IEnumerator TearDown_Memory()
    {
        TearDown_PromiseKeepers();

        if (Environment.i.platform.memoryManager != null)
            yield return Environment.i.platform.memoryManager.CleanupPoolsIfNeeded(true);

        if (PoolManager.i != null)
            PoolManager.i.Cleanup();

        Caching.ClearCache();
        Resources.UnloadUnusedAssets();
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

    public void SetUp_TestScene()
    {
        scene = sceneController.CreateTestScene() as ParcelScene;
    }

    public virtual IEnumerator SetUp_CharacterController()
    {
        if (DCLCharacterController.i == null)
        {
            GameObject.Instantiate(Resources.Load("Prefabs/CharacterController"));
        }

        yield return null;
        Assert.IsTrue(DCLCharacterController.i != null);
        DCLCharacterController.i.gameObject.SetActive(true);
        DCLCharacterController.i.characterController.enabled = true;
    }

    public virtual void SetUp_Camera()
    {
        cameraController = GameObject.FindObjectOfType<CameraController>();

        if (cameraController == null)
            cameraController = GameObject.Instantiate(Resources.Load<GameObject>("CameraController")).GetComponent<CameraController>();
    }

    public void SetUp_SceneController()
    {
        PoolManager.enablePrewarm = false;
        DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = false;
        sceneController = Environment.i.world.sceneController;
        sceneController.deferredMessagesDecoding = false;
        sceneController.prewarmSceneMessagesPool = false;
        sceneController.prewarmEntitiesPool = false;
    }

    private void SetUp_GlobalScene()
    {
        string globalSceneId = "global-scene";

        sceneController.CreateGlobalScene(
            JsonConvert.SerializeObject(
                new CreateGlobalSceneMessage
                {
                    id = globalSceneId,
                    baseUrl = "",
                })
        );
    }

    public virtual void SetUp_Renderer()
    {
        CommonScriptableObjects.rendererState.Set(true);
    }

    protected virtual IEnumerator InitScene(bool spawnCharController = true, bool spawnTestScene = true, bool spawnUIScene = true, bool debugMode = false, bool reloadUnityScene = true)
    {
        yield return InitUnityScene(TEST_SCENE_NAME);

        if (debugMode)
            Environment.i.platform.debugController.SetDebug();

        SetUp_SceneController();

        if (spawnTestScene)
            SetUp_TestScene();

        if (spawnCharController)
            yield return SetUp_CharacterController();

        var newPos = new Vector3(10, 0, 10);
        DCLCharacterController.i.SetPosition(newPos);
        yield return null;

        if (spawnUIScene)
        {
            SetUp_GlobalScene();
        }
    }

    protected IEnumerator WaitForUICanvasUpdate()
    {
        yield break;
    }

    public static T Reflection_GetStaticField<T>(System.Type baseType, string fieldName)
    {
        return (T) baseType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
    }

    public static T Reflection_GetField<T>(object instance, string fieldName)
    {
        return (T) instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
    }

    public static void Reflection_SetField<T>(object instance, string fieldName, T newValue)
    {
        instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, newValue);
    }


    protected GameObject CreateTestGameObject(string name)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(runtimeGameObjectsRoot.transform);
        return gameObject;
    }

    protected GameObject InstantiateTestGameObject(GameObject reference)
    {
        GameObject gameObject = Object.Instantiate(reference, runtimeGameObjectsRoot.transform, true);
        return gameObject;
    }
}