using System.Collections;
using DCL;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Texture = DCL.ECSComponents.Texture;
using TextureWrapMode = DCL.ECSComponents.TextureWrapMode;

namespace Tests
{
    public class MaterialHandlerShould
    {
        private MaterialHandler handler;
        private IInternalECSComponent<InternalMaterial> internalMaterialComponent;
        private ICatalyst catalyst;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entity;

        [SetUp]
        public void SetUp()
        {
            internalMaterialComponent = Substitute.For<IInternalECSComponent<InternalMaterial>>();
            handler = new MaterialHandler(internalMaterialComponent);
            testUtils = new ECS7TestUtilsScenesAndEntities();
            scene = testUtils.CreateScene("temptation");
            entity = scene.CreateEntity(1000);

            Environment.Setup(ServiceLocatorFactory.CreateDefault());
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
            AssetPromiseKeeper_Material.i.Cleanup();
            AssetPromiseKeeper_Texture.i.Cleanup();
            Environment.Dispose();
        }

        [UnityTest]
        public IEnumerator CreateMaterial()
        {
            PBMaterial model = new PBMaterial()
            {
                Texture = new TextureUnion()
                {
                    Texture = new Texture()
                    {
                        Src = TestAssetsUtils.GetPath() + "/Images/avatar.png"
                    }
                }
            };

            handler.OnComponentModelUpdated(scene, entity, model);
            AssetPromise_Material promiseMaterial = handler.materialPromises.Dequeue();
            yield return promiseMaterial;

            Assert.NotNull(promiseMaterial.asset.material);
        }

        [UnityTest]
        public IEnumerator ReuseMaterialWhenModelDidntChange()
        {
            PBMaterial model = new PBMaterial()
            {
                Texture = new TextureUnion()
                {
                    Texture = new Texture()
                    {
                        Src = TestAssetsUtils.GetPath() + "/Images/avatar.png"
                    }
                }
            };

            handler.OnComponentModelUpdated(scene, entity, model);
            AssetPromise_Material promiseMaterial = handler.materialPromises.Dequeue();
            yield return promiseMaterial;

            Material firstMaterial = promiseMaterial.asset.material;

            PBMaterial model2 = new PBMaterial(model);
            handler.OnComponentModelUpdated(scene, entity, model2);
            promiseMaterial = handler.materialPromises.Count > 0 ? handler.materialPromises.Dequeue() : promiseMaterial;
            yield return promiseMaterial;

            Assert.AreEqual(firstMaterial, promiseMaterial.asset.material);
        }

        [UnityTest]
        public IEnumerator ChangeMaterialWhenModelChange()
        {
            PBMaterial model = new PBMaterial()
            {
                Texture = new TextureUnion()
                {
                    Texture = new Texture()
                    {
                        Src = TestAssetsUtils.GetPath() + "/Images/avatar.png"
                    }
                }
            };
            yield return null;

            handler.OnComponentModelUpdated(scene, entity, model);
            AssetPromise_Material promiseMaterial = handler.materialPromises.Dequeue();
            yield return promiseMaterial;

            Material firstMaterial = promiseMaterial.asset.material;

            PBMaterial model2 = new PBMaterial(model);
            model2.Texture.Texture.WrapMode = TextureWrapMode.TwmMirror;
            handler.OnComponentModelUpdated(scene, entity, model2);
            promiseMaterial = handler.materialPromises.Dequeue();
            yield return promiseMaterial;

            Assert.AreNotEqual(firstMaterial, promiseMaterial.asset.material);
        }

        [UnityTest]
        public IEnumerator ChangeMaterialWhenNoTextureSource()
        {
            Debug.Log(TestAssetsUtils.GetPath() + "/Images/avatar.png");
            PBMaterial model = new PBMaterial()
            {
                Texture = new TextureUnion()
                {
                    Texture = new Texture()
                    {
                        Src = TestAssetsUtils.GetPath() + "/Images/avatar.png"
                    }
                }
            };
            yield return null;

            handler.OnComponentModelUpdated(scene, entity, model);
            AssetPromise_Material promiseMaterial = handler.materialPromises.Dequeue();
            yield return promiseMaterial;

            Material firstMaterial = promiseMaterial.asset.material;

            PBMaterial model2 = new PBMaterial(model)
            {
                Texture = new TextureUnion()
                {
                    Texture = new Texture()
                    {
                        WrapMode = TextureWrapMode.TwmMirror
                    }
                }
            };
            handler.OnComponentModelUpdated(scene, entity, model2);
            promiseMaterial = handler.materialPromises.Dequeue();
            yield return promiseMaterial;

            Assert.AreNotEqual(firstMaterial, promiseMaterial.asset.material);
        }

        [UnityTest]
        public IEnumerator ForgetMaterialOnRemoved()
        {
            PBMaterial model = new PBMaterial()
            {
                Texture = new TextureUnion()
                {
                    Texture = new Texture()
                    {
                        Src = TestAssetsUtils.GetPath() + "/Images/avatar.png"
                    }
                }
            };

            handler.OnComponentModelUpdated(scene, entity, model);
            AssetPromise_Material promiseMaterial = handler.materialPromises.Peek();
            yield return promiseMaterial;

            Assert.AreEqual(1, AssetPromiseKeeper_Material.i.library.masterAssets.Count);

            handler.OnComponentRemoved(scene, entity);
            Assert.AreEqual(0, AssetPromiseKeeper_Material.i.library.masterAssets.Count);
        }

        [UnityTest]
        public IEnumerator PutInternalMaterialOnUpdate()
        {
            PBMaterial model = new PBMaterial()
            {
                Texture = new TextureUnion()
                {
                    Texture = new Texture()
                    {
                        Src = TestAssetsUtils.GetPath() + "/Images/avatar.png"
                    }
                }
            };

            handler.OnComponentModelUpdated(scene, entity, model);
            AssetPromise_Material promiseMaterial = handler.materialPromises.Dequeue();
            yield return promiseMaterial;

            Material currentMaterial = promiseMaterial.asset.material;

            internalMaterialComponent.Received(1)
                                     .PutFor(scene, entity,
                                         Arg.Is<InternalMaterial>(x => x.material == currentMaterial && x.dirty));

            internalMaterialComponent.ClearReceivedCalls();

            PBMaterial model2 = new PBMaterial(model)
            {
                Texture = new TextureUnion()
                {
                    Texture = new Texture()
                    {
                        WrapMode = TextureWrapMode.TwmMirror
                    }
                }
            };
            handler.OnComponentModelUpdated(scene, entity, model2);
            promiseMaterial = handler.materialPromises.Dequeue();
            yield return promiseMaterial;

            currentMaterial = promiseMaterial.asset.material;

            internalMaterialComponent.Received(1)
                                     .PutFor(scene, entity,
                                         Arg.Is<InternalMaterial>(x => x.material == currentMaterial && x.dirty));
        }

        [UnityTest]
        public IEnumerator NotPutInternalMaterialIfModelNotUpdated()
        {
            PBMaterial model = new PBMaterial()
            {
                Texture = new TextureUnion()
                {
                    Texture = new Texture()
                    {
                        Src = TestAssetsUtils.GetPath() + "/Images/avatar.png"
                    }
                }
            };

            handler.OnComponentModelUpdated(scene, entity, model);
            AssetPromise_Material promiseMaterial = handler.materialPromises.Dequeue();
            yield return promiseMaterial;

            Material currentMaterial = promiseMaterial.asset.material;

            internalMaterialComponent.Received(1)
                                     .PutFor(scene, entity,
                                         Arg.Is<InternalMaterial>(x => x.material == currentMaterial && x.dirty));

            internalMaterialComponent.ClearReceivedCalls();

            PBMaterial model2 = new PBMaterial(model);
            handler.OnComponentModelUpdated(scene, entity, model2);
            promiseMaterial = handler.materialPromises.Count > 0 ? handler.materialPromises.Dequeue() : promiseMaterial;
            yield return promiseMaterial;

            internalMaterialComponent.DidNotReceive()
                                     .PutFor(scene, entity, Arg.Any<InternalMaterial>());
        }

        [Test]
        public void RemoveInternalMaterialOnRemove()
        {
            handler.OnComponentRemoved(scene, entity);
            internalMaterialComponent.Received(1)
                                     .RemoveFor(scene, entity,
                                         Arg.Is<InternalMaterial>(x => x.material == null));
        }
    }
}