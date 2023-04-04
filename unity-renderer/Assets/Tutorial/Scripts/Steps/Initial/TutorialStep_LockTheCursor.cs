using System.Collections;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to lock/unlock the cursor.
    /// </summary>
    public class TutorialStep_LockTheCursor : TutorialStep
    {
        public override void OnStepStart()
        {
            base.OnStepStart();
            
            if (tutorialController == null)
                return;
            
            CommonScriptableObjects.featureKeyTriggersBlocked.Set(true);
            if (tutorialController.currentPath == TutorialController.TutorialPath.FromGenesisPlaza)
            {
                tutorialController.ShowTeacher3DModel(true);
                tutorialController.SetTeacherPosition(teacherPositionRef.position);
                if (tutorialController.configuration.teacher != null &&
                    tutorialController.configuration.teacher.IsHiddenByAnimation)
                    tutorialController.configuration.teacher.PlayAnimation(TutorialTeacher.TeacherAnimation.Reset);
            }
            else
            {
                tutorialController.hudController?.taskbarHud?.SetVisibility(false);
                tutorialController.ShowTeacher3DModel(false);
                OnShowAnimationFinished += SetupAlicePosition;
            }
        }
        
        private void SetupAlicePosition()
        {
            if (tutorialController != null)
            {
                tutorialController.SetTeacherPosition(teacherPositionRef.position, false);
                tutorialController.ShowTeacher3DModel(true);
                tutorialController.configuration?.teacher?.PlayAnimation(TutorialTeacher.TeacherAnimation.Reset);
            }
            OnShowAnimationFinished -= SetupAlicePosition;
        }

        public override IEnumerator OnStepExecute() { yield return new WaitUntil(() => mouseCatcher == null || mouseCatcher.IsLocked); }
    }
}