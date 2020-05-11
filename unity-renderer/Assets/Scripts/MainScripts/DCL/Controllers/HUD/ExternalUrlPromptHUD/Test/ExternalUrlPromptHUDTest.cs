using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests
{
    public class ExternalUrlPromptHUDShould : TestsBase
    {

        private ExternalUrlPromptHUDController controller;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new ExternalUrlPromptHUDController();
        }

        [UnityTest]
        public IEnumerator CreateView()
        {
            Assert.NotNull(controller.view);
            Assert.NotNull(controller.view.gameObject);
            yield break;
        }

        [UnityTest]
        public IEnumerator PromptWhenExternalUrlIsRequested()
        {
            controller.ProcessOpenUrlRequest(scene, "https://decentraland.org/press");
            Assert.True(controller.view.content.activeInHierarchy, "ExternalUrlPromptHUD content should be visible");
            yield break;
        }

        [UnityTest]
        public IEnumerator CloseCorrectly()
        {
            controller.ProcessOpenUrlRequest(scene, "https://etherscan.io/gasTracker");
            controller.view.closeButton.onClick.Invoke();
            Assert.True(!controller.view.content.activeInHierarchy, "ExternalUrlPromptHUD content should NOT be visible");

            controller.ProcessOpenUrlRequest(scene, "https://etherscan.io/gasTracker");
            controller.view.cancelButton.onClick.Invoke();
            Assert.True(!controller.view.content.activeInHierarchy, "ExternalUrlPromptHUD content should NOT be visible");

            controller.ProcessOpenUrlRequest(scene, "https://etherscan.io/gasTracker");
            controller.view.continueButton.onClick.Invoke();
            Assert.True(!controller.view.content.activeInHierarchy, "ExternalUrlPromptHUD content should NOT be visible");
            yield break;
        }

        [UnityTest]
        public IEnumerator RememberTrustedDomains()
        {
            controller.ProcessOpenUrlRequest(scene, "https://decentraland.org/press");
            Assert.True(controller.view.content.activeInHierarchy, "ExternalUrlPromptHUD content should be visible");

            controller.view.trustToggle.isOn = true;
            controller.view.continueButton.onClick.Invoke();
            Assert.True(!controller.view.content.activeInHierarchy, "ExternalUrlPromptHUD content should NOT be visible");
            Assert.True(controller.trustedDomains.ContainsKey(scene.sceneData.id)
                && controller.trustedDomains[scene.sceneData.id].Contains("decentraland.org"),
                "domain not set as trusted");

            controller.ProcessOpenUrlRequest(scene, "https://decentraland.org/press");
            Assert.True(!controller.view.content.activeInHierarchy, "ExternalUrlPromptHUD content should NOT be visible cause we trust this domain");

            yield break;
        }
    }
}