using System.Collections;
using System.Collections.Generic;
using DCL.Huds.QuestsTracker;
using UnityEngine;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsNotificationsController : MonoBehaviour
    {
        internal static float NOTIFICATIONS_SEPARATION { get; set; } = 0.5f;
        public static float DEFAULT_NOTIFICATION_DURATION { get; set; } = 2.5f;

        internal readonly Queue<IQuestNotification> notificationsQueue = new Queue<IQuestNotification>();
        private IQuestNotification currentNotification;

        [SerializeField] private GameObject questCompletedPrefab;
        [SerializeField] private GameObject rewardObtainedPrefab;
        private bool isDestroyed = false;

        internal static QuestsNotificationsController Create()
        {
            QuestsNotificationsController view = Instantiate(Resources.Load<GameObject>("QuestsNotificationsHUD")).GetComponent<QuestsNotificationsController>();
#if UNITY_EDITOR
            view.gameObject.name = "_QuestsNotificationsHUDView";
#endif
            return view;
        }

        private void Awake() { StartCoroutine(ProcessSectionsNotificationQueue()); }

        public void ShowQuestCompleted(QuestModel quest)
        {
            var questNotification = Instantiate(questCompletedPrefab, transform).GetComponent<QuestNotification_QuestCompleted>();
            questNotification.Populate(quest);
            questNotification.gameObject.SetActive(false);
            notificationsQueue.Enqueue(questNotification);
        }

        public void ShowRewardObtained(QuestReward reward)
        {
            var questNotification = Instantiate(rewardObtainedPrefab, transform).GetComponent<QuestNotification_RewardObtained>();
            questNotification.Populate(reward);
            questNotification.gameObject.SetActive(false);
            notificationsQueue.Enqueue(questNotification);
        }

        public void SetVisibility(bool visible) { gameObject.SetActive(visible); }

        public void Dispose()
        {
            if (isDestroyed)
                return;

            Destroy(gameObject);
        }

        public void ClearAllNotifications()
        {
            foreach ( var noti in notificationsQueue )
            {
                noti.Dispose();
            }

            if ( currentNotification != null )
                currentNotification.Dispose();
        }

        private void OnDestroy()
        {
            ClearAllNotifications();
            isDestroyed = true;
        }

        private IEnumerator ProcessSectionsNotificationQueue()
        {
            while (true)
            {
                if (notificationsQueue.Count > 0)
                {
                    IQuestNotification notification = notificationsQueue.Dequeue();
                    currentNotification = notification;
                    notification.Show();
                    yield return notification.Waiter();
                    notification.Dispose();
                    currentNotification = null;
                }

                yield return WaitForSecondsCache.Get(NOTIFICATIONS_SEPARATION);
            }
        }
    }
}