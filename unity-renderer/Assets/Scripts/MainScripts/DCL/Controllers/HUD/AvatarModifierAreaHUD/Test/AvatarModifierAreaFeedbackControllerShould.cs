using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.AvatarModifierAreaFeedback;
using NSubstitute;
using NUnit.Framework;
using Tests;
using UnityEngine;

namespace Tests.AvatarModifierAreaFeedback
{
    public class AvatarModifierAreaFeedbackControllerShould
    {
    
        private AvatarModifierAreaFeedbackController hudController;
        private IAvatarModifierAreaFeedbackView hudView;
        private BaseStack<List<string>> warningMessagesStack => DataStore.i.HUDs.inAvatarModifierStackWarnings;
        private List<string> mockWarningMessages;

        [SetUp]
        public void SetUp()
        {
            hudView = Substitute.For<IAvatarModifierAreaFeedbackView>();
            hudController = new AvatarModifierAreaFeedbackController(warningMessagesStack, hudView);
            mockWarningMessages = new List<string>();
            mockWarningMessages.Add("MOCK_MESSAGE_1");
            mockWarningMessages.Add("MOCK_MESSAGE_2");
        }
        
        [Test]
        public void InitializeProperly()
        {
            Assert.AreEqual(hudView, hudController.view);
        }
        
        [Test]
        public void EntryAndExitOfAvatar()
        {
            warningMessagesStack.Add(mockWarningMessages);
            hudController.view.Received().SetWarningMessage(mockWarningMessages);
            warningMessagesStack.Remove(mockWarningMessages);
            hudController.view.Received().ResetWarningMessage();
        }

        [TearDown]
        protected void TearDown()
        {
            hudController.Dispose();
        }

    }
}
