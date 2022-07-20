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
        private BaseCollection<string> warningMessageList => DataStore.i.HUDs.inAvatarModifierStackWarnings;

        [SetUp]
        public void SetUp()
        {
            hudView = Substitute.For<IAvatarModifierAreaFeedbackView>();
            hudController = new AvatarModifierAreaFeedbackController(warningMessageList, hudView);
        }
        
        [Test]
        public void InitializeProperly()
        {
            Assert.AreEqual(hudView, hudController.view);
        }
        
        [Test]
        public void EntryAndExitOfAvatar()
        {
            warningMessageList.Add("MOCK_WARNING_1");
            hudController.view.Received().SetWarningMessage(warningMessageList.Get());
            hudController.view.Received().SetVisibility(true);
            
            warningMessageList.Add("MOCK_WARNING_2");
            hudController.view.Received().SetWarningMessage(warningMessageList.Get());
            
            warningMessageList.Remove("MOCK_WARNING_1");
            hudController.view.Received().SetWarningMessage(warningMessageList.Get());
            
            warningMessageList.Remove("MOCK_WARNING_2");
            hudController.view.Received().SetWarningMessage(warningMessageList.Get());
            hudController.view.Received().SetVisibility(false);
        }

        [TearDown]
        protected void TearDown()
        {
            hudController.Dispose();
        }

    }
}
