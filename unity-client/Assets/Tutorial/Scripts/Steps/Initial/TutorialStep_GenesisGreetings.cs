using DCL.Interface;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to the greetings showed in Genesis Plaza.
    /// </summary>
    public class TutorialStep_GenesisGreetings : TutorialStep
    {
        private const int TEACHER_CANVAS_SORT_ORDER_START = 4;

        [SerializeField] Button okButton;
        [SerializeField] TMP_Text titleText;

        private bool stepIsFinished = false;
        private int defaultTeacherCanvasSortOrder;

        public override void OnStepStart()
        {
            base.OnStepStart();

            CommonScriptableObjects.featureKeyTriggersBlocked.Set(true);

            titleText.text = titleText.text.Replace("{userName}", UserProfile.GetOwnUserProfile().userName);

            okButton.onClick.AddListener(OnOkButtonClick);

            if (tutorialController)
            {
                tutorialController.SetEagleEyeCameraActive(true);

                defaultTeacherCanvasSortOrder = tutorialController.teacherCanvas.sortingOrder;
                tutorialController.SetTeacherCanvasSortingOrder(TEACHER_CANVAS_SORT_ORDER_START);

                tutorialController.hudController?.taskbarHud?.SetVisibility(false);
                tutorialController.hudController?.profileHud?.SetBackpackButtonVisibility(false);

                if (SceneController.i != null)
                {
                    WebInterface.SendSceneExternalActionEvent(Environment.i.worldState.currentSceneId, "tutorial", "begin");
                }
            }
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => stepIsFinished);
        }

        public override IEnumerator OnStepPlayHideAnimation()
        {
            tutorialController?.SetEagleEyeCameraActive(false);
            yield return base.OnStepPlayHideAnimation();
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();
            tutorialController.SetTeacherCanvasSortingOrder(defaultTeacherCanvasSortOrder);
        }

        private void OnOkButtonClick()
        {
            stepIsFinished = true;
        }
    }
}