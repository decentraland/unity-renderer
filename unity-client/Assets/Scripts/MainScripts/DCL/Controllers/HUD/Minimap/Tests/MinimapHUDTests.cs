using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MinimapHUDTests : IntegrationTestSuite_Legacy
    {
        private MinimapHUDController controller;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new MinimapHUDController();
        }

        protected override IEnumerator TearDown()
        {
            controller.Dispose();
            yield return base.TearDown();
        }

        [Test]
        public void MinimapHUD_Creation()
        {
            var containerName = MinimapHUDView.VIEW_OBJECT_NAME;
            var viewContainer = GameObject.Find(containerName);

            Assert.NotNull(viewContainer);
            Assert.NotNull(viewContainer.GetComponent<MinimapHUDView>());
        }

        [Test]
        public void MinimapHUD_DefaultSceneName()
        {
            var view = controller.view;
            Assert.AreEqual("Unnamed", Reflection_GetField<TextMeshProUGUI>(view, "sceneNameText").text);
        }

        [Test]
        public void MinimapHUD_DefaultPlayerCoordinates()
        {
            var view = controller.view;
            Assert.IsEmpty(Reflection_GetField<TextMeshProUGUI>(view, "playerPositionText").text);
        }

        [Test]
        public void MinimapHUD_SetSceneName()
        {
            const string sceneName = "SCENE_NAME";

            controller.UpdateSceneName(sceneName);
            var view = controller.view;
            Assert.AreEqual(sceneName, Reflection_GetField<TextMeshProUGUI>(view, "sceneNameText").text);
        }

        [Test]
        public void MinimapHUD_SetPlayerCoordinatesVector2()
        {
            Vector2 coords = new Vector2(Random.Range(-150, 150), Random.Range(-150, 150));
            string coordString = string.Format("{0},{1}", coords.x, coords.y);

            controller.UpdatePlayerPosition(coords);
            Assert.AreEqual(coordString, Reflection_GetField<TextMeshProUGUI>(this.controller.view, "playerPositionText").text);
        }

        [Test]
        public void MinimapHUD_SetPlayerCoordinatesString()
        {
            Vector2 coords = new Vector2(Random.Range(-150, 150), Random.Range(-150, 150));
            string coordString = string.Format("{0},{1}", coords.x, coords.y);

            controller.UpdatePlayerPosition(coordString);
            Assert.AreEqual(coordString, Reflection_GetField<TextMeshProUGUI>(this.controller.view, "playerPositionText").text);
        }

        [Test]
        public void MinimapHUD_OptionsPanel()
        {
            Assert.IsNotNull(controller);
        }
    }
}