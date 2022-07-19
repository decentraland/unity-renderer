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
        private BaseStack<List<IAvatarModifier>> warningMessagesStack => DataStore.i.HUDs.inAvatarModifierStackWarnings;
        private List<IAvatarModifier> mockAvatarModifiers;

        [SetUp]
        public void SetUp()
        {
            hudView = Substitute.For<IAvatarModifierAreaFeedbackView>();
            hudController = new AvatarModifierAreaFeedbackController(warningMessagesStack, hudView);
            mockAvatarModifiers = new List<IAvatarModifier>();
            mockAvatarModifiers.Add(new HideAvatarsModifier());
            mockAvatarModifiers.Add(new DisablePassportModifier());
        }
        
        [Test]
        public void InitializeProperly()
        {
            Assert.AreEqual(hudView, hudController.view);
        }
        
        [Test]
        public void EntryAndExitOfAvatar()
        {
            warningMessagesStack.Add(mockAvatarModifiers);
            hudController.view.Received().SetWarningMessage(mockAvatarModifiers);
            warningMessagesStack.Remove(mockAvatarModifiers);
            hudController.view.Received().ResetWarningMessage();
        }

        [TearDown]
        protected void TearDown()
        {
            hudController.Dispose();
        }

    }
}
