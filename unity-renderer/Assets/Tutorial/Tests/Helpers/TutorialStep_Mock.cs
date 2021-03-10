using DCL.Tutorial;
using System.Collections;

namespace DCL.Tutorial_Tests
{
    public class TutorialStep_Mock : TutorialStep
    {
        public delegate void CustomOnStepStart();
        public CustomOnStepStart customOnStepStart;

        public delegate IEnumerator CustomOnStepExecute();
        public CustomOnStepExecute customOnStepExecute;

        public delegate IEnumerator CustomOnStepPlayAnimationForHidding();
        public CustomOnStepPlayAnimationForHidding customOnStepPlayAnimationForHidding;

        public delegate void CustomOnStepFinished();
        public CustomOnStepFinished customOnStepFinished;

        public override void OnStepStart()
        {
            customOnStepStart();
        }

        public override IEnumerator OnStepExecute()
        {
            yield return customOnStepExecute;
        }

        public override IEnumerator OnStepPlayHideAnimation()
        {
            yield return customOnStepPlayAnimationForHidding;
        }

        public override void OnStepFinished()
        {
            customOnStepFinished();
        }
    }
}