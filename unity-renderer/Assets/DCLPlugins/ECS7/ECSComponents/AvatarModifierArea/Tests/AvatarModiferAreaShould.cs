using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DCL.Controllers;
using DCL.Models;
using Google.Protobuf;
using Google.Protobuf.Collections;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.ECSComponents.Test
{
    public class AvatarModifierAreaShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private AvatarModifierAreaComponentHandler componentHandler;
        private GameObject gameObject;
        private DataStore_Player dataStorePlayer;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            var modifierFactory =  new AvatarModifierFactory();
            dataStorePlayer = new DataStore_Player();
            componentHandler = new AvatarModifierAreaComponentHandler(Substitute.For<IUpdateEventHandler>(), dataStorePlayer, modifierFactory);

            entity.entityId.Returns(1);
            entity.gameObject.Returns(gameObject);
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
            Assert.IsNotNull(componentHandler.excludedColliders);
            Assert.AreEqual(model,componentHandler.model);
        }

        [Test]
        public void DisposeComponentCorrectly()
        {
            // Arrange
            var model = CreateModel();
            componentHandler.OnComponentModelUpdated(scene, entity, model);
            componentHandler.avatarsInArea = new HashSet<GameObject>();
            componentHandler.avatarsInArea.Add(gameObject);

            // Act
            componentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.AreEqual(0,componentHandler.avatarsInArea.Count);
        }

        [Test]
        public void ExcludeAvatarsIdsCorrectly()
        {
            // Arrange
            var model = CreateModel();
            model.ExcludeIds.Add("PlayerId");
            dataStorePlayer.otherPlayers.Add("PlayerId", new Player());
            
            // Act
            var result = componentHandler.GetExcludedColliders(model);
            
            // Assert
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void SerializeAndDeserialzeCorrectly()
        {
            // Arrange
            var model = CreateModel();

            // Act
            var newModel = SerializaAndDeserialize(model);
            
            // Assert
            Assert.AreEqual(model.Area, newModel.Area);
            Assert.AreEqual(model.Modifiers, newModel.Modifiers);
            Assert.AreEqual(model.ExcludeIds, newModel.ExcludeIds);
        }

        private PBAvatarModifierArea SerializaAndDeserialize(PBAvatarModifierArea pb)
        {
            var serialized = AvatarModifierAreaSerializer.Serialize(pb);
            return AvatarModifierAreaSerializer.Deserialize(serialized);
        }

        private PBAvatarModifierArea CreateModel()
        {
            PBAvatarModifierArea model = new PBAvatarModifierArea();
            model.Area = new Vector3();
            model.Area.X = 2f;
            model.Area.Y = 2f;
            model.Area.Z = 2f;

            model.Modifiers.Add(AvatarModifier.HideAvatars);
            model.ExcludeIds.Add("IdToExclude");
            
            return model;
        }
    }
}
