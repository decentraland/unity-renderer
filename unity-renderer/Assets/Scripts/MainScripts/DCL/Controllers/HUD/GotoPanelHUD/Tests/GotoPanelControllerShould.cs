using DCL.Map;
using NSubstitute;
using NUnit.Framework;

namespace DCL.GoToPanel
{
    public class GotoPanelControllerShould
    {
        private GotoPanelHUDController controller;
        private IGotoPanelHUDView view;
        private DataStore dataStore;
        private BaseVariable<bool> visible => dataStore.HUDs.gotoPanelVisible;
        private BaseVariable<ParcelCoordinates> coords => dataStore.HUDs.gotoPanelCoordinates;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IGotoPanelHUDView>();

            dataStore = new DataStore();

            controller = new GotoPanelHUDController(view, dataStore, Substitute.For<ITeleportController>(),
                Substitute.For<IMinimapApiBridge>());
        }

        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
        }

        [Test]
        public void GetVisibleValueChangeToTrue()
        {
            visible.Set(true, true);
            view.Received().SetVisible(true);
        }

        [Test]
        public void GetVisibleValueChangeToFalse()
        {
            visible.Set(false, true);
            view.Received().SetVisible(false);
        }

        [Test]
        public void GetCoordinatesValueChange()
        {
            ParcelCoordinates gotoCoords = new ParcelCoordinates(10, 30);
            coords.Set(gotoCoords);
            view.Received().SetPanelInfo(gotoCoords);
        }
    }
}
