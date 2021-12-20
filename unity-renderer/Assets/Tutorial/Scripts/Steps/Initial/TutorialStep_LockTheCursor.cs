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

            CommonScriptableObjects.featureKeyTriggersBlocked.Set(true);

            tutorialController?.hudController?.taskbarHud?.SetVisibility(false);
        }

        public override IEnumerator OnStepExecute() { yield return new WaitUntil(() => mouseCatcher == null || mouseCatcher.isLocked); }
    }
}