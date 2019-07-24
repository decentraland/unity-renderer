using System.Collections;
using System.Reflection;
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
        [UnityTest]
        public IEnumerator AvatarHUD_Creation()
        {
            yield return InitScene();
            var controller = new AvatarHUDController();
            var viewContainer = GameObject.Find(AvatarHUDTestHelpers.ReflectionGet_ViewObjectName());

            Assert.NotNull(viewContainer);
            Assert.NotNull(viewContainer.GetComponent<AvatarHUDView>());
        }

        [UnityTest]
        public IEnumerator AvatarHUD_VisibilityDefaulted()
        {
            yield return InitScene();
            var controller = new AvatarHUDController();

            Assert.AreEqual(true, controller.visibility);
            Assert.AreEqual(true, controller.ReflectionGet_View().gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_VisibilityOverridenTrue()
        {
            yield return InitScene();
            var controller = new AvatarHUDController(visibility: true);

            Assert.AreEqual(true, controller.visibility);
            Assert.AreEqual(true, controller.ReflectionGet_View().gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_VisibilityOverridenFalse()
        {
            yield return InitScene();
            var controller = new AvatarHUDController(visibility: false);

            Assert.AreEqual(false, controller.visibility);
            Assert.AreEqual(false, controller.ReflectionGet_View().gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ExpandedDefaulted()
        {
            yield return InitScene();
            var controller = new AvatarHUDController();

            Assert.AreEqual(false, controller.expanded);
            Assert.AreEqual(false, controller.ReflectionGet_View().ReflectionGet_ViewField<GameObject>("expandedContainer").activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ExpandedOverridenTrue()
        {
            yield return InitScene();
            var controller = new AvatarHUDController(expanded: true);

            Assert.AreEqual(true, controller.expanded);
            Assert.AreEqual(true, controller.ReflectionGet_View().ReflectionGet_ViewField<GameObject>("expandedContainer").activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ExpandedOverridenFalse()
        {
            yield return InitScene();
            var controller = new AvatarHUDController(expanded: false);

            Assert.AreEqual(false, controller.expanded);
            Assert.AreEqual(false, controller.ReflectionGet_View().ReflectionGet_ViewField<GameObject>("expandedContainer").activeSelf);
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

            controller.ReflectionGet_View().SetVisibility(true);

            Assert.AreEqual(true, controller.ReflectionGet_View().gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ViewSetVisibilityFalse()
        {
            yield return InitScene();
            var controller = new AvatarHUDController();

            controller.ReflectionGet_View().SetVisibility(false);

            Assert.AreEqual(false, controller.ReflectionGet_View().gameObject.activeSelf);
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

            controller.ReflectionGet_View().UpdateData(new AvatarHUDModel()
            {
                name =  "name",
                mail = "mail",
                avatarPic = sprite
            });

            Assert.AreEqual("name", controller.ReflectionGet_View().ReflectionGet_ViewField<TextMeshProUGUI>("nameText").text);
            Assert.AreEqual("mail", controller.ReflectionGet_View().ReflectionGet_ViewField<TextMeshProUGUI>("mailText").text);
            Assert.AreEqual(sprite, controller.ReflectionGet_View().ReflectionGet_ViewField<Image>("topAvatarPic").sprite);
            Assert.AreEqual(sprite, controller.ReflectionGet_View().ReflectionGet_ViewField<Image>("expandedAvatarPic").sprite);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ExpandToggleFromFalse()
        {
            yield return InitScene();
            var controller = new AvatarHUDController(expanded: false);

            controller.ReflectionGet_View().ReflectionGet_ViewField<Button>("toggleExpandButton").onClick.Invoke();

            Assert.AreEqual(true, controller.expanded);
            Assert.AreEqual(true, controller.ReflectionGet_View().ReflectionGet_ViewField<GameObject>("expandedContainer").activeSelf);
        }

        [UnityTest]
        public IEnumerator AvatarHUD_ExpandToggleFromTrue()
        {
            yield return InitScene();
            var controller = new AvatarHUDController(expanded: true);

            controller.ReflectionGet_View().ReflectionGet_ViewField<Button>("toggleExpandButton").onClick.Invoke();

            Assert.AreEqual(false, controller.expanded);
            Assert.AreEqual(false, controller.ReflectionGet_View().ReflectionGet_ViewField<GameObject>("expandedContainer").activeSelf);
        }

        private static Sprite CreateEmptySprite()
        {
            return Sprite.Create(new Texture2D(10, 10), new Rect(Vector2.one, Vector2.one), Vector2.one);
        }
    }

    public static class AvatarHUDTestHelpers
    {
        public static T ReflectionGet_ViewField<T>(this AvatarHUDView view, string fieldName)
        {
            return ReflectionGet_Field<T, AvatarHUDView>(view, fieldName);
        }

        public static string ReflectionGet_ViewObjectName()
        {
            return (string) typeof(AvatarHUDView).GetField("VIEW_OBJECT_NAME", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }

        public static AvatarHUDView ReflectionGet_View(this AvatarHUDController controller)
        {
            return ReflectionGet_Field<AvatarHUDView, AvatarHUDController>(controller, "view");
        }

        private static T ReflectionGet_Field<T, K>( K instance, string fieldName)
        {
            return (T) instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
        }
    }
}