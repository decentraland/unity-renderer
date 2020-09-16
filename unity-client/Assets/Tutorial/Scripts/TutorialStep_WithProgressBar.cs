using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Tutorial
{
    /// <summary>
    /// Base class that will be used by all those tutorial steps that need the progress bar actived during their life-cycle.
    /// </summary>
    public class TutorialStep_WithProgressBar : TutorialStep
    {
        [SerializeField] Tutorial_ProgressBar progressBar;
        [SerializeField] int initPercentage;
        [SerializeField] int finishPercentage;

        protected bool progressBarIsFinished = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            progressBar.OnNewProgressBarSizeSet += ProgressBar_OnNewProgressBarSizeSet;

            progressBar.SetPercentage(initPercentage, false);
        }

        public override IEnumerator OnStepPlayAnimationForHidding()
        {
            progressBarIsFinished = false;
            progressBar.SetPercentage(finishPercentage);

            yield return base.OnStepPlayAnimationForHidding();
            yield return WaitForProgressBarFinish();
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            progressBar.OnNewProgressBarSizeSet -= ProgressBar_OnNewProgressBarSizeSet;
        }

        private IEnumerator WaitForProgressBarFinish()
        {
            yield return new WaitUntil(() => progressBarIsFinished);
        }

        private void ProgressBar_OnNewProgressBarSizeSet()
        {
            progressBarIsFinished = true;
        }
    }
}