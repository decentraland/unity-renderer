using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Quests
{
    public class QuestAnalyticsService : IQuestAnalyticsService
    {
        private const string STARTED_QUEST = "quest_started";
        private const string ABORTED_QUEST = "quest_aborted";
        private const string PINNED_QUEST = "quest_pinned";

        private readonly IAnalytics analytics;

        public QuestAnalyticsService(IAnalytics analytics)
        {
            this.analytics = analytics;
        }
    }
}
