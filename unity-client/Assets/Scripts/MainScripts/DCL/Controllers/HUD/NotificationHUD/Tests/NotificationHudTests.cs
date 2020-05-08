using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class NotificationHudTests : TestsBase
    {
        protected override IEnumerator SetUp()
        {
            sceneInitialized = false;
            return base.SetUp();
        }

        [Test]
        public void NotificationHud_Creation()
        {
            var controller = new NotificationHUDController();
            var views = GameObject.FindObjectsOfType<NotificationHUDView>();

            Assert.AreEqual(1, views.Length);

            var view = views[0];
            Assert.NotNull(view);
            Assert.AreEqual(view, controller.view);
        }

        [Test]
        public void NotificationHud_ModelDefaulted()
        {
            var controller = new NotificationHUDController();

            Assert.IsNotNull(controller.model);
            Assert.IsNotNull(controller.model.notifications);
            Assert.AreEqual(controller.model.notifications.Count, 0);
        }

        [UnityTest]
        public IEnumerator NotificationHud_ShowNotification()
        {
            var controller = new NotificationHUDController();

            Notification.Model model = new Notification.Model()
            {
                type = NotificationFactory.Type.GENERIC,
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
            var controller = new NotificationHUDController();

            Notification.Model model = new Notification.Model()
            {
                type = NotificationFactory.Type.GENERIC,
                message = "text",
                timer = -1,
                scene = ""
            };

            controller.ShowNotification(model);

            Notification.Model model2 = new Notification.Model()
            {
                type = NotificationFactory.Type.SCRIPTING_ERROR,
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
            var controller = new NotificationHUDController();

            Notification.Model model = new Notification.Model()
            {
                type = NotificationFactory.Type.GENERIC,
                message = "text",
                timer = 3,
                scene = ""
            };

            controller.ShowNotification(model);
            yield return null;

            Notification[] notifications = GameObject.FindObjectsOfType<Notification>();
            Assert.AreEqual(notifications.Length, 1);
            Assert.AreEqual(controller.model.notifications.Count, 1);

            yield return new WaitForSeconds(4);

            notifications = GameObject.FindObjectsOfType<Notification>();
            Assert.AreEqual(notifications.Length, 0);
            Assert.AreEqual(controller.model.notifications.Count, 0);
        }
    }
}
