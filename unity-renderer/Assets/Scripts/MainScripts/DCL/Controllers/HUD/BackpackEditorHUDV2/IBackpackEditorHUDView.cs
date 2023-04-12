using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackEditorHUDView
    {
        Transform EmotesSectionTransform { get; }
        
        void Initialize();
        void Dispose();
        void Show();
        void Hide();
        void SetAsFullScreenMenuMode(Transform parentTransform);
    }
}
