using DCL;
using DCL.CRDT;
using DCL.ECSRuntime;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace Tests
{
    public class ECSVisualTestsBase
    {
		protected ECS7TestUtilsScenesAndEntities testUtils;
        protected ECS7TestScene scene;
        protected Camera camera;
        protected AnisotropicFiltering originalAnisoSetting;
        protected InternalECSComponents internalEcsComponents;
        protected ECSComponentsManager componentsManager;

		[SetUp]
        public virtual void SetUp()
        {
            VisualTestUtils.snapshotIndex = 0;

            // Deterministic rendering settings
            VisualTestUtils.SetSSAOActive(false);
            VisualTestUtils.SetTestingRenderSettings();
            originalAnisoSetting = QualitySettings.anisotropicFiltering;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            camera = new GameObject("VisualTestsCamera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.allowHDR = true;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            camera.GetUniversalAdditionalCameraData().volumeLayerMask = LayerMask.GetMask("PostProcessing");
            camera.GetUniversalAdditionalCameraData().renderShadows = true;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 100;
            RenderSettings.fogEndDistance = 110;

            // Scene settings
            MainSceneFactory.CreateEnvironment();

            ECSComponentsFactory componentFactory = new ECSComponentsFactory();
            componentsManager = new ECSComponentsManager(componentFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalEcsComponents = new InternalECSComponents(componentsManager, componentFactory, executors);

            // To be able to use local test resources as scene assets
            ServiceLocator serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
            Environment.Setup(serviceLocator);

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);
            scene = testUtils.CreateScene(666);
        }

        [TearDown]
        public virtual void TearDown()
        {
            testUtils.Dispose();
            PoolManager.i.Dispose();
            Object.Destroy(camera.gameObject);
            QualitySettings.anisotropicFiltering = originalAnisoSetting;
            Environment.Dispose();
        }
    }
}
