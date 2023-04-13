using Cysharp.Threading.Tasks;
using DCL;
using DCL.Camera;
using DCL.CameraTool;
using DCL.Configuration;
using DCL.Emotes;
using DCL.Helpers.NFT.Markets;
using DCL.ProfanityFiltering;
using DCL.Providers;
using DCL.Rendering;
using DCL.SettingsCommon;
using DCLServices.MapRendererV2;
using DCLServices.WearablesCatalogService;
using NSubstitute;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using MapRenderer = DCL.MapRenderer;

public class IntegrationTestSuite_Legacy
{
    private const string VISUAL_TEST_CUBEMAP_PATH = "Assets/Scripts/Tests/VisualTests/Textures/VisualTest Reflection.png";

    /// <summary>
    /// Use this as a parent for your dynamically created gameobjects in tests
    /// so they are cleaned up automatically in the teardown
    /// </summary>
    private GameObject runtimeGameObjectsRoot;

    private List<GameObject> legacySystems = new List<GameObject>();
    private AddressableResourceProvider addressableResourceProvider;

    [UnitySetUp]
    protected virtual IEnumerator SetUp()
    {
        EnvironmentSettings.RUNNING_TESTS = true;
        AssetPromiseKeeper_GLTF.i.throttlingCounter.enabled = false;
        PoolManager.enablePrewarm = false;

        // TODO(Brian): Move these variants to a DataStore object to avoid having to reset them
        //              like this.
        CommonScriptableObjects.allUIHidden.Set(false);
        CommonScriptableObjects.isFullscreenHUDOpen.Set(false);
        CommonScriptableObjects.rendererState.Set(true);

        Settings.CreateSharedInstance(new DefaultSettingsFactory());

        InitializeDefaultRenderSettings();

        Environment.Setup(InitializeServiceLocator());

        legacySystems = SetUp_LegacySystems();

        yield return SetUp_Camera();
    }

    protected virtual ServiceLocator InitializeServiceLocator()
    {
        var result = ServiceLocatorFactory.CreateDefault();
        result.Register<IMemoryManager>(() => Substitute.For<IMemoryManager>());
        result.Register<IParcelScenesCleaner>(() => Substitute.For<IParcelScenesCleaner>());
        result.Register<ICullingController>(() => Substitute.For<ICullingController>());
        result.Register<IProfanityFilter>(() =>
        {
            IProfanityFilter profanityFilter = Substitute.For<IProfanityFilter>();
            profanityFilter.Filter(Arg.Any<string>()).Returns(info => UniTask.FromResult(info[0].ToString()));
            return profanityFilter;
        });

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

        IEmotesCatalogService emotesCatalogService = Substitute.For<IEmotesCatalogService>();
        emotesCatalogService.GetEmbeddedEmotes().Returns(GetEmbeddedEmotesSO());
        result.Register<IEmotesCatalogService>(() => emotesCatalogService);

        IWearablesCatalogService wearablesCatalogService = Substitute.For<IWearablesCatalogService>();
        wearablesCatalogService.WearablesCatalog.Returns(new BaseDictionary<string, WearableItem>());
        result.Register<IWearablesCatalogService>(() => wearablesCatalogService);

        result.Register<IMapRenderer>(() => Substitute.For<IMapRenderer>());
        return result;
    }

    private async UniTask<EmbeddedEmotesSO> GetEmbeddedEmotesSO()
    {
        EmbeddedEmotesSO embeddedEmotes = ScriptableObject.CreateInstance<EmbeddedEmotesSO>();
        embeddedEmotes.emotes = new EmbeddedEmote[] { };
        return embeddedEmotes;
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
            Object.Destroy(go);
        }

        yield return null;
    }


    [UnityTearDown]
    protected virtual IEnumerator TearDown()
    {
        yield return null;

        // Set rendererState as false so ParcelScene.CleanUp is called with immediate as true
        CommonScriptableObjects.rendererState.Set(false);

        if (runtimeGameObjectsRoot != null)
            Object.Destroy(runtimeGameObjectsRoot.gameObject);

        yield return TearDown_LegacySystems();
        DataStore.Clear();
        ThumbnailsManager.Clear();
        TearDown_Memory();

        if (MapRenderer.i != null)
            MapRenderer.i.Cleanup();

        Environment.Dispose();

        yield return null;

        NotificationScriptableObjects.UnloadAll();
        AudioScriptableObjects.UnloadAll();
        CommonScriptableObjects.UnloadAll();

        yield return null;

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
        AssetPromiseKeeper_Texture.i?.Cleanup();
        AssetPromiseKeeper_AudioClip.i?.Cleanup();
        AssetPromiseKeeper_Gif.i?.Cleanup();

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

    private void InitializeDefaultRenderSettings()
    {
        RenderSettings.customReflection = AssetDatabase.LoadAssetAtPath<Cubemap>(VISUAL_TEST_CUBEMAP_PATH);
        RenderSettings.ambientMode = AmbientMode.Trilight;

        RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/Scripts/Tests/VisualTests/VisualTest Skybox.mat");
        RenderSettings.ambientEquatorColor = new Color(0.98039216f, 0.8352941f, 0.74509805f);
        RenderSettings.ambientSkyColor = new Color(0.60784316f, 0.92941177f, 1);
        RenderSettings.ambientGroundColor = Color.white;

        RenderSettings.fogColor = new Color(0.8443396f, 0.93445873f, 1);

        if (RenderSettings.sun != null)
        {
            RenderSettings.sun.color =  new Color(0.85882354f, 0.90795577f, 0.9137255f);
            RenderSettings.sun.transform.rotation = Quaternion.Euler(Vector3.one * 45);
        }
    }
}
