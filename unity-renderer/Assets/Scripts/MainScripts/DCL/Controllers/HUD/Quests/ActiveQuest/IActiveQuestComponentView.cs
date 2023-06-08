using System;

namespace DCL.Quests
{
    public interface IActiveQuestComponentView
    {
        event Action<QuestDetailsComponentModel> OnActiveQuestSelected;

        void SetQuestName(string title);
        void SetQuestCreator(string creator);
        void SetQuestId(string questId);
        void SetIsPinned(bool isPinned);
        void SetQuestImage(string imageUri);
        void Deselect();
        void SetQuestDetailsModel(QuestDetailsComponentModel questDetailsComponentModel);
    }
}
