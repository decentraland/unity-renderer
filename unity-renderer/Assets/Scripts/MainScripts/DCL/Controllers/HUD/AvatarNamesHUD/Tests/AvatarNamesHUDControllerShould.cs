using System;
using AvatarNamesHUD;
using DCL;
using NSubstitute;
using NUnit.Framework;

namespace Tests.AvatarNamessHUD
{
    public class AvatarNamessHUDControllerShould
    {
        private AvatarNamesHUDController hudController;
        private IAvatarNamesHUDView hudView;
        private IHUD avatarEditorHUD;
        private BaseVariable<bool> signupVisible => DataStore.i.HUDs.signupVisible;

        [SetUp]
        public void SetUp()
        {
            hudView = Substitute.For<IAvatarNamesHUDView>();
            avatarEditorHUD = Substitute.For<IHUD>();
            hudController = Substitute.ForPartsOf<AvatarNamesHUDController>();
            //hudController.Configure().CreateView().Returns(info => hudView);
            //hudController.Initialize(avatarEditorHUD);
        }

        [Test]
        public void InitializeProperly()
        {
            //Assert.AreEqual(hudView, hudController.view);
            //Assert.AreEqual(avatarEditorHUD, hudController.avatarEditorHUD);
            Assert.IsFalse(signupVisible.Get());
        }

        [TearDown]
        public void TearDown() { DataStore.Clear(); }
    }
}