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
using DCL.Camera;
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
    protected DCL.Camera.CameraController cameraController;

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

        testSceneIntegrityChecker = new TestSceneIntegrityChecker();

        //NOTE(Brian): integrity checker is disabled in batch mode to make it run faster in CI
        if (Application.isBatchMode || !enableSceneIntegrityChecker)
        {
            testSceneIntegrityChecker.enabled = false;
        }

        if (justSceneSetUp)
        {
            RenderProfileManifest.i.Initialize();

            Environment.SetupWithBuilders
            (
                MessagingContextFactory.CreateDefault,
                PlatformContextFactory.CreateDefault,
                WorldRuntimeContextFactory.CreateDefault,
                HUDContextFactory.CreateDefault
            );

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

        Environment.SetupWithBuilders
        (
            MessagingContextFactory.CreateDefault,
            PlatformContextFactory.CreateDefault,
            WorldRuntimeContextFactory.CreateDefault,
            HUDContextFactory.CreateDefault
        );

        SetUp_SceneController();
        SetUp_TestScene();

        yield return SetUp_Camera();
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
        DataStore.Clear();

        TearDown_Memory();

        if (MapRenderer.i != null)
            MapRenderer.i.Cleanup();

        CatalogController.Clear();

        yield return testSceneIntegrityChecker?.TestSceneSnapshot();
    }

    protected void TearDown_PromiseKeepers()
    {
        AssetPromiseKeeper_GLTF.i?.Cleanup();
        AssetPromiseKeeper_AB_GameObject.i?.Cleanup();
        AssetPromiseKeeper_AB.i?.Cleanup();
    }

    protected void TearDown_Memory()
    {
        TearDown_PromiseKeepers();

        if (PoolManager.i != null)
            PoolManager.i.Dispose();

        Caching.ClearCache();
        Resources.UnloadUnusedAssets();
    }

    protected IEnumerator InitUnityScene(string sceneName = null)
    {
        yield return TestUtils.UnloadAllUnityScenes();

        Scene? newScene;

        if (string.IsNullOrEmpty(sceneName))
        {
            newScene = SceneManager.CreateScene(TestUtils.testingSceneName + (TestUtils.testSceneIteration++));
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

    public void SetUp_TestScene() { scene = sceneController.CreateTestScene() as ParcelScene; }

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

    public virtual IEnumerator SetUp_Camera()
    {
        cameraController = GameObject.FindObjectOfType<DCL.Camera.CameraController>();

        if (cameraController == null)
            cameraController = GameObject.Instantiate(Resources.Load<GameObject>("CameraController")).GetComponent<CameraController>();

        yield return null;

        var tpsMode = cameraController.GetCameraMode(CameraMode.ModeId.ThirdPerson) as CameraStateTPS;

        if ( tpsMode != null )
        {
            tpsMode.cameraDampOnGroundType.settings.enabled = false;
            tpsMode.cameraFreefall.settings.enabled = false;
            tpsMode.cameraDampOnSprint.settings.enabled = false;
        }
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

    public virtual void SetUp_Renderer() { CommonScriptableObjects.rendererState.Set(true); }

    protected virtual IEnumerator InitScene(bool spawnCharController = true, bool spawnTestScene = true, bool spawnUIScene = true, bool debugMode = false, bool reloadUnityScene = true)
    {
        yield return InitUnityScene(TEST_SCENE_NAME);

        DataStore.i.debugConfig.isDebugMode.Set(debugMode);

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

    public static T Reflection_GetField<T>(object instance, string fieldName) { return (T) instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance); }

    protected GameObject CreateTestGameObject(string name) { return CreateTestGameObject(name, Vector3.zero); }

    protected GameObject CreateTestGameObject(string name, Vector3 localPosition)
    {
        if (runtimeGameObjectsRoot == null)
            runtimeGameObjectsRoot = new GameObject("_RuntimeGameObjectsRoot");

        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(runtimeGameObjectsRoot.transform);
        gameObject.transform.localPosition = localPosition;
        return gameObject;
    }

    protected GameObject InstantiateTestGameObject(GameObject reference)
    {
        if (runtimeGameObjectsRoot == null)
            runtimeGameObjectsRoot = new GameObject("_RuntimeGameObjectsRoot");

        GameObject gameObject = Object.Instantiate(reference, runtimeGameObjectsRoot.transform, true);
        return gameObject;
    }
}