using DCL;
using LoadingHUD;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;

namespace Tests.LoadingHUD
{
    public class LoadingHUDControllerShould
    {
        private LoadingHUDController hudController;
        private ILoadingHUDView hudView;
        private BaseVariable<bool> visible => DataStore.i.HUDs.loadingHUD.visible;
        private BaseVariable<string> message => DataStore.i.HUDs.loadingHUD.message;
        private BaseVariable<float> percentage => DataStore.i.HUDs.loadingHUD.percentage;
        private BaseVariable<bool> showTips => DataStore.i.HUDs.loadingHUD.showTips;

        [SetUp]
        public void SetUp()
        {
            hudView = Substitute.For<ILoadingHUDView>();
            hudController = Substitute.ForPartsOf<LoadingHUDController>();
            hudController.Configure().CreateView().Returns(info => hudView);
            hudController.Initialize();
        }

        [Test]
        public void InitializeProperly()
        {
            Assert.AreEqual(hudView, hudController.view);
        }

        [Test]
        public void ReactToLoadingHUDVisibleTrue()
        {
            visible.Set(true, true); //Force event notification
            hudView.Received().SetVisible(true);
        }

        [Test]
        public void ReactToLoadingHUDVisibleFalse()
        {
            visible.Set(false, true); //Force event notification
            hudView.Received().SetVisible(false);
        }

        [Test]
        public void ReactToMessageChanged()
        {
            message.Set("new_message", true); //Force event notification
            hudView.Received().SetMessage("new_message");
        }

        [Test]
        public void ReactToPercentageChanged()
        {
            percentage.Set(0.7f, true); //Force event notification
            hudView.Received().SetPercentage(0.7f / 100);
        }

        [Test]
        public void ReactToShowTipsChanged()
        {
            showTips.Set(false, true); //Force event notification
            hudView.Received().SetTips(false);
        }

        [TearDown]
        public void TearDown() { DataStore.Clear(); }
    }
}