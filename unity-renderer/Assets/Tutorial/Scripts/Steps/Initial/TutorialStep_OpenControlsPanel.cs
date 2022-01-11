using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the controls panel.
    /// </summary>
    public class TutorialStep_OpenControlsPanel : TutorialStep
    {
        [SerializeField] AudioEvent audioEventSuccess;
        [SerializeField] private InputAction_Trigger toggleControlsHud;

        private bool controlsHasBeenOpened = false;
        private bool controlsHasBeenClosed = false;
        private BooleanVariable originalControlsTriggerIsBlocked;

        public override void OnStepStart()
        {
            base.OnStepStart();

            originalControlsTriggerIsBlocked = toggleControlsHud.isTriggerBlocked;
            if (toggleControlsHud != null)
                toggleControlsHud.isTriggerBlocked = null;

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.controlsHud != null)
            {
                tutorialController.hudController.controlsHud.OnControlsOpened += ControlsHud_OnControlsOpened;
                tutorialController.hudController.controlsHud.OnControlsClosed += ControlsHud_OnControlsClosed;
            }
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => controlsHasBeenOpened && controlsHasBeenClosed);
            audioEventSuccess.Play(true);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.controlsHud != null)
            {
                tutorialController.hudController.controlsHud.OnControlsOpened -= ControlsHud_OnControlsOpened;
                tutorialController.hudController.controlsHud.OnControlsClosed -= ControlsHud_OnControlsClosed;
            }

            if (toggleControlsHud != null)
                toggleControlsHud.isTriggerBlocked = originalControlsTriggerIsBlocked;
        }

        internal void ControlsHud_OnControlsOpened()
        {
            if (!controlsHasBeenOpened && mainSection.activeSelf)
                controlsHasBeenOpened = true;

            tutorialController?.hudController?.taskbarHud?.SetVisibility(true);
            CommonScriptableObjects.featureKeyTriggersBlocked.Set(false);
        }

        internal void ControlsHud_OnControlsClosed()
        {
            if (controlsHasBeenOpened && mainSection.activeSelf)
                controlsHasBeenClosed = true;
        }
    }
}