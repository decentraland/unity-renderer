using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Friends
{
    public class CancelFriendRequestHUDComponentViewShould
    {
        private SentFriendRequestHUDComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = SentFriendRequestHUDComponentView.Create();
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

            Assert.IsTrue(view.cancelButton.interactable);

            foreach (Button button in view.closeButtons)
                Assert.IsTrue(button.interactable);
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
            var testTimestamp = DateTime.Now;
            view.SetTimestamp(testTimestamp);

            Assert.AreEqual(testTimestamp.Date.ToString("MMM dd", new CultureInfo("en-US")).ToUpper(), view.dateLabel.text);
        }
    }
}
