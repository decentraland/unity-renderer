using DCL;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using GotoPanel;

namespace Test.GotoPanel
{
    public class GotoPanelControllerShould
    {
        private GotoPanelHUDController hudController;
        private IGotoPanelHUDView hudView;
        private BaseVariable<bool> visible => DataStore.i.HUDs.gotoPanelVisible;
        private BaseVariable<ParcelCoordinates> coords => DataStore.i.HUDs.gotoPanelCoordinates;

        [SetUp]
        public void SetUp()
        {
            hudView = Substitute.For<IGotoPanelHUDView>();
            hudController = Substitute.ForPartsOf<GotoPanelHUDController>();
            hudController.Configure().CreateView().Returns(info => hudView);
            hudController.Initialize();
        }

        [Test]
        public void Initialize()
        {
            Assert.AreEqual(hudView, hudController.view);
        }

        [Test]
        public void GetVisibleValueChangeToTrue()
        {
            visible.Set(true, true);
            hudView.Received().SetVisible(true);
        }

        [Test]
        public void GetVisibleValueChangeToFalse()
        {
            visible.Set(false, true);
            hudView.Received().SetVisible(false);
        }

        [Test]
        public void GetCoordinatesValueChange() 
        {
            ParcelCoordinates gotoCoords = new ParcelCoordinates(10,30);
            coords.Set(gotoCoords);
            hudView.Received().SetPanelInfo(gotoCoords);
        }

        [TearDown]
        public void TearDown() { DataStore.Clear(); }
    }
}