using DCL;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DCL.Camera;
using DCL.Controllers;
using DCL.Helpers.NFT.Markets;
using DCL.Rendering;
using DCL.SettingsCommon;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;

public class IntegrationTestSuite_Legacy
{
    /// <summary>
    /// Use this as a parent for your dynamically created gameobjects in tests
    /// so they are cleaned up automatically in the teardown
    /// </summary>
    private GameObject runtimeGameObjectsRoot;

    private List<GameObject> legacySystems = new List<GameObject>();

    [UnitySetUp]
    protected virtual IEnumerator SetUp()
    {
        DCL.Configuration.EnvironmentSettings.RUNNING_TESTS = true;
        DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = false;
        AssetPromiseKeeper_GLTF.i.throttlingCounter.enabled = false;
        PoolManager.enablePrewarm = false;

        // TODO(Brian): Move these variants to a DataStore object to avoid having to reset them
        //              like this.
        CommonScriptableObjects.isFullscreenHUDOpen.Set(false);
        CommonScriptableObjects.rendererState.Set(true);

        Settings.CreateSharedInstance(new DefaultSettingsFactory());

        legacySystems = SetUp_LegacySystems();

        RenderProfileManifest.i.Initialize();

        Environment.Setup(InitializeServiceLocator());

        yield return SetUp_Camera();
    }

    protected virtual ServiceLocator InitializeServiceLocator()
    {
        var result = ServiceLocatorFactory.CreateDefault();
        result.Register<IMemoryManager>(() => Substitute.For<IMemoryManager>());
        result.Register<IParcelScenesCleaner>(() => Substitute.For<IParcelScenesCleaner>());
        result.Register<ICullingController>(() => Substitute.For<ICullingController>());

        result.Register<IServiceProviders>(
            () =>
            {
                var mockedProviders = Substitute.For<IServiceProviders>();
                mockedProviders.theGraph.Returns( Substitute.For<ITheGraph>() );
                mockedProviders.analytics.Returns( Substitute.For<IAnalytics>() );
                mockedProviders.catalyst.Returns( Substitute.For<ICatalyst>() );
                mockedProviders.openSea.Returns( Substitute.For<INFTMarket>() );
                return mockedProviders;
            });

        return result;
    }

    protected virtual List<GameObject> SetUp_LegacySystems()
    {
        List<GameObject> result = new List<GameObject>();
        result.AddRange(MainSceneFactory.CreatePlayerSystems());
        return result;
    }

    protected IEnumerator TearDown_LegacySystems()
    {
        Settings.i.Dispose();

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

        GameObject[] gos = Object.FindObjectsOfType<GameObject>(true);

        foreach ( var go in gos )
        {
            Object.Destroy(go);
        }

        yield return null;
    }

    protected void TearDown_Memory()
    {
        AssetPromiseKeeper_GLTF.i?.Cleanup();
        AssetPromiseKeeper_AB_GameObject.i?.Cleanup();
        AssetPromiseKeeper_AB.i?.Cleanup();

        if (PoolManager.i != null)
            PoolManager.i.Dispose();
    }

    public virtual IEnumerator SetUp_Camera()
    {
        CameraController cameraController = Object.FindObjectOfType<CameraController>();

        if ( cameraController == null )
            yield break;

        var tpsMode = cameraController.GetCameraMode(CameraMode.ModeId.ThirdPerson) as CameraStateTPS;

        if ( tpsMode != null )
        {
            tpsMode.cameraDampOnGroundType.settings.enabled = false;
            tpsMode.cameraFreefall.settings.enabled = false;
            tpsMode.cameraDampOnSprint.settings.enabled = false;
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