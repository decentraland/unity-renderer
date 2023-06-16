using DCL;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.Helpers;
using Decentraland.Common;
using ECSSystems.MaterialSystem;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Vector3 = UnityEngine.Vector3;
using Texture = Decentraland.Common.Texture;

namespace Tests
{
    public class MaterialVisualTests : ECSVisualTestsBase
    {
        private const string SNAPSHOT_BASE_FILENAME = "SDK7_MaterialVisualTests_";
        private Action systemsUpdate;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

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
        public override void TearDown()
        {
            base.TearDown();
        }

        private IEnumerator CreateMesh(ECS7TestEntity entity, PBMaterial materialModel)
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
            yield return CreateMesh(entity1, new PBMaterial()
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
            // entity2.gameObject.transform.localScale *= 2;
            entity2.gameObject.transform.Rotate(Vector3.up, -90);
            yield return CreateMesh(entity2, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    AlbedoColor = new Color4() { R = 1, G = 1, B = 1, A = 1 },
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

            yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest1", camera);
        }
    }
}
