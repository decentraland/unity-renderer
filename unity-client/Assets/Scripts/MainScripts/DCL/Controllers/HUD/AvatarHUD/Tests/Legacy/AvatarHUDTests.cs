using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Legacy.Tests
{
    public class AvatarHUDTests : TestsBase
    {
        protected override bool justSceneSetUp => true;
        AvatarHUDController controller;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new AvatarHUDController();
            controller.Initialize();
        }

        [UnityTearDown]
        protected override IEnumerator TearDown()
        {
            controller.Dispose();
            yield return base.TearDown();
        }

        private static AvatarHUDView GetViewFromController(AvatarHUDController controller)
        {
            return Reflection_GetField<AvatarHUDView>(controller, "view");
        }

        [Test]
        public void AvatarHUD_Creation()
        {
            var viewContainerName = Reflection_GetStaticField<string>(typeof(AvatarHUDView), "VIEW_OBJECT_NAME");
            var viewContainer = GameObject.Find(viewContainerName);

            Assert.NotNull(viewContainer);
            Assert.NotNull(viewContainer.GetComponent<AvatarHUDView>());
        }

        [Test]
        public void AvatarHUD_VisibilityDefaulted()
        {
            Assert.AreEqual(true, controller.visibility);
            Assert.AreEqual(true, GetViewFromController(controller).gameObject.activeSelf);
        }

        [Test]
        public void AvatarHUD_VisibilityOverridenTrue()
        {
            controller.SetVisibility(true);
            Assert.AreEqual(true, controller.visibility);
            Assert.AreEqual(true, GetViewFromController(controller).gameObject.activeSelf);
        }

        [Test]
        public void AvatarHUD_VisibilityOverridenFalse()
        {
            controller.SetVisibility(false);
            Assert.AreEqual(false, controller.visibility);
            Assert.AreEqual(false, GetViewFromController(controller).gameObject.activeSelf);
        }

        [Test]
        public void AvatarHUD_ExpandedDefaulted()
        {
            Assert.AreEqual(false, controller.expanded);
            var view = GetViewFromController(controller);
            Assert.AreEqual(false, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        [Test]
        public void AvatarHUD_ExpandedOverridenTrue()
        {
            controller.SetExpanded(true);
            Assert.AreEqual(true, controller.expanded);
            var view = GetViewFromController(controller);
            Assert.AreEqual(true, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        [Test]
        public void AvatarHUD_ExpandedOverridenFalse()
        {
            controller.SetExpanded(false);
            Assert.AreEqual(false, controller.expanded);
            var view = GetViewFromController(controller);
            Assert.AreEqual(false, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        [Test]
        public void AvatarHUD_ModelDefaulted()
        {
            Assert.AreEqual("", controller.model.name);
            Assert.AreEqual("", controller.model.mail);
            Assert.AreEqual(null, controller.model.avatarPic);
        }

        [Test]
        public void AvatarHUD_ModelOverriden()
        {
            Sprite sprite = CreateEmptySprite();

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
        public void AvatarHUD_ControllerSetVisibilityTrue()
        {
            controller.SetVisibility(true);
            Assert.AreEqual(true, controller.visibility);
        }

        [Test]
        public void AvatarHUD_ControllerSetVisibilityFalse()
        {
            controller.SetVisibility(false);
            Assert.AreEqual(false, controller.visibility);
        }

        [Test]
        public void AvatarHUD_ViewSetVisibilityTrue()
        {
            controller.SetVisibility(false);
            GetViewFromController(controller).SetVisibility(true);
            Assert.AreEqual(true, GetViewFromController(controller).gameObject.activeSelf);
        }

        [Test]
        public void AvatarHUD_ViewSetVisibilityFalse()
        {
            GetViewFromController(controller).SetVisibility(false);
            Assert.AreEqual(false, GetViewFromController(controller).gameObject.activeSelf);
        }

        [Test]
        public void AvatarHUD_ControllerUpdateData()
        {
            Sprite sprite = CreateEmptySprite();
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

            GetViewFromController(controller).UpdateData(new AvatarHUDModel()
            {
                name = "name",
                avatarPic = sprite
            });

            var view = GetViewFromController(controller);
            Assert.AreEqual("name", Reflection_GetField<TextMeshProUGUI>(view, "nameText").text);
            Assert.AreEqual(sprite, view.topAvatarPic.sprite);
        }

        [Test]
        public void AvatarHUD_ExpandToggleFromFalse()
        {
            controller.SetExpanded(false);
            var view = GetViewFromController(controller);
            Reflection_GetField<Button>(view, "toggleExpandButton").onClick.Invoke();

            Assert.AreEqual(true, controller.expanded);
            Assert.AreEqual(true, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        [Test]
        public void AvatarHUD_ExpandToggleFromTrue()
        {
            controller.SetExpanded(true);

            var view = GetViewFromController(controller);
            Reflection_GetField<Button>(view, "toggleExpandButton").onClick.Invoke();

            Assert.AreEqual(false, controller.expanded);
            Assert.AreEqual(false, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        [Test]
        public void AvatarHUD_ExpandToggleFromTrueWhenEditAvatarIsClicked()
        {
            controller.SetExpanded(true);

            var view = GetViewFromController(controller);
            Reflection_GetField<Button>(view, "editAvatarButton").onClick.Invoke();

            Assert.AreEqual(false, controller.expanded);
            Assert.AreEqual(false, Reflection_GetField<GameObject>(view, "expandedContainer").activeSelf);
        }

        [Test]
        public void AvatarHUD_ExpandToggleFromTrueWhenLogOutIsClicked()
        {
            controller.SetExpanded(true);

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