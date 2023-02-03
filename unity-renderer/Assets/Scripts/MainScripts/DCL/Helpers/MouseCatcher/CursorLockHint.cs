using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class CursorLockHint : MonoBehaviour
    {
        private bool hasBeenShown;

        [SerializeField] private ShowHideAnimator toastRoot;
        [SerializeField] private float duration;

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
            if (!isLocked)
            {
                toastRoot.Hide();
                return;
            }

            if (DataStore.i.camera.panning.Get() || hasBeenShown || toastRoot.isVisible)
                return;

            hasBeenShown = true;
            toastRoot.ShowDelayHide(duration);
        }
    }
}
