using System.Collections;
using DCL.Helpers;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class AvatarHUDTests : TestsBase
    {
        private static AvatarHUDView GetViewFromController(AvatarHUDController controller)
        {
            return Reflection_GetField<AvatarHUDView>(controller, "view");
        }

        [UnityTest]
        public IEnumerator AvatarHUD_Creation()
        {
            yield return InitScene();
            var controller = new AvatarHUDController();
            var viewContainerName = Reflection_GetStaticField<string>(typeof(AvatarHUDView), "VIEW_OBJECT_NAME");
            var viewContainer = GameObject.Find(viewContainerName);

            Assert.NotNull(viewContainer);
            Assert.NotNull(viewContainer.GetComponent<AvatarHUDView>());
        }

        [UnityTest]
        public IEnumerator AvatarHUD_VisibilityDefaulted()
        {
            yield return InitScene();
            var controller = new AvatarHUDController();

            Assert.AreEqual(true, controller.visibility);
            Assert.AreEqual(true, GetViewFromController(controller).gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_VisibilityOverridenTrue()
        {
            yield return InitScene();
            var controller = new AvatarHUDController(visibility: true);

            Assert.AreEqual(true, controller.visibility);
            Assert.AreEqual(true, GetViewFromController(controller).gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_VisibilityOverridenFalse()
        {
            yield return InitScene();
            var controller = new AvatarHUDController(visibility: false);

            Assert.AreEqual(false, controller.visibility);
            Assert.AreEqual(false, GetViewFromController(controller).gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ExpandedDefaulted()
        {
            yield return InitScene();
            var controller = new AvatarHUDController();

            Assert.AreEqual(false, controller.expanded);
            var view = GetViewFromController(controller);
            Assert.AreEqual(false, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ExpandedOverridenTrue()
        {
            yield return InitScene();
            var controller = new AvatarHUDController(expanded: true);

            Assert.AreEqual(true, controller.expanded);
            var view = GetViewFromController(controller);
            Assert.AreEqual(true, Reflection_GetField<GameObject>(view,"expandedContainer").activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ExpandedOverridenFalse()
        {
            yield return InitScene();
            var controller = new AvatarHUDController(expanded: false);

            Assert.AreEqual(false, controller.expanded);
            var view = GetViewFromController(controller);
            Assert.AreEqual(false, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ModelDefaulted()
        {
            yield return InitScene();
            var controller = new AvatarHUDController();

            Assert.AreEqual("", controller.model.name);
            Assert.AreEqual("", controller.model.mail);
            Assert.AreEqual(null, controller.model.avatarPic);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ModelOverriden()
        {
            yield return InitScene();
            Sprite sprite = CreateEmptySprite();
            var controller = new AvatarHUDController(new AvatarHUDModel()
            {
                name = "name",
                mail = "mail",
                avatarPic = sprite
            });

            Assert.AreEqual("name", controller.model.name);
            Assert.AreEqual("mail", controller.model.mail);
            Assert.AreEqual(sprite, controller.model.avatarPic);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ControllerSetVisibilityTrue()
        {
            yield return InitScene();
            var controller = new AvatarHUDController(false);

            controller.SetVisibility(true);

            Assert.AreEqual(true, controller.visibility);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ControllerSetVisibilityFalse()
        {
            yield return InitScene();
            var controller = new AvatarHUDController();

            controller.SetVisibility(false);

            Assert.AreEqual(false, controller.visibility);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ViewSetVisibilityTrue()
        {
            yield return InitScene();
            var controller = new AvatarHUDController(false);

            GetViewFromController(controller).SetVisibility(true);

            Assert.AreEqual(true, GetViewFromController(controller).gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ViewSetVisibilityFalse()
        {
            yield return InitScene();
            var controller = new AvatarHUDController();

            GetViewFromController(controller).SetVisibility(false);

            Assert.AreEqual(false, GetViewFromController(controller).gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ControllerUpdateData()
        {
            yield return InitScene();
            Sprite sprite = CreateEmptySprite();
            var controller = new AvatarHUDController();

            controller.UpdateData(new AvatarHUDModel()
            {
                name =  "name",
                mail = "mail",
                avatarPic = sprite
            });

            Assert.AreEqual("name", controller.model.name);
            Assert.AreEqual("mail", controller.model.mail);
            Assert.AreEqual(sprite, controller.model.avatarPic);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ViewUpdateData()
        {
            yield return InitScene();
            Sprite sprite = CreateEmptySprite();
            var controller = new AvatarHUDController();

            GetViewFromController(controller).UpdateData(new AvatarHUDModel()
            {
                name =  "name",
                mail = "mail",
                avatarPic = sprite
            });

            var view = GetViewFromController(controller);
            Assert.AreEqual("name", Reflection_GetField<TextMeshProUGUI>(view, "nameText").text);
            Assert.AreEqual("mail", Reflection_GetField<TextMeshProUGUI>(view, "mailText").text);
            Assert.AreEqual(sprite, Reflection_GetField<Image>(view, "topAvatarPic").sprite);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ExpandToggleFromFalse()
        {
            yield return InitScene();
            var controller = new AvatarHUDController(expanded: false);

            var view = GetViewFromController(controller);
            Reflection_GetField<Button>(view, "toggleExpandButton").onClick.Invoke();

            Assert.AreEqual(true, controller.expanded);
            Assert.AreEqual(true, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ExpandToggleFromTrue()
        {
            yield return InitScene();
            var controller = new AvatarHUDController(expanded: true);

            var view = GetViewFromController(controller);
            Reflection_GetField<Button>(view, "toggleExpandButton").onClick.Invoke();

            Assert.AreEqual(false, controller.expanded);
            Assert.AreEqual(false, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        private static Sprite CreateEmptySprite()
        {
            return Sprite.Create(new Texture2D(10, 10), new Rect(Vector2.one, Vector2.one), Vector2.one);
        }
    }
}