using DCL.Helpers;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to view the players in scene and configure voice chat.
    /// </summary>
    public class TutorialStep_Tooltip_UsersAround : TutorialStep_Tooltip
    {
        public override void OnStepStart()
        {
            base.OnStepStart();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.usersAroundListHud != null)
            {
                tutorialController.hudController.usersAroundListHud.OnOpen += UsersAroundListHud_OnOpen;
            }
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.usersAroundListHud != null)
            {
                tutorialController.hudController.usersAroundListHud.OnOpen -= UsersAroundListHud_OnOpen;
            }
        }

        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.minimapHud != null &&
                tutorialController.hudController.minimapHud.usersAroundTooltipReference)
            {
                tooltipTransform.position = tutorialController.hudController.minimapHud.usersAroundTooltipReference.position;
            }
        }

        internal void UsersAroundListHud_OnOpen()
        {
            stepIsFinished = true;
            tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);
        }
    }
}