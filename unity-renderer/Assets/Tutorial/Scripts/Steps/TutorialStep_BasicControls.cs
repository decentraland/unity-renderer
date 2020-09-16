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
        [SerializeField] Button okButton;

        private bool stepIsFinished = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            okButton.onClick.AddListener(OnOkButtonClick);
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => stepIsFinished);
        }

        private void OnOkButtonClick()
        {
            stepIsFinished = true;
        }
    }
}