using System.Collections;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.Helpers.NFT;
using NFTShape_Internal;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ECSNFTShapeShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECSNFTShapeComponentHandler handler;

        private ECS7TestScene scene;
        private ECS7TestEntity entity;
        private IInternalECSComponent<InternalRenderers> renderersComponent;
        private INFTInfoRetriever infoRetriever;
        private INFTAssetRetriever assetRetriever;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            Environment.Setup(serviceLocator);

            testUtils = new ECS7TestUtilsScenesAndEntities();
            scene = testUtils.CreateScene("temptation");
            entity = scene.CreateEntity(10399);

            renderersComponent = Substitute.For<IInternalECSComponent<InternalRenderers>>();
            infoRetriever = Substitute.For<INFTInfoRetriever>();
            assetRetriever = Substitute.For<INFTAssetRetriever>();
            var shapeFrameFactory = Resources.Load<NFTShapeFrameFactory>("NFTShapeFrameFactory");
            handler = new ECSNFTShapeComponentHandler(shapeFrameFactory,
                infoRetriever,
                assetRetriever,
                renderersComponent);
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void UpdateImageCorrectly()
        {
            infoRetriever.FetchNFTInfo(Arg.Any<string>()).Returns(UniTask.FromResult(new NFTInfo()));
            assetRetriever.LoadNFTAsset(Arg.Any<string>()).Returns(UniTask.FromResult(Substitute.For<INFTAsset>()));

            PBNFTShape model = new PBNFTShape()
            {
                Src = "ethereum://0x06012c8cf97bead5deae237070f9587f8e7a266d/1540722"
            };
            handler.OnComponentModelUpdated(scene, entity, model);
            infoRetriever.Received(1).FetchNFTInfo(model.Src);

            model.Src = "ethereum://0x8eaa9ae1ac89b1c8c8a8104d08c045f78aadb42d/450";
            handler.OnComponentModelUpdated(scene, entity, model);
            infoRetriever.Received(1).FetchNFTInfo(model.Src);

            handler.OnComponentRemoved(scene, entity);
        }

        [Test]
        public void CreateFrame()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBNFTShape());

            NFTShapeFrame frame = (NFTShapeFrame)handler.shapeFrame;
            Assert.IsTrue(frame);

            handler.OnComponentRemoved(scene, entity);
        }

        [UnityTest]
        public IEnumerator DestroyFrame()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBNFTShape());

            NFTShapeFrame frame = (NFTShapeFrame)handler.shapeFrame;

            handler.OnComponentRemoved(scene, entity);
            yield return null;

            Assert.IsFalse(frame);
        }

        [Test]
        public void AddAndRemoveRenderer()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBNFTShape());
            renderersComponent.Received(1)
                              .PutFor(scene, entity,
                                  Arg.Is<InternalRenderers>(r => r.renderers.Contains(handler.shapeFrame.frameRenderer)));

            handler.OnComponentRemoved(scene, entity);
            renderersComponent.Received(1).RemoveFor(scene, entity);
        }

        [Test]
        public void UpdateStyle()
        {
            PBNFTShape model = new PBNFTShape() { Style = PBNFTShape.Types.PictureFrameStyle.GoldEdges };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual("Golden_01", handler.shapeFrame.gameObject.transform.GetChild(0).name);

            model = new PBNFTShape() { Style = PBNFTShape.Types.PictureFrameStyle.Classic };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual("Classic", handler.shapeFrame.gameObject.transform.GetChild(0).name);

            handler.OnComponentRemoved(scene, entity);
        }

        [Test]
        public void UpdateColor()
        {
            PBNFTShape model = new PBNFTShape() { Color = new Color3() { R = 1, G = 1, B = 1 } };
            handler.OnComponentModelUpdated(scene, entity, model);

            MeshRenderer renderer = (MeshRenderer)handler.shapeFrame.frameRenderer;
            Assert.AreEqual(model.Color.R, renderer.sharedMaterials[1].color.r);
            Assert.AreEqual(model.Color.G, renderer.sharedMaterials[1].color.g);
            Assert.AreEqual(model.Color.B, renderer.sharedMaterials[1].color.b);

            model = new PBNFTShape() { Color = new Color3() { R = 0, G = 0, B = 0 } };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual(model.Color.R, renderer.sharedMaterials[1].color.r);
            Assert.AreEqual(model.Color.G, renderer.sharedMaterials[1].color.g);
            Assert.AreEqual(model.Color.B, renderer.sharedMaterials[1].color.b);

            handler.OnComponentRemoved(scene, entity);
        }
    }
}