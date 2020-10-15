using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the controls panel.
    /// </summary>
    public class TutorialStep_OpenControlsPanel : TutorialStep
    {
        private bool controlsHasBeenOpened = false;
        private bool controlsHasBeenClosed = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            if (tutorialController != null && tutorialController.hudController != null)
            {
                tutorialController.hudController.controlsHud.OnControlsOpened += ControlsHud_OnControlsOpened;
                tutorialController.hudController.controlsHud.OnControlsClosed += ControlsHud_OnControlsClosed;
            }
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => controlsHasBeenOpened && controlsHasBeenClosed);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            if (tutorialController != null && tutorialController.hudController != null)
            {
                tutorialController.hudController.controlsHud.OnControlsOpened -= ControlsHud_OnControlsOpened;
                tutorialController.hudController.controlsHud.OnControlsClosed -= ControlsHud_OnControlsClosed;
            }
        }

        private void ControlsHud_OnControlsOpened()
        {
            if (!controlsHasBeenOpened && mainSection.activeSelf)
                controlsHasBeenOpened = true;

            tutorialController?.hudController?.taskbarHud?.SetVisibility(true);
        }

        private void ControlsHud_OnControlsClosed()
        {
            if (controlsHasBeenOpened && mainSection.activeSelf)
                controlsHasBeenClosed = true;
        }
    }
}