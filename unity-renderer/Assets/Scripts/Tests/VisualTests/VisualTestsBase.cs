using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCLPlugins.UIRefresherPlugin;
using DCLServices.MapRendererV2;
using NSubstitute;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

public class VisualTestsBase : IntegrationTestSuite_Legacy
{
    protected ParcelScene scene;
    protected Camera camera;
    private AnisotropicFiltering originalAnisoSetting;
    private CoreComponentsPlugin coreComponentsPlugin;
    private UIComponentsPlugin uiComponentsPlugin;
    private UIRefresherPlugin uiRefresherPlugin;

    protected override ServiceLocator InitializeServiceLocator()
    {
        ServiceLocator result = DCL.ServiceLocatorTestFactory.CreateMocked();
        result.Register<IWebRequestController>(WebRequestController.Create);
        result.Register<IServiceProviders>( () => new ServiceProviders());
        result.Register<IRuntimeComponentFactory>( () => new RuntimeComponentFactory());
        result.Register<IWorldState>( () => new WorldState());
        result.Register<IUpdateEventHandler>(() => new UpdateEventHandler());
        result.Register<IMapRenderer>(() => Substitute.For<IMapRenderer>());
        return result;
    }

    protected override List<GameObject> SetUp_LegacySystems()
    {
        List<GameObject> result = new List<GameObject>();
        result.Add(MainSceneFactory.CreateEnvironment());
        return result;
    }

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        originalAnisoSetting = QualitySettings.anisotropicFiltering;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;

        VisualTestUtils.SetSSAOActive(false);
        scene = TestUtils.CreateTestScene();
        coreComponentsPlugin = new CoreComponentsPlugin();
        uiComponentsPlugin = new UIComponentsPlugin();
        uiRefresherPlugin = new UIRefresherPlugin();

        VisualTestUtils.snapshotIndex = 0;

        VisualTestUtils.SetTestingRenderSettings();

        // Position character inside parcel (0,0)
        camera = TestUtils.CreateComponentWithGameObject<Camera>("CameraContainer");
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.allowHDR = true;
        camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        camera.GetUniversalAdditionalCameraData().volumeLayerMask = LayerMask.GetMask("PostProcessing");
        camera.GetUniversalAdditionalCameraData().renderShadows = true;

        UnityEngine.RenderSettings.fog = true;
        UnityEngine.RenderSettings.fogMode = FogMode.Linear;
        UnityEngine.RenderSettings.fogStartDistance = 100;
        UnityEngine.RenderSettings.fogEndDistance = 110;

        DCL.Environment.i.world.state.ForceCurrentScene( scene.sceneData.sceneNumber, scene.sceneData.id);

        VisualTestUtils.RepositionVisualTestsCamera(camera, new Vector3(0, 2, 0));
    }

    protected override IEnumerator TearDown()
    {
        coreComponentsPlugin.Dispose();
        uiComponentsPlugin.Dispose();
        uiRefresherPlugin.Dispose();
        Object.Destroy(camera.gameObject);
        QualitySettings.anisotropicFiltering = originalAnisoSetting;
        yield return base.TearDown();
    }
}
