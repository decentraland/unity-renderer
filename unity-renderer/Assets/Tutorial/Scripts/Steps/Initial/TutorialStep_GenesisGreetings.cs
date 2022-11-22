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
        private const int GENESIS_PLAZA_TUTORIAL_LOCATION = 3;

        [SerializeField]
        Button okButton;

        [SerializeField]
        TMP_Text titleText;

        private bool stepIsFinished = false;
        private int defaultTeacherCanvasSortOrder;

        public override void OnStepStart()
        {
            base.OnStepStart();

            OnChangePlayerCoords(new Vector2Int(0, 0), CommonScriptableObjects.playerCoords.Get());
            CommonScriptableObjects.playerCoords.OnChange += OnChangePlayerCoords;

            CommonScriptableObjects.userMovementKeysBlocked.Set(true);
            CommonScriptableObjects.featureKeyTriggersBlocked.Set(true);

            titleText.text = titleText.text.Replace("{userName}", UserProfile.GetOwnUserProfile().userName);

            okButton.onClick.AddListener(OnOkButtonClick);

            if (tutorialController != null)
            {
                tutorialController.SetEagleEyeCameraActive(true);

                if (tutorialController.configuration.teacherCanvas != null)
                    defaultTeacherCanvasSortOrder = tutorialController.configuration.teacherCanvas.sortingOrder;

                tutorialController.SetTeacherCanvasSortingOrder(TEACHER_CANVAS_SORT_ORDER_START);

                tutorialController.hudController?.taskbarHud?.SetVisibility(false);

                if (Environment.i != null && Environment.i.world != null)
                {
                    WebInterface.SendSceneExternalActionEvent(Environment.i.world.state.GetCurrentSceneNumber(), "tutorial", "begin");
                }
            }
        }

        public override IEnumerator OnStepExecute() { yield return new WaitUntil(() => stepIsFinished); }

        public override IEnumerator OnStepPlayHideAnimation()
        {
            tutorialController?.SetEagleEyeCameraActive(false);
            yield return base.OnStepPlayHideAnimation();
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();
            tutorialController.SetTeacherCanvasSortingOrder(defaultTeacherCanvasSortOrder);

            CommonScriptableObjects.userMovementKeysBlocked.Set(false);

            CommonScriptableObjects.playerCoords.OnChange -= OnChangePlayerCoords;
        }

        internal void OnOkButtonClick() { stepIsFinished = true; }

        private void OnChangePlayerCoords(Vector2Int prevCoords, Vector2Int coords)
        {
            if (stepIsFinished)
                return;

            if (Mathf.Abs(coords.x) > GENESIS_PLAZA_TUTORIAL_LOCATION ||
                Mathf.Abs(coords.y) > GENESIS_PLAZA_TUTORIAL_LOCATION)
            {
                stepIsFinished = true;
                return;
            }
        }
    }
}