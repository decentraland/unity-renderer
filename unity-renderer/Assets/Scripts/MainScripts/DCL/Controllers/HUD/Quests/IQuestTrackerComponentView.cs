using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Quests
{
    public interface IQuestTrackerComponentView
    {
        event Action<Vector2Int> OnJumpIn;
        void SetQuestTitle(string questTitle);
        void SetQuestCoordinates(Vector2Int coordinates);
        void SetQuestSteps(List<QuestStepComponentModel> questSteps);
        void SetSupportsJumpIn(bool supportsJumpIn);
        void SetVisible(bool isVisible);
    }
}
