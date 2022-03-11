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
    }
}