using System;
using DCL;
using DCL.Components;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AvatarAttach_Tests
{
    public class AvatarAttachHandlerShould
    {
        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
        }

        [Test]
        public void DoNotDetachOrAttachIfIdMatchPreviousModel()
        {
            AvatarAttachHandler handler = Substitute.ForPartsOf<AvatarAttachHandler>();
            handler.prevModel = new AvatarAttachComponent.Model() { avatarId = "Temptation" };
            handler.OnModelUpdated(new AvatarAttachComponent.Model() { avatarId = "Temptation" });
            handler.DidNotReceive().Detach();
            handler.DidNotReceive().Attach(Arg.Any<string>(), Arg.Any<AvatarAnchorPointIds>());
            handler.CleanUp();
        }

        [Test]
        public void AttachWhenValidUserId()
        {
            AvatarAttachHandler handler = Substitute.ForPartsOf<AvatarAttachHandler>();

            handler.OnModelUpdated(new AvatarAttachComponent.Model() { avatarId = "Temptation" });
            handler.Received(1).Attach(Arg.Any<string>(), Arg.Any<AvatarAnchorPointIds>());
            handler.CleanUp();
        }

        [Test]
        public void OnlyDetachWhenInvalidUserId()
        {
            AvatarAttachHandler handler = Substitute.ForPartsOf<AvatarAttachHandler>();

            handler.OnModelUpdated(new AvatarAttachComponent.Model() { avatarId = "" });
            handler.Received(1).Detach();
            handler.DidNotReceive().Attach(Arg.Any<string>(), Arg.Any<AvatarAnchorPointIds>());
            handler.CleanUp();
        }

        [Test]
        public void DetachWhenUserDisconnect()
        {
            const string userId = "Temptation";

            DataStore.i.player.otherPlayers.Add(userId, new Player() { id = userId });
            AvatarAttachHandler handler = Substitute.ForPartsOf<AvatarAttachHandler>();

            handler.OnModelUpdated(new AvatarAttachComponent.Model() { avatarId = userId });
            handler.Received(1).Detach();

            DataStore.i.player.otherPlayers.Remove(userId);
            handler.Received(2).Detach();
            handler.CleanUp();
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
            handler.CleanUp();
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            const string userId = "Temptation";

            GameObject entityGo = new GameObject("AvatarAttachHandlerShould");
            entityGo.transform.position = Vector3.zero;
            entityGo.transform.rotation = Quaternion.identity;

            // NOTE: can't mock using entity.gameObject.Returns(entityGo) for some reason NSubstitute goes
            // crazy when running in the test queue but works ok when running it isolated
            DecentralandEntity entity = new DecentralandEntity { gameObject = entityGo };

            // NOTE: can't mock using NSubstitute. for some reason NSubstitute goes
            // crazy when running in the test queue but works ok when running it isolated
            IAvatarAnchorPoints anchorPoints = new AvatarAnchorPoints_Mock();

            DataStore.i.player.otherPlayers.Add(userId, new Player() { id = userId, anchorPoints = anchorPoints });

            AvatarAttachHandler handler = new AvatarAttachHandler();
            handler.Initialize(null, entity);
            handler.OnModelUpdated(new AvatarAttachComponent.Model() { avatarId = userId, anchorPointId = 0 });

            Assert.AreEqual(AvatarAnchorPoints_Mock.targetPosition, entityGo.transform.position);
            Assert.AreEqual(AvatarAnchorPoints_Mock.targetRotation.eulerAngles, entityGo.transform.rotation.eulerAngles);

            handler.CleanUp();
            Object.Destroy(entityGo);
        }

        class AvatarAnchorPoints_Mock : IAvatarAnchorPoints
        {
            public static readonly Vector3 targetPosition = new Vector3(70, -135, 0);
            public static readonly Quaternion targetRotation = Quaternion.Euler(0, 30, 0);

            void IAvatarAnchorPoints.Prepare(Transform avatarTransform, Transform[] bones, float nameTagY)
            {
                throw new NotImplementedException();
            }
            (Vector3 position, Quaternion rotation, Vector3 scale) IAvatarAnchorPoints.GetTransfom(AvatarAnchorPointIds anchorPointId)
            {
                return (targetPosition, targetRotation, Vector3.one);
            }
        }
    }
}