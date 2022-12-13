using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Friends
{
    public class CancelFriendRequestHUDComponentViewShould
    {
        private CancelFriendRequestHUDComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = CancelFriendRequestHUDComponentView.Create();
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
            Assert.IsTrue(view.cancelButton.interactable);

            foreach (Button button in view.closeButtons)
                Assert.IsTrue(button.interactable);
        }

        [Test]
        public void Close()
        {
            view.Close();

            Assert.IsFalse(view.gameObject.activeSelf);
        }

        [Test]
        public void SetRecipientName()
        {
            view.SetRecipientName("bleh");

            Assert.AreEqual("bleh", view.nameLabel.text);
        }

        [Test]
        public void SetValidBodyMessage()
        {
            view.SetBodyMessage("hey");

            Assert.AreEqual("hey", view.messageBodyInput.text);
            Assert.IsTrue(view.bodyMessageContainer.activeSelf);
        }

        [Test]
        public void SetInvalidBodyMessage()
        {
            view.SetBodyMessage("");

            Assert.AreEqual("", view.messageBodyInput.text);
            Assert.IsFalse(view.bodyMessageContainer.activeSelf);
        }

        [Test]
        public void ShowPendingState()
        {
            view.ShowPendingToCancel();

            Assert.IsTrue(view.defaultContainer.activeSelf);
            Assert.IsFalse(view.failedContainer.activeSelf);
            Assert.IsFalse(view.cancelButton.interactable);

            foreach (Button button in view.closeButtons)
                Assert.IsFalse(button.interactable);
        }

        [Test]
        public void ChangeRecipientProfilePicture()
        {
            ILazyTextureObserver obs1 = Substitute.For<ILazyTextureObserver>();
            view.SetRecipientProfilePicture(obs1);
            ILazyTextureObserver obs2 = Substitute.For<ILazyTextureObserver>();
            view.SetRecipientProfilePicture(obs2);
            ILazyTextureObserver obs3 = Substitute.For<ILazyTextureObserver>();
            view.SetRecipientProfilePicture(obs3);

            obs1.Received(1).AddListener(Arg.Any<Action<Texture2D>>());
            obs2.Received(1).AddListener(Arg.Any<Action<Texture2D>>());
            obs3.Received(1).AddListener(Arg.Any<Action<Texture2D>>());
        }

        [Test]
        public void AvoidSubscriptionToRecipientProfilePictureManyTimes()
        {
            ILazyTextureObserver observer = Substitute.For<ILazyTextureObserver>();
            view.SetRecipientProfilePicture(observer);
            view.SetRecipientProfilePicture(observer);
            view.SetRecipientProfilePicture(observer);

            observer.Received(1).AddListener(Arg.Any<Action<Texture2D>>());
        }

        [Test]
        public void ChangeSenderProfilePicture()
        {
            ILazyTextureObserver obs1 = Substitute.For<ILazyTextureObserver>();
            view.SetSenderProfilePicture(obs1);
            ILazyTextureObserver obs2 = Substitute.For<ILazyTextureObserver>();
            view.SetSenderProfilePicture(obs2);
            ILazyTextureObserver obs3 = Substitute.For<ILazyTextureObserver>();
            view.SetSenderProfilePicture(obs3);

            obs1.Received(1).AddListener(Arg.Any<Action<Texture2D>>());
            obs2.Received(1).AddListener(Arg.Any<Action<Texture2D>>());
            obs3.Received(1).AddListener(Arg.Any<Action<Texture2D>>());
        }

        [Test]
        public void AvoidSubscriptionToSenderProfilePictureManyTimes()
        {
            ILazyTextureObserver observer = Substitute.For<ILazyTextureObserver>();
            view.SetSenderProfilePicture(observer);
            view.SetSenderProfilePicture(observer);
            view.SetSenderProfilePicture(observer);

            observer.Received(1).AddListener(Arg.Any<Action<Texture2D>>());
        }

        [Test]
        public void SetTimestamp()
        {
            view.SetTimestamp(new DateTime(2022, 1, 14));

            Assert.AreEqual("January 14", view.dateLabel.text);
        }
    }
}
