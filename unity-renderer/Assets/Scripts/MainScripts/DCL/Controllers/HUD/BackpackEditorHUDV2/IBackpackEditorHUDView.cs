using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackEditorHUDView
    {
        void Dispose();
        void Show();
        void Hide();
        void SetAsFullScreenMenuMode(Transform parentTransform);
    }
}
