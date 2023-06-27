using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Quests
{
    public class QuestAnalyticsService : IQuestAnalyticsService
    {
        private const string STARTED_QUEST = "quest_started";
        private const string COMPLETED_QUEST = "quest_completed";
        private const string CANCELLED_QUEST = "quest_cancelled";
        private const string PINNED_QUEST = "quest_pinned";

        private readonly IAnalytics analytics;

        public QuestAnalyticsService(IAnalytics analytics)
        {
            this.analytics = analytics;
        }

        public void SendQuestStarted(string questId) =>
            analytics.SendAnalytic(STARTED_QUEST, new Dictionary<string, string> { { "quest_id", questId } });

        public void SendQuestCompleted(string questId) =>
            analytics.SendAnalytic(COMPLETED_QUEST, new Dictionary<string, string> { { "quest_id", questId } });

        public void SendQuestCancelled(string questId) =>
                analytics.SendAnalytic(CANCELLED_QUEST, new Dictionary<string, string> { { "quest_id", questId } });

        public void SendQuestTaskProgressed(string questId, string taskId, string taskName)
        {
            throw new NotImplementedException();
        }

        public void SendQuestTaskCompleted(string questId, string taskId, string taskName)
        {
            throw new NotImplementedException();
        }

        public void SendQuestJumpIn(string questId, string taskId, Vector2Int coordinates)
        {
            throw new NotImplementedException();
        }

        public void SendQuestLogVisibilityChanged(bool isVisible)
        {
            throw new NotImplementedException();
        }
    }
}
