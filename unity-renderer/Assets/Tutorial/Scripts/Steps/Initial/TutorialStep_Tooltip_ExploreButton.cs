namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the Explore window from the taskbar.
    /// </summary>
    public class TutorialStep_Tooltip_ExploreButton : TutorialStep_Tooltip
    {
        public override void OnStepStart()
        {
            base.OnStepStart();

            DataStore.i.exploreV2.isOpen.OnChange += ExploreV2IsOpenChanged;
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            DataStore.i.exploreV2.isOpen.OnChange -= ExploreV2IsOpenChanged;
        }

        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.taskbarHud != null &&
                tutorialController.hudController.taskbarHud.exploreTooltipReference)
            {
                tooltipTransform.position = tutorialController.hudController.taskbarHud.exploreTooltipReference.position;
            }
        }

        internal void ExploreV2IsOpenChanged(bool current, bool previous)
        {
            if (current)
            {
                isRelatedFeatureActived = true;
                stepIsFinished = true;
                tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);
            }
            else if (isRelatedFeatureActived)
            {
                isRelatedFeatureActived = false;
            }
        }
    }
}