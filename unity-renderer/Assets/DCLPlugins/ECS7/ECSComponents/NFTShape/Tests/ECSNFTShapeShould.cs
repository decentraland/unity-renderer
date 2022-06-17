using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using DCL.Controllers;
using DCL.Helpers.NFT;
using DCL.Models;
using Google.Protobuf;
using NFTShape_Internal;
using NSubstitute;
using NSubstitute.Extensions;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace DCL.ECSComponents.Test
{
    public class ECSNFTShapeShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private ECSNFTShapeComponentHandler componentHandler;
        private GameObject gameObject;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            var shapeFrameFactory = Resources.Load<NFTShapeFrameFactory>("NFTShapeFrameFactory");
            componentHandler = new ECSNFTShapeComponentHandler(shapeFrameFactory, new NFTInfoRetriever(), new NFTAssetRetriever());

            var meshInfo = new MeshesInfo();
            meshInfo.meshRootGameObject = gameObject;
            entity.Configure().meshesInfo.Returns(meshInfo);
            entity.Configure().meshRootGameObject.Returns(meshInfo.meshRootGameObject);
            entity.Configure().entityId.Returns(1);
            entity.Configure().gameObject.Returns(gameObject);
            
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = "1";
            scene.sceneData.Configure().Returns(sceneData);
            
            componentHandler.OnComponentCreated(scene, entity);
        }

        [TearDown]
        protected void TearDown()
        {
            componentHandler.OnComponentRemoved(scene, entity);
            GameObject.Destroy(gameObject);
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            // Arrange
            var model = CreateModel();

            // Act
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsNotNull(entity.meshesInfo);
            Assert.IsNotNull(entity.meshesInfo.meshRootGameObject);
        }
        
        [Test]
        public void CreateShapeCorrectly()
        {
            // Arrange
            var model = CreateModel();
            
            // Act
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsNotNull(componentHandler.shapeFrame);
        }
        
        [Test]
        public void CreateNFTLoadersCorrectly()
        {
            // Arrange
            var model = CreateModel();
            
            // Act
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsNotNull(componentHandler.infoRetriever);
            Assert.IsNotNull(componentHandler.assetRetriever);
        }
        
        [Test]
        public void ApplyModelCorrectly()
        {
            // Arrange
            var model = CreateModel();
            model.Visible = true;
            model.WithCollisions = true;
            componentHandler.shapeFrame = Substitute.For<INFTShapeFrame>();
            
            // Act
            componentHandler.ApplyModel(model);

            // Assert
            componentHandler.shapeFrame.Received().SetVisibility(true);
            componentHandler.shapeFrame.Received().SetVisibility(true);
            componentHandler.shapeFrame.Received().UpdateBackgroundColor(Arg.Any<UnityEngine.Color>());
        }
        
        [Test]
        public void FailWithNullNFTInfoCorrectly()
        {
            // Arrange
            componentHandler.infoRetriever = Substitute.For<INFTInfoRetriever>();
            componentHandler.infoRetriever.Configure().FetchNFTInfo(Arg.Any<string>()).Returns(new UniTask<NFTInfo>(null));
            componentHandler.shapeFrame = Substitute.For<INFTShapeFrame>();
            componentHandler.factory = Substitute.For<INFTShapeFrameFactory>();
            
            var model = CreateModel();
            
            // Act
            componentHandler.LoadNFT(scene, model);

            // Assert
            componentHandler.shapeFrame.Received().FailLoading();
        }
        
        [Test]
        public void FailWithNullNFTAssetCorrectly()
        {
            // Arrange
            NFTInfo info = new NFTInfo();
            componentHandler.infoRetriever = Substitute.For<INFTInfoRetriever>();
            componentHandler.infoRetriever.Configure().FetchNFTInfo(Arg.Any<string>()).Returns(new UniTask<NFTInfo>(info));
            
            componentHandler.assetRetriever = Substitute.For<INFTAssetRetriever>();
            componentHandler.assetRetriever.Configure().LoadNFTAsset(Arg.Any<string>()).Returns(new UniTask<INFTAsset>(null));

            componentHandler.shapeFrame = Substitute.For<INFTShapeFrame>();
            componentHandler.factory = Substitute.For<INFTShapeFrameFactory>();
            
            var model = CreateModel();
            
            // Act
            componentHandler.LoadNFT(scene, model);

            // Assert
            componentHandler.shapeFrame.Received().FailLoading();
        }
        
        [Test]
        public void LoadNFTCorrectly()
        {
            // Arrange
            NFTInfo info = new NFTInfo();
            componentHandler.infoRetriever = Substitute.For<INFTInfoRetriever>();
            componentHandler.infoRetriever.Configure().FetchNFTInfo(Arg.Any<string>()).Returns(new UniTask<NFTInfo>(info));
            
            componentHandler.assetRetriever = Substitute.For<INFTAssetRetriever>();
            componentHandler.assetRetriever.Configure().LoadNFTAsset(Arg.Any<string>()).Returns(new UniTask<INFTAsset>(new NFTAsset_Image(null)));

            componentHandler.shapeFrame = Substitute.For<INFTShapeFrame>();

            var model = CreateModel();
            
            // Act
            componentHandler.LoadNFT(scene, model);

            // Assert
            componentHandler.shapeFrame.Received().SetImage(Arg.Any<string>(),Arg.Any<string>(),Arg.Any<INFTAsset>());
        }
        
        [Test]
        public void LoadResourceCorrectly()
        {
            // Arrange
            NFTInfo info = new NFTInfo();
            componentHandler.infoRetriever = Substitute.For<INFTInfoRetriever>();
            componentHandler.infoRetriever.Configure().FetchNFTInfo(Arg.Any<string>()).Returns(new UniTask<NFTInfo>(info));
            
            componentHandler.assetRetriever = Substitute.For<INFTAssetRetriever>();
            componentHandler.assetRetriever.Configure().LoadNFTAsset(Arg.Any<string>()).Returns(new UniTask<INFTAsset>(new NFTAsset_Image(null)));

            componentHandler.shapeFrame = Substitute.For<INFTShapeFrame>();

            var model = CreateModel();
            
            // Act
            componentHandler.LoadNFT(scene, model);

            // Assert
            componentHandler.shapeFrame.Received().SetImage(Arg.Any<string>(),Arg.Any<string>(), Arg.Any<INFTAsset>());
        }

        [Test]
        public void SerializeCorrectly()
        {
            // Arrange
            PBNFTShape model = new PBNFTShape();
            byte[] byteArray;
            
            // Act
            using(var memoryStream = new MemoryStream())
            {
                model.WriteTo(memoryStream);
                byteArray = memoryStream.ToArray();
            }

            // Assert
            Assert.IsNotNull(byteArray);
        }
        
        [TestCase(false,false,false)]
        [TestCase(false,true,false)]
        [TestCase(false,false,true)]
        public void SerializeAndDeserialzeCorrectly(bool visible, bool withCollision, bool isPointerBlocker)
        {
            // Arrange
            PBNFTShape model = new PBNFTShape();
            model.Visible = visible;
            model.WithCollisions = withCollision;
            model.IsPointerBlocker = isPointerBlocker;
            model.Color = new Color3();
            model.Color.R = 1;
            model.Color.B = 0;
            model.Color.G = 0.5f;
            model.Src = "ethereum://test/123";

            // Act
            var newModel = SerializaAndDeserialize(model);
            
            // Assert
            Assert.AreEqual(model.Visible, newModel.Visible);
            Assert.AreEqual(model.WithCollisions, newModel.WithCollisions);
            Assert.AreEqual(model.IsPointerBlocker, newModel.IsPointerBlocker);
            Assert.AreEqual(model.Src, newModel.Src);
            Assert.AreEqual(model.Color.R, newModel.Color.R);
            Assert.AreEqual(model.Color.B, newModel.Color.B);
            Assert.AreEqual(model.Color.G, newModel.Color.G);
        }

        private PBNFTShape CreateModel()
        {
            var model = new PBNFTShape();
            model.Src = "ethereum://test";
            model.Color = new Color3();
            model.Color.B = 1f;
            model.Color.R = 0f;
            model.Color.G = 0f;
            return model;
        }
        
        private PBNFTShape SerializaAndDeserialize(PBNFTShape pb)
        {
            var result = NFTShapeSerializer.Serialize(pb);

            return NFTShapeSerializer.Deserialize(result);
        }
    }
}
