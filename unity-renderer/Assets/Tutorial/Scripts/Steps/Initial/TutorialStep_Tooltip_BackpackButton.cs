using UnityEngine.EventSystems;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the Backpack window from the taskbar.
    /// </summary>
    public class TutorialStep_Tooltip_BackpackButton : TutorialStep_Tooltip
    {
        public override void OnStepStart()
        {
            base.OnStepStart();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.profileHud != null)
            {
                tutorialController.hudController.profileHud.OnOpen += ProfileHud_OnOpen;
            }
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.profileHud != null)
            {
                tutorialController.hudController.profileHud.OnOpen -= ProfileHud_OnOpen;
            }
        }

        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.profileHud != null &&
                tutorialController.hudController.profileHud.tutorialTooltipReference)
            {
                tooltipTransform.position = tutorialController.hudController.profileHud.tutorialTooltipReference.position;
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);
        }

        internal void ProfileHud_OnOpen()
        {
            stepIsFinished = true;
            tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);
        }
    }
}