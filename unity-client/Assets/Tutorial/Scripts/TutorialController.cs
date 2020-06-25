using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        public static TutorialController i { private set; get; }

        public bool isTutorialEnabled { private set; get; } = false;

        public void SetTutorialEnabled()
        {
            CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;
            isTutorialEnabled = true;
        }

        private void Awake()
        {
            i = this;
        }


        private void OnDestroy()
        {
            CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;
            i = null;
        }

        private int GetTutorialStepFromProfile()
        {
            return UserProfile.GetOwnUserProfile().tutorialStep;
        }

        private void OnRenderingStateChanged(bool renderingEnabled, bool prevState)
        {
            if (!isTutorialEnabled || !renderingEnabled) return;
        }
    }
}
