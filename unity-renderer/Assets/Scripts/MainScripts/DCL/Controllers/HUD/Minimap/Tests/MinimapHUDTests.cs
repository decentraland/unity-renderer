using System.Collections;
using DCL.Helpers;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MinimapHUDTests : TestsBase
    {
        private static MinimapHUDView GetViewFromController(MinimapHUDController controller)
        {
            return Reflection_GetField<MinimapHUDView>(controller, "view");
        }

        [UnityTest]
        public IEnumerator MinimapHUD_Creation()
        {
            yield return InitScene();
            var controller = new MinimapHUDController();
            var containerName = Reflection_GetStaticField<string>(typeof(MinimapHUDView), "VIEW_OBJECT_NAME");
            var viewContainer = GameObject.Find(containerName);

            Assert.NotNull(viewContainer);
            Assert.NotNull(viewContainer.GetComponent<MinimapHUDView>());
        }

        [UnityTest]
        public IEnumerator MinimapHUD_DefaultSceneName()
        {
            yield return InitScene();
            var controller = new MinimapHUDController();

            var view = GetViewFromController(controller);
            Assert.AreEqual("Unnamed", Reflection_GetField<TextMeshProUGUI>(view, "sceneNameText").text);
        }

        [UnityTest]
        public IEnumerator MinimapHUD_DefaultPlayerCoordinates()
        {
            yield return InitScene();
            var controller = new MinimapHUDController();

            var view = GetViewFromController(controller);
            Assert.IsEmpty(Reflection_GetField<TextMeshProUGUI>(view, "playerPositionText").text);
        }

        [UnityTest]
        public IEnumerator MinimapHUD_SetSceneName()
        {
            const string sceneName = "SCENE_NAME";

            yield return InitScene();
            var controller = new MinimapHUDController();
            controller.UpdateSceneName(sceneName);
            var view = GetViewFromController(controller);
            Assert.AreEqual(sceneName, Reflection_GetField<TextMeshProUGUI>(view, "sceneNameText").text);
        }

        [UnityTest]
        public IEnumerator MinimapHUD_SetPlayerCoordinatesVector2()
        {
            Vector2 coords = new Vector2(Random.Range(-150, 150), Random.Range(-150, 150));
            string coordString = string.Format("{0},{1}", coords.x, coords.y);

            yield return InitScene();
            var controller = new MinimapHUDController();
            controller.UpdatePlayerPosition(coords);
            var view = GetViewFromController(controller);
            Assert.AreEqual(coordString, Reflection_GetField<TextMeshProUGUI>(view, "playerPositionText").text);
        }

        [UnityTest]
        public IEnumerator MinimapHUD_SetPlayerCoordinatesString()
        {
            Vector2 coords = new Vector2(Random.Range(-150, 150), Random.Range(-150, 150));
            string coordString = string.Format("{0},{1}", coords.x, coords.y);

            yield return InitScene();
            var controller = new MinimapHUDController();
            controller.UpdatePlayerPosition(coordString);
            var view = GetViewFromController(controller);
            Assert.AreEqual(coordString, Reflection_GetField<TextMeshProUGUI>(view, "playerPositionText").text);
        }

        [UnityTest]
        public IEnumerator MinimapHUD_OptionsPanel()
        {
            yield return InitScene();
            var controller = new MinimapHUDController();
            Assert.IsNotNull(controller);
        }
    }
}