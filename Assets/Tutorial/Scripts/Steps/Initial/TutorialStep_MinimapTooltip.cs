namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the Minimap.
    /// </summary>
    public class TutorialStep_MinimapTooltip : TutorialStep_Tooltip
    {
        public override void OnStepStart()
        {
            base.OnStepStart();

            NavmapView.OnToggle += NavmapView_OnToggle;
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            NavmapView.OnToggle -= NavmapView_OnToggle;
        }

        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.minimapHud.minimapTooltipReference)
            {
                tooltipTransform.position = tutorialController.hudController.minimapHud.minimapTooltipReference.position;
            }
        }

        private void NavmapView_OnToggle(bool isVisible)
        {
            if (isVisible)
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