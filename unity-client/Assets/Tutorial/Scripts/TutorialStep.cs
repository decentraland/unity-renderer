using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents one of the steps included in the onboarding tutorial.
    /// </summary>
    public class TutorialStep : MonoBehaviour
    {
        protected static int STEP_FINISHED_ANIMATOR_TRIGGER = Animator.StringToHash("StepFinished");

        [SerializeField] internal bool unlockCursorAtStart = false;
        [SerializeField] internal bool show3DTeacherAtStart = false;
        [SerializeField] internal protected RectTransform teacherPositionRef;

        protected TutorialController tutorialController;
        protected Animator stepAnimator;
        protected MouseCatcher mouseCatcher;
        internal bool letInstantiation = true;

        /// <summary>
        /// Step initialization (occurs before OnStepExecute() execution).
        /// </summary>
        public virtual void OnStepStart()
        {
            tutorialController = TutorialController.i;
            stepAnimator = GetComponent<Animator>();

            mouseCatcher = InitialSceneReferences.i?.mouseCatcher;

            if (unlockCursorAtStart)
                mouseCatcher?.UnlockCursor();

            if (tutorialController != null)
            {
                tutorialController.ShowTeacher3DModel(show3DTeacherAtStart);

                if (show3DTeacherAtStart && teacherPositionRef != null)
                    tutorialController.SetTeacherPosition(teacherPositionRef.position);
            }
        }

        /// <summary>
        /// Executes the main flow of the step and waits for its finalization.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator OnStepExecute()
        {
            yield break;
        }

        /// <summary>
        /// Executes the final animation and waits for its finalization.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator OnStepPlayAnimationForHidding()
        {
            yield return WaitForAnimation(STEP_FINISHED_ANIMATOR_TRIGGER);
        }

        /// <summary>
        /// Step finalization (occurs after OnStepExecute() execution).
        /// </summary>
        public virtual void OnStepFinished()
        {
        }

        private IEnumerator WaitForAnimation(int animationTrigger)
        {
            if (stepAnimator == null)
                yield break;

            stepAnimator.SetTrigger(animationTrigger);
            yield return null; // NOTE(Santi): It is needed to wait a frame for get the reference to the next animation clip correctly.
            yield return new WaitForSeconds(stepAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        }
    }
}