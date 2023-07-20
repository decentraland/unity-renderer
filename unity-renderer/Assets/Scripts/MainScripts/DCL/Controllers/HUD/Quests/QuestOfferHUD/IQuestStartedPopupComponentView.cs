
using System;

namespace DCL.Quests
{
    public interface IQuestStartedPopupComponentView
    {
        event Action OnOpenQuestLog;

        void SetQuestName(string questName);
        void SetVisible(bool setVisible);
    }
}
