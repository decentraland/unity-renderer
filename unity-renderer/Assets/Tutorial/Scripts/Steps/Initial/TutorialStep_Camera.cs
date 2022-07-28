using System.Collections;
using DCL.CameraTool;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to change the camera view.
    /// </summary>
    public class TutorialStep_Camera : TutorialStep
    {
        [SerializeField] AudioEvent audioEventSuccess;

        private bool stepIsFinished = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            CommonScriptableObjects.cameraMode.OnChange += CameraMode_OnChange;
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            CommonScriptableObjects.cameraMode.OnChange -= CameraMode_OnChange;
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => stepIsFinished);
            yield return new WaitForSeconds(0.5f);
            audioEventSuccess.Play(true);
        }

        internal void CameraMode_OnChange(CameraMode.ModeId current, CameraMode.ModeId previous)
        {
            if (current != previous && mainSection.activeSelf)
                stepIsFinished = true;
        }
    }
}