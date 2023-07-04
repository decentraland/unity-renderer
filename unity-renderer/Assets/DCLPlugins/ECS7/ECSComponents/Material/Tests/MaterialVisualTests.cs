using DCL;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.Helpers;
using Decentraland.Common;
using ECSSystems.MaterialSystem;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;
using Vector3 = UnityEngine.Vector3;
using Texture = Decentraland.Common.Texture;

namespace Tests
{
    public class MaterialVisualTests : ECSVisualTestsBase
    {
        private const string SNAPSHOT_BASE_FILENAME = "SDK7_MaterialVisualTests_";
        private Action systemsUpdate;
        private Dictionary<ECS7TestEntity, MeshRendererHandler> meshRendererHandlers = new Dictionary<ECS7TestEntity, MeshRendererHandler>();
        private Dictionary<ECS7TestEntity, MaterialHandler> materialHandlers = new Dictionary<ECS7TestEntity, MaterialHandler>();

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
            AssetPromiseKeeper_Material.i.Cleanup();
            AssetPromiseKeeper_Texture.i.Cleanup();
            foreach ((ECS7TestEntity entity, MeshRendererHandler handler) in meshRendererHandlers)
            {
                handler.OnComponentRemoved(scene, entity);
            }
            foreach ((ECS7TestEntity entity, MaterialHandler handler) in materialHandlers)
            {
                handler.OnComponentRemoved(scene, entity);
            }
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
            meshRendererHandlers[entity] = meshRendererHandler;
            yield return null;

            // Create material
            var materialHandler = new MaterialHandler(internalEcsComponents.materialComponent,
                internalEcsComponents.videoMaterialComponent);
            materialHandler.OnComponentCreated(scene, entity);
            materialHandler.OnComponentModelUpdated(scene, entity, materialModel);
            materialHandlers[entity] = materialHandler;
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

            var entity_test_alpha = scene.CreateEntity(6661);
            entity_test_alpha.gameObject.transform.position = new Vector3(-3, 11, 0);
            entity_test_alpha.gameObject.transform.localScale *= 0.5f;
            yield return CreateMesh(entity_test_alpha, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    AlbedoColor = new Color4() { R = 0, G = 0, B = 1, A = 0.5f },
                    Metallic = 0,
                    Roughness = 0
                }
            });

            // Emissive values working
            var entity_test_emissive = scene.CreateEntity(6662);
            entity_test_emissive.gameObject.transform.position = new Vector3(-2f, 11, 0);
            entity_test_emissive.gameObject.transform.localScale *= 0.5f;
            yield return CreateMesh(entity_test_emissive, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    AlbedoColor = new Color4() { R = 0, G = 0, B = 1, A = 1f },
                    EmissiveColor = new Color3() { R = 0, G = 0, B = 1 },
                    EmissiveIntensity = 100
                }
            });

            var entity_test_emissive2 = scene.CreateEntity(6663);
            entity_test_emissive2.gameObject.transform.position = new Vector3(-1, 11, 0);
            entity_test_emissive2.gameObject.transform.localScale *= 2;
            yield return CreateMesh(entity_test_emissive2, new PBMaterial()
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
                    EmissiveColor =  new Color3() { R = 0, G = 0, B = 1 },
                    EmissiveIntensity = 100
                }
            }, false);

            // alpha texture working
            var entity_test_alpha_tex = scene.CreateEntity(6664);
            entity_test_alpha_tex.gameObject.transform.position = new Vector3(0, 11, 0);
            entity_test_alpha_tex.gameObject.transform.Rotate(Vector3.up, -90);
            yield return CreateMesh(entity_test_alpha_tex, new PBMaterial()
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
            var entity_test_same_albedo_alpha = scene.CreateEntity(6665);
            entity_test_same_albedo_alpha.gameObject.transform.position = new Vector3(1.5f, 11, 0);
            entity_test_same_albedo_alpha.gameObject.transform.localScale *= 2;
            yield return CreateMesh(entity_test_same_albedo_alpha, new PBMaterial()
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

            // Metallic and Roughness
            var entity_test_metallic_roughness1 = scene.CreateEntity(6666);
            entity_test_metallic_roughness1.gameObject.transform.position = new Vector3(3, 11, 0);
            yield return CreateMesh(entity_test_metallic_roughness1, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    AlbedoColor = new Color4() { R = 0, G = 0, B = 1f, A = 1 },
                    Metallic = 0,
                    Roughness = 1
                }
            });

            var entity_test_metallic_roughness2 = scene.CreateEntity(6667);
            entity_test_metallic_roughness2.gameObject.transform.position = new Vector3(-2.5f, 9, 0);
            entity_test_metallic_roughness2.gameObject.transform.localScale *= 2;
            entity_test_metallic_roughness2.gameObject.transform.Rotate(Vector3.up, -22f);
            entity_test_metallic_roughness2.gameObject.transform.Rotate(Vector3.right, -30f);
            yield return CreateMesh(entity_test_metallic_roughness2, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    AlbedoColor = new Color4() { R = 0, G = 0, B = 1f, A = 1 },
                    Metallic = 0.5f,
                    Roughness = 0.25f,
                    BumpTexture = new TextureUnion()
                    {
                        Texture = new Texture() { Src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Textures/Lantern Bump Map.png" }
                    }
                }
            });

            var entity_test_metallic_roughness3 = scene.CreateEntity(6668);
            entity_test_metallic_roughness3.gameObject.transform.position = new Vector3(-0.5f, 9, 0);
            yield return CreateMesh(entity_test_metallic_roughness3, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    AlbedoColor = new Color4() { R = 0, G = 0, B = 1f, A = 1 },
                    Metallic = 0.5f,
                    Roughness = 0.5f
                }
            });

            var entity_test_metallic_roughness4 = scene.CreateEntity(6669);
            entity_test_metallic_roughness4.gameObject.transform.position = new Vector3(1, 9, 0);
            yield return CreateMesh(entity_test_metallic_roughness4, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    AlbedoColor = new Color4() { R = 0, G = 0, B = 1f, A = 1 },
                    Metallic = 1,
                    Roughness = 0
                }
            });

            var entity_test_metallic_roughness5 = scene.CreateEntity(6670);
            entity_test_metallic_roughness5.gameObject.transform.position = new Vector3(2.5f, 9, 0);
            yield return CreateMesh(entity_test_metallic_roughness5, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    AlbedoColor = new Color4() { R = 0, G = 0, B = 1f, A = 1 },
                    Metallic = 1,
                    Roughness = 1
                }
            });

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
