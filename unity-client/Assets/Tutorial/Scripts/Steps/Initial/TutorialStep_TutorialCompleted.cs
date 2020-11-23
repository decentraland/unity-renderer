using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to the end of the tutorial.
    /// </summary>
    public class TutorialStep_TutorialCompleted : TutorialStep
    {
        [SerializeField] AudioEvent audioEventSuccess;

        public override IEnumerator OnStepExecute()
        {
            audioEventSuccess.Play(true);
            yield break;
        }
    }
}