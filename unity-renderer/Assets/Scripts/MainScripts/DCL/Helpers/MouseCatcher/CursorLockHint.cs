using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class CursorLockHint : MonoBehaviour
    {
        private bool hasBeenShown;

        [SerializeField] private ShowHideAnimator toastRoot;
        [SerializeField] private float duration;
        
        private Coroutine hideRoutine;

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
            if (DataStore.i.camera.panning.Get()) return;
            if (hasBeenShown) return;
            if (toastRoot.isVisible) return;
            toastRoot.gameObject.SetActive(true);
            toastRoot.Show();
            hasBeenShown = true;
            HideToastAfterDelay();
        }

        private void HideToastAfterDelay()
        {
            if (hideRoutine != null)
                StopCoroutine(hideRoutine);
            hideRoutine = StartCoroutine(Utils.Wait(duration, () =>
            {
                toastRoot.Hide();
                hideRoutine = null;
            }));
        }
    }
}