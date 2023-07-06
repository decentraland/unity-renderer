using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using Decentraland.Common;
using NSubstitute;
using NSubstitute.Extensions;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECSComponents.Tests
{
    public class AvatarShapeComponentHandlerShould
    {
        private const string AVATAR_POOL_NAME = "AvatarShapeECS";

        private IDCLEntity entity;
        private IParcelScene scene;
        private IAvatarShape avatarShape;
        private AvatarShapeComponentHandler componentHandler;
        private GameObject gameObject;

        private Pool pool;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();

            GameObject poolGameObject = new GameObject();
            ConfigurePool(poolGameObject);

            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            var internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);

            componentHandler = new AvatarShapeComponentHandler(pool, internalComponents.renderersComponent);
            avatarShape = Substitute.For<IAvatarShape>();
            avatarShape.Configure().transform.Returns(gameObject.transform);
            componentHandler.avatar = avatarShape;

            entity.entityId.Returns(1);
            entity.gameObject.Returns(gameObject);
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.sceneNumber = 1;
            scene.sceneData.Configure().Returns(sceneData);

            componentHandler.OnComponentCreated(scene, entity);
        }

        [TearDown]
        protected void TearDown()
        {
            PoolManager.i.RemovePool(AVATAR_POOL_NAME);
            pool.ReleaseAll();
            componentHandler.OnComponentRemoved(scene, entity);
            GameObject.Destroy(gameObject);
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            // Arrange
            var model = CreateModel();
            componentHandler.OnComponentCreated(scene,entity);

            // Act
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            avatarShape.Received(1).ApplyModel(scene,entity,model);
        }

        [Test]
        public void DisposeComponentCorrectly()
        {
            // Arrange
            var model = CreateModel();
            componentHandler.OnComponentCreated(scene,entity);
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Act
            componentHandler.OnComponentRemoved(scene,entity);

            // Assert
            avatarShape.Received(1).Cleanup();
        }

        [Test]
        public void SerializeAndDeserialzeCorrectly()
        {
            // Arrange
            var model = CreateModel();

            // Act
            var newModel = SerializaAndDeserialize(model);

            // Assert
            Assert.AreEqual(model.Id, newModel.Id);
            Assert.AreEqual(model.Name, newModel.Name);
            Assert.AreEqual(model.BodyShape, newModel.BodyShape);
            Assert.AreEqual(model.ExpressionTriggerId, newModel.ExpressionTriggerId);

            Assert.AreEqual(model.EyeColor, newModel.EyeColor);
            Assert.AreEqual(model.HairColor, newModel.HairColor);
            Assert.AreEqual(model.SkinColor, newModel.SkinColor);

            Assert.AreEqual(model.Wearables, newModel.Wearables);
        }

        private PBAvatarShape SerializaAndDeserialize(PBAvatarShape pb)
        {
            var serialized = AvatarShapeSerializer.Serialize(pb);
            return AvatarShapeSerializer.Deserialize(serialized);
        }

        public static PBAvatarShape CreateModel()
        {
            PBAvatarShape avatarShape = new PBAvatarShape();
            avatarShape.Id = "0xe7dd153081b0526e0a8c582497cbcee7cd44e464";
            avatarShape.Name = "TestName#2354";
            avatarShape.BodyShape = "urn:decentraland:off-chain:base-avatars:BaseFemale";
            avatarShape.ExpressionTriggerId = "Idle";
            avatarShape.ExpressionTriggerTimestamp = 0;

            avatarShape.EyeColor = new Color3();
            avatarShape.EyeColor.R = 0.223f;
            avatarShape.EyeColor.G = 0.484f;
            avatarShape.EyeColor.B = 0.691f;

            avatarShape.HairColor = new Color3();
            avatarShape.HairColor.R = 0.223f;
            avatarShape.HairColor.G = 0.484f;
            avatarShape.HairColor.B = 0.691f;

            avatarShape.SkinColor = new Color3();
            avatarShape.SkinColor.R = 0.223f;
            avatarShape.SkinColor.G = 0.484f;
            avatarShape.SkinColor.B = 0.691f;

            avatarShape.Wearables.Add("urn:decentraland:off-chain:base-avatars:f_eyebrows_07");
            avatarShape.Wearables.Add("urn:decentraland:off-chain:base-avatars:eyes_02");
            avatarShape.Wearables.Add("urn:decentraland:off-chain:base-avatars:f_mouth_03");
            avatarShape.Wearables.Add("urn:decentraland:off-chain:base-avatars:f_school_skirt");
            avatarShape.Wearables.Add("urn:decentraland:off-chain:base-avatars:SchoolShoes");
            avatarShape.Wearables.Add("urn:decentraland:matic:collections-v2:0x177535a421e7867ec52f2cc19b7dfed4f289a2bb:0");
            avatarShape.Wearables.Add("urn:decentraland:matic:collections-v2:0xd89efd0be036410d4ff194cd6ecece4ef8851d86:1");
            avatarShape.Wearables.Add("urn:decentraland:matic:collections-v2:0x1df3011a14ea736314df6cdab4fff824c5d46ec1:0");
            avatarShape.Wearables.Add("urn:decentraland:matic:collections-v2:0xbada8a315e84e4d78e3b6914003647226d9b4001:1");
            avatarShape.Wearables.Add("urn:decentraland:matic:collections-v2:0x1df3011a14ea736314df6cdab4fff824c5d46ec1:5");
            avatarShape.Wearables.Add("urn:decentraland:matic:collections-v2:0xd89efd0be036410d4ff194cd6ecece4ef8851d86:0");

            return avatarShape;
        }

        internal void ConfigurePool(GameObject prefab)
        {
            pool = PoolManager.i.GetPool(AVATAR_POOL_NAME);
            if (pool == null)
            {
                pool = PoolManager.i.AddPool(
                    AVATAR_POOL_NAME,
                    GameObject.Instantiate(prefab).gameObject,
                    isPersistent: true);

                pool.ForcePrewarm();
            }
        }
    }
}
