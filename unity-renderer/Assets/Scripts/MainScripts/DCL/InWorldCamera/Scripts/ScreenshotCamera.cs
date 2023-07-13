using System;
using UnityEngine;

namespace MainScripts.DCL.InWorldCamera.Scripts
{
    public class ScreenshotCamera : MonoBehaviour
    {
        [SerializeField] private GameObject cameraPrefab;
        [SerializeField] private InputAction_Trigger cameraInputAction;

        private readonly int desiredWidth = 1920;
        private readonly int desiredHeight = 1080;

        private bool isInitialized;

        private void OnEnable()
        {
            cameraInputAction.OnTriggered += ToggleScreenshotCamera;
        }

        private void OnDisable()
        {
            cameraInputAction.OnTriggered -= ToggleScreenshotCamera;
        }

        private void ToggleScreenshotCamera(DCLAction_Trigger action)
        {
            Debug.Log("VV:: Camera enabled");
        }
    }
}
