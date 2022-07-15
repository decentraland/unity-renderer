using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.AvatarModifierAreaFeedback;
using NSubstitute;
using NUnit.Framework;
using Tests;
using UnityEditor;
using UnityEngine;

namespace Tests.AvatarModifierAreaFeedback
{
    public class AvatarModifierAreaFeedbackViewShould
    {
    
        private AvatarModifierAreaFeedbackView hudView;
        private List<string> mockWarningMessages;

        [SetUp]
        public void SetUp()
        {
            hudView = AvatarModifierAreaFeedbackView.Create();
            mockWarningMessages = new List<string>();
            mockWarningMessages.Add("MOCK_MESSAGE_1");
        }
        
        [Test]
        public void InitializeProperly()
        {
            Assert.AreEqual(hudView.currentState, AvatarModifierAreaFeedbackView.AvatarModifierAreaFeedbackState.NEVER_SHOWN);
        }
        
        [Test]
        public void IsVisible()
        {
            hudView.SetWarningMessage(mockWarningMessages);
            hudView.SetVisibility(true);
            Assert.True(hudView.isVisible);
            
            hudView.ResetWarningMessage();
            hudView.SetVisibility(false);
            Assert.False(hudView.isVisible);
        }
        
        [Test]
        public void CheckPointerEnter()
        {
            hudView.SetWarningMessage(mockWarningMessages);
            hudView.SetVisibility(true);
            hudView.OnPointerEnter(null);
            Assert.True(hudView.isVisible);
        }
        
        [Test]
        public void CheckPointerExit()
        {
            hudView.OnPointerExit(null);
            hudView.SetVisibility(false);
            Assert.False(hudView.isVisible);
        }
        
        [Test]
        public void CheckMessageDescription()
        {
            hudView.SetWarningMessage(mockWarningMessages);
            Assert.AreEqual(hudView.descriptionText.text, mockWarningMessages[0] + "\n");
        }
        
        

        [TearDown]
        protected void TearDown()
        {
            hudView.Dispose();
        }

    }
}
