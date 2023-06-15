using DCL;
using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using Decentraland.Common;
using ECSSystems.MaterialSystem;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using Environment = DCL.Environment;
using Object = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;
using Texture = Decentraland.Common.Texture;

namespace Tests
{
    public class MaterialVisualTests
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private InternalECSComponents internalEcsComponents;
        private Action systemsUpdate;

        private Camera camera;
        private AnisotropicFiltering originalAnisoSetting;
        private const string SNAPSHOT_BASE_FILENAME = "SDK7_MaterialVisualTests_";

        [SetUp]
        public void SetUp()
        {
            VisualTestUtils.SetSSAOActive(false);
            VisualTestUtils.snapshotIndex = 0;
            VisualTestUtils.SetTestingRenderSettings();

            // Deterministic rendering settings
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
            ServiceLocator serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
            Environment.Setup(serviceLocator);

            ECSComponentsFactory componentFactory = new ECSComponentsFactory();
            ECSComponentsManager componentsManager = new ECSComponentsManager(componentFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalEcsComponents = new InternalECSComponents(componentsManager, componentFactory, executors);

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);
            scene = testUtils.CreateScene(666);

            var materialSystemUpdate = ECSMaterialSystem.CreateSystem(componentsManager.CreateComponentGroup<InternalMaterial, InternalTexturizable>
                    ((int)InternalECSComponentsId.MATERIAL, (int)InternalECSComponentsId.TEXTURIZABLE),
                internalEcsComponents.texturizableComponent,
                internalEcsComponents.materialComponent);

            systemsUpdate = () =>
            {
                internalEcsComponents.MarkDirtyComponentsUpdate();
                materialSystemUpdate();
                internalEcsComponents.ResetDirtyComponentsUpdate();
            };
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
            PoolManager.i.Dispose();

            Object.Destroy(camera.gameObject);
            QualitySettings.anisotropicFiltering = originalAnisoSetting;
        }

        private IEnumerator CreateSphereMesh(ECS7TestEntity entity, PBMaterial materialModel)
        {
            // Create mesh
            var meshRendererHandler = new MeshRendererHandler(new DataStore_ECS7(),
                internalEcsComponents.texturizableComponent,
                internalEcsComponents.renderersComponent);
            // PBMeshRenderer meshModel = new PBMeshRenderer() { Plane = new PBMeshRenderer.Types.PlaneMesh() };
            PBMeshRenderer meshModel = new PBMeshRenderer() { Sphere = new PBMeshRenderer.Types.SphereMesh() };
            meshRendererHandler.OnComponentCreated(scene, entity);
            meshRendererHandler.OnComponentModelUpdated(scene, entity, meshModel);
            yield return null;

            // Create material
            var materialHandler = new MaterialHandler(internalEcsComponents.materialComponent,
                internalEcsComponents.videoMaterialComponent);
            materialHandler.OnComponentCreated(scene, entity);
            materialHandler.OnComponentModelUpdated(scene, entity, materialModel);
            yield return materialHandler.promiseMaterial;

            systemsUpdate.Invoke();
            yield return null;
            yield return null;
            yield return null;
        }

        // Manually run to generate baseline image for later comparisons
        [UnityTest, VisualTest, Explicit]
        public IEnumerator VisualTest1_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(VisualTest1()); }

        [UnityTest, VisualTest]
        public IEnumerator VisualTest1()
        {
            Vector3 cameraPos = (Vector3.up * 10) + (Vector3.back * 4);
            VisualTestUtils.RepositionVisualTestsCamera( camera, cameraPos, cameraPos + Vector3.forward);

            var entity1 = scene.CreateEntity(6661);
            entity1.gameObject.transform.position = new Vector3(-3, 11, 0);
            yield return CreateSphereMesh(entity1, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    AlbedoColor = new Color4()
                    {
                        R = Color.green.r,
                        G = Color.green.g,
                        B = Color.green.b,
                        A = 0.75f
                    }
                }
            });

            var entity2 = scene.CreateEntity(6662);
            entity2.gameObject.transform.position = new Vector3(-1, 11, 0);
            entity2.gameObject.transform.Rotate(Vector3.up, -90);
            yield return CreateSphereMesh(entity2, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    Texture = new TextureUnion()
                    {
                        Texture = new Texture() { Src = TestAssetsUtils.GetPath() + "/Images/avatar.png" }
                    },
                    AlphaTexture = new TextureUnion()
                    {
                        Texture = new Texture() { Src = TestAssetsUtils.GetPath() + "/Images/alphaTexture.png" }
                    },
                    EmissiveTexture = new TextureUnion()
                    {
                        Texture = new Texture() { Src = TestAssetsUtils.GetPath() + "/Images/Gradient A4.png" }
                    },
                    TransparencyMode = MaterialTransparencyMode.MtmAlphaBlend,
                    EmissiveColor =  new Color3()
                    {
                        R = Color.blue.r,
                        G = Color.blue.g,
                        B = Color.blue.b
                    },
                    EmissiveIntensity = 100
                }
            });

            Debug.Break();
            yield return null;

            // yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest1", camera);
        }
    }
}
