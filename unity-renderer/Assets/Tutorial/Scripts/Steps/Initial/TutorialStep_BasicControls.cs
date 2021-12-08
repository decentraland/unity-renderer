using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to the very basic controls.
    /// </summary>
    public class TutorialStep_BasicControls : TutorialStep
    {
        private const int TEACHER_CANVAS_SORT_ORDER_START = 4;

        [SerializeField] AudioEvent audioEventSuccess;
        [SerializeField] Button okButton;

        internal bool stepIsFinished = false;
        private int defaultTeacherCanvasSortOrder;

        public override void OnStepStart()
        {
            base.OnStepStart();

            CommonScriptableObjects.featureKeyTriggersBlocked.Set(true);

            okButton.onClick.AddListener(OnOkButtonClick);

            if (tutorialController.configuration.teacherCanvas != null)
                defaultTeacherCanvasSortOrder = tutorialController.configuration.teacherCanvas.sortingOrder;

            tutorialController.SetTeacherCanvasSortingOrder(TEACHER_CANVAS_SORT_ORDER_START);

            tutorialController.hudController?.taskbarHud?.SetVisibility(false);
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => stepIsFinished);
            audioEventSuccess.Play(true);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();
            tutorialController.SetTeacherCanvasSortingOrder(defaultTeacherCanvasSortOrder);
            tutorialController.hudController?.taskbarHud?.SetVisibility(true);
            CommonScriptableObjects.featureKeyTriggersBlocked.Set(false);
        }

        internal void OnOkButtonClick() { stepIsFinished = true; }
    }
}