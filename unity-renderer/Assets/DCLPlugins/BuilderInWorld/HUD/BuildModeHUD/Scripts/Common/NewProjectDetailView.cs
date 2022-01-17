using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Builder
{
    public interface INewProjectDetailView
    {
        /// <summary>
        /// Create project button pressed
        /// </summary>
        event Action<string, string> OnCreatePressed;

        void SetActive(bool isActive);
    }

    public class NewProjectDetailView : MonoBehaviour, INewProjectDetailView
    {
        [SerializeField] internal Button createProjectButton;
        [SerializeField] internal TMP_InputField sceneNameInput;
        [SerializeField] internal TMP_Text sceneNameValidationText;
        [SerializeField] internal TMP_InputField sceneDescriptionInput;
        [SerializeField] internal TMP_Text sceneNameCharCounterText;
        [SerializeField] internal int sceneNameCharLimit = 30;
        [SerializeField] internal TMP_Text sceneDescriptionCharCounterText;
        [SerializeField] internal int sceneDescriptionCharLimit = 140;

        public event Action<string, string> OnCreatePressed;

        private void Awake()
        {
            createProjectButton.onClick.AddListener(CreateProject);

            sceneNameInput.onValueChanged.AddListener((newText) =>
            {
                UpdateSceneNameCharCounter();
            });

            sceneDescriptionInput.onValueChanged.AddListener((newText) =>
            {
                UpdateSceneDescriptionCharCounter();
            });

            sceneNameInput.characterLimit = sceneNameCharLimit;
            sceneDescriptionInput.characterLimit = sceneDescriptionCharLimit;
        }

        private void OnDestroy()
        {
            createProjectButton.onClick.RemoveListener(CreateProject);

            sceneNameInput.onValueChanged.RemoveAllListeners();
            sceneDescriptionInput.onValueChanged.RemoveAllListeners();
        }

        public void SetSceneNameValidationActive(bool isActive) { sceneNameValidationText.enabled = isActive; }

        public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

        public void CreateProject() { OnCreatePressed?.Invoke(sceneNameInput.text, sceneDescriptionInput.text); }

        public void UpdateSceneNameCharCounter() { sceneNameCharCounterText.text = $"{sceneNameInput.text.Length}/{sceneNameCharLimit}"; }

        public void UpdateSceneDescriptionCharCounter() { sceneDescriptionCharCounterText.text = $"{sceneDescriptionInput.text.Length}/{sceneDescriptionCharLimit}"; }

    }
}