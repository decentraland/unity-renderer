using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ExperiencesViewer
{
    public interface IExperiencesViewerComponentView
    {
        /// <summary>
        /// It will be triggered when the close button is clicked.
        /// </summary>
        event Action OnCloseButtonPressed;

        /// <summary>
        /// It will be triggered when the UI visibility of some experience changes.
        /// </summary>
        event Action<string, bool> OnExperienceUiVisibilityChanged;

        /// <summary>
        /// It will be triggered when the execution of some experience changes.
        /// </summary>
        event Action<string, bool> OnExperienceExecutionChanged;

        /// <summary>
        /// It represents the container transform of the component.
        /// </summary>
        Transform ExperienceViewerTransform { get; }

        /// <summary>
        /// Set the available experiences component with a list of experiences.
        /// </summary>
        /// <param name="experiences">List of experiences (model) to be loaded.</param>
        void SetAvailableExperiences(List<ExperienceRowComponentModel> experiences);

        /// <summary>
        /// Add an experience into the available experiences component.
        /// </summary>
        /// <param name="experience">Experience to add.</param>
        void AddAvailableExperience(ExperienceRowComponentModel experience);

        /// <summary>
        /// Remove an experience from the available experiences component.
        /// </summary>
        /// <param name="id">Experience id to remove.</param>
        void RemoveAvailableExperience(string id);

        /// <summary>
        /// Get all experiences.
        /// </summary>
        /// <returns>A list of experiences.</returns>
        List<ExperienceRowComponentView> GetAllAvailableExperiences();

        /// <summary>
        /// Get a specific experience.
        /// </summary>
        /// <param name="id">Id of the experience to search.</param>
        /// <returns>An experience.</returns>
        ExperienceRowComponentView GetAvailableExperienceById(string id);

        /// <summary>
        /// Shows/Hides the game object of the Experiences Viewer.
        /// </summary>
        /// <param name="isActive">True to show it.</param>
        void SetVisible(bool isActive);

        /// <summary>
        /// Shows the info toast for when the UI of an experience is hidden.
        /// </summary>
        /// <param name="getSceneName"></param>
        void ShowUiHiddenToast(string pxName);
        void ShowUiShownToast(string pxName);
        void ShowEnabledToast(string pxName);
        void ShowDisabledToast(string pxName);
    }
}
