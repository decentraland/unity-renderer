using DCL;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.Helpers;
using Decentraland.Common;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Texture = Decentraland.Common.Texture;
using TextureWrapMode = Decentraland.Common.TextureWrapMode;

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
            scene = testUtils.CreateScene(666);
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
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    Texture = new TextureUnion()
                    {
                        Texture = new Texture()
                        {
                            Src = TestAssetsUtils.GetPath() + "/Images/avatar.png"
                        }
                    }
                }
            };

            handler.OnComponentModelUpdated(scene, entity, model);
            yield return handler.promiseMaterial;

            Assert.NotNull(handler.promiseMaterial.asset.material);
        }

        [UnityTest]
        public IEnumerator ReuseMaterialWhenModelDidntChange()
        {
            PBMaterial model = new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    Texture = new TextureUnion()
                    {
                        Texture = new Texture()
                        {
                            Src = TestAssetsUtils.GetPath() + "/Images/avatar.png"
                        }
                    }
                }
            };

            handler.OnComponentModelUpdated(scene, entity, model);
            yield return handler.promiseMaterial;

            Material firstMaterial = handler.promiseMaterial.asset.material;

            PBMaterial model2 = new PBMaterial(model);
            handler.OnComponentModelUpdated(scene, entity, model2);
            yield return handler.promiseMaterial;

            Assert.AreEqual(firstMaterial, handler.promiseMaterial.asset.material);
        }

        [UnityTest]
        public IEnumerator ChangeMaterialWhenModelChange()
        {
            PBMaterial model = new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    Texture = new TextureUnion()
                    {
                        Texture = new Texture()
                        {
                            Src = TestAssetsUtils.GetPath() + "/Images/avatar.png"
                        }
                    }
                }
            };

            yield return null;

            handler.OnComponentModelUpdated(scene, entity, model);
            yield return handler.promiseMaterial;

            Material firstMaterial = handler.promiseMaterial.asset.material;

            PBMaterial model2 = new PBMaterial(model);
            model2.Pbr.Texture.Texture.WrapMode = TextureWrapMode.TwmMirror;
            handler.OnComponentModelUpdated(scene, entity, model2);
            yield return handler.promiseMaterial;

            Assert.AreNotEqual(firstMaterial, handler.promiseMaterial.asset.material);
        }

        [UnityTest]
        public IEnumerator ChangeMaterialWhenNoTextureSource()
        {
            Debug.Log(TestAssetsUtils.GetPath() + "/Images/avatar.png");

            PBMaterial model = new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    Texture = new TextureUnion()
                    {
                        Texture = new Texture()
                        {
                            Src = TestAssetsUtils.GetPath() + "/Images/avatar.png"
                        }
                    }
                }
            };

            yield return null;

            handler.OnComponentModelUpdated(scene, entity, model);
            yield return handler.promiseMaterial;

            Material firstMaterial = handler.promiseMaterial.asset.material;

            PBMaterial model2 = new PBMaterial(model)
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    Texture = new TextureUnion()
                    {
                        Texture = new Texture()
                        {
                            WrapMode = TextureWrapMode.TwmMirror
                        }
                    }
                }
            };

            handler.OnComponentModelUpdated(scene, entity, model2);
            yield return handler.promiseMaterial;

            Assert.AreNotEqual(firstMaterial, handler.promiseMaterial.asset.material);
        }

        [UnityTest]
        public IEnumerator ForgetMaterialOnRemoved()
        {
            PBMaterial model = new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    Texture = new TextureUnion()
                    {
                        Texture = new Texture()
                        {
                            Src = TestAssetsUtils.GetPath() + "/Images/avatar.png"
                        }
                    }
                }
            };

            handler.OnComponentModelUpdated(scene, entity, model);
            yield return handler.promiseMaterial;
            Assert.AreEqual(1, AssetPromiseKeeper_Material.i.library.masterAssets.Count);

            handler.OnComponentRemoved(scene, entity);
            Assert.AreEqual(0, AssetPromiseKeeper_Material.i.library.masterAssets.Count);
        }

        [UnityTest]
        public IEnumerator PutInternalMaterialOnUpdate()
        {
            PBMaterial model = new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    Texture = new TextureUnion()
                    {
                        Texture = new Texture()
                        {
                            Src = TestAssetsUtils.GetPath() + "/Images/avatar.png"
                        }
                    }
                }
            };

            handler.OnComponentModelUpdated(scene, entity, model);
            yield return handler.promiseMaterial;
            Material currentMaterial = handler.promiseMaterial.asset.material;

            internalMaterialComponent.Received(1)
                                     .PutFor(scene, entity,
                                          Arg.Is<InternalMaterial>(x => x.material == currentMaterial && x.dirty));

            internalMaterialComponent.ClearReceivedCalls();

            PBMaterial model2 = new PBMaterial(model)
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    Texture = new TextureUnion()
                    {
                        Texture = new Texture()
                        {
                            WrapMode = TextureWrapMode.TwmMirror
                        }
                    }
                }
            };

            handler.OnComponentModelUpdated(scene, entity, model2);
            yield return handler.promiseMaterial;
            currentMaterial = handler.promiseMaterial.asset.material;

            internalMaterialComponent.Received(1)
                                     .PutFor(scene, entity,
                                          Arg.Is<InternalMaterial>(x => x.material == currentMaterial && x.dirty));
        }

        [UnityTest]
        public IEnumerator NotPutInternalMaterialIfModelNotUpdated()
        {
            PBMaterial model = new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    Texture = new TextureUnion()
                    {
                        Texture = new Texture()
                        {
                            Src = TestAssetsUtils.GetPath() + "/Images/avatar.png"
                        }
                    }
                }
            };

            handler.OnComponentModelUpdated(scene, entity, model);
            yield return handler.promiseMaterial;
            Material currentMaterial = handler.promiseMaterial.asset.material;

            internalMaterialComponent.Received(1)
                                     .PutFor(scene, entity,
                                          Arg.Is<InternalMaterial>(x => x.material == currentMaterial && x.dirty));

            internalMaterialComponent.ClearReceivedCalls();

            PBMaterial model2 = new PBMaterial(model);
            handler.OnComponentModelUpdated(scene, entity, model2);
            yield return handler.promiseMaterial;

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

        [UnityTest]
        public IEnumerator CreateUnlitMaterial()
        {
            PBMaterial model = new PBMaterial()
            {
                Unlit = new PBMaterial.Types.UnlitMaterial()
                {
                    Texture = new TextureUnion()
                    {
                        Texture = new Texture()
                        {
                            Src = TestAssetsUtils.GetPath() + "/Images/avatar.png"
                        }
                    }
                }
            };

            handler.OnComponentModelUpdated(scene, entity, model);
            yield return handler.promiseMaterial;

            Assert.NotNull(handler.promiseMaterial.asset.material);
        }

        [UnityTest]
        public IEnumerator ForgetPreviousPromisesCorrectly()
        {
            PBMaterial CreateModel(Color color)
            {
                return new PBMaterial()
                {
                    Pbr = new PBMaterial.Types.PbrMaterial()
                    {
                        AlbedoColor = new Color4() { R = color.r, G = color.g, B = color.b }
                    }
                };
            }

            handler.OnComponentModelUpdated(scene, entity, CreateModel(Color.black));
            handler.OnComponentModelUpdated(scene, entity, CreateModel(Color.white));
            handler.OnComponentModelUpdated(scene, entity, CreateModel(Color.green));

            // Test possible race condition of material being forgotten before loading or failing
            AssetPromiseKeeper_Material.i.Forget(handler.promiseMaterial);

            handler.OnComponentModelUpdated(scene, entity, CreateModel(Color.grey));
            handler.OnComponentModelUpdated(scene, entity, CreateModel(Color.blue));

            yield return handler.promiseMaterial;

            // Wait a couple of frames for previous material to be forgotten
            yield return null;
            yield return null;
            Assert.AreEqual(1, AssetPromiseKeeper_Material.i.library.masterAssets.Count);
        }

        [UnityTest]
        public IEnumerator ForgetWontRaiseExceptionWhenCalledAfterComponentIsRemoved()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    AlbedoColor = new Color4() { R = 0, G = 0, B = 0 }
                }
            });

            handler.OnComponentModelUpdated(scene, entity, new PBMaterial()
            {
                Pbr = new PBMaterial.Types.PbrMaterial()
                {
                    AlbedoColor = new Color4() { R = 1, G = 1, B = 1 }
                }
            });

            yield return handler.promiseMaterial;
            handler.OnComponentRemoved(scene, entity);
            handler = null;

            // Wait for materials to be forgotten
            yield return new WaitUntil(() => AssetPromiseKeeper_Material.i.library.masterAssets.Count == 0);
        }
    }
}
