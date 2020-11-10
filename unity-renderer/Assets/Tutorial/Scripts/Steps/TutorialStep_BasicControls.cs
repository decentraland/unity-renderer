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

        private bool stepIsFinished = false;
        private int defaultTeacherCanvasSortOrder;

        public override void OnStepStart()
        {
            base.OnStepStart();

            CommonScriptableObjects.featureKeyTriggersBlocked.Set(true);

            okButton.onClick.AddListener(OnOkButtonClick);

            defaultTeacherCanvasSortOrder = tutorialController.teacherCanvas.sortingOrder;
            tutorialController.SetTeacherCanvasSortingOrder(TEACHER_CANVAS_SORT_ORDER_START);

            tutorialController.hudController?.taskbarHud?.SetVisibility(false);
            tutorialController.hudController?.profileHud?.SetBackpackButtonVisibility(false);
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
            tutorialController.hudController?.profileHud?.SetBackpackButtonVisibility(true);
            CommonScriptableObjects.featureKeyTriggersBlocked.Set(false);
        }

        private void OnOkButtonClick()
        {
            stepIsFinished = true;
        }
    }
}