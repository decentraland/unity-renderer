using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to jump with the avatar.
    /// </summary>
    public class TutorialStep_AvatarJumping : TutorialStep
    {
        [SerializeField] AudioEvent audioEventSuccess;
        [SerializeField] internal InputAction_Hold jumpingInputAction;

        public override void OnStepStart()
        {
            base.OnStepStart();
            jumpingInputAction.RaiseOnFinished();
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => mainSection.activeSelf && jumpingInputAction.isOn);
            audioEventSuccess.Play(true);
        }
    }
}