using UnityEngine;

namespace DCL.Quests
{
    public interface IQuestStepComponentView
    {
        void SetIsCompleted(bool isCompleted);
        void SetQuestStepText(string stepText);
        void SetCoordinates(Vector2Int coordinates);
        void SetSupportsJumpIn(bool supportsJumpIn);
    }
}
