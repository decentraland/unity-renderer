using DCL.Interface;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to the greetings showed in Genesis Plaza.
    /// </summary>
    public class TutorialStep_GenesisGreetingsAfterDeepLink : TutorialStep_GenesisGreetings
    {
        public override void OnStepFinished()
        {
            base.OnStepFinished();
            tutorialController?.hudController?.taskbarHud?.SetVisibility(true);
            tutorialController?.hudController?.profileHud?.SetBackpackButtonVisibility(true);
            CommonScriptableObjects.featureKeyTriggersBlocked.Set(false);
        }
    }
}