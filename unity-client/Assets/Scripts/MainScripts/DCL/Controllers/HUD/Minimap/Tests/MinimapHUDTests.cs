using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MinimapHUDTests : TestsBase
    {
        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield break;
        }

        private static MinimapHUDView GetViewFromController(MinimapHUDController controller)
        {
            return Reflection_GetField<MinimapHUDView>(controller, "view");
        }

        [Test]
        public void MinimapHUD_Creation()
        {

            var controller = new MinimapHUDController();
            var containerName = Reflection_GetStaticField<string>(typeof(MinimapHUDView), "VIEW_OBJECT_NAME");
            var viewContainer = GameObject.Find(containerName);

            Assert.NotNull(viewContainer);
            Assert.NotNull(viewContainer.GetComponent<MinimapHUDView>());
        }

        [Test]
        public void MinimapHUD_DefaultSceneName()
        {

            var controller = new MinimapHUDController();

            var view = GetViewFromController(controller);
            Assert.AreEqual("Unnamed", Reflection_GetField<TextMeshProUGUI>(view, "sceneNameText").text);
        }

        [Test]
        public void MinimapHUD_DefaultPlayerCoordinates()
        {

            var controller = new MinimapHUDController();

            var view = GetViewFromController(controller);
            Assert.IsEmpty(Reflection_GetField<TextMeshProUGUI>(view, "playerPositionText").text);
        }

        [Test]
        public void MinimapHUD_SetSceneName()
        {
            const string sceneName = "SCENE_NAME";


            var controller = new MinimapHUDController();
            controller.UpdateSceneName(sceneName);
            var view = GetViewFromController(controller);
            Assert.AreEqual(sceneName, Reflection_GetField<TextMeshProUGUI>(view, "sceneNameText").text);
        }

        [Test]
        public void MinimapHUD_SetPlayerCoordinatesVector2()
        {
            Vector2 coords = new Vector2(Random.Range(-150, 150), Random.Range(-150, 150));
            string coordString = string.Format("{0},{1}", coords.x, coords.y);


            var controller = new MinimapHUDController();
            controller.UpdatePlayerPosition(coords);
            var view = GetViewFromController(controller);
            Assert.AreEqual(coordString, Reflection_GetField<TextMeshProUGUI>(view, "playerPositionText").text);
        }

        [Test]
        public void MinimapHUD_SetPlayerCoordinatesString()
        {
            Vector2 coords = new Vector2(Random.Range(-150, 150), Random.Range(-150, 150));
            string coordString = string.Format("{0},{1}", coords.x, coords.y);


            var controller = new MinimapHUDController();
            controller.UpdatePlayerPosition(coordString);
            var view = GetViewFromController(controller);
            Assert.AreEqual(coordString, Reflection_GetField<TextMeshProUGUI>(view, "playerPositionText").text);
        }

        [Test]
        public void MinimapHUD_OptionsPanel()
        {

            var controller = new MinimapHUDController();
            Assert.IsNotNull(controller);
        }
    }
}
