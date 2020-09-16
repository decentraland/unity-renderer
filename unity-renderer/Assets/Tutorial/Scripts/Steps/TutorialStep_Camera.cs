using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to change the camera view.
    /// </summary>
    public class TutorialStep_Camera : TutorialStep
    {
        private bool stepIsFinished = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            CommonScriptableObjects.cameraMode.OnChange += CameraMode_OnChange;
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => stepIsFinished);
            yield return new WaitForSeconds(0.5f);
        }

        private void CameraMode_OnChange(CameraMode.ModeId current, CameraMode.ModeId previous)
        {
            if (current != previous)
                stepIsFinished = true;
        }
    }
}