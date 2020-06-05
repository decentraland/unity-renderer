using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests
{
    public class NFTPromptHUDTests : TestsBase
    {
        private NFTPromptHUDController controller;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new NFTPromptHUDController();
        }

        [UnityTest]
        public IEnumerator CreateView()
        {
            Assert.NotNull(controller.view);
            Assert.NotNull(controller.view.gameObject);
            yield break;
        }

        [UnityTest]
        public IEnumerator OpenAndCloseCorrectly()
        {
            controller.OpenNftInfoDialog("0xf64dc33a192e056bb5f0e5049356a0498b502d50", "2481", null);
            Assert.IsTrue(controller.view.content.activeSelf, "NFT dialog should be visible");
            controller.view.buttonClose.onClick.Invoke();
            Assert.IsFalse(controller.view.content.activeSelf, "NFT dialog should not be visible");
            yield break;
        }
    }
}