using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tests
{
    public class AvatarHUDTests : TestsBase
    {
        private static AvatarHUDView GetViewFromController(AvatarHUDController controller)
        {
            return Reflection_GetField<AvatarHUDView>(controller, "view");
        }

        [Test]
        public void AvatarHUD_Creation()
        {
            var controller = new AvatarHUDController();
            var viewContainerName = Reflection_GetStaticField<string>(typeof(AvatarHUDView), "VIEW_OBJECT_NAME");
            var viewContainer = GameObject.Find(viewContainerName);

            Assert.NotNull(viewContainer);
            Assert.NotNull(viewContainer.GetComponent<AvatarHUDView>());
        }

        [Test]
        public void AvatarHUD_VisibilityDefaulted()
        {
            var controller = new AvatarHUDController();
            controller.Initialize();

            Assert.AreEqual(true, controller.visibility);
            Assert.AreEqual(true, GetViewFromController(controller).gameObject.activeSelf);
        }

        [Test]
        public void AvatarHUD_VisibilityOverridenTrue()
        {
            var controller = new AvatarHUDController();
            controller.Initialize(visibility: true);

            Assert.AreEqual(true, controller.visibility);
            Assert.AreEqual(true, GetViewFromController(controller).gameObject.activeSelf);
        }

        [Test]
        public void AvatarHUD_VisibilityOverridenFalse()
        {
            var controller = new AvatarHUDController();
            controller.Initialize(visibility: false);

            Assert.AreEqual(false, controller.visibility);
            Assert.AreEqual(false, GetViewFromController(controller).gameObject.activeSelf);
        }

        [Test]
        public void AvatarHUD_ExpandedDefaulted()
        {
            var controller = new AvatarHUDController();
            controller.Initialize();

            Assert.AreEqual(false, controller.expanded);
            var view = GetViewFromController(controller);
            Assert.AreEqual(false, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        [Test]
        public void AvatarHUD_ExpandedOverridenTrue()
        {
            var controller = new AvatarHUDController();
            controller.Initialize(expanded: true);

            Assert.AreEqual(true, controller.expanded);
            var view = GetViewFromController(controller);
            Assert.AreEqual(true, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        [Test]
        public void AvatarHUD_ExpandedOverridenFalse()
        {
            var controller = new AvatarHUDController();
            controller.Initialize(expanded: false);

            Assert.AreEqual(false, controller.expanded);
            var view = GetViewFromController(controller);
            Assert.AreEqual(false, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        [Test]
        public void AvatarHUD_ModelDefaulted()
        {
            var controller = new AvatarHUDController();
            controller.Initialize();

            Assert.AreEqual("", controller.model.name);
            Assert.AreEqual("", controller.model.mail);
            Assert.AreEqual(null, controller.model.avatarPic);
        }

        [Test]
        public void AvatarHUD_ModelOverriden()
        {
            Sprite sprite = CreateEmptySprite();
            var controller = new AvatarHUDController();

            controller.Initialize(new AvatarHUDModel()
            {
                name = "name",
                mail = "mail",
                avatarPic = sprite
            });

            Assert.AreEqual("name", controller.model.name);
            Assert.AreEqual("mail", controller.model.mail);
            Assert.AreEqual(sprite, controller.model.avatarPic);
        }

        [Test]
        public void AvatarHUD_ControllerSetVisibilityTrue()
        {
            var controller = new AvatarHUDController();
            controller.Initialize(false);

            controller.SetVisibility(true);

            Assert.AreEqual(true, controller.visibility);
        }

        [Test]
        public void AvatarHUD_ControllerSetVisibilityFalse()
        {
            var controller = new AvatarHUDController();
            controller.Initialize();

            controller.SetVisibility(false);

            Assert.AreEqual(false, controller.visibility);
        }

        [Test]
        public void AvatarHUD_ViewSetVisibilityTrue()
        {
            var controller = new AvatarHUDController();
            controller.Initialize(false);

            GetViewFromController(controller).SetVisibility(true);

            Assert.AreEqual(true, GetViewFromController(controller).gameObject.activeSelf);
        }

        [Test]
        public void AvatarHUD_ViewSetVisibilityFalse()
        {
            var controller = new AvatarHUDController();
            controller.Initialize();

            GetViewFromController(controller).SetVisibility(false);

            Assert.AreEqual(false, GetViewFromController(controller).gameObject.activeSelf);
        }

        [Test]
        public void AvatarHUD_ControllerUpdateData()
        {
            Sprite sprite = CreateEmptySprite();
            var controller = new AvatarHUDController();
            controller.Initialize();

            controller.UpdateData(new AvatarHUDModel()
            {
                name = "name",
                mail = "mail",
                avatarPic = sprite
            });

            Assert.AreEqual("name", controller.model.name);
            Assert.AreEqual("mail", controller.model.mail);
            Assert.AreEqual(sprite, controller.model.avatarPic);
        }

        [Test]
        public void AvatarHUD_ViewUpdateData()
        {
            Sprite sprite = CreateEmptySprite();
            var controller = new AvatarHUDController();
            controller.Initialize();

            GetViewFromController(controller).UpdateData(new AvatarHUDModel()
            {
                name = "name",
                mail = "mail",
                avatarPic = sprite
            });

            var view = GetViewFromController(controller);
            Assert.AreEqual("name", Reflection_GetField<TextMeshProUGUI>(view, "nameText").text);
            Assert.AreEqual("mail", Reflection_GetField<TextMeshProUGUI>(view, "mailText").text);
            Assert.AreEqual(sprite, view.topAvatarPic.sprite);
        }

        [Test]
        public void AvatarHUD_ExpandToggleFromFalse()
        {
            var controller = new AvatarHUDController();
            controller.Initialize(expanded: false);

            var view = GetViewFromController(controller);
            Reflection_GetField<Button>(view, "toggleExpandButton").onClick.Invoke();

            Assert.AreEqual(true, controller.expanded);
            Assert.AreEqual(true, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        [Test]
        public void AvatarHUD_ExpandToggleFromTrue()
        {
            var controller = new AvatarHUDController();
            controller.Initialize(expanded: true);

            var view = GetViewFromController(controller);
            Reflection_GetField<Button>(view, "toggleExpandButton").onClick.Invoke();

            Assert.AreEqual(false, controller.expanded);
            Assert.AreEqual(false, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        [Test]
        public void AvatarHUD_ExpandToggleFromTrueWhenEditAvatarIsClicked()
        {
            var controller = new AvatarHUDController();
            controller.Initialize(expanded: true);

            var view = GetViewFromController(controller);
            Reflection_GetField<Button>(view, "editAvatarButton").onClick.Invoke();

            Assert.AreEqual(false, controller.expanded);
            Assert.AreEqual(false, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        [Test]
        public void AvatarHUD_ExpandToggleFromTrueWhenLogOutIsClicked()
        {
            var controller = new AvatarHUDController();
            controller.Initialize(expanded: true);

            var view = GetViewFromController(controller);
            Reflection_GetField<Button>(view, "signOutButton").onClick.Invoke();

            Assert.AreEqual(false, controller.expanded);
            Assert.AreEqual(false, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        private static Sprite CreateEmptySprite()
        {
            return Sprite.Create(new Texture2D(10, 10), new Rect(0, 0, 10, 10), Vector2.zero);
        }
    }
}
