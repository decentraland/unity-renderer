using System.Collections;
using System.Reflection;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class NotificationHudTests : TestsBase
    {
        [UnityTest]
        public IEnumerator NotificationHud_Creation()
        {
            yield return InitScene();
            var controller = new NotificationHUDController();
            var views = GameObject.FindObjectsOfType<NotificationHUDView>();

            Assert.AreEqual(1, views.Length);

            var view = views[0];
            Assert.NotNull(view);
            Assert.AreEqual(view, controller.view);
        }

        [UnityTest]
        public IEnumerator NotificationHud_ModelDefaulted()
        {
            yield return InitScene();
            var controller = new NotificationHUDController();

            Assert.IsNotNull(controller.model);
            Assert.IsNotNull(controller.model.notifications);
            Assert.AreEqual(controller.model.notifications.Count, 0);
        }

        [UnityTest]
        public IEnumerator NotificationHud_ShowNotification()
        {
            yield return InitScene();
            var controller = new NotificationHUDController();

            NotificationModel model = new NotificationModel()
            {
                type = NotificationModel.NotificationType.GENERIC,
                message = "text",
                timer = -1,
                scene = ""
            };

            controller.ShowNotification(model);

            yield return null;

            Notification[] notifications = GameObject.FindObjectsOfType<Notification>();
            Assert.AreEqual(notifications.Length, 1);

            Notification n = notifications[0];
            Assert.AreEqual(n.notificationModel.type, model.type);
            Assert.AreEqual(n.notificationModel.message, model.message);
            Assert.AreEqual(n.notificationModel.timer, model.timer);
            Assert.AreEqual(n.notificationModel.scene, model.scene);
        }

        [UnityTest]
        public IEnumerator NotificationHud_ShowSeveralNotifications()
        {
            yield return InitScene();
            var controller = new NotificationHUDController();

            NotificationModel model = new NotificationModel()
            {
                type = NotificationModel.NotificationType.GENERIC,
                message = "text",
                timer = -1,
                scene = ""
            };

            controller.ShowNotification(model);

            NotificationModel model2 = new NotificationModel()
            {
                type = NotificationModel.NotificationType.SCRIPTING_ERROR,
                message = "text",
                timer = -1,
                scene = ""
            };

            controller.ShowNotification(model2);

            yield return null;

            Notification[] notifications = GameObject.FindObjectsOfType<Notification>();
            Assert.AreEqual(notifications.Length, 2);
        }

        [UnityTest]
        public IEnumerator NotificationHud_ShowTimedNotification()
        {
            yield return InitScene();
            var controller = new NotificationHUDController();

            NotificationModel model = new NotificationModel()
            {
                type = NotificationModel.NotificationType.GENERIC,
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