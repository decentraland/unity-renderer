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
        private BaseRefCounter<AvatarModifierAreaID> warningMessageList => DataStore.i.HUDs.avatarAreaWarnings;

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
            hudController.view.Received().SetUp(warningMessageList);
        }
        

        [TearDown]
        protected void TearDown()
        {
            hudController.Dispose();
        }

    }
}
