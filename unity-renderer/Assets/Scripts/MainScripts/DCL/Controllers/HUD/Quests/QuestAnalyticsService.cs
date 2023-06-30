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
        private const string PROGRESSED_QUEST = "quest_progressed";
        private const string JUMPED_IN_QUEST = "quest_jumped_in";
        private const string QUEST_LOG_VISIBILITY_CHANGED = "quest_log_visibility_changed";
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

        public void SendQuestProgressed(string questId, List<string> completedTasks, List<string> pendingTasks) =>
            analytics.SendAnalytic(PROGRESSED_QUEST,
                new Dictionary<string, string>
                {
                    { "quest_id", questId },
                    { "completed_tasks", string.Join(",", completedTasks) },
                    { "pending_tasks", string.Join(",", pendingTasks) }
                });

        public void SendQuestJumpIn(string questId, string taskId, Vector2Int coordinates) =>
            analytics.SendAnalytic(JUMPED_IN_QUEST, new Dictionary<string, string>
            {
                { "quest_id", questId },
                { "task_id", taskId },
                { "coordinates", $"{coordinates.x},{coordinates.y}" }
            });

        public void SendQuestLogVisibilityChanged(bool isVisible) =>
            analytics.SendAnalytic(QUEST_LOG_VISIBILITY_CHANGED, new Dictionary<string, string>
            {
                { "is_visible", isVisible.ToString() }
            });

        public void SendPinnedQuest(string questId, bool isPinned) =>
            analytics.SendAnalytic(PINNED_QUEST, new Dictionary<string, string>
            {
                { "quest_id", questId },
                { "is_pinned", isPinned.ToString() }
            });
    }
}
