using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Builder
{
    public interface IPublishProjectController
    {
        /// <summary>
        /// The publish has been confirmed
        /// </summary>
        event Action<IBuilderScene, PublishInfo> OnPublishPressed;
        
        /// <summary>
        /// When the publish action is canceled
        /// </summary>
        event Action OnPublishCancel;

        /// <summary>
        /// Start the publish flow for a project
        /// </summary>
        /// <param name="builderScene"></param>
        void StartPublishFlow(IBuilderScene builderScene);

        /// <summary>
        /// Set the view active
        /// </summary>
        /// <param name="isActive"></param>
        void SetActive(bool isActive);

        void Initialize();
        void Dispose();
    }

    public class PublishProjectController : IPublishProjectController
    {
        public event Action<IBuilderScene, PublishInfo> OnPublishPressed;
        public event Action OnPublishCancel;

        private const string DETAIL_PREFAB_PATH = "Project/PublishPopupView";

        internal IPublishProjectDetailView detailView;

        internal IBuilderScene sceneToPublish;

        private PublishInfo.ProjectRotation projectRotation = PublishInfo.ProjectRotation.NORTH;

        public PublishProjectController() { detailView = GameObject.Instantiate(Resources.Load<PublishProjectDetailView>(DETAIL_PREFAB_PATH)); }

        public void Initialize()
        {
            detailView.OnCancel += ViewClosed;
            detailView.OnPublishButtonPressed += PublishButtonPressedButtonPressed;
            detailView.OnProjectRotateChange += RotationChange;
        }

        public void Dispose()
        {
            detailView.OnCancel -= ViewClosed;
            detailView.OnPublishButtonPressed -= PublishButtonPressedButtonPressed;
            detailView.OnProjectRotateChange -= RotationChange;

            detailView.Dispose();
        }

        internal void RotationChange(PublishInfo.ProjectRotation newRotation) { projectRotation = newRotation; }

        public void StartPublishFlow(IBuilderScene builderScene)
        {
            sceneToPublish = builderScene;

            detailView.SetProjectToPublish(sceneToPublish);
            detailView.Show();
            detailView.ResetView();
        }

        public void SetActive(bool isActive)
        {
            if (isActive)
                detailView.Show();
            else
                ViewClosed();
        }

        private void ViewClosed()
        {
            detailView.Hide();
            OnPublishCancel?.Invoke();
        }

        private void PublishButtonPressedButtonPressed(PublishInfo info)
        {
            info.rotation = projectRotation;
            OnPublishPressed?.Invoke(sceneToPublish, info);
        }
    }
}