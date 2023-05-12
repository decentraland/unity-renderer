using System;

namespace DCL.Quests
{
    public interface IQuestOfferComponentView
    {
        void SetQuestId(string questId);
        void SetIsGuest(bool isGuest);
        void SetQuestTitle(string title);
        void SetQuestDescription(string description);
    }
}
