using System.Collections;
using System.Collections.Generic;
using DCL.AvatarModifierAreaFeedback;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.AvatarModifierAreaFeedback
{
    public class AvatarModifierAreaFeedbackViewShould
    {
    
        private AvatarModifierAreaFeedbackView hudView;
        private List<string> warningMessagesMock;

        [SetUp]
        public void SetUp()
        {
            hudView = AvatarModifierAreaFeedbackView.Create();
            warningMessagesMock = new List<string>() { "MOCK_WARNING_1", "MOCK_WARNING_2" };
        }
        
        [Test]
        public void IsVisible()
        {
            hudView.SetWarningMessage(warningMessagesMock);
            hudView.SetVisibility(true);
            Assert.True(hudView.isVisible);
        }
        
        [Test]
        public void CheckPointerEnter()
        {
            Assert.AreEqual(AvatarModifierAreaFeedbackView.AvatarModifierAreaFeedbackState.NEVER_SHOWN, hudView.currentState);
            hudView.SetWarningMessage(warningMessagesMock);
            hudView.SetVisibility(true);
            hudView.OnPointerEnter(null);
            Assert.AreEqual(AvatarModifierAreaFeedbackView.AvatarModifierAreaFeedbackState.WARNING_MESSAGE_VISIBLE,hudView.currentState);
        }
        
        [Test]
        public void CheckPointerExit()
        {
            hudView.SetWarningMessage(warningMessagesMock);
            hudView.SetVisibility(true);
            hudView.OnPointerExit(null);
            Assert.AreEqual(AvatarModifierAreaFeedbackView.AvatarModifierAreaFeedbackState.ICON_VISIBLE, hudView.currentState);
        }
        
        [Test]
        public void CheckMessageDescription()
        {
            string testDescription = "";
            foreach (string newAvatarModifierWarning in warningMessagesMock)
            {
                testDescription += newAvatarModifierWarning + "\n";
            }
            
            hudView.SetWarningMessage(warningMessagesMock);
            Assert.AreEqual(hudView.descriptionText.text, testDescription);
            
            warningMessagesMock.Add("MOCK_WARNING_1");
            hudView.SetWarningMessage(warningMessagesMock);
            Assert.AreEqual(hudView.descriptionText.text, testDescription);
        }

        [TearDown]
        protected void TearDown()
        {
            hudView.Dispose();
            GameObject.Destroy(hudView);
        }

    }
}
