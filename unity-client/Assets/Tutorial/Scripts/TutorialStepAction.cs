
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// This class coordinates the step animations with the animations of any action object that can exist as child.
    /// </summary>
    public class TutorialStepAction : MonoBehaviour
    {
        protected static int KEY_LOOP_ANIMATOR_BOOL = Animator.StringToHash("KeyLoop");

        [SerializeField] internal TutorialStep step;

        private Animator actionAnimator;

        private void Start()
        {
            actionAnimator = GetComponent<Animator>();

            if (actionAnimator != null)
                actionAnimator.SetBool(KEY_LOOP_ANIMATOR_BOOL, false);

            if (step != null)
            {
                step.OnShowAnimationFinished += Step_OnShowAnimationFinished;
                step.OnJustAfterStepExecuted += Step_OnJustAfterStepExecuted;
            }
        }

        private void OnDestroy()
        {
            if (step != null)
            {
                step.OnShowAnimationFinished -= Step_OnShowAnimationFinished;
                step.OnJustAfterStepExecuted -= Step_OnJustAfterStepExecuted;
            }
        }

        private void Step_OnShowAnimationFinished()
        {
            if (actionAnimator != null)
                actionAnimator.SetBool(KEY_LOOP_ANIMATOR_BOOL, true);
        }

        private void Step_OnJustAfterStepExecuted()
        {
            if (actionAnimator != null)
                actionAnimator.SetBool(KEY_LOOP_ANIMATOR_BOOL, false);
        }
    }
}