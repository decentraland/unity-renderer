using System;

namespace DCL.Quest
{
    public interface IQuestOfferComponentView
    {
        event Action<string> OnQuestAccepted;

        void SetQuestId(string questId);
        void SetQuestTitle(string title);
        void SetQuestDescription(string description);
    }
}
