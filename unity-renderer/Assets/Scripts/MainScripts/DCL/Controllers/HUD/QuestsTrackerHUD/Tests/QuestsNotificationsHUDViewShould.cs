using DCL;
using DCL.Huds.QuestsTracker;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.QuestsNotificationsHUD
{
    public class QuestsNotificationsHUDViewShould
    {
        private QuestsNotificationsController controller;

        [SetUp]
        public void SetUp()
        {
            // Even though we set duration to 0, we have to wait a frame anyway for the yields to be performed.
            QuestsNotificationsController.DEFAULT_NOTIFICATION_DURATION = 0f;
            QuestsNotificationsController.NOTIFICATIONS_SEPARATION = 0f;
            controller = Object.Instantiate(Resources.Load<GameObject>("QuestsTrackerHUD")).GetComponentInChildren<QuestsNotificationsController>();
        }

        [Test]
        public void ReactToQuestCompleted()
        {
            controller.ShowQuestCompleted(new QuestModel { name = "theName" });
            Assert.AreEqual(1, controller.transform.childCount);
            var notificationComponent = controller.transform.GetChild(0).GetComponent<QuestNotification_QuestCompleted>();
            Assert.NotNull(notificationComponent);
            Assert.AreEqual("theName", notificationComponent.questName.text);
        }

        [UnityTest]
        public IEnumerator ProcessNotificationsQueue()
        {
            controller.ShowQuestCompleted(new QuestModel());
            controller.ShowQuestCompleted(new QuestModel());

            Assert.AreEqual(2, controller.notificationsQueue.Count);

            yield return null; // Wait for notification to go away.
            yield return null; // Wait for notification time separation.
            Assert.AreEqual(1, controller.notificationsQueue.Count);

            yield return null; // Wait for notification to go away.
            yield return null; // Wait for notification time separation.
            Assert.AreEqual(0, controller.notificationsQueue.Count);
        }

        [TearDown]
        public void TearDown() { Object.Destroy(controller.gameObject); }
    }
}