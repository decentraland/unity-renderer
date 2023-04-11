using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackEditorHUDView
    {
        Transform EmotesSectionTransform { get; }
        void Dispose();
        void Show();
        void Hide();
        void SetAsFullScreenMenuMode(Transform parentTransform);
    }
}
