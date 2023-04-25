using System.Collections;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using RPC.Context;
using UnityEngine.TestTools;

namespace Tests
{
    public class NFTPromptHUDTests
    {
        private NFTPromptHUDController controller;
        private NFTPromptHUDView view;
        private RestrictedActionsContext restrictedActionsContext;

        [SetUp]
        public void SetUp()
        {
            restrictedActionsContext = new RestrictedActionsContext();
            controller = new NFTPromptHUDController(restrictedActionsContext, new BaseVariable<NFTPromptModel>());
            view = (NFTPromptHUDView)controller.view;
        }

        [TearDown]
        public void TearDown() { controller.Dispose(); }

        [Test]
        public void CreateView()
        {
            Assert.NotNull(view);
            Assert.NotNull(view.gameObject);
        }

        [Test]
        public void OpenAndCloseCorrectly()
        {
            controller.OpenNftInfoDialog(new NFTPromptModel("0xf64dc33a192e056bb5f0e5049356a0498b502d50",
                "2481", null), null);
            Assert.IsTrue(view.content.activeSelf, "NFT dialog should be visible");
            view.buttonClose.onClick.Invoke();
            Assert.IsFalse(view.content.activeSelf, "NFT dialog should not be visible");
        }

        [Test]
        public void OwnersTooltipSetupCorrectly()
        {
            var tooltip = view.ownersTooltip;
            Assert.AreEqual(0, tooltip.ownerElementsContainer.childCount);
        }

        [Test]
        public void OwnersTooltipShowCorrectly()
        {
            var tooltip = view.ownersTooltip;
            IOwnersTooltipView tooltipView = tooltip;

            List<IOwnerInfoElement> owners = new List<IOwnerInfoElement>();
            for (int i = 0; i < OwnersTooltipView.MAX_ELEMENTS; i++)
            {
                owners.Add(Substitute.For<IOwnerInfoElement>());
            }

            tooltipView.SetElements(owners);
            Assert.IsFalse(tooltip.viewAllButton.gameObject.activeSelf, "View All button shouldn't be active");

            owners.Add(Substitute.For<IOwnerInfoElement>());
            tooltipView.SetElements(owners);

            Assert.IsTrue(tooltip.viewAllButton.gameObject.activeSelf, "View All button should be active");

            tooltipView.Show();
            Assert.IsTrue(tooltipView.IsActive());
            tooltipView.Hide(true);
            Assert.IsFalse(tooltipView.IsActive());
        }

        [Test]
        public void OwnersPopupSetupCorrectly()
        {
            var popup = view.ownersPopup;
            Assert.AreEqual(0, popup.ownerElementsContainer.childCount);
        }

        [Test]
        public void OwnersPopupShowCorrectly()
        {
            var popup = view.ownersPopup;
            IOwnersPopupView popupView = popup;

            popupView.Show();
            Assert.IsTrue(popupView.IsActive());
            popupView.Hide(true);
            Assert.IsFalse(popupView.IsActive());
        }

        [Test]
        public void PromptWhenNftPromptIsRequestedByRpcService()
        {
            restrictedActionsContext.OpenNftPrompt("0x00", "123");
            Assert.IsTrue(view.content.activeSelf, "NFT dialog should be visible");
        }
    }
}
