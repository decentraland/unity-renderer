using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using System;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class SendFriendRequestHUDComponentViewShould
    {
        private SendFriendRequestHUDComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = SendFriendRequestHUDComponentView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            view.Dispose();
        }

        [Test]
        public void Show()
        {
            view.Show();

            Assert.IsTrue(view.gameObject.activeSelf);
            Assert.IsTrue(view.defaultContainer.activeSelf);
            Assert.IsFalse(view.failedContainer.activeSelf);
            Assert.IsFalse(view.successContainer.activeSelf);
            Assert.IsFalse(view.pendingToSendContainer.activeSelf);
        }

        [Test]
        public void ShowFailed()
        {
            view.ShowSendFailed();

            Assert.IsFalse(view.defaultContainer.activeSelf);
            Assert.IsTrue(view.failedContainer.activeSelf);
            Assert.IsFalse(view.successContainer.activeSelf);
            Assert.IsFalse(view.pendingToSendContainer.activeSelf);
        }

        [Test]
        public void ShowSuccess()
        {
            view.ShowSendSuccess();

            Assert.IsFalse(view.defaultContainer.activeSelf);
            Assert.IsFalse(view.failedContainer.activeSelf);
            Assert.IsTrue(view.successContainer.activeSelf);
            Assert.IsFalse(view.pendingToSendContainer.activeSelf);
        }

        [Test]
        public void ShowPending()
        {
            view.ShowPendingToSend();

            Assert.IsFalse(view.defaultContainer.activeSelf);
            Assert.IsFalse(view.failedContainer.activeSelf);
            Assert.IsFalse(view.successContainer.activeSelf);
            Assert.IsTrue(view.pendingToSendContainer.activeSelf);
        }

        [Test]
        public void Close()
        {
            view.Close();

            Assert.IsFalse(view.gameObject.activeSelf);
        }

        [TestCase("bleh")]
        [TestCase("woah")]
        public void SetName(string expectedName)
        {
            view.SetName(expectedName);

            Assert.AreEqual(expectedName, view.nameLabel.text);
            Assert.AreEqual($"Sending friend request to {expectedName}", view.pendingStateLabel.text);
            Assert.AreEqual($"Friend request sent to {expectedName}", view.successStateLabel.text);
        }

        [Test]
        public void SetProfilePicture()
        {
            ILazyTextureObserver textureObserver = Substitute.For<ILazyTextureObserver>();

            view.SetProfilePicture(textureObserver);

            textureObserver.Received(1).AddListener(Arg.Any<Action<Texture2D>>());
        }

        [Test]
        public void ChangeProfilePicture()
        {
            ILazyTextureObserver obs1 = Substitute.For<ILazyTextureObserver>();
            view.SetProfilePicture(obs1);
            ILazyTextureObserver obs2 = Substitute.For<ILazyTextureObserver>();
            view.SetProfilePicture(obs2);
            ILazyTextureObserver obs3 = Substitute.For<ILazyTextureObserver>();
            view.SetProfilePicture(obs3);

            obs1.Received(1).AddListener(Arg.Any<Action<Texture2D>>());
            obs2.Received(1).AddListener(Arg.Any<Action<Texture2D>>());
            obs3.Received(1).AddListener(Arg.Any<Action<Texture2D>>());
        }

        [Test]
        public void AvoidSubscriptionToProfilePictureManyTimes()
        {
            ILazyTextureObserver observer = Substitute.For<ILazyTextureObserver>();
            view.SetProfilePicture(observer);
            view.SetProfilePicture(observer);
            view.SetProfilePicture(observer);

            observer.Received(1).AddListener(Arg.Any<Action<Texture2D>>());
        }
    }
}
