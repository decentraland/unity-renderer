using System;
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

        internal event Action OnShowAnimationFinished;
        internal event Action OnJustAfterStepExecuted;

        [SerializeField] internal bool unlockCursorAtStart = false;
        [SerializeField] internal bool show3DTeacherAtStart = false;
        [SerializeField] internal protected RectTransform teacherPositionRef;
        [SerializeField] internal GameObject mainSection;
        [SerializeField] internal GameObject skipTutorialSection;
        [SerializeField] internal InputAction_Hold yesSkipInputAction;
        [SerializeField] internal InputAction_Hold noSkipInputAction;

        protected TutorialController tutorialController;
        internal Animator stepAnimator;
        internal MouseCatcher mouseCatcher;
        protected bool hideAnimationFinished = false;
        internal bool blockSkipActions = false;

        internal bool letInstantiation = true;
        private HUDCanvasCameraModeController hudCanvasCameraMode;

        private void Awake()
        {
            var canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
                hudCanvasCameraMode = new HUDCanvasCameraModeController(canvas, DataStore.i.camera.hudsCamera);
        }

        private void OnDestroy() { hudCanvasCameraMode?.Dispose(); }

        /// <summary>
        /// Step initialization (occurs before OnStepExecute() execution).
        /// </summary>
        public virtual void OnStepStart()
        {
            tutorialController = TutorialController.i;
            stepAnimator = GetComponent<Animator>();

            mouseCatcher = SceneReferences.i?.mouseCatcher;

            if (unlockCursorAtStart)
                mouseCatcher?.UnlockCursor();

            if (tutorialController != null)
            {
                tutorialController.ShowTeacher3DModel(show3DTeacherAtStart);

                if (tutorialController.configuration.teacher != null)
                {
                    if (tutorialController.currentStepIndex > 0)
                        tutorialController.configuration.teacher.PlaySpeakSound();
                    else
                        tutorialController.configuration.teacher.PlayHappySound(1f);
                }

                if (show3DTeacherAtStart && teacherPositionRef != null)
                {
                    tutorialController.SetTeacherPosition(teacherPositionRef.position);

                    if (tutorialController.configuration.teacher != null &&
                        tutorialController.configuration.teacher.IsHiddenByAnimation)
                        tutorialController.configuration.teacher.PlayAnimation(TutorialTeacher.TeacherAnimation.Reset);
                }
            }

            ConfigureSkipOptions();
        }

        /// <summary>
        /// Executes the main flow of the step and waits for its finalization.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator OnStepExecute() { yield break; }

        /// <summary>
        /// Executes the final animation and waits for its finalization and for any camera blending.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator OnStepPlayHideAnimation()
        {
            blockSkipActions = true;
            OnJustAfterStepExecuted?.Invoke();
            yield return PlayAndWaitForHideAnimation();
            yield return null;
            yield return new WaitUntil(() => !CommonScriptableObjects.cameraIsBlending.Get());
        }

        /// <summary>
        /// Step finalization (occurs after OnStepExecute() execution).
        /// </summary>
        public virtual void OnStepFinished()
        {
            if (mainSection != null &&
                skipTutorialSection != null &&
                yesSkipInputAction != null &&
                noSkipInputAction)
            {
                yesSkipInputAction.OnFinished -= YesSkipInputAction_OnFinished;
                noSkipInputAction.OnFinished -= NoSkipInputAction_OnFinished;
            }
        }

        internal void OnShowAnimationFinish() { OnShowAnimationFinished?.Invoke(); }

        /// <summary>
        /// Warn about the finalization of the hide animation of the step
        /// </summary>
        internal void OnHideAnimationFinish() { hideAnimationFinished = true; }

        private IEnumerator PlayAndWaitForHideAnimation()
        {
            if (stepAnimator == null)
                yield break;

            stepAnimator.SetTrigger(STEP_FINISHED_ANIMATOR_TRIGGER);
            yield return new WaitUntil(() => hideAnimationFinished);
        }

        private void ConfigureSkipOptions()
        {
            if (mainSection != null &&
                skipTutorialSection != null &&
                yesSkipInputAction != null &&
                noSkipInputAction)
            {
                yesSkipInputAction.OnFinished += YesSkipInputAction_OnFinished;
                noSkipInputAction.OnFinished += NoSkipInputAction_OnFinished;
            }
        }

        private void YesSkipInputAction_OnFinished(DCLAction_Hold action)
        {
            if (skipTutorialSection.activeSelf)
            {
                tutorialController.SkipTutorial();
            }
        }

        internal void NoSkipInputAction_OnFinished(DCLAction_Hold action)
        {
            if (blockSkipActions)
                return;

            if (mainSection.activeSelf)
            {
                mainSection.SetActive(false);
                skipTutorialSection.SetActive(true);
            }
            else if (skipTutorialSection.activeSelf)
            {
                mainSection.SetActive(true);
                skipTutorialSection.SetActive(false);
            }
        }
    }
}