using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class CursorLockHint : MonoBehaviour
    {
        private bool hasBeenShown;

        [SerializeField] private ShowHideAnimator toastRoot;
        
        private void Start()
        {
            Utils.OnCursorLockChanged += HandleCursorLockChanges;
        }

        private void OnDestroy()
        {
            Utils.OnCursorLockChanged -= HandleCursorLockChanges;
        }

        private void HandleCursorLockChanges(bool isLocked)
        {
            if (!isLocked) return;
            if (hasBeenShown) return;
            if (toastRoot.isVisible) return;
            toastRoot.Show();
            hasBeenShown = true;
        }
    }
}