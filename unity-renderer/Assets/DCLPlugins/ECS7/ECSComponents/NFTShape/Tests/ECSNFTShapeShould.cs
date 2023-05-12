using Cysharp.Threading.Tasks;
using DCL;
using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers.NFT;
using Decentraland.Common;
using NFTShape_Internal;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
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
            var factory = new ECSComponentsFactory();
            var manager = new ECSComponentsManager(factory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            var internalComponent = new InternalECSComponents(manager, factory, executors);

            testUtils = new ECS7TestUtilsScenesAndEntities(manager, executors);
            scene = testUtils.CreateScene(666);
            entity = scene.CreateEntity(10399);

            renderersComponent = internalComponent.renderersComponent;
            infoRetriever = Substitute.For<INFTInfoRetriever>();
            assetRetriever = Substitute.For<INFTAssetRetriever>();
            var shapeFrameFactory = Resources.Load<NFTShapeFrameFactory>("NFTShapeFrameFactory");

            handler = new ECSNFTShapeComponentHandler(shapeFrameFactory,
                infoRetriever,
                assetRetriever,
                renderersComponent);

            var keepEntityAliveComponent = new InternalECSComponent<InternalComponent>(
                0, manager, factory, null,
                new KeyValueSet<ComponentIdentifier, ComponentWriteData>(), executors);

            keepEntityAliveComponent.PutFor(scene, entity, new InternalComponent());
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void UpdateImageCorrectly()
        {
            infoRetriever.FetchNFTInfoAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(UniTask.FromResult(new NFTInfo()));
            assetRetriever.LoadNFTAsset(Arg.Any<string>()).Returns(UniTask.FromResult(Substitute.For<INFTAsset>()));

            PBNftShape model = new PBNftShape()
            {
                Urn = "urn:decentraland:ethereum:erc721:0x06012c8cf97bead5deae237070f9587f8e7a266d:1540722"
            };

            handler.OnComponentModelUpdated(scene, entity, model);
            NFTUtils.TryParseUrn(model.Urn, out string contractAddress, out string tokenID);
            infoRetriever.Received(1).FetchNFTInfoAsync(contractAddress, tokenID);

            model.Urn = "urn:decentraland:ethereum:erc721:0x8eaa9ae1ac89b1c8c8a8104d08c045f78aadb42d:450";

            handler.OnComponentModelUpdated(scene, entity, model);
            NFTUtils.TryParseUrn(model.Urn, out contractAddress, out tokenID);
            infoRetriever.Received(1).FetchNFTInfoAsync(contractAddress, tokenID);

            handler.OnComponentRemoved(scene, entity);
        }

        [Test]
        public void CreateFrame()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBNftShape());

            NFTShapeFrame frame = (NFTShapeFrame)handler.shapeFrame;
            Assert.IsTrue(frame);

            handler.OnComponentRemoved(scene, entity);
        }

        [UnityTest]
        public IEnumerator DestroyFrame()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBNftShape());

            NFTShapeFrame frame = (NFTShapeFrame)handler.shapeFrame;

            handler.OnComponentRemoved(scene, entity);
            yield return null;

            Assert.IsFalse(frame);
        }

        [Test]
        public void AddAndRemoveRenderer()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBNftShape());
            Assert.IsTrue(renderersComponent.GetFor(scene, entity).model.renderers.Contains(handler.shapeFrame.frameRenderer));

            handler.OnComponentRemoved(scene, entity);
            Assert.IsNull(renderersComponent.GetFor(scene, entity));
        }

        [Test]
        public void UpdateStyle()
        {
            PBNftShape model = new PBNftShape() { Style = NftFrameType.NftGoldEdges };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual("Golden_01", handler.shapeFrame.gameObject.transform.GetChild(0).name);

            model = new PBNftShape() { Style = NftFrameType.NftClassic };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual("Classic", handler.shapeFrame.gameObject.transform.GetChild(0).name);

            handler.OnComponentRemoved(scene, entity);
        }

        [Test]
        public void UpdateColor()
        {
            PBNftShape model = new PBNftShape() { Color = new Color3() { R = 1, G = 1, B = 1 } };
            handler.OnComponentModelUpdated(scene, entity, model);

            MeshRenderer renderer = (MeshRenderer)handler.shapeFrame.frameRenderer;
            Assert.AreEqual(model.Color.R, renderer.sharedMaterials[1].color.r);
            Assert.AreEqual(model.Color.G, renderer.sharedMaterials[1].color.g);
            Assert.AreEqual(model.Color.B, renderer.sharedMaterials[1].color.b);

            model = new PBNftShape() { Color = new Color3() { R = 0, G = 0, B = 0 } };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual(model.Color.R, renderer.sharedMaterials[1].color.r);
            Assert.AreEqual(model.Color.G, renderer.sharedMaterials[1].color.g);
            Assert.AreEqual(model.Color.B, renderer.sharedMaterials[1].color.b);

            handler.OnComponentRemoved(scene, entity);
        }
    }
}
