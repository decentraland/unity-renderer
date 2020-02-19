using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL.Tutorial
{
    [RequireComponent(typeof(Animator))]
    public class TutorialTooltip : MonoBehaviour, IPointerClickHandler
    {
        private static int VISIBLE = Animator.StringToHash("visible");
        public float secondsOnScreen = 12f;

        public bool isVisible => animator.GetBool(VISIBLE);

        [SerializeField] private Animator animator;

        public void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            gameObject.SetActive(false);
        }

        public void Show(bool autoHide)
        {
            if (animator.GetBool(VISIBLE))
                return;

            gameObject.SetActive(true);
            animator.SetBool(VISIBLE, true);

            if (autoHide)
                StartCoroutine(WaitUntilFinished());
        }

        public IEnumerator ShowAndHideRoutine()
        {
            if (animator.GetBool(VISIBLE))
                yield break;

            Show(false);
            yield return WaitUntilFinished();
        }

        public IEnumerator WaitUntilFinished()
        {
            yield return new WaitForSeconds(secondsOnScreen);
            Hide();
        }

        public void Hide()
        {
            if (!animator.GetBool(VISIBLE))
                return;

            animator.SetBool(VISIBLE, false);
        }

        // Animation event
        public void OnHideFinished()
        {
            gameObject.SetActive(false);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            Hide();
        }
    }
}
