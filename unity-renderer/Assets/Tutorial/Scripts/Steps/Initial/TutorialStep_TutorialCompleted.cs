using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to the end of the tutorial.
    /// </summary>
    public class TutorialStep_TutorialCompleted : TutorialStep
    {
        [SerializeField] AudioEvent audioEventSuccess;
        [SerializeField] ButtonComponentView okButton;
        [SerializeField] GameObject modal;

        internal bool okPressed = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            modal.SetActive(true);

            okButton.onClick.AddListener(() =>
            {
                modal.SetActive(false);
                stepAnimator.SetTrigger("OkPressed");
                okPressed = true;
            });
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => okPressed);
            audioEventSuccess.Play(true);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();
            okButton.onClick.RemoveAllListeners();
        }
    }
}