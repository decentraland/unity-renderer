using System;

namespace DCL.Quests
{
    public interface IQuestOfferComponentView
    {
        event Action<string> OnQuestAccepted;
        event Action<string> OnQuestRefused;

        void SetQuestId(string questId);
        void SetIsGuest(bool isGuest);
        void SetQuestTitle(string title);
        void SetQuestDescription(string description);
    }
}
