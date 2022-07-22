using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.AvatarModifierAreaFeedback;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.AvatarModifierAreaFeedback
{
    public class AvatarModifierAreaFeedbackViewShould
    {
    
        private AvatarModifierAreaFeedbackView hudView;
        private BaseRefCounter<AvatarAreaWarningID> warningMessageList => DataStore.i.HUDs.avatarAreaWarnings;

        [SetUp]
        public void SetUp()
        {
            hudView = AvatarModifierAreaFeedbackView.Create();
            hudView.SetUp(warningMessageList);
        }
        
        [Test]
        public void ShowsProperly()
        {
            warningMessageList.Clear();
            warningMessageList.AddRefCount(AvatarAreaWarningID.HIDE_AVATAR);
            Assert.True(hudView.isVisible);
        }
        
        [Test]
        public void HidesProperly()
        {
            warningMessageList.Clear();
            warningMessageList.AddRefCount(AvatarAreaWarningID.HIDE_AVATAR);
            warningMessageList.AddRefCount(AvatarAreaWarningID.HIDE_AVATAR);
            warningMessageList.AddRefCount(AvatarAreaWarningID.DISABLE_PASSPORT);
            Assert.True(hudView.isVisible);
            warningMessageList.RemoveRefCount(AvatarAreaWarningID.HIDE_AVATAR);
            warningMessageList.RemoveRefCount(AvatarAreaWarningID.HIDE_AVATAR);
            Assert.True(hudView.isVisible);
            warningMessageList.RemoveRefCount(AvatarAreaWarningID.DISABLE_PASSPORT);
            Assert.False(hudView.isVisible);
        }
        
        
        [Test]
        public void CheckPointerEnter()
        {
            warningMessageList.Clear();
            Assert.AreEqual(AvatarModifierAreaFeedbackView.AvatarModifierAreaFeedbackState.NEVER_SHOWN, hudView.currentState);
            warningMessageList.AddRefCount(AvatarAreaWarningID.HIDE_AVATAR);
            hudView.OnPointerEnter(null);
            Assert.AreEqual(AvatarModifierAreaFeedbackView.AvatarModifierAreaFeedbackState.WARNING_MESSAGE_VISIBLE,hudView.currentState);
        }
        
        [Test]
        public void CheckPointerExit()
        {
            warningMessageList.Clear();
            warningMessageList.AddRefCount(AvatarAreaWarningID.HIDE_AVATAR);
            hudView.OnPointerExit(null);
            Assert.AreEqual(AvatarModifierAreaFeedbackView.AvatarModifierAreaFeedbackState.ICON_VISIBLE, hudView.currentState);
        }
        
        [TearDown]
        protected void TearDown()
        {
            hudView.Dispose();
            GameObject.Destroy(hudView);
        }

    }
}
