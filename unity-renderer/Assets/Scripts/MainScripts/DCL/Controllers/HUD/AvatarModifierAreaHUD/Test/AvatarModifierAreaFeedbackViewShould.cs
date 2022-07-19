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
        private List<IAvatarModifier> mockAvatarModifiers;

        [SetUp]
        public void SetUp()
        {
            hudView = AvatarModifierAreaFeedbackView.Create();
            mockAvatarModifiers = new List<IAvatarModifier>();
            mockAvatarModifiers.Add(new HideAvatarsModifier());
        }
        
        [Test]
        public void InitializeProperly()
        {
            Assert.AreEqual(hudView.currentState, AvatarModifierAreaFeedbackView.AvatarModifierAreaFeedbackState.NEVER_SHOWN);
        }
        
        [Test]
        public void IsVisible()
        {
            hudView.SetWarningMessage(mockAvatarModifiers);
            hudView.SetVisibility(true);
            Assert.True(hudView.isVisible);
            
            hudView.ResetWarningMessage();
            hudView.SetVisibility(false);
            Assert.False(hudView.isVisible);
        }
        
        [Test]
        public void CheckPointerEnter()
        {
            hudView.SetWarningMessage(mockAvatarModifiers);
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
            hudView.SetWarningMessage(mockAvatarModifiers);
            Assert.AreEqual(hudView.descriptionText.text, mockAvatarModifiers[0].GetWarningDescription() + "\n");
        }

        [TearDown]
        protected void TearDown()
        {
            hudView.Dispose();
            GameObject.Destroy(hudView);
        }

    }
}
