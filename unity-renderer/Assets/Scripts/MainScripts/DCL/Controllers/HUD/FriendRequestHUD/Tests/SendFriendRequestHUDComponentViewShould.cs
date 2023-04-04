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

            Assert.IsTrue(view.showHideAnimatorForDefaultState.isVisible);
            Assert.IsFalse(view.pendingToSendContainer.activeSelf);
            Assert.IsFalse(view.showHideAnimatorForSuccessState.isVisible);
        }

        [Test]
        public void ShowSuccess()
        {
            view.ShowSendSuccess();

            Assert.IsFalse(view.showHideAnimatorForDefaultState.isVisible);
            Assert.IsFalse(view.pendingToSendContainer.activeSelf);
            Assert.IsTrue(view.showHideAnimatorForSuccessState.isVisible);
        }

        [Test]
        public void ShowPending()
        {
            view.ShowPendingToSend();

            Assert.IsTrue(view.showHideAnimatorForDefaultState.isVisible);
            Assert.IsTrue(view.pendingToSendContainer.activeSelf);
            Assert.IsFalse(view.showHideAnimatorForSuccessState.isVisible);
        }

        [TestCase("bleh")]
        [TestCase("woah")]
        public void SetName(string expectedName)
        {
            view.SetName(expectedName);

            Assert.AreEqual(expectedName, view.nameLabel.text);
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

        [TestCase("hey", "3/140")]
        [TestCase("whassah", "7/140")]
        public void ShowBodyMessageLengthWhenInputChanges(string message, string expectedLengthText)
        {
            view.messageBodyInput.onValueChanged.Invoke(message);
            Assert.AreEqual(expectedLengthText, view.messageBodyLengthLabel.text);
        }
    }
}
