using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NotificationModel;

namespace Tests
{
    public class NotificationHudTests : IntegrationTestSuite_Legacy
    {
        private NotificationHUDController controller;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new NotificationHUDController();
            sceneInitialized = false;
        }

        protected override IEnumerator TearDown()
        {
            controller.Dispose();
            yield return base.TearDown();
        }

        [Test]
        public void NotificationHud_Creation()
        {
            var views = GameObject.FindObjectsOfType<NotificationHUDView>();

            Assert.AreEqual(1, views.Length);

            var view = views[0];
            Assert.NotNull(view);
            Assert.AreEqual(view, controller.view);
        }

        [Test]
        public void NotificationHud_ModelDefaulted()
        {
            Assert.IsNotNull(controller.model);
            Assert.IsNotNull(controller.model.notifications);
            Assert.AreEqual(controller.model.notifications.Count, 0);
        }

        [UnityTest]
        public IEnumerator NotificationHud_ShowNotification()
        {
            Model model = new Model()
            {
                type = Type.GENERIC,
                message = "text",
                timer = -1,
                scene = ""
            };

            controller.ShowNotification(model);

            yield return null;

            Notification[] notifications = GameObject.FindObjectsOfType<Notification>();
            Assert.AreEqual(notifications.Length, 1);

            Notification n = notifications[0];
            Assert.AreEqual(n.model.type, model.type);
            Assert.AreEqual(n.model.message, model.message);
            Assert.AreEqual(n.model.timer, model.timer);
            Assert.AreEqual(n.model.scene, model.scene);
        }

        [UnityTest]
        public IEnumerator NotificationHud_ShowSeveralNotifications()
        {
            Model model = new Model()
            {
                type = Type.GENERIC,
                message = "text",
                timer = -1,
                scene = ""
            };

            controller.ShowNotification(model);

            Model model2 = new Model()
            {
                type = Type.SCRIPTING_ERROR,
                message = "text",
                timer = -1,
                scene = ""
            };

            controller.ShowNotification(model2);

            yield return null;

            Notification[] notifications = GameObject.FindObjectsOfType<Notification>();
            Assert.AreEqual(2, notifications.Length);
        }

        [UnityTest]
        public IEnumerator NotificationHud_ShowTimedNotification()
        {
            Model model = new Model()
            {
                type = Type.GENERIC,
                message = "text",
                timer = 0.25f,
                scene = ""
            };

            controller.ShowNotification(model);
            yield return null;

            Notification[] notifications = GameObject.FindObjectsOfType<Notification>();
            Assert.AreEqual(notifications.Length, 1);
            Assert.AreEqual(controller.model.notifications.Count, 1);

            yield return new DCL.WaitUntil(() => notifications.Length == 0, 0.5f);

            notifications = GameObject.FindObjectsOfType<Notification>();
            Assert.AreEqual(notifications.Length, 0);
            Assert.AreEqual(controller.model.notifications.Count, 0);
        }
    }
}