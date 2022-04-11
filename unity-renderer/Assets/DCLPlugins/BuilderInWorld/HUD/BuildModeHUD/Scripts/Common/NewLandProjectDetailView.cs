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
        
        event Action<string> OnSceneNameChange;

        void SetActive(bool isActive);

        void SetSceneNameValidationActive(bool isActive);
        
        void SetCreateProjectButtonActive(bool isActive);
    }

    public class NewLandProjectDetailView : MonoBehaviour, INewProjectDetailView
    {
        [SerializeField] internal Button cancelButton;
        [SerializeField] internal Button createProjectButton;
        [SerializeField] internal TMP_Text createProjectButtonText;
        
        [Header("Project name")]
        [SerializeField] internal TMP_InputField sceneNameInput;
        [SerializeField] internal TMP_Text sceneNameValidationText;
        [SerializeField] internal TMP_Text sceneNameCharCounterText;
        [SerializeField] internal int sceneNameCharLimit = 30;
        
        [Header("Project description")]
        [SerializeField] internal TMP_InputField sceneDescriptionInput;
        [SerializeField] internal TMP_Text sceneDescriptionCharCounterText;
        [SerializeField] internal int sceneDescriptionCharLimit = 140;

        public event Action<string, string> OnCreatePressed;
        public event Action<string> OnSceneNameChange;

        private void Awake()
        {
            cancelButton.onClick.AddListener(Cancel);
            createProjectButton.onClick.AddListener(CreateProject);

            sceneNameInput.onValueChanged.AddListener((newText) =>
            {
                UpdateSceneNameCharCounter();
                OnSceneNameChange?.Invoke(newText);
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

        public void SetCreateProjectButtonActive(bool isActive)
        {
            createProjectButton.interactable = isActive;
            createProjectButtonText.color = new Color(createProjectButtonText.color.r, createProjectButtonText.color.g, createProjectButtonText.color.b, isActive ? 1f : 0.5f);
        }

        internal void CreateProject()
        {
            OnCreatePressed?.Invoke(sceneNameInput.text, sceneDescriptionInput.text);
            SetActive(false);
        }

        internal void UpdateSceneNameCharCounter() { sceneNameCharCounterText.text = $"{sceneNameInput.text.Length}/{sceneNameCharLimit}"; }

        internal void UpdateSceneDescriptionCharCounter() { sceneDescriptionCharCounterText.text = $"{sceneDescriptionInput.text.Length}/{sceneDescriptionCharLimit}"; }

        internal void Cancel() { SetActive(false); }
    }
}