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
        void SendQuestTaskProgressed(string questId, string taskId, string taskName);
        void SendQuestTaskCompleted(string questId, string taskId, string taskName);
        void SendQuestJumpIn(string questId, string taskId, Vector2Int coordinates);
        void SendQuestLogVisibilityChanged(bool isVisible);
    }
}
