using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AvatarAttach_Tests
{
    public class AvatarAttachHandlerShould
    {
        private UserProfile userProfile;

        [SetUp]
        public void SetUp()
        {
            userProfile = UserProfile.GetOwnUserProfile();
            userProfile.UpdateData(new UserProfileModel() { userId = "ownUserId" });
        }

        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
            if (userProfile != null && AssetDatabase.Contains(userProfile))
            {
                Resources.UnloadAsset(userProfile);
            }
        }

        [Test]
        public void DoNotDetachOrAttachIfIdMatchPreviousModel()
        {
            AvatarAttachHandler handler = Substitute.ForPartsOf<AvatarAttachHandler>();
            handler.model = new AvatarAttachComponent.Model() { avatarId = "Temptation" };

            handler.OnModelUpdated(new AvatarAttachComponent.Model() { avatarId = "Temptation" });
            handler.DidNotReceive().Detach();
            handler.DidNotReceive().Attach(Arg.Any<string>(), Arg.Any<AvatarAnchorPointIds>());
            handler.Dispose();
        }

        [Test]
        public void AttachWhenValidUserId()
        {
            AvatarAttachHandler handler = Substitute.ForPartsOf<AvatarAttachHandler>();

            handler.OnModelUpdated(new AvatarAttachComponent.Model() { avatarId = "Temptation" });
            handler.Received(1).Attach(Arg.Any<string>(), Arg.Any<AvatarAnchorPointIds>());
            handler.Dispose();
        }

        [Test]
        public void OnlyDetachWhenInvalidUserId()
        {
            AvatarAttachHandler handler = Substitute.ForPartsOf<AvatarAttachHandler>();

            handler.OnModelUpdated(new AvatarAttachComponent.Model() { avatarId = "" });
            handler.Received(1).Detach();
            handler.DidNotReceive().Attach(Arg.Any<string>(), Arg.Any<AvatarAnchorPointIds>());
            handler.Dispose();
        }

        [Test]
        public void DetachWhenUserDisconnect()
        {
            const string userId = "Temptation";

            DataStore.i.player.otherPlayers.Add(userId, new Player() { id = userId });
            AvatarAttachHandler handler = Substitute.ForPartsOf<AvatarAttachHandler>();
            handler.IsInsideScene(Arg.Any<Vector3>()).Returns(true);

            handler.OnModelUpdated(new AvatarAttachComponent.Model() { avatarId = userId });
            handler.Received(1).Detach();

            DataStore.i.player.otherPlayers.Remove(userId);
            handler.Received(1).Detach();
            handler.Dispose();
        }

        [Test]
        public void DoAttachWhenUserIsFound()
        {
            const string userId = "Temptation";

            DataStore.i.player.otherPlayers.Add(userId, new Player() { id = userId });
            AvatarAttachHandler handler = Substitute.ForPartsOf<AvatarAttachHandler>();

            handler.OnModelUpdated(new AvatarAttachComponent.Model() { avatarId = userId });
            handler.Received(1).Attach(Arg.Any<string>(), Arg.Any<AvatarAnchorPointIds>());
            handler.Received(1).Attach(Arg.Any<IAvatarAnchorPoints>(), Arg.Any<AvatarAnchorPointIds>());
            handler.Dispose();
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            const string userId = "Temptation";
            Vector3 targetPosition = new Vector3(70, -135, 0);
            Quaternion targetRotation = Quaternion.Euler(0, 30, 0);

            GameObject entityGo = new GameObject("AvatarAttachHandlerShould");
            entityGo.transform.position = Vector3.zero;
            entityGo.transform.rotation = Quaternion.identity;

            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.gameObject.Returns(entityGo);

            IAvatarAnchorPoints anchorPoints = Substitute.For<IAvatarAnchorPoints>();
            anchorPoints.GetTransform(Arg.Any<AvatarAnchorPointIds>()).Returns((targetPosition, targetRotation, Vector3.one));

            DataStore.i.player.otherPlayers.Add(userId, new Player() { id = userId, anchorPoints = anchorPoints });

            AvatarAttachHandler handler = Substitute.ForPartsOf<AvatarAttachHandler>();
            handler.IsInsideScene(Arg.Any<Vector3>()).Returns(true);

            handler.Initialize(Substitute.For<IParcelScene>(), entity, Substitute.For<IUpdateEventHandler>());
            handler.OnModelUpdated(new AvatarAttachComponent.Model() { avatarId = userId, anchorPointId = 0 });
            handler.LateUpdate();

            Assert.AreEqual(targetPosition, entityGo.transform.position);
            Assert.AreEqual(targetRotation.eulerAngles, entityGo.transform.rotation.eulerAngles);

            handler.Dispose();
            Object.Destroy(entityGo);
        }

        [Test]
        public void SendEntityToMORDORwhenOutOfScene()
        {
            const string userId = "Temptation";
            Vector3 targetPosition = new Vector3(70, -135, 0);
            Quaternion targetRotation = Quaternion.Euler(0, 30, 0);

            GameObject entityGo = new GameObject("AvatarAttachHandlerShould");
            entityGo.transform.position = Vector3.zero;
            entityGo.transform.rotation = Quaternion.identity;

            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.gameObject.Returns(entityGo);

            IAvatarAnchorPoints anchorPoints = Substitute.For<IAvatarAnchorPoints>();
            anchorPoints.GetTransform(Arg.Any<AvatarAnchorPointIds>()).Returns((targetPosition, targetRotation, Vector3.one));

            DataStore.i.player.otherPlayers.Add(userId, new Player() { id = userId, anchorPoints = anchorPoints });

            AvatarAttachHandler handler = Substitute.ForPartsOf<AvatarAttachHandler>();
            handler.IsInsideScene(Arg.Any<Vector3>()).Returns(false);

            handler.Initialize(Substitute.For<IParcelScene>(), entity, Substitute.For<IUpdateEventHandler>());
            handler.OnModelUpdated(new AvatarAttachComponent.Model() { avatarId = userId, anchorPointId = 0 });
            handler.LateUpdate();

            Assert.AreEqual(EnvironmentSettings.MORDOR, entityGo.transform.position);

            handler.Dispose();
            Object.Destroy(entityGo);
        }
    }
}