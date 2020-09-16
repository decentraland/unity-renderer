using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to Jump In the Genesis Plaza and become a DCL Citizen.
    /// </summary>
    public class TutorialStep_Tooltip_GoToGenesisButton : TutorialStep_Tooltip
    {
        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.taskbarHud.goToGenesisTooltipReference)
            {
                tutorialController.hudController.taskbarHud.ShowGoToGenesisPlazaButton();
                tooltipTransform.position = tutorialController.hudController.taskbarHud.goToGenesisTooltipReference.position;
            }
        }
    }
}