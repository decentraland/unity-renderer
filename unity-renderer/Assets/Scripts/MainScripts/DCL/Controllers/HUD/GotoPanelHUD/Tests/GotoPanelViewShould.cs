using UnityEngine;
using NUnit.Framework;
using GotoPanel;

namespace Test.GotoPanel
{
    public class GotoPanelViewShould
    {
        private GotoPanelHUDView hudView;

        [SetUp]
        public void SetUp() { hudView = Object.Instantiate(Resources.Load<GameObject>("GotoPanelHUD")).GetComponent<GotoPanelHUDView>(); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityProperly(bool visible)
        {
            hudView.SetVisible(visible);
            Assert.AreEqual(visible, hudView.container.gameObject.activeSelf);
        }

        [Test]
        public void SetParcelCoordinatesProperly() 
        {
            ParcelCoordinates coords = new ParcelCoordinates(1, 2);
            hudView.SetPanelInfo(coords);
            Assert.AreEqual(coords, hudView.targetCoordinates);
        }
    }
}