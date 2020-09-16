using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Tutorial
{
    public class TutorialStep_CollectReward : TutorialStep
    {
        [SerializeField] TMP_Text titleText;
        [SerializeField] Button collectButton;

        private bool rewardCollected = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            collectButton.onClick.AddListener(CollectReward);
            titleText.text = titleText.text.Replace("{userName}", UserProfile.GetOwnUserProfile().userName);
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => rewardCollected);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            collectButton.onClick.RemoveListener(CollectReward);
        }

        private void CollectReward()
        {
            rewardCollected = true;
        }
    }
}