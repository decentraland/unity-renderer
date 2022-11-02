using AvatarSystem;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace DCL.ECSComponents.Tests
{
    public class AvatarShapeShould
    {
        private IAvatar avatar;
        private IAvatarMovementController movementController;
        private IAvatarReporterController avatarReporterController;
        private IDCLEntity entity;
        private IParcelScene scene;
        private AvatarShape avatarShape;
        private GameObject gameObject;
        
        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            avatar = Substitute.For<IAvatar>();
            avatarReporterController = Substitute.For<IAvatarReporterController>();
            movementController = Substitute.For<IAvatarMovementController>();
            
            AvatarShape avatarShapePrefab = Resources.Load<AvatarShape>("NewAvatarShape");
            avatarShape = GameObject.Instantiate(avatarShapePrefab);
            avatarShape.OnPoolGet();
            avatarShape.avatar = avatar;
            avatarShape.avatarReporterController = avatarReporterController;
            avatarShape.avatarMovementController = movementController;
            
            entity.entityId.Returns(1);
            entity.gameObject.Returns(gameObject);
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = "1";
            scene.sceneData.Configure().Returns(sceneData);
        }

        [TearDown]
        protected void TearDown()
        {
            GameObject.Destroy(gameObject);
            GameObject.Destroy(avatarShape.gameObject);
        }
        
        [Test]
        public void ApplyModelCorrectly()
        {
            // Arrange
            var model = AvatarShapeComponentHandlerShould.CreateModel();
            
            // Act
            avatarShape.ApplyModel(scene, entity, model);
            
            // Assert
            Assert.AreEqual(entity,avatarShape.entity);
            Assert.IsTrue(avatarShape.initializedPosition);
 
            avatar.Received(1).PlayEmote(model.ExpressionTriggerId, model.ExpressionTriggerTimestamp);
            movementController.Received(1).OnTransformChanged(Arg.Any<UnityEngine.Vector3>(), Arg.Any<Quaternion>(), Arg.Any<bool>());
        }
        
        [Test]
        public void UpdatePlayerStatusCorrectly()
        {
            // Arrange
            var model = AvatarShapeComponentHandlerShould.CreateModel();
            
            // Act
            avatarShape.UpdatePlayerStatus(entity, model);
            
            // Assert
            Assert.AreEqual(model.Id,avatarShape.player.id);
            Assert.AreEqual(model.Name,avatarShape.player.name);
            Assert.AreEqual(model.Talking,avatarShape.player.isTalking);
            Assert.AreEqual(avatar,avatarShape.player.avatar);
            Assert.AreEqual(avatarShape.avatarCollider, avatarShape.player.collider); 
            Assert.AreEqual(avatarShape.onPointerDown,avatarShape.player.onPointerDownCollider);
        }
        
        [Test]
        public void CleanUpCorrectly()
        {
            // Arrange
            var model = AvatarShapeComponentHandlerShould.CreateModel();
            avatarShape.ApplyModel(scene, entity, model);
            
            // Act
            avatarShape.Cleanup();
            
            // Assert
            Assert.IsFalse(avatarShape.otherPlayers.ContainsKey(model.Id));
            Assert.IsNull(avatarShape.loadingCts);
            Assert.IsNull(avatarShape.entity);
            avatar.Received(1).Dispose();
            avatarReporterController.Received().ReportAvatarRemoved();
        }

        [Test]
        public void ApplyHideModiferAreaCorrectly()
        {
            // Arrange
            var model = AvatarShapeComponentHandlerShould.CreateModel();
            avatarShape.ApplyModel(scene, entity, model);
            
            // Act
            avatarShape.ApplyHideAvatarModifier();
            
            // Assert
            avatar.Received(1).AddVisibilityConstraint(AvatarShape.IN_HIDE_AREA);
            Assert.IsFalse(avatarShape.playerNameContainer.activeInHierarchy);
        }
        
        
        [Test]
        public void RemoveHideModiferAreaCorrectly()
        {
            // Arrange
            var model = AvatarShapeComponentHandlerShould.CreateModel();
            avatarShape.ApplyModel(scene, entity, model);
            
            // Act
            avatarShape.RemoveHideAvatarModifier();
            
            // Assert
            avatar.Received(1).RemoveVisibilityConstrain(AvatarShape.IN_HIDE_AREA);
            Assert.IsTrue(avatarShape.playerNameContainer.activeInHierarchy);
        }
    }
}