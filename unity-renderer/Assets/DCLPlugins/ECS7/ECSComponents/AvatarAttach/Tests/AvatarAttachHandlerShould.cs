using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace DCL.ECSComponents.Test
{
    public class AvatarAttachComponentHandlerShould
    {
        private AvatarAttachComponentHandler handler;
        private IParcelScene scene;
        private IDCLEntity entity;
        private GameObject entityGo;

        [SetUp]
        public void Setup()
        {
            handler = Substitute.ForPartsOf<AvatarAttachComponentHandler>(Substitute.For<IUpdateEventHandler>());

            scene = Substitute.For<IParcelScene>();

            entityGo = new GameObject("AvatarAttachHandlerShould");
            entityGo.transform.position = UnityEngine.Vector3.zero;
            entityGo.transform.rotation = Quaternion.identity;

            entity = Substitute.For<IDCLEntity>();
            entity.Configure().gameObject.Returns(entityGo);
        }
        
        [TearDown]
        public void TearDown()
        {
            handler.Dispose();
            DataStore.Clear();
            Object.Destroy(entityGo);
        }

        [Test]
        public void DoNotDetachOrAttachIfIdMatchPreviousModel()
        {
            handler.prevModel = new PBAvatarAttach() { AvatarId = "Temptation" };
            var newModel = new PBAvatarAttach() { AvatarId = "Temptation" }; 

            handler.OnComponentModelUpdated(Substitute.For<IParcelScene>(),Substitute.For<IDCLEntity>(), newModel);
            handler.DidNotReceive().Detach();
            handler.DidNotReceive().Attach(Arg.Any<string>(), Arg.Any<AvatarAnchorPointIds>());
        }

        [Test]
        public void AttachWhenValidUserId()
        {
            var newModel = new PBAvatarAttach() { AvatarId = "Temptation" }; 

            handler.OnComponentModelUpdated(Substitute.For<IParcelScene>(),Substitute.For<IDCLEntity>(), newModel);
            handler.Received(1).Attach(Arg.Any<string>(), Arg.Any<AvatarAnchorPointIds>());
        }

        [Test]
        public void OnlyDetachWhenInvalidUserId()
        {   
            var newModel = new PBAvatarAttach() { AvatarId = "" }; 

            handler.OnComponentModelUpdated(Substitute.For<IParcelScene>(),Substitute.For<IDCLEntity>(), newModel);
            handler.Received(1).Detach();
            handler.DidNotReceive().Attach(Arg.Any<string>(), Arg.Any<AvatarAnchorPointIds>());
        }

        [Test]
        public void DetachWhenUserDisconnect()
        {
            const string userId = "Temptation";

            DataStore.i.player.otherPlayers.Add(userId, new Player() { id = userId });
            var newModel = new PBAvatarAttach() { AvatarId = userId };
            handler.IsInsideScene(Arg.Any<UnityEngine.Vector3>()).Returns(true);

            handler.OnComponentCreated(scene, entity);

            handler.OnComponentModelUpdated(scene, entity, newModel);
            
            DataStore.i.player.otherPlayers.Remove(userId);
            handler.Received(2).Detach();
        }

        [Test]
        public void DoAttachWhenUserIsFound()
        {
            const string userId = "Temptation";

            DataStore.i.player.otherPlayers.Add(userId, new Player() { id = userId });

            handler.OnComponentCreated(scene, entity);
            handler.OnComponentModelUpdated(scene, entity, new PBAvatarAttach() { AvatarId = userId });
            handler.Received(1).Attach(userId, Arg.Any<AvatarAnchorPointIds>());
            handler.Received(1).Attach(Arg.Any<IAvatarAnchorPoints>(), Arg.Any<AvatarAnchorPointIds>());
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            const string userId = "Temptation";
            UnityEngine.Vector3 targetPosition = new UnityEngine.Vector3(70, -135, 0);
            Quaternion targetRotation = Quaternion.Euler(0, 30, 0);

            IAvatarAnchorPoints anchorPoints = Substitute.For<IAvatarAnchorPoints>();
            anchorPoints.GetTransform(Arg.Any<AvatarAnchorPointIds>()).Returns((targetPosition, targetRotation, UnityEngine.Vector3.one));

            handler.IsInsideScene(Arg.Any<UnityEngine.Vector3>()).Returns(true);
            DataStore.i.player.otherPlayers.Add(userId, new Player() { id = userId, anchorPoints = anchorPoints });
            handler.OnComponentCreated(scene, entity);
            
            handler.OnComponentModelUpdated(scene, entity, new PBAvatarAttach() { AvatarId = userId, AnchorPointId = 0 });
            handler.LateUpdate();

            Assert.AreEqual(targetPosition, entityGo.transform.position);
            Assert.AreEqual(targetRotation.eulerAngles, entityGo.transform.rotation.eulerAngles);
        }

        [Test]
        public void SendEntityToMORDORwhenOutOfScene()
        {
            const string userId = "Temptation";
            UnityEngine.Vector3 targetPosition = new UnityEngine.Vector3(70, -135, 0);
            Quaternion targetRotation = Quaternion.Euler(0, 30, 0);

            IAvatarAnchorPoints anchorPoints = Substitute.For<IAvatarAnchorPoints>();
            anchorPoints.GetTransform(Arg.Any<AvatarAnchorPointIds>()).Returns((targetPosition, targetRotation, UnityEngine.Vector3.one));

            DataStore.i.player.otherPlayers.Add(userId, new Player() { id = userId, anchorPoints = anchorPoints });

            handler.OnComponentCreated(scene,entity);
            handler.OnComponentModelUpdated(scene, entity, new PBAvatarAttach() { AvatarId = userId, AnchorPointId = 0 });
            handler.LateUpdate();

            Assert.AreEqual(EnvironmentSettings.MORDOR, entityGo.transform.position);
        }
    }
}