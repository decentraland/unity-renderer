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

        private IEnumerator CreateMesh(ECS7TestEntity entity, PBMaterial materialModel, bool sphere = true)
        {
            // Create mesh
            var meshRendererHandler = new MeshRendererHandler(new DataStore_ECS7(),
                internalEcsComponents.texturizableComponent,
                internalEcsComponents.renderersComponent);

            PBMeshRenderer meshModel = new PBMeshRenderer() { Sphere = new PBMeshRenderer.Types.SphereMesh() };
            if (!sphere)
                meshModel = new PBMeshRenderer() { Plane = new PBMeshRenderer.Types.PlaneMesh() };

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
                        A = 0.5f
                    }
                }
            });

            // Emissive values working
            var entity2 = scene.CreateEntity(6662);
            entity2.gameObject.transform.position = new Vector3(-1, 11, 0);
            entity2.gameObject.transform.localScale *= 2;
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
            }, false);

            // alpha texture working
            var entity3 = scene.CreateEntity(6663);
            entity3.gameObject.transform.position = new Vector3(1, 11, 0);
            entity3.gameObject.transform.Rotate(Vector3.up, -90);
            yield return CreateMesh(entity3, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    AlbedoColor = new Color4() { R = 0, G = 0, B = 1, A = 1 },
                    Texture = new TextureUnion()
                    {
                        Texture = new Texture() { Src = TestAssetsUtils.GetPath() + "/Images/alphaTexture.png" }
                    },
                    TransparencyMode = MaterialTransparencyMode.MtmAlphaBlend
                }
            });

            // Same albedo-alpha texture
            var entity4 = scene.CreateEntity(6664);
            entity4.gameObject.transform.position = new Vector3(3, 11, 0);
            entity4.gameObject.transform.localScale *= 2;
            yield return CreateMesh(entity4, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    Texture = new TextureUnion()
                    {
                        Texture = new Texture() { Src = TestAssetsUtils.GetPath() + "/Images/avatar.png" }
                    },
                    AlphaTexture = new TextureUnion()
                    {
                        Texture = new Texture() { Src = TestAssetsUtils.GetPath() + "/Images/avatar.png" }
                    },
                    TransparencyMode = MaterialTransparencyMode.MtmAlphaBlend
                }
            }, false);

            /*var entity5 = scene.CreateEntity(6665);
            entity5.gameObject.transform.position = new Vector3(-3, 8, 0);
            yield return CreateMesh(entity5, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                }
            });

            var entity6 = scene.CreateEntity(6666);
            entity6.gameObject.transform.position = new Vector3(-1, 8, 0);
            yield return CreateMesh(entity6, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                }
            });

            var entity7 = scene.CreateEntity(6667);
            entity7.gameObject.transform.position = new Vector3(1, 8, 0);
            yield return CreateMesh(entity7, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                }
            });

            var entity8 = scene.CreateEntity(6668);
            entity8.gameObject.transform.position = new Vector3(3, 8, 0);
            yield return CreateMesh(entity8, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                }
            });*/

            yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest1", camera);
        }

        // Manually run to generate baseline image for later comparisons
        [UnityTest, VisualTest, Explicit]
        public IEnumerator VisualTest2_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(VisualTest2()); }

        [UnityTest, VisualTest]
        public IEnumerator VisualTest2()
        {
            Vector3 cameraPos = (Vector3.up * 10) + (Vector3.back * 4);
            VisualTestUtils.RepositionVisualTestsCamera( camera, cameraPos, cameraPos + Vector3.forward);

            var entity1 = scene.CreateEntity(6661);
            entity1.gameObject.transform.position = new Vector3(0.2f, 10, 0.4f);
            yield return CreateMesh(entity1, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    TransparencyMode = MaterialTransparencyMode.MtmOpaque,
                    AlbedoColor = new Color4() { R = 1, G = 0, B = 0, A = 1 }
                }
            }, false);

            var entity2 = scene.CreateEntity(6662);
            entity2.gameObject.transform.position = new Vector3(-0.35f, 10, 0.2f);
            yield return CreateMesh(entity2, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    TransparencyMode = MaterialTransparencyMode.MtmOpaque,
                    AlbedoColor = new Color4() { R = 0, G = 0, B = 1, A = 1 }
                }
            }, false);

            var entity3 = scene.CreateEntity(6663);
            entity3.gameObject.transform.position = new Vector3(0, 9.75f, 0);
            yield return CreateMesh(entity3, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    Texture = new TextureUnion()
                    {
                        Texture = new Texture() { Src = TestAssetsUtils.GetPath() + "/Images/avatar.png" }
                    },
                    AlphaTest =  1f,
                    TransparencyMode = MaterialTransparencyMode.MtmAlphaTest,
                    AlbedoColor = new Color4() { R = 1, G = 1, B = 1, A = 1}
                }
            }, false);

            yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest2", camera);
        }
    }
}
