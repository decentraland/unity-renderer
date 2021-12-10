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
    /// <summary>
    /// Use this as a parent for your dynamically created gameobjects in tests
    /// so they are cleaned up automatically in the teardown
    /// </summary>
    private GameObject runtimeGameObjectsRoot;

    protected virtual bool enableSceneIntegrityChecker => true;

    protected TestSceneIntegrityChecker testSceneIntegrityChecker;

    private List<GameObject> legacySystems = new List<GameObject>();

    [UnitySetUp]
    protected virtual IEnumerator SetUp()
    {
        DCL.Configuration.EnvironmentSettings.RUNNING_TESTS = true;

        testSceneIntegrityChecker = new TestSceneIntegrityChecker();

        //NOTE(Brian): integrity checker is disabled in batch mode to make it run faster in CI
        if (Application.isBatchMode || !enableSceneIntegrityChecker)
        {
            testSceneIntegrityChecker.enabled = false;
        }

        legacySystems = SetUp_LegacySystems();

        RenderProfileManifest.i.Initialize();

        Environment.SetupWithBuilders
        (
            MessagingContextFactory.CreateDefault,
            PlatformContextFactory.CreateDefault,
            WorldRuntimeContextFactory.CreateDefault,
            HUDContextFactory.CreateDefault
        );

        SetUp_SceneController();

        yield return testSceneIntegrityChecker.SaveSceneSnapshot();
        yield return null;
        //TODO(Brian): Remove when the init layer is ready
        Environment.i.platform.cullingController.Stop();
    }

    protected virtual List<GameObject> SetUp_LegacySystems()
    {
        List<GameObject> result = new List<GameObject>();
        result.Add(MainSceneFactory.CreateBridges());
        result.Add(MainSceneFactory.CreateEnvironment());
        result.AddRange(MainSceneFactory.CreatePlayerSystems());
        result.Add(MainSceneFactory.CreateNavMap());
        result.Add(MainSceneFactory.CreateAudioHandler());
        result.Add(MainSceneFactory.CreateHudController());
        result.Add(MainSceneFactory.CreateMouseCatcher());
        result.Add(MainSceneFactory.CreateSettingsController());
        result.Add(MainSceneFactory.CreateEventSystem());
        result.Add(MainSceneFactory.CreateInteractionHoverCanvas());
        return result;
    }

    protected IEnumerator TearDown_LegacySystems()
    {
        foreach ( var go in legacySystems )
        {
            UnityEngine.Object.Destroy(go);
        }

        yield return null;
    }


    [UnityTearDown]
    protected virtual IEnumerator TearDown()
    {
        yield return null;

        if (runtimeGameObjectsRoot != null)
            Object.Destroy(runtimeGameObjectsRoot.gameObject);

        yield return TearDown_LegacySystems();
        Environment.Dispose();
        DataStore.Clear();
        TearDown_Memory();

        if (MapRenderer.i != null)
            MapRenderer.i.Cleanup();

        CatalogController.Clear();

        yield return testSceneIntegrityChecker?.TestSceneSnapshot();
    }

    protected void TearDown_Memory()
    {
        AssetPromiseKeeper_GLTF.i?.Cleanup();
        AssetPromiseKeeper_AB_GameObject.i?.Cleanup();
        AssetPromiseKeeper_AB.i?.Cleanup();

        if (PoolManager.i != null)
            PoolManager.i.Dispose();

        Caching.ClearCache();
        Resources.UnloadUnusedAssets();
    }

    public void SetUp_SceneController()
    {
        PoolManager.enablePrewarm = false;
        DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = false;
        var sceneController = Environment.i.world.sceneController;
        sceneController.deferredMessagesDecoding = false;
        sceneController.prewarmSceneMessagesPool = false;
        sceneController.prewarmEntitiesPool = false;
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