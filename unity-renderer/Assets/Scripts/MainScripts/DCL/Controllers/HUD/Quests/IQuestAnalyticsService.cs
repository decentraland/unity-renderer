using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Quests
{
    public interface IQuestAnalyticsService
    {
        void SendQuestStarted(string questId);
        void SendQuestCompleted(string questId);
        void SendQuestCancelled(string questId);
        void SendQuestProgressed(string questId, List<string> completedTasks, List<string> pendingTasks);
        void SendQuestJumpIn(string questId, string taskId, Vector2Int coordinates);
        void SendQuestLogVisibilityChanged(bool isVisible);
        void SendPinnedQuest(string questId, bool isPinned);
    }
}
