using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public interface INewProjectDetailController
    {
        /// <summary>
        /// Fired when the title and the description has been set
        /// </summary>
        event Action<string, string> OnNameAndDescriptionSet;
        
        /// <summary>
        /// Set the view active
        /// </summary>
        /// <param name="isActive"></param>
        void SetActive(bool isActive);

        /// <summary>
        /// Inits the view
        /// </summary>
        /// <param name="view"></param>
        void Initialize(INewProjectDetailView view);

        /// <summary>
        /// Dispose the controller
        /// </summary>
        void Dispose();
    }

    public class NewLandProjectDetailController : INewProjectDetailController
    {
        public event Action<string, string> OnNameAndDescriptionSet;
        
        private INewProjectDetailView view;
        internal bool isValidated = false;
        
        public void Initialize(INewProjectDetailView view)
        {
            this.view = view;
            view.OnCreatePressed += CreateProject;
            view.OnSceneNameChange += ValidatePublicationInfo;
        }

        public void Dispose()
        {
            if (view != null)
            {
                view.OnCreatePressed -= CreateProject;
                view.OnSceneNameChange -= ValidatePublicationInfo;
            }
        }

        private void CreateProject(string sceneName, string sceneDescription)
        {
            OnNameAndDescriptionSet?.Invoke(sceneName, sceneDescription);
        }

        public void SetActive(bool isActive)
        {
            view.SetActive(isActive);
        }

        public void ValidatePublicationInfo(string sceneName)
        {
            isValidated = sceneName.Length > 0;
            view.SetSceneNameValidationActive(!isValidated);
            view.SetCreateProjectButtonActive(isValidated);
        }
    }
}