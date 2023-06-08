using NUnit.Framework;
using DCL.HelpAndSupportHUD;
using NSubstitute;
using Analytics;
using System;

namespace Tests
{
    public class HelpAndSupportHUDControllerShould
    {
        private HelpAndSupportHUDController controller;
        private IHelpAndSupportHUDView view;

        [SetUp]
        protected void SetUp()
        {
            view = Substitute.For<IHelpAndSupportHUDView>();
            controller = new HelpAndSupportHUDController(view, Substitute.For<ISupportAnalytics>());
        }

        [TearDown]
        protected void TearDown()
        {
            controller.Dispose();
        }


        [Test]
        public void ShowViewProperly()
        {
            controller.SetVisibility(true);
            view.Received().SetVisibility(true);
        }

        [Test]
        public void HideViewProperly()
        {
            controller.SetVisibility(false);
            view.Received().SetVisibility(false);
        }
    }
}
