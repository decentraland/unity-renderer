using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.AvatarModifierAreaFeedback;
using NUnit.Framework;
using Tests;
using UnityEngine;

namespace  AvatarModifierAreaFeedback_Test
{
    public class AvatarModifierAreaFeedbackShould 
    {
        private AvatarModifierAreaFeedbackController controller;

        [SetUp]
        protected void SetUp()
        {
            controller = new AvatarModifierAreaFeedbackController();
        }
        
        [Test]
        public void CreateView()
        {
            Assert.NotNull(controller.view);
            Assert.NotNull(controller.view.gameObject);
        }
        
        [Test]
        public void EntryAndExitOfAvatar()
        {
            DataStore.i.HUDs.inAvatarModifierAreaForSelfCounter.Set(1);
            Assert.True(controller.view.isVisible);
            DataStore.i.HUDs.inAvatarModifierAreaForSelfCounter.Set(0);
            Assert.False(controller.view.isVisible);
        }
        
        [Test]
        public void AddNewWarningMessage()
        {
            string warningMessageToTest = "Warning Message";
            DataStore.i.HUDs.inAvatarModifierAreaWarningDescription.Add(warningMessageToTest);
            Assert.AreEqual(warningMessageToTest, controller.view.descriptionText.text);
        }
        
        [Test]
        public void RemoveAllWarningMessages()
        {
            DataStore.i.HUDs.inAvatarModifierAreaWarningDescription.Set(new List<string>());
            Assert.AreEqual("", controller.view.descriptionText.text);
        }
        
        [TearDown]
        protected void TearDown()
        {
            controller.Dispose();
        }

    }

}