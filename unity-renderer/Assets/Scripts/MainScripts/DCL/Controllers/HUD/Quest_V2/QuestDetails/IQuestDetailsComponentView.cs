using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Quests
{
    public interface IQuestDetailsComponentView
    {
        public event Action<Vector2Int> OnJumpIn;
        public event Action<string, bool> OnPinChange;
        public event Action<string> OnQuestAbandon;

        public void SetQuestName(string nameText);
        public void SetQuestCreator(string questCreator);
        public void SetQuestDescription(string questDescription);
        public void SetQuestId(string questId);
        void SetIsPinned(bool isPinned);
        public void SetCoordinates(Vector2Int coordinates);
        public void SetQuestSteps(List<QuestStepComponentModel> questSteps);
        public void SetQuestRewards(List<QuestRewardComponentModel> questRewards);
        public void SetIsGuest(bool isGuest);
    }
}
