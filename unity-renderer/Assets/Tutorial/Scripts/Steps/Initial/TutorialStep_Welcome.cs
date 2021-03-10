using System.Collections;
using TMPro;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to the welcoming.
    /// </summary>
    public class TutorialStep_Welcome : TutorialStep
    {
        [SerializeField] InputAction_Hold confirmInputAction;
        [SerializeField] TMP_Text descriptionText;

        public override void OnStepStart()
        {
            base.OnStepStart();

            descriptionText.text = descriptionText.text.Replace("{userName}", UserProfile.GetOwnUserProfile().userName);
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => mainSection.activeSelf && confirmInputAction.isOn);
        }
    }
}