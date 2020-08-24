using DCL.Interface;
using System;
using UnityEngine;

namespace DCL.Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        public static TutorialController i { private set; get; }
        public bool isTutorialEnabled { private set; get; } = false;

        [Flags]
        public enum TutorialStep
        {
            None = 0,
            OldTutorialValue = 99, // NOTE: old tutorial set tutorialStep to 99 when finished
            EmailRequested = 128
        }

        public void SetTutorialEnabled()
        {
            isTutorialEnabled = true;
        }

        public void SetStepCompleted(TutorialStep step)
        {
            WebInterface.SaveUserTutorialStep(GetTutorialStepFromProfile() | (int)step);
        }

        private void Awake()
        {
            i = this;
        }

        private void OnDestroy()
        {
            i = null;
        }

        private int GetTutorialStepFromProfile()
        {
            return UserProfile.GetOwnUserProfile().tutorialStep;
        }
    }
}
