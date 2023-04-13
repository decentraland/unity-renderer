using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackEditorHUDView
    {
        void Initialize();
        void Dispose();
        void Show();
        void Hide();
        void SetAsFullScreenMenuMode(Transform parentTransform);
    }
}
