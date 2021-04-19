using DCL;
using DCL.Huds.QuestsNotifications;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.QuestsNotificationsHUD
{
    public class QuestsNotificationsHUDViewShould
    {
        private QuestsNotificationsHUDView hudView;

        [SetUp]
        public void SetUp()
        {
            // Even though we set duration to 0, we have to wait a frame anyway for the yields to be performed.
            QuestsNotificationsHUDView.SECTION_NOTIFICATION_DURATION = 0f;
            QuestsNotificationsHUDView.NOTIFICATIONS_SEPARATION = 0f;
            hudView = Object.Instantiate(Resources.Load<GameObject>("QuestsNotificationsHUD")).GetComponent<QuestsNotificationsHUDView>();
        }

        [Test]
        public void ReactToQuestCompleted()
        {
            hudView.ShowQuestCompleted(new QuestModel { name = "theName" });
            Assert.AreEqual(1, hudView.transform.childCount);
            var notificationComponent = hudView.transform.GetChild(0).GetComponent<QuestNotification_QuestCompleted>();
            Assert.NotNull(notificationComponent);
            Assert.AreEqual("theName", notificationComponent.questName.text);
        }

        [Test]
        public void ReactToSectionCompleted()
        {
            hudView.ShowSectionCompleted(new QuestSection { name = "theName"});
            Assert.AreEqual(1, hudView.transform.childCount);
            var notificationComponent = hudView.transform.GetChild(0).GetComponent<QuestNotification_SectionCompleted>();
            Assert.NotNull(notificationComponent);
            Assert.AreEqual("theName", notificationComponent.sectionName.text);
        }

        [Test]
        public void ReactToSectionUnlocked()
        {
            hudView.ShowSectionUnlocked(new QuestSection
            {
                name = "sectionName",
                tasks = new []{new QuestTask { name = "taskName"}}
            });
            Assert.AreEqual(1, hudView.transform.childCount);
            var notificationComponent = hudView.transform.GetChild(0).GetComponent<QuestNotification_SectionUnlocked>();
            Assert.NotNull(notificationComponent);
            Assert.AreEqual("sectionName", notificationComponent.sectionName.text);
            Assert.AreEqual("taskName", notificationComponent.taskName.text);
        }

        [UnityTest]
        public IEnumerator ProcessNotificationsQueue()
        {
            hudView.ShowQuestCompleted(new QuestModel());
            hudView.ShowQuestCompleted(new QuestModel());

            Assert.AreEqual(2, hudView.notificationsQueue.Count);

            yield return null; // Wait for notification to go away.
            yield return null; // Wait for notification time separation.
            Assert.AreEqual(1, hudView.notificationsQueue.Count);

            yield return null; // Wait for notification to go away.
            yield return null; // Wait for notification time separation.
            Assert.AreEqual(0, hudView.notificationsQueue.Count);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(hudView.gameObject);
        }
    }
}